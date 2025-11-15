using Core.Enums;
using Core.Enums.Core.Enums;
using Core.Models.Base;
using System;
using System.Collections.Generic;

namespace Core.Models.Buildings
{
    public class IndustrialBuilding : Building
    {
        public IndustrialBuildingType Type { get; set; }
        public int ProductionRate { get; set; }
        public ConstructionMaterial ProducedMaterial { get; set; }
        public int StoredResources { get; set; }
        public int MaxStorage { get; set; }

        // Реализация абстрактных свойств
        public override BuildingType BuildingType => BuildingType.Industrial;
        public override decimal BuildCost => GetBuildCostByType(Type);
        public override Dictionary<ConstructionMaterial, int> RequiredMaterials => GetRequiredMaterialsByType(Type);

        public IndustrialBuilding(IndustrialBuildingType type)
        {
            Type = type;
            SetDefaultValuesByType(type);
        }

        private void SetDefaultValuesByType(IndustrialBuildingType type)
        {
            switch (type)
            {
                case IndustrialBuildingType.Factory:
                    Width = 4; Height = 3; Floors = 2;
                    ProductionRate = 10;
                    ProducedMaterial = ConstructionMaterial.Plastic;
                    MaxStorage = 200;
                    break;

                case IndustrialBuildingType.Farm:
                    Width = 3; Height = 3; Floors = 1;
                    ProductionRate = 15;
                    ProducedMaterial = ConstructionMaterial.Wood;
                    MaxStorage = 150;
                    break;

                case IndustrialBuildingType.Mine:
                    Width = 2; Height = 2; Floors = 1;
                    ProductionRate = 5;
                    ProducedMaterial = ConstructionMaterial.Steel;
                    MaxStorage = 100;
                    break;

                case IndustrialBuildingType.PowerPlant:
                    Width = 5; Height = 4; Floors = 3;
                    ProductionRate = 0; // Электростанция не производит материалы
                    ProducedMaterial = ConstructionMaterial.None;
                    MaxStorage = 0;
                    break;
            }
        }

        private decimal GetBuildCostByType(IndustrialBuildingType type)
        {
            return type switch
            {
                IndustrialBuildingType.Factory => 300_000m,
                IndustrialBuildingType.Farm => 100_000m,
                IndustrialBuildingType.Mine => 250_000m,
                IndustrialBuildingType.PowerPlant => 1_000_000m,
                _ => 200_000m
            };
        }

        private Dictionary<ConstructionMaterial, int> GetRequiredMaterialsByType(IndustrialBuildingType type)
        {
            return type switch
            {
                IndustrialBuildingType.Factory => new Dictionary<ConstructionMaterial, int>
                    { { ConstructionMaterial.Steel, 20 }, { ConstructionMaterial.Concrete, 15 } },
                IndustrialBuildingType.Farm => new Dictionary<ConstructionMaterial, int>
                    { { ConstructionMaterial.Wood, 10 }, { ConstructionMaterial.Concrete, 5 } },
                IndustrialBuildingType.Mine => new Dictionary<ConstructionMaterial, int>
                    { { ConstructionMaterial.Steel, 10 }, { ConstructionMaterial.Concrete, 5 } },
                IndustrialBuildingType.PowerPlant => new Dictionary<ConstructionMaterial, int>
                    { { ConstructionMaterial.Steel, 50 }, { ConstructionMaterial.Concrete, 30 }, { ConstructionMaterial.Glass, 20 } },
                _ => new Dictionary<ConstructionMaterial, int> { { ConstructionMaterial.Concrete, 10 } }
            };
        }

        public void ProduceResources()
        {
            if (IsOperational && ProducedMaterial != ConstructionMaterial.None)
            {
                StoredResources = Math.Min(MaxStorage, StoredResources + ProductionRate);
            }
        }

        public int CollectResources()
        {
            var collected = StoredResources;
            StoredResources = 0;
            return collected;
        }

        public override void OnBuildingPlaced()
        {
            // Логика при размещении промышленного здания
        }
    }
}