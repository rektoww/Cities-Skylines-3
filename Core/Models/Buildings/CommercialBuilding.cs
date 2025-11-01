using Core.Enums;
using Core.Models.Base;
using Core.Models.Map;
using System.Collections.Generic;

namespace Core.Models.Buildings
{
    public abstract class CommercialBuilding : Building
    {
        public CommercialBuildingType Type { get; set; }
        public int Capacity { get; set; }
        public int EmployeeCount { get; set; }
        public List<string> ProductCategories { get; set; }

        public CommercialBuilding(CommercialBuildingType type, int capacity = 0)
        {
            Type = type;
            Capacity = capacity == 0 ? GetDefaultCapacity(type) : capacity;
            ProductCategories = new List<string>();
            SetDefaultValuesByType(type);
        }

        private void SetDefaultValuesByType(CommercialBuildingType type)
        {
            switch (type)
            {
                case CommercialBuildingType.Shop:
                    EmployeeCount = 3;
                    BuildCost = 50000m;
                    Width = 2; Height = 1; Floors = 1;
                    ProductCategories = new List<string> { "Продовольствие", "Напитки" };
                    break;

                case CommercialBuildingType.Supermarket:
                    EmployeeCount = 15;
                    BuildCost = 200000m;
                    Width = 3; Height = 2; Floors = 1;
                    ProductCategories = new List<string> { "Продовольствие", "Напитки", "Хозтовары" };
                    break;

                case CommercialBuildingType.Cafe:
                    EmployeeCount = 5;
                    BuildCost = 80000m;
                    Width = 2; Height = 1; Floors = 1;
                    ProductCategories = new List<string> { "Еда", "Напитки", "Десерты" };
                    break;

                case CommercialBuildingType.Restaurant:
                    EmployeeCount = 12;
                    BuildCost = 150000m;
                    Width = 3; Height = 2; Floors = 1;
                    ProductCategories = new List<string> { "Еда", "Напитки", "Десерты", "Алкоголь" };
                    break;

                case CommercialBuildingType.GasStation:
                    EmployeeCount = 4;
                    BuildCost = 100000m;
                    Width = 2; Height = 2; Floors = 1;
                    ProductCategories = new List<string> { "Бензин", "Дизель", "Сопутствующие товары" };
                    break;

                case CommercialBuildingType.Pharmacy:
                    EmployeeCount = 4;
                    BuildCost = 70000m;
                    Width = 2; Height = 1; Floors = 1;
                    ProductCategories = new List<string> { "Лекарства", "Медицинские товары", "Витамины" };
                    break;
            }
        }

        private int GetDefaultCapacity(CommercialBuildingType type)
        {
            return type switch
            {
                CommercialBuildingType.Shop => 10,
                CommercialBuildingType.Supermarket => 50,
                CommercialBuildingType.Cafe => 20,
                CommercialBuildingType.Restaurant => 40,
                CommercialBuildingType.GasStation => 15,
                CommercialBuildingType.Pharmacy => 12,
                _ => 10
            };
        }

        public override void OnBuildingPlaced()
        {
            // Пока пустая реализация
        }
    }
}