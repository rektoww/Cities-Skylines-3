using Core.Enums;
using Core.Enums.Core.Enums;
using Core.Models.Base;
using Core.Models.Mobs;
using System.Collections.Generic;

namespace Core.Models.Buildings
{
    public class ResidentialBuilding : Building
    {
        public ResidentialType Type { get; set; }
        public int Capacity { get; set; }
        public List<Citizen> CurrentResidents { get; set; } = new();
        public int ApartmentCount { get; set; }
        public decimal RentPrice { get; set; }
        public bool HasVacancy => CurrentResidents.Count < Capacity;

        // Реализация абстрактных свойств
        public override BuildingType BuildingType => BuildingType.Residential;
        public override decimal BuildCost => GetBuildCostByType(Type);
        public override Dictionary<ConstructionMaterial, int> RequiredMaterials => GetRequiredMaterialsByType(Type);

        public ResidentialBuilding(ResidentialType type)
        {
            Type = type;
            SetDefaultValuesByType();
        }

        private void SetDefaultValuesByType()
        {
            switch (Type)
            {
                case ResidentialType.Apartment:
                    Capacity = 100;
                    ApartmentCount = 50;
                    RentPrice = 10000m;
                    Width = 3;
                    Height = 3;
                    Floors = 9;
                    break;

                case ResidentialType.Dormitory:
                    Capacity = 200;
                    ApartmentCount = 100;
                    RentPrice = 3000m;
                    Width = 4;
                    Height = 3;
                    Floors = 5;
                    break;
                case ResidentialType.Hotel:
                    Capacity = 250;
                    ApartmentCount = 25;
                    RentPrice = 5000m;
                    Width = 4;
                    Height = 1;
                    Floors = 5;
                    break;
            }
        }

        private decimal GetBuildCostByType(ResidentialType type)
        {
            return type switch
            {
                ResidentialType.Apartment => 500_000m,
                ResidentialType.Dormitory => 300_000m,
                ResidentialType.Hotel => 400_000m,
                _ => 200_000m
            };
        }

        private Dictionary<ConstructionMaterial, int> GetRequiredMaterialsByType(ResidentialType type)
        {
            return type switch
            {
                ResidentialType.Apartment => new Dictionary<ConstructionMaterial, int>
                    { { ConstructionMaterial.Concrete, 30 }, { ConstructionMaterial.Steel, 15 }, { ConstructionMaterial.Glass, 10 } },
                ResidentialType.Dormitory => new Dictionary<ConstructionMaterial, int>
                    { { ConstructionMaterial.Concrete, 20 }, { ConstructionMaterial.Steel, 10 }, { ConstructionMaterial.Glass, 5 } },
                ResidentialType.Hotel => new Dictionary<ConstructionMaterial, int>
                    { { ConstructionMaterial.Concrete, 25 }, { ConstructionMaterial.Steel, 12 }, { ConstructionMaterial.Glass, 8 } },
                _ => new Dictionary<ConstructionMaterial, int> { { ConstructionMaterial.Concrete, 10 } }
            };
        }

        public bool TryAddResident(Citizen citizen)
        {
            if (CurrentResidents.Count < Capacity)
            {
                CurrentResidents.Add(citizen);
                citizen.Home = this;
                return true;
            }
            return false;
        }

        public override void OnBuildingPlaced()
        {
            // Логика при размещении жилого здания
        }
    }
}