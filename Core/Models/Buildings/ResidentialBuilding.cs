using Core.Enums;
using Core.Models.Base;
using Core.Models.Mobs;

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
            //
        }
    }
}