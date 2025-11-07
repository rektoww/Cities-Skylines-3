using Core.Enums;
using Core.Resourses;
using System;
using System.Collections.Generic;

namespace Core.Models.Base
{
    /// <summary>
    /// Абстрактный класс для портов (морских и воздушных).
    /// </summary>
    public abstract class Port
    {
        public string Name { get; set; }

        // УПРОЩЕНИЕ: Оставляем только одно абстрактное свойство, которое должны реализовать наследники.
        protected abstract int MaxUnits { get; }

        public List<PortUnit> Units { get; private set; } = new(); // Список юнитов
        public PlayerResources PlayerResources { get; private set; } // Ссылка на ресурсы игрока

        // Параметры юнитов
        protected abstract string ResourceType { get; } // Тип ресурса
        protected abstract int UnitCapacity { get; } // Вместимость юнита
        protected abstract int UnitCooldown { get; } // Время одного цикла продажи (в тиках)
        protected abstract int UnitRevenue { get; } // Доход за один цикл продажи

        protected Port(string name, PlayerResources playerResources)
        {
            Name = name;
            PlayerResources = playerResources;

            // Создаем юниты при создании порта
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
                Units.Add(CreateUnit());
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
            return true;
        }
    }


    public class PortUnit
    {
        private readonly int _capacity;
        private readonly int _cooldownMax;
        private int _revenuePerDelivery;

        private int _cooldown;
        private ConstructionMaterial? _resourceType;
        private int _resourceAmount;

        public PortUnit(int capacity, int cooldownMax, int revenuePerDelivery)
        {
            _capacity = capacity;
            _cooldownMax = Math.Max(1, cooldownMax);
            _revenuePerDelivery = Math.Max(0, revenuePerDelivery);
            _cooldown = -1;
            _resourceType = null;
            _resourceAmount = 0;
        }

        public bool SetResource(ConstructionMaterial resourceType, int amount)
        {
            if (amount <= 0 || amount > _capacity)
                return false;

            _resourceType = resourceType;
            _resourceAmount = amount;
            _cooldown = 0;
            return true;
        }

        public void SetRevenue(int revenue)
        {
            if (revenue > 0)
            {
                _revenuePerDelivery = revenue;
            }
        }

        public void Process(PlayerResources playerResources)
        {
            if (_cooldown < 0)
                return;

            if (_cooldown > 0)
            {
                _cooldown--;
                return;
            }

            if (!_resourceType.HasValue)
                return;

            playerResources.StoredMaterials.TryGetValue(_resourceType.Value, out int available);

            if (available <= 0)
            {
                return;
            }

            int amountToSell = Math.Min(_resourceAmount, available);

            playerResources.StoredMaterials[_resourceType.Value] = available - amountToSell;

            if (_resourceAmount > 0)
            {
                decimal revenueThisTime = (decimal)_revenuePerDelivery * amountToSell / _resourceAmount;
                playerResources.Balance += revenueThisTime;
            }

            _cooldown = _cooldownMax;
        }
    }
}