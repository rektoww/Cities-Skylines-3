using Core.Enums;
using Core.Resourses;
using System;
using System.Collections.Generic;

namespace Core.Models.Base
{
    /// <summary>
    /// Абстрактный класс для портов (морских и воздушных).
    /// </summary>
    public abstract class Port : TransitStation
    {
        protected abstract int MaxUnits { get; }

        public List<PortUnit> Units { get; private set; } = new(); // Список юнитов
        public PlayerResources PlayerResources { get; private set; } // Ссылка на ресурсы игрока

        // Параметры юнитов
        protected abstract string ResourceType { get; } // Тип ресурса 
        protected abstract int UnitCapacity { get; } // Вместимость юнита
        protected abstract int UnitCooldown { get; } // Время одного цикла продажи в тиках
        protected abstract int UnitRevenue { get; } // Доход за один цикл продажи

        // События для UI/логики
        // unitId, amountSold, revenueReceived
        public event Action<int, int, decimal>? UnitSold;
        public event Action<int>? UnitSuspended;
        public event Action<int>? UnitResumed;
        public event Action<int>? UnitAssignmentCleared;

        // Теперь конструктор принимает имя и ресурсы игрока
        protected Port(PlayerResources playerResources)
        {
            PlayerResources = playerResources;
        }

        /// <summary>
        /// Вызывается после размещения здания на карте (Building.TryPlace -> OnBuildingPlaced).
        /// Здесь безопасно создавать юниты, потому что наследник полностью сконструирован.
        /// </summary>
        public override void OnBuildingPlaced()
        {
            InitializeUnits();
        }

        /// <summary>
        /// Создает юниты для порта.
        /// </summary>
        private void InitializeUnits()
        {
            // Защита от некорректных значений
            var count = Math.Max(0, MaxUnits);
            for (int i = 0; i < count; i++)
            {
                var unit = CreateUnit();
                // Подписываемся на событие продажи внутри юнита, чтобы форвардить его наружу с id
                unit.Sold += (u, amount, revenue) =>
                {
                    var id = Units.IndexOf(u);
                    if (id >= 0)
                        UnitSold?.Invoke(id, amount, revenue);
                };
                Units.Add(unit);
            }
        }

        /// <summary>
        /// Создает новый юнит с параметрами, определенными в наследниках.
        /// </summary>
        protected abstract PortUnit CreateUnit();

        /// <summary>
        /// Выполняет тик симуляции для всех юнитов порта.
        /// </summary>
        public void ProcessTick()
        {
            foreach (var unit in Units)
            {
                unit.Process(PlayerResources);
            }
        }

        /// <summary>
        /// Устанавливает тип ресурса, его количество и доход для юнита.
        /// Возобновляет отправку (если юнит был в неактивном состоянии), т.к. игрок явно назначил ресурс.
        /// </summary>
        /// <param name="unitId">ID юнита</param>
        /// <param name="resourceType">Тип ресурса</param>
        /// <param name="amount">Количество ресурса</param>
        /// <param name="revenue">Доход за один цикл продажи</param>
        /// <returns>True если настройка применена успешно</returns>
        public bool SetUnitResourceAndRevenue(int unitId, ConstructionMaterial resourceType, int amount, int revenue)
        {
            if (unitId < 0 || unitId >= Units.Count)
                return false;

            var unit = Units[unitId];
            if (!unit.SetResource(resourceType, amount))
                return false;

            unit.SetRevenue(revenue);
            unit.ResumeSending(); // назначение ресурса подразумевает старт отправки
            return true;
        }

        // Управление отправкой со стороны игрока
        public void SuspendUnit(int unitId)
        {
            if (unitId < 0 || unitId >= Units.Count) return;
            Units[unitId].SuspendSending();
            UnitSuspended?.Invoke(unitId);
        }

        public void ResumeUnit(int unitId)
        {
            if (unitId < 0 || unitId >= Units.Count) return;
            Units[unitId].ResumeSending();
            UnitResumed?.Invoke(unitId);
        }

        public void ClearUnitAssignment(int unitId)
        {
            if (unitId < 0 || unitId >= Units.Count) return;
            Units[unitId].ClearAssignment();
            UnitAssignmentCleared?.Invoke(unitId);
        }

        /// <summary>
        /// Возвращает снимок состояния всех юнитов для UI.
        /// </summary>
        public IReadOnlyList<PortUnitInfo> GetUnitsSnapshot()
        {
            var list = new List<PortUnitInfo>(Units.Count);
            for (int i = 0; i < Units.Count; i++)
            {
                var u = Units[i];
                list.Add(new PortUnitInfo(
                    i,
                    u.CurrentResourceType,
                    u.ResourceAmount,
                    u.RevenuePerDelivery,
                    u.Cooldown,
                    u.Capacity,
                    u.IsSuspended));
            }
            return list;
        }

        public record PortUnitInfo(int Id, ConstructionMaterial? ResourceType, int Amount, int RevenuePerDelivery, int Cooldown, int Capacity, bool IsSuspended);
    }

    /// <summary>   
    /// Базовый класс для юнитов порта (корабли/самолеты).
    /// </summary>
    public class PortUnit
    {
        private readonly int _capacity;
        private readonly int _cooldownMax;
        private int _revenuePerDelivery;

        private int _cooldown;
        private ConstructionMaterial? _resourceType;
        private int _resourceAmount;
        private bool _suspended;

        // Событие о факте продажи: sender , amountSold, revenueDecimal
        public event Action<PortUnit, int, decimal>? Sold;

        public PortUnit(int capacity, int cooldownMax, int revenuePerDelivery)
        {
            _capacity = capacity;
            _cooldownMax = Math.Max(1, cooldownMax);
            _revenuePerDelivery = Math.Max(0, revenuePerDelivery);
            _cooldown = -1;
            _resourceType = null;
            _resourceAmount = 0;
            _suspended = false;
        }

        // Публичные геттеры для UI (не меняем имена приватных полей)
        public int Capacity => _capacity;
        public int Cooldown => _cooldown;
        public ConstructionMaterial? CurrentResourceType => _resourceType;
        public int ResourceAmount => _resourceAmount;
        public int RevenuePerDelivery => _revenuePerDelivery;
        public bool IsSuspended => _suspended;

        /// <summary>
        /// Устанавливает тип ресурса и его количество для юнита.
        /// Цикл запускается (ставится в 0) только после успешной установки ресурса.
        /// </summary>
        /// <param name="resourceType">Тип ресурса</param>
        /// <param name="amount">Количество ресурса</param>
        /// <returns>True если настройка применена успешно</returns>
        public bool SetResource(ConstructionMaterial resourceType, int amount)
        {
            if (amount <= 0 || amount > _capacity)
                return false;

            _resourceType = resourceType;
            _resourceAmount = amount;
            _cooldown = 0; // запуск цикла: следующая итерация Process попытается продать
            return true;
        }

        /// <summary>
        /// Устанавливает доход за один цикл продажи для юнита.
        /// </summary>
        /// <param name="revenue">Доход</param>
        public void SetRevenue(int revenue)
        {
            if (revenue > 0)
            {
                _revenuePerDelivery = revenue;
            }
        }

        public void SuspendSending() => _suspended = true;
        public void ResumeSending() => _suspended = false;

        public void ClearAssignment()
        {
            _resourceType = null;
            _resourceAmount = 0;
            _cooldown = -1; // неактивен
        }

        /// <summary>
        /// Выполняет тик симуляции для юнита.
        /// </summary>
        public void Process(PlayerResources playerResources)
        {
            if (_cooldown < 0)
                return;

            if (_cooldown > 0)
            {
                _cooldown--;
                return;
            }

            // _cooldown == 0: пробуем продать
            if (!_resourceType.HasValue)
                return;

            // если пользователь остановил отправку — не начинаем новую продажу
            if (_suspended)
                return;

            if (!playerResources.StoredMaterials.TryGetValue(_resourceType.Value, out int available) || available <= 0)
            {
                // нет ресурса — оставляем _cooldown == 0, чтобы проверять каждую итерацию
                return;
            }

            int amountToSell = Math.Min(_resourceAmount, available);

            playerResources.StoredMaterials[_resourceType.Value] = available - amountToSell;

            decimal revenueThisTime = 0m;
            if (_resourceAmount > 0)
            {
                revenueThisTime = (decimal)_revenuePerDelivery * amountToSell / _resourceAmount;
                playerResources.Balance += revenueThisTime;
            }

            // Уведомляем подписчиков о продаже
            Sold?.Invoke(this, amountToSell, revenueThisTime);

            // Сбрасываем таймер после успешной продажи (полной или частичной)
            _cooldown = _cooldownMax;
        }
    }
}