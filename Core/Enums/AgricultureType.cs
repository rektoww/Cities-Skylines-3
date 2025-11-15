using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Enums
{
    public enum AgricultureType
    {
        // Растениеводство
        CropFarm,
        GrainFarm,
        VegetableFarm,
        Orchard,
        Vineyard,
        Greenhouse,
        // Животноводство
        LivestockFarm,
        DairyFarm,
        PoultryFarm,
        SheepFarm
    }

    namespace Core.Enums
    {
        public enum BuildingType
        {
            Commercial,
            Residential,
            Industrial,
            Service,
            Park,
            PoliceStation,
            Airport,
            Seaport,
            TrainStation,
            BusStop
        }

        public enum CommercialBuildingType
        {
            Shop,
            Supermarket,
            Cafe,
            Restaurant,
            GasStation,
            Pharmacy,
            Factory
        }
        public enum IndustrialBuildingType
        {
            Factory,
            Farm,
            Mine,
            PowerPlant
        }

        public enum ResidentialType
        {
            Apartment,
            Dormitory,
            Hotel
        }
        public enum ServiceBuildingType
        {
            School,
            Hospital,
            University,
            Park,
            PoliceStation
        }
    }
}
