using Core.Enums;
using Core.Enums.Core.Enums;
using Core.Models.Base;
using Core.Models.Buildings;
using Core.Models.Police;
using Core.Resourses;
using System;
using System.Collections.Generic;

namespace Core.Services
{
    /// <summary>
    /// Фабрика для создания зданий через enum типы
    /// </summary>
    public class BuildingFactory
    {
        private readonly PlayerResources _defaultResources;

        public BuildingFactory()
        {
            // Создаем дефолтные ресурсы для зданий, которые требуют PlayerResources
            _defaultResources = new PlayerResources(0, new Dictionary<ConstructionMaterial, int>());
        }

        public BuildingFactory(PlayerResources defaultResources)
        {
            _defaultResources = defaultResources;
        }

        /// <summary>
        /// Создает здание по основному типу BuildingType
        /// </summary>
        public Building CreateBuilding(BuildingType type)
        {
            return type switch
            {
                BuildingType.Park => new ServiceBuilding(ServiceBuildingType.Park),
                BuildingType.PoliceStation => new PoliceStation(),
                BuildingType.Airport => new AirPort(_defaultResources),
                BuildingType.Seaport => new SeaPort(_defaultResources),
                BuildingType.BusStop => new BusStop(),
                _ => throw new ArgumentException($"Unknown building type: {type}")
            };
        }

        /// <summary>
        /// Создает коммерческое здание по типу
        /// </summary>
        public CommercialBuilding CreateCommercialBuilding(CommercialBuildingType type)
        {
            return new CommercialBuilding(type);
        }

        /// <summary>
        /// Создает сервисное здание по типу
        /// </summary>
        public ServiceBuilding CreateServiceBuilding(ServiceBuildingType type)
        {
            return new ServiceBuilding(type);
        }

        /// <summary>
        /// Создает промышленное здание по типу
        /// </summary>
        public IndustrialBuilding CreateIndustrialBuilding(IndustrialBuildingType type)
        {
            return new IndustrialBuilding(type);
        }

        /// <summary>
        /// Создает жилое здание по типу
        /// </summary>
        public ResidentialBuilding CreateResidentialBuilding(ResidentialType type)
        {
            return new ResidentialBuilding(type);
        }

        /// <summary>
        /// Универсальный метод создания здания с определением категории
        /// </summary>
        public Building CreateBuildingByCategory(string category, string buildingName)
        {
            return category switch
            {
                "Коммерция" => CreateCommercialBuilding(GetCommercialTypeByName(buildingName)),
                "Социум" => CreateServiceBuilding(GetServiceTypeByName(buildingName)),
                "Производство" => CreateIndustrialBuilding(GetIndustrialTypeByName(buildingName)),
                "Транспорт" => CreateTransportBuilding(buildingName),
                _ => throw new ArgumentException($"Unknown category: {category}")
            };
        }

        /// <summary>
        /// Создает транспортное здание по имени
        /// </summary>
        private Building CreateTransportBuilding(string buildingName)
        {
            return buildingName switch
            {
                "Аэропорт" => new AirPort(_defaultResources),
                "Морской порт" => new SeaPort(_defaultResources),
                "Автобусная остановка" => new BusStop(),
                _ => throw new ArgumentException($"Unknown transport building: {buildingName}")
            };
        }

        private CommercialBuildingType GetCommercialTypeByName(string name)
        {
            return name switch
            {
                "Магазин" => CommercialBuildingType.Shop,
                "Супермаркет" => CommercialBuildingType.Supermarket,
                "Кафе" => CommercialBuildingType.Cafe,
                "Ресторан" => CommercialBuildingType.Restaurant,
                "Заправка" => CommercialBuildingType.GasStation,
                "Аптека" => CommercialBuildingType.Pharmacy,
                "Завод" => CommercialBuildingType.Factory,
                _ => CommercialBuildingType.Shop
            };
        }

        private ServiceBuildingType GetServiceTypeByName(string name)
        {
            return name switch
            {
                "Школа" => ServiceBuildingType.School,
                "Больница" => ServiceBuildingType.Hospital,
                "Университет" => ServiceBuildingType.University,
                "Парк" => ServiceBuildingType.Park,
                "Полицейский участок" => ServiceBuildingType.PoliceStation,
                _ => ServiceBuildingType.School
            };
        }

        private IndustrialBuildingType GetIndustrialTypeByName(string name)
        {
            return name switch
            {
                "Завод" => IndustrialBuildingType.Factory,
                "Ферма" => IndustrialBuildingType.Farm,
                "Шахта" => IndustrialBuildingType.Mine,
                "Электростанция" => IndustrialBuildingType.PowerPlant,
                _ => IndustrialBuildingType.Factory
            };
        }
    }
}