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
        public int MaxUnits { get; protected set; } // Максимальное количество юнитов
        public List<PortUnit> Units { get; private set; } = new(); // Список юнитов
        public PlayerResources PlayerResources { get; private set; } // Ссылка на ресурсы игрока

        // Параметры юнитов (определяются в наследниках)
        protected abstract string ResourceType { get; } // Тип ресурса
        protected abstract int UnitCapacity { get; } // Вместимость юнита
        protected abstract int UnitCooldown { get; } // Время одного цикла продажи (в тиках)
        protected abstract int UnitRevenue { get; } // Доход за один цикл продажи

        protected Port(string name, int maxUnits, PlayerResources playerResources)
        {
            Name = name;
            MaxUnits = maxUnits;
            PlayerResources = playerResources;

            // Создаем юниты при создании порта
            InitializeUnits();
        }

        /// <summary>
        /// Создает юниты для порта.
        /// </summary>
        private void InitializeUnits()
        {
            for (int i = 0; i < MaxUnits; i++)
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
        /// Устанавливает тип ресурса и его количество для юнита.
        /// </summary>
        /// <param name="unitId">ID юнита</param>
        /// <param name="resourceType">Тип ресурса</param>
        /// <param name="amount">Количество ресурса</param>
        /// <returns>True если настройка применена успешно</returns>
        public bool SetUnitResource(int unitId, ConstructionMaterial resourceType, int amount)
        {
            if (unitId < 0 || unitId >= Units.Count)
                return false;

            var unit = Units[unitId];
            return unit.SetResource(resourceType, amount);
        }
    }

    /// <summary>
    /// Базовый класс для юнитов порта (корабли/самолеты).
    /// </summary>
    public class PortUnit
    {
        private readonly int _capacity; // Вместимость юнита
        private readonly int _cooldownMax; // Время одного цикла продажи
        private readonly int _revenuePerDelivery; // Доход за один цикл продажи

        private int _cooldown; // Текущий таймер до следующей продажи
        private ConstructionMaterial? _resourceType; // Текущий тип ресурса
        private int _resourceAmount; // Текущее количество ресурса

        public PortUnit(int capacity, int cooldownMax, int revenuePerDelivery)
        {
            _capacity = capacity;
            _cooldownMax = cooldownMax;
            _revenuePerDelivery = revenuePerDelivery;
            _cooldown = cooldownMax; // Инициализируем таймер
            _resourceType = null;
            _resourceAmount = 0;
        }

        /// <summary>
        /// Устанавливает тип ресурса и его количество для юнита.
        /// </summary>
        /// <param name="resourceType">Тип ресурса</param>
        /// <param name="amount">Количество ресурса</param>
        /// <returns>True если настройка применена успешно</returns>
        public bool SetResource(ConstructionMaterial resourceType, int amount)
        {
            if (amount <= 0)
                return false;

            _resourceType = resourceType;
            _resourceAmount = amount;
            return true;
        }

        /// <summary>
        /// Выполняет тик симуляции для юнита.
        /// </summary>
        public void Process(PlayerResources playerResources)
        {
            if (_cooldown > 0)
            {
                _cooldown--;
                return;
            }

            // Проверяем, установлен ли тип ресурса и есть ли ресурсы для продажи
            if (_resourceType.HasValue &&
                playerResources.StoredMaterials.TryGetValue(_resourceType.Value, out int available) &&
                available >= _resourceAmount)
            {
                // Списываем ресурсы и начисляем деньги
                playerResources.StoredMaterials[_resourceType.Value] -= _resourceAmount;
                playerResources.Balance += _revenuePerDelivery;
                _cooldown = _cooldownMax; // Сбрасываем таймер
            }
        }
    }
}