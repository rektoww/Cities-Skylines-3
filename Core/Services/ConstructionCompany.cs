using Core.Enums;
using Core.Enums.Core.Enums;
using Core.Interfaces;
using Core.Models.Base;
using Core.Models.Map;
using Core.Resourses;

namespace Core.Services
{
    public class ConstructionCompany : IGameService
    {
        private readonly PlayerResources _resources;
        private readonly FinancialSystem? _financialSystem;
        private readonly BuildingFactory _buildingFactory;

        public ConstructionCompany(PlayerResources resources, FinancialSystem? financialSystem = null)
        {
            _resources = resources;
            _financialSystem = financialSystem;
            _buildingFactory = new BuildingFactory(resources);
        }

        public void Initialize()
        {
        }

        public void Update()
        {
        }

        public decimal Balance => _resources.Balance;
        public Dictionary<ConstructionMaterial, int> StoredMaterials => _resources.StoredMaterials;

        public bool TryBuild(Building building, int x, int y, GameMap map)
        {
            if (building == null) return false;
            return TryBuildInternal(building, x, y, map);
        }

        public bool TryBuildByType(BuildingType buildingType, int x, int y, GameMap map, out Building building)
        {
            building = _buildingFactory.CreateBuilding(buildingType);
            return TryBuildInternal(building, x, y, map);
        }

        public bool TryBuildCommercial(CommercialBuildingType commercialType, int x, int y, GameMap map, out Building building)
        {
            building = _buildingFactory.CreateCommercialBuilding(commercialType);
            return TryBuildInternal(building, x, y, map);
        }

        public bool TryBuildService(ServiceBuildingType serviceType, int x, int y, GameMap map, out Building building)
        {
            building = _buildingFactory.CreateServiceBuilding(serviceType);
            return TryBuildInternal(building, x, y, map);
        }

        public bool TryBuildIndustrial(IndustrialBuildingType industrialType, int x, int y, GameMap map, out Building building)
        {
            building = _buildingFactory.CreateIndustrialBuilding(industrialType);
            return TryBuildInternal(building, x, y, map);
        }

        public bool TryBuildResidential(ResidentialType residentialType, int x, int y, GameMap map, out Building building)
        {
            building = _buildingFactory.CreateResidentialBuilding(residentialType);
            return TryBuildInternal(building, x, y, map);
        }

        public bool TryBuildByCategory(string category, string buildingName, int x, int y, GameMap map, out Building building)
        {
            building = _buildingFactory.CreateBuildingByCategory(category, buildingName);
            return TryBuildInternal(building, x, y, map);
        }

        private bool TryBuildInternal(Building building, int x, int y, GameMap map)
        {
            if (!HasEnoughResources(building.RequiredMaterials, building.BuildCost))
                return false;

            if (!building.TryPlace(x, y, map))
                return false;

            DeductResources(building.RequiredMaterials, building.BuildCost);
            return true;
        }

        private bool HasEnoughResources(Dictionary<ConstructionMaterial, int> requiredMaterials, decimal buildCost)
        {
            foreach (var material in requiredMaterials)
            {
                if (!_resources.StoredMaterials.ContainsKey(material.Key) ||
                    _resources.StoredMaterials[material.Key] < material.Value)
                    return false;
            }

            var availableBudget = _financialSystem?.CityBudget ?? _resources.Balance;
            return availableBudget >= buildCost;
        }

        private void DeductResources(Dictionary<ConstructionMaterial, int> requiredMaterials, decimal buildCost)
        {
            foreach (var material in requiredMaterials)
            {
                _resources.StoredMaterials[material.Key] -= material.Value;
            }

            _resources.Balance -= buildCost;
        }

        public bool TryRemoveMaterial(ConstructionMaterial material, int amount, out int balance)
        {
            if (amount > 0 && _resources.StoredMaterials.ContainsKey(material) &&
                _resources.StoredMaterials[material] >= amount)
            {
                _resources.StoredMaterials[material] -= amount;
                balance = _resources.StoredMaterials[material];
                return true;
            }

            balance = _resources.StoredMaterials.ContainsKey(material) ? _resources.StoredMaterials[material] : -1;
            return false;
        }
    }
}