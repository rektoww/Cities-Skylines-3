using Core.Enums;
using Core.Models.Base;
using Core.Models.Mobs;

namespace Core.Models.Buildings
{
    public class ServiceBuilding : Building
    {
        public ServiceBuildingType Type { get; set; }
        public int Capacity { get; set; }
        public List<Citizen> Clients { get; set; }
        public int EmployeesCount { get; set; }
        public decimal ServiceCost { get; set; }
        public bool CanAcceptClient => Clients.Count < Capacity;

        /// Работоспособно ли здание (все коммуникации подключены)
        public bool IsOperational => HasElectricity && HasWater && HasGas && HasSewage;

        public ServiceBuilding(ServiceBuildingType type, int capacity = 0)
        {
            Type = type;
            Capacity = capacity == 0 ? GetDefaultCapacity(type) : capacity;
            Clients = new List<Citizen>();
            SetDefaultValuesByType(type);
        }

        private void SetDefaultValuesByType(ServiceBuildingType type)
        {
            switch (type)
            {
                case ServiceBuildingType.School:
                    EmployeesCount = 20;
                    ServiceCost = 0;
                    Width = 3; Height = 2; Floors = 3;
                    break;
                    
                case ServiceBuildingType.Hospital:
                    EmployeesCount = 50;
                    ServiceCost = 1000m;
                    Width = 4; Height = 3; Floors = 5;
                    break;
                    
                case ServiceBuildingType.University:
                    EmployeesCount = 100;
                    ServiceCost = 50000m;
                    Width = 5; Height = 4; Floors = 8;
                    break;
            }
        }

        private int GetDefaultCapacity(ServiceBuildingType type)
        {
            return type switch
            {
                ServiceBuildingType.School => 500,
                ServiceBuildingType.Hospital => 200,
                ServiceBuildingType.University => 2000,
                _ => 100
            };
        }

        public override void OnBuildingPlaced()
        {
        }

        public bool TryAddClient(Citizen citizen)
        {
            if (CanAcceptClient)
            {
                Clients.Add(citizen);
                return true;
            }
            return false;
        }

        public void ProvideService()
        {
            foreach (var client in Clients.ToList())
            {
                switch (Type)
                {
                    case ServiceBuildingType.School:
                        client.Education = EducationLevel.School;
                        break;
                    case ServiceBuildingType.University:
                        client.Education = EducationLevel.University;
                        break;
                    case ServiceBuildingType.Hospital:
                        client.Health = Math.Min(100, client.Health + 20);
                        break;
                }
                
                Clients.Remove(client);
            }
        }
    }
}