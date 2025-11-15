using Core.Enums;
using Core.Interfaces;
using Core.Models.Base;

namespace Core.Services
{
    /// <summary>
    /// Сервис управления коммунальными услугами (ЖКХ)
    /// </summary>
    public class UtilityService : IGameService
    {
        public UtilityType ServiceType { get; set; }
        public List<Building> ConnectedBuildings { get; set; } = new List<Building>();
        public bool IsNetworkOperational { get; set; } = true;

        public void Initialize()
        {
        }

        public void Update()
        {
        }

        public void ConnectBuilding(Building building)
        {
            if (!ConnectedBuildings.Contains(building))
            {
                ConnectedBuildings.Add(building);
                UpdateBuildingConnection(building, true);
            }
        }

        public void DisconnectBuilding(Building building)
        {
            if (ConnectedBuildings.Contains(building))
            {
                ConnectedBuildings.Remove(building);
                UpdateBuildingConnection(building, false);
            }
        }

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

        public void SetNetworkState(bool isOperational)
        {
            IsNetworkOperational = isOperational;
            foreach (var building in ConnectedBuildings)
            {
                UpdateBuildingConnection(building, isOperational);
            }
        }

        public int GetConnectedBuildingsCount()
        {
            return ConnectedBuildings.Count;
        }
    }
}