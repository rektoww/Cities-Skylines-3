using Core.Enums;
using Core.Interfaces;
using Core.Models.Base;

namespace Core.Services
{
    /// <summary>
    /// Менеджер для управления всеми коммунальными услугами
    /// </summary>
    public class UtilityManager : IGameService
    {
        public Dictionary<UtilityType, UtilityService> UtilityServices { get; private set; }

        public UtilityManager()
        {
            UtilityServices = new Dictionary<UtilityType, UtilityService>
            {
                { UtilityType.Electricity, new UtilityService { ServiceType = UtilityType.Electricity } },
                { UtilityType.Water, new UtilityService { ServiceType = UtilityType.Water } },
                { UtilityType.Gas, new UtilityService { ServiceType = UtilityType.Gas } },
                { UtilityType.Sewage, new UtilityService { ServiceType = UtilityType.Sewage } }
            };
        }

        public void Initialize()
        {
            foreach (var svc in UtilityServices.Values)
            {
                svc.Initialize();
            }
        }

        public void Update()
        {
            foreach (var svc in UtilityServices.Values)
            {
                svc.Update();
            }
        }

        public void ConnectBuildingToAllUtilities(Building building)
        {
            foreach (var service in UtilityServices.Values)
            {
                service.ConnectBuilding(building);
            }
        }

        public void DisconnectBuildingFromAllUtilities(Building building)
        {
            foreach (var service in UtilityServices.Values)
            {
                service.DisconnectBuilding(building);
            }
        }

        public void GetUtilityStatistics()
        {
            foreach (var service in UtilityServices)
            {
                var count = service.Value.GetConnectedBuildingsCount();
            }
        }
    }
}