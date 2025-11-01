using System.Collections.Generic;
using Core.Enums;
using Core.Models.Base;
using Core.Models.Buildings;

namespace Core.Services
{
    /// <summary>
    /// Сервис управления коммунальными услугами (ЖКХ)
    /// Отвечает за подключение зданий к сетям и управление сетями
    /// </summary>
    public class UtilityService
    {
        /// <summary>
        /// Тип коммунальной услуги (электричество, вода, газ, канализация)
        /// </summary>
        public UtilityType ServiceType { get; set; }

        /// <summary>
        /// Список зданий, подключенных к этой сети
        /// </summary>
        public List<Building> ConnectedBuildings { get; set; } = new List<Building>();

        /// <summary>
        /// Работает ли сеть (true - работает, false - авария/отключение)
        /// </summary>
        public bool IsNetworkOperational { get; set; } = true;

        /// <summary>
        /// Подключить здание к сети
        /// </summary>
        /// <param name="building">Здание для подключения</param>
        public void ConnectBuilding(Building building)
        {
            if (!ConnectedBuildings.Contains(building))
            {
                ConnectedBuildings.Add(building);
                UpdateBuildingConnection(building, true);
            }
        }

        /// <summary>
        /// Отключить здание от сети
        /// </summary>
        /// <param name="building">Здание для отключения</param>
        public void DisconnectBuilding(Building building)
        {
            if (ConnectedBuildings.Contains(building))
            {
                ConnectedBuildings.Remove(building);
                UpdateBuildingConnection(building, false);
            }
        }

        /// <summary>
        /// Обновить состояние подключения здания
        /// </summary>
        /// <param name="building">Здание</param>
        /// <param name="isConnected">Подключено ли</param>
        private void UpdateBuildingConnection(Building building, bool isConnected)
        {
            switch (ServiceType)
            {
                case UtilityType.Electricity:
                    building.HasElectricity = isConnected && IsNetworkOperational;
                    break;
                case UtilityType.Water:
                    building.HasWater = isConnected && IsNetworkOperational;
                    break;
                case UtilityType.Gas:
                    building.HasGas = isConnected && IsNetworkOperational;
                    break;
                case UtilityType.Sewage:
                    building.HasSewage = isConnected && IsNetworkOperational;
                    break;
            }
        }

        /// <summary>
        /// Включить или выключить всю сеть
        /// </summary>
        /// <param name="isOperational">Состояние сети</param>
        public void SetNetworkState(bool isOperational)
        {
            IsNetworkOperational = isOperational;
            foreach (var building in ConnectedBuildings)
            {
                UpdateBuildingConnection(building, isOperational);
            }
        }

        /// <summary>
        /// Получить количество подключенных зданий
        /// </summary>
        /// <returns>Количество подключенных зданий</returns>
        public int GetConnectedBuildingsCount()
        {
            return ConnectedBuildings.Count;
        }
    }
}