using Core.Enums;
using Core.Enums.Core.Enums;
using Core.Models.Base;
using System.Collections.Generic;

namespace Core.Models.Buildings
{
    public class CommercialBuilding : Building
    {
        public CommercialBuildingType Type { get; set; }
        public int Capacity { get; set; }
        public int EmployeeCount { get; set; }
        public List<string> ProductCategories { get; set; }

        // Реализация абстрактных свойств
        public override BuildingType BuildingType => BuildingType.Commercial;
        public override decimal BuildCost => GetBuildCostByType(Type);
        public override Dictionary<ConstructionMaterial, int> RequiredMaterials => GetRequiredMaterialsByType(Type);

        public CommercialBuilding(CommercialBuildingType type)
        {
            Type = type;
            ProductCategories = new List<string>();
            SetDefaultValuesByType(type);
        }

        private void SetDefaultValuesByType(CommercialBuildingType type)
        {
            switch (type)
            {
                case CommercialBuildingType.Shop:
                    EmployeeCount = 3;
                    Width = 1; Height = 1; Floors = 1;
                    Capacity = 10;
                    ProductCategories = new List<string> { "Продовольствие", "Напитки" };
                    break;

                case CommercialBuildingType.Supermarket:
                    EmployeeCount = 15;
                    Width = 3; Height = 3; Floors = 1;
                    Capacity = 50;
                    ProductCategories = new List<string> { "Продовольствие", "Напитки", "Хозтовары" };
                    break;

                case CommercialBuildingType.Cafe:
                    EmployeeCount = 5;
                    Width = 1; Height = 1; Floors = 1;
                    Capacity = 20;
                    ProductCategories = new List<string> { "Еда", "Напитки", "Десерты" };
                    break;

                case CommercialBuildingType.Restaurant:
                    EmployeeCount = 12;
                    Width = 2; Height = 2; Floors = 1;
                    Capacity = 40;
                    ProductCategories = new List<string> { "Еда", "Напитки", "Десерты", "Алкоголь" };
                    break;

                case CommercialBuildingType.GasStation:
                    EmployeeCount = 4;
                    Width = 2; Height = 2; Floors = 1;
                    Capacity = 15;
                    ProductCategories = new List<string> { "Бензин", "Дизель", "Сопутствующие товары" };
                    break;

                case CommercialBuildingType.Pharmacy:
                    EmployeeCount = 4;
                    Width = 1; Height = 1; Floors = 1;
                    Capacity = 12;
                    ProductCategories = new List<string> { "Лекарства", "Медицинские товары", "Витамины" };
                    break;

                case CommercialBuildingType.Factory:
                    EmployeeCount = 25;
                    Width = 4; Height = 3; Floors = 2;
                    Capacity = 30;
                    ProductCategories = new List<string> { "Промышленные товары", "Производство" };
                    break;
            }
        }

        private decimal GetBuildCostByType(CommercialBuildingType type)
        {
            return type switch
            {
                CommercialBuildingType.Shop => 50_000m,
                CommercialBuildingType.Supermarket => 200_000m,
                CommercialBuildingType.Cafe => 80_000m,
                CommercialBuildingType.Restaurant => 150_000m,
                CommercialBuildingType.GasStation => 100_000m,
                CommercialBuildingType.Pharmacy => 70_000m,
                CommercialBuildingType.Factory => 500_000m,
                _ => 50_000m
            };
        }

        private Dictionary<ConstructionMaterial, int> GetRequiredMaterialsByType(CommercialBuildingType type)
        {
            return type switch
            {
                CommercialBuildingType.Shop => new Dictionary<ConstructionMaterial, int>
                    { { ConstructionMaterial.Concrete, 5 }, { ConstructionMaterial.Glass, 2 } },
                CommercialBuildingType.Supermarket => new Dictionary<ConstructionMaterial, int>
                    { { ConstructionMaterial.Concrete, 20 }, { ConstructionMaterial.Glass, 10 }, { ConstructionMaterial.Steel, 5 } },
                CommercialBuildingType.Cafe => new Dictionary<ConstructionMaterial, int>
                    { { ConstructionMaterial.Concrete, 8 }, { ConstructionMaterial.Glass, 3 } },
                CommercialBuildingType.Restaurant => new Dictionary<ConstructionMaterial, int>
                    { { ConstructionMaterial.Concrete, 15 }, { ConstructionMaterial.Glass, 6 }, { ConstructionMaterial.Steel, 3 } },
                CommercialBuildingType.GasStation => new Dictionary<ConstructionMaterial, int>
                    { { ConstructionMaterial.Concrete, 12 }, { ConstructionMaterial.Steel, 8 } },
                CommercialBuildingType.Pharmacy => new Dictionary<ConstructionMaterial, int>
                    { { ConstructionMaterial.Concrete, 6 }, { ConstructionMaterial.Glass, 4 } },
                CommercialBuildingType.Factory => new Dictionary<ConstructionMaterial, int>
                    { { ConstructionMaterial.Steel, 20 }, { ConstructionMaterial.Concrete, 15 } },
                _ => new Dictionary<ConstructionMaterial, int> { { ConstructionMaterial.Concrete, 5 } }
            };
        }

        public override void OnBuildingPlaced()
        {
            // Логика при размещении коммерческого здания
        }
    }
}