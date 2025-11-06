using Core.Enums;
using Core.Interfaces;
using Core.Models.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Models.Buildings.CommercialBuildings
{
    /// <summary>
    /// Логистическая компания - осуществляет доставку грузов между предприятиями
    /// </summary>
    public class LogisticsCompany : CommercialBuilding, IConstructable<LogisticsCompany>
    {
        #region Static Properties - Construction Cost

        public static decimal BuildCost { get; protected set; } = 280000m;

        public static Dictionary<ConstructionMaterial, int> RequiredMaterials { get; protected set; }
            = new Dictionary<ConstructionMaterial, int>
            {
                { ConstructionMaterial.Steel, 10 },
                { ConstructionMaterial.Concrete, 8 },
                { ConstructionMaterial.Glass, 4 },
                { ConstructionMaterial.Plastic, 6 }
            };

        #endregion

        #region Instance Properties

        /// <summary>
        /// Специализация логистической компании
        /// </summary>
        public LogisticsType CompanyType { get; private set; }

        /// <summary>
        /// Парк транспортных средств
        /// </summary>
        public Dictionary<LogisticsVehicle, int> VehicleFleet { get; private set; } = new Dictionary<LogisticsVehicle, int>();

        /// <summary>
        /// Текущие заказы на доставку
        /// </summary>
        public List<DeliveryOrder> ActiveOrders { get; private set; } = new List<DeliveryOrder>();

        /// <summary>
        /// История выполненных заказов
        /// </summary>
        public List<DeliveryOrder> CompletedOrders { get; private set; } = new List<DeliveryOrder>();

        /// <summary>
        /// Доступные типы грузов для перевозки
        /// </summary>
        public List<CargoType> SupportedCargoTypes { get; private set; } = new List<CargoType>();

        /// <summary>
        /// Максимальное количество одновременных заказов
        /// </summary>
        public int MaxConcurrentOrders { get; private set; } = 10;

        /// <summary>
        /// Общая грузоподъемность компании
        /// </summary>
        public int TotalCapacity { get; private set; }

        /// <summary>
        /// Текущая загрузка компании
        /// </summary>
        public int CurrentLoad => ActiveOrders.Sum(order => order.CargoWeight);

        /// <summary>
        /// Доступная грузоподъемность
        /// </summary>
        public int AvailableCapacity => TotalCapacity - CurrentLoad;

        /// <summary>
        /// Доход компании
        /// </summary>
        public decimal Revenue { get; private set; }

        /// <summary>
        /// Расходы компании
        /// </summary>
        public decimal Expenses { get; private set; }

        /// <summary>
        /// Прибыль компании
        /// </summary>
        public decimal Profit => Revenue - Expenses;

        #endregion

        #region Constructor

        public LogisticsCompany(LogisticsType companyType) : base(CommercialBuildingType.Factory)
        {
            CompanyType = companyType;
            InitializeCompany();
        }

        #endregion

        #region Initialization

        private void InitializeCompany()
        {
            InitializeVehicleFleet();
            InitializeSupportedCargo();
            CalculateTotalCapacity();
        }

        private void InitializeVehicleFleet()
        {
            // Базовая комплектация в зависимости от типа компании
            switch (CompanyType)
            {
                case LogisticsType.TruckingCompany:
                    VehicleFleet[LogisticsVehicle.Truck] = 5;
                    VehicleFleet[LogisticsVehicle.RefrigeratedTruck] = 2;
                    break;

                case LogisticsType.RefrigeratedLogistics:
                    VehicleFleet[LogisticsVehicle.RefrigeratedTruck] = 6;
                    break;

                case LogisticsType.ExpressDelivery:
                    VehicleFleet[LogisticsVehicle.Truck] = 8;
                    break;

                case LogisticsType.BulkCargo:
                    VehicleFleet[LogisticsVehicle.DumpTruck] = 4;
                    VehicleFleet[LogisticsVehicle.TankerTruck] = 2;
                    break;

                default:
                    VehicleFleet[LogisticsVehicle.Truck] = 4;
                    break;
            }
        }

        private void InitializeSupportedCargo()
        {
            // Определяем поддерживаемые типы грузов по специализации
            switch (CompanyType)
            {
                case LogisticsType.RefrigeratedLogistics:
                    SupportedCargoTypes.Add(CargoType.PerishableGoods);
                    SupportedCargoTypes.Add(CargoType.RefrigeratedGoods);
                    break;

                case LogisticsType.BulkCargo:
                    SupportedCargoTypes.Add(CargoType.BulkMaterials);
                    SupportedCargoTypes.Add(CargoType.LiquidCargo);
                    break;

                case LogisticsType.ExpressDelivery:
                    SupportedCargoTypes.Add(CargoType.Documents);
                    SupportedCargoTypes.Add(CargoType.HighValueGoods);
                    break;

                default:
                    SupportedCargoTypes.Add(CargoType.GeneralCargo);
                    SupportedCargoTypes.Add(CargoType.FragileGoods);
                    break;
            }
        }

        private void CalculateTotalCapacity()
        {
            TotalCapacity = VehicleFleet.Sum(vehicle => GetVehicleCapacity(vehicle.Key) * vehicle.Value);
        }

        private int GetVehicleCapacity(LogisticsVehicle vehicle)
        {
            return vehicle switch
            {
                LogisticsVehicle.Truck => 10,
                LogisticsVehicle.RefrigeratedTruck => 8,
                LogisticsVehicle.TankerTruck => 15,
                LogisticsVehicle.DumpTruck => 12,
                LogisticsVehicle.ContainerTruck => 20,
                LogisticsVehicle.CargoPlane => 50,
                LogisticsVehicle.CargoShip => 200,
                _ => 5
            };
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Создать новый заказ на доставку
        /// </summary>
        public bool CreateDeliveryOrder(string fromLocation, string toLocation, CargoType cargoType,
            int weight, decimal offeredPrice, int deliveryTime)
        {
            if (ActiveOrders.Count >= MaxConcurrentOrders)
                return false;

            if (!SupportedCargoTypes.Contains(cargoType))
                return false;

            if (weight > AvailableCapacity)
                return false;

            var order = new DeliveryOrder
            {
                Id = GenerateOrderId(),
                FromLocation = fromLocation,
                ToLocation = toLocation,
                CargoType = cargoType,
                CargoWeight = weight,
                OfferedPrice = offeredPrice,
                RequiredDeliveryTime = deliveryTime,
                Status = DeliveryStatus.Pending,
                CreationTime = DateTime.Now
            };

            ActiveOrders.Add(order);
            return true;
        }

        /// <summary>
        /// Выполнить заказ доставки
        /// </summary>
        public bool CompleteDeliveryOrder(string orderId)
        {
            var order = ActiveOrders.FirstOrDefault(o => o.Id == orderId);
            if (order == null) return false;

            order.Status = DeliveryStatus.Completed;
            order.CompletionTime = DateTime.Now;

            // Расчет доходов и расходов
            Revenue += order.OfferedPrice;
            Expenses += CalculateDeliveryCosts(order);

            ActiveOrders.Remove(order);
            CompletedOrders.Add(order);

            return true;
        }

        /// <summary>
        /// Добавить транспортное средство в парк
        /// </summary>
        public void AddVehicle(LogisticsVehicle vehicle, int count = 1)
        {
            if (VehicleFleet.ContainsKey(vehicle))
                VehicleFleet[vehicle] += count;
            else
                VehicleFleet[vehicle] = count;

            CalculateTotalCapacity();
        }

        /// <summary>
        /// Получить статистику компании
        /// </summary>
        public Dictionary<string, object> GetCompanyStats()
        {
            return new Dictionary<string, object>
            {
                { "CompanyType", CompanyType },
                { "ActiveOrders", ActiveOrders.Count },
                { "CompletedOrders", CompletedOrders.Count },
                { "VehicleCount", VehicleFleet.Values.Sum() },
                { "TotalCapacity", TotalCapacity },
                { "CurrentLoad", CurrentLoad },
                { "AvailableCapacity", AvailableCapacity },
                { "Revenue", Revenue },
                { "Expenses", Expenses },
                { "Profit", Profit },
                { "SupportedCargoTypes", SupportedCargoTypes.Count },
                { "UtilizationRate", TotalCapacity > 0 ? (double)CurrentLoad / TotalCapacity : 0 }
            };
        }

        /// <summary>
        /// Получить список доступных транспортных средств
        /// </summary>
        public Dictionary<LogisticsVehicle, int> GetAvailableVehicles()
        {
            return new Dictionary<LogisticsVehicle, int>(VehicleFleet);
        }

        #endregion

        #region Private Methods

        private string GenerateOrderId()
        {
            return $"ORD_{DateTime.Now:yyyyMMddHHmmss}_{new Random().Next(1000, 9999)}";
        }

        private decimal CalculateDeliveryCosts(DeliveryOrder order)
        {
            // Базовая формула расчета затрат
            decimal baseCost = order.CargoWeight * 0.5m; // 0.5 за единицу веса
            decimal distanceCost = CalculateDistanceCost(order.FromLocation, order.ToLocation);
            decimal specialHandlingCost = GetSpecialHandlingCost(order.CargoType);

            return baseCost + distanceCost + specialHandlingCost;
        }

        private decimal CalculateDistanceCost(string from, string to)
        {
            // Упрощенный расчет стоимости по расстоянию
            // В реальной реализации здесь была бы логика расчета расстояния
            return 100m; // Базовая стоимость
        }

        private decimal GetSpecialHandlingCost(CargoType cargoType)
        {
            return cargoType switch
            {
                CargoType.DangerousGoods => 200m,
                CargoType.PerishableGoods => 150m,
                CargoType.FragileGoods => 100m,
                CargoType.LiveAnimals => 180m,
                _ => 0m
            };
        }

        #endregion

        public override void OnBuildingPlaced()
        {
            // Инициализация логистической компании при размещении
            Console.WriteLine($"Логистическая компания {CompanyType} размещена и готова к работе!");
        }
    }

    /// <summary>
    /// Класс заказа на доставку
    /// </summary>
    public class DeliveryOrder
    {
        public string Id { get; set; }
        public string FromLocation { get; set; }
        public string ToLocation { get; set; }
        public CargoType CargoType { get; set; }
        public int CargoWeight { get; set; }
        public decimal OfferedPrice { get; set; }
        public int RequiredDeliveryTime { get; set; } // в часах
        public DeliveryStatus Status { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? CompletionTime { get; set; }
    }

    /// <summary>
    /// Статус доставки
    /// </summary>
    public enum DeliveryStatus
    {
        Pending,        // Ожидает выполнения
        InProgress,     // В процессе доставки
        Completed,      // Завершена
        Cancelled,      // Отменена
        Delayed         // Задержана
    }
}