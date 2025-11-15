using Core.Enums;
using Core.Enums.Core.Enums;
using Core.Interfaces;
using Core.Models.Base;
using Core.Resourses;
using System;
using System.Collections.Generic;

namespace Core.Models.Buildings.IndustrialBuildings
{
    /// <summary>
    /// Шахта - добывающее здание, производящее строительные материалы со временем
    /// </summary>
    public class Mine : Building
    {
        /// <summary>
        /// Накопленные ресурсы в шахте
        /// </summary>
        public int StoredResources { get; set; }

        /// <summary>
        /// Скорость добычи (единиц за тик)
        /// </summary>
        public int ProductionRate { get; set; } = 3;

        /// <summary>
        /// Максимальная вместимость хранилища
        /// </summary>
        public int MaxStorage { get; set; } = 100;

        /// <summary>
        /// Тип добываемого материала
        /// </summary>
        public ConstructionMaterial ProducedMaterial { get; set; }

        /// <summary>
        /// Время последней добычи
        /// </summary>
        public DateTime LastProductionTime { get; set; }

        // Реализация абстрактных свойств Building
        public override decimal BuildCost { get; } = 30000m;

        public override Dictionary<ConstructionMaterial, int> RequiredMaterials { get; } = new()
        {
            { ConstructionMaterial.Steel, 5 },
            { ConstructionMaterial.Concrete, 8 }
        };

        public override BuildingType BuildingType { get; } = BuildingType.Industrial;

        public Mine() 
        {
            Name = "Шахта";
            MaxOccupancy = 20;
            CurrentOccupancy = 0;
            StoredResources = 0;
            LastProductionTime = DateTime.Now;

            // Случайный тип добываемого материала
            var random = new Random();
            var materials = new[] { ConstructionMaterial.Steel, ConstructionMaterial.Concrete };
            ProducedMaterial = materials[random.Next(materials.Length)];
        }

        public override void OnBuildingPlaced()
        {
            // При размещении шахты начинаем производство
            LastProductionTime = DateTime.Now;
        }

        /// <summary>
        /// Симулирует добычу ресурсов
        /// </summary>
        public void ProduceResources()
        {
            if (!IsOperational)
                return;

            if (StoredResources >= MaxStorage)
                return;

            int produced = Math.Min(ProductionRate, MaxStorage - StoredResources);
            StoredResources += produced;
            LastProductionTime = DateTime.Now;
        }

        /// <summary>
        /// Собирает накопленные ресурсы из шахты
        /// </summary>
        /// <param name="amount">Количество для сбора (0 = всё)</param>
        /// <returns>Фактически собранное количество</returns>
        public int CollectResources(int amount = 0)
        {
            if (amount <= 0)
                amount = StoredResources;

            int collected = Math.Min(amount, StoredResources);
            StoredResources -= collected;
            return collected;
        }
    }
}