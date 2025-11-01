using System.Collections.Generic;
using Core.Enums;
using Core.Models.Base;
using Core.Models.Buildings;

namespace Core.Services
{
    /// <summary>
    /// Менеджер для управления всеми коммунальными услугами
    /// </summary>
    public class UtilityManager
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

        /// <summary>
        /// Подключить здание ко всем коммунальным услугам
        /// </summary>
        public void ConnectBuildingToAllUtilities(Building building)
        {
            foreach (var service in UtilityServices.Values)
            {
                service.ConnectBuilding(building);
            }
        }

        /// <summary>
        /// Отключить здание от всех коммунальных услуг
        /// </summary>
        public void DisconnectBuildingFromAllUtilities(Building building)
        {
            foreach (var service in UtilityServices.Values)
            {
                service.DisconnectBuilding(building);
            }
        }

        /// <summary>
        /// Получить статистику по подключенным зданиям
        /// </summary>
        public void GetUtilityStatistics()
        {
            foreach (var service in UtilityServices)
            {
                var count = service.Value.GetConnectedBuildingsCount();
            }
        }
    }
}