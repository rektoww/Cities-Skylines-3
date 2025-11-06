using Core.Enums;
using Core.Resourses;
    using System;
using System.Collections.Generic;

namespace Core.Models.Base
{
    /// <summary>
    /// Абстрактный класс для портов (морских и воздушных).
    /// </summary>
    public abstract class AirPort
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

        protected AirPort(string name, int maxUnits, PlayerResources playerResources)
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
    }

    /// <summary>
    /// Базовый класс для юнитов порта (корабли/самолеты).
    /// </summary>
    public class PortUnit
    {
        private readonly string _resourceType; // Тип ресурса
        private readonly int _capacity; // Вместимость юнита
        private readonly int _cooldownMax; // Время одного цикла продажи
        private readonly int _revenuePerDelivery; // Доход за один цикл продажи

        private int _cooldown; // Текущий таймер до следующей продажи

        public PortUnit(string resourceType, int capacity, int cooldownMax, int revenuePerDelivery)
        {
            _resourceType = resourceType;
            _capacity = capacity;
            _cooldownMax = cooldownMax;
            _revenuePerDelivery = revenuePerDelivery;
            _cooldown = cooldownMax; // Инициализируем таймер
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

            // Проверяем, есть ли ресурсы для продажи
            if (playerResources.StoredMaterials.TryGetValue(Enum.Parse<ConstructionMaterial>(_resourceType), out int available) && available >= _capacity)
            {
                // Списываем ресурсы и начисляем деньги
                playerResources.StoredMaterials[Enum.Parse<ConstructionMaterial>(_resourceType)] -= _capacity;
                playerResources.Balance += _revenuePerDelivery;
                _cooldown = _cooldownMax; // Сбрасываем таймер
            }
        }
    }
}
