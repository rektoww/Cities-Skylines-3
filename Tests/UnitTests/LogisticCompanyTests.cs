using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core.Enums;
using System.Linq;
using Core.Models.Buildings.CommercialBuildings;

namespace Tests.UnitTests
{
    /// <summary>
    /// Тесты для логистической компании
    /// </summary>
    [TestClass]
    public sealed class LogisticsCompanyTest
    {
        /// <summary>
        /// Тест создания логистической компании
        /// </summary>
        [TestMethod]
        public void TestLogisticsCompanyCreation()
        {
            var company = new LogisticsCompany(LogisticsType.TruckingCompany);

            // Проверка статических свойств строительства
            Assert.AreEqual(280000m, LogisticsCompany.BuildCost);
            Assert.AreEqual(4, LogisticsCompany.RequiredMaterials.Count);
            Assert.AreEqual(10, LogisticsCompany.RequiredMaterials[ConstructionMaterial.Steel]);
            Assert.AreEqual(8, LogisticsCompany.RequiredMaterials[ConstructionMaterial.Concrete]);
            Assert.AreEqual(4, LogisticsCompany.RequiredMaterials[ConstructionMaterial.Glass]);
            Assert.AreEqual(6, LogisticsCompany.RequiredMaterials[ConstructionMaterial.Plastic]);

            // Проверка базовых свойств
            Assert.AreEqual(LogisticsType.TruckingCompany, company.CompanyType);
            Assert.AreEqual(10, company.MaxConcurrentOrders);
            Assert.AreEqual(0, company.ActiveOrders.Count);
            Assert.AreEqual(0, company.CompletedOrders.Count);
            Assert.IsTrue(company.SupportedCargoTypes.Count > 0);
        }

        /// <summary>
        /// Тест инициализации автопарка
        /// </summary>
        [TestMethod]
        public void TestVehicleFleetInitialization()
        {
            var company = new LogisticsCompany(LogisticsType.TruckingCompany);

            var vehicles = company.GetAvailableVehicles();

            Assert.IsTrue(vehicles.ContainsKey(LogisticsVehicle.Truck));
            Assert.IsTrue(vehicles.ContainsKey(LogisticsVehicle.RefrigeratedTruck));
            Assert.AreEqual(5, vehicles[LogisticsVehicle.Truck]);
            Assert.AreEqual(2, vehicles[LogisticsVehicle.RefrigeratedTruck]);
        }

        /// <summary>
        /// Тест создания заказа на доставку
        /// </summary>
        [TestMethod]
        public void TestCreateDeliveryOrder()
        {
            var company = new LogisticsCompany(LogisticsType.TruckingCompany);

            // Успешное создание заказа
            bool created = company.CreateDeliveryOrder(
                "Завод А", "Магазин Б",
                CargoType.GeneralCargo, 50, 5000m, 24);

            Assert.IsTrue(created);
            Assert.AreEqual(1, company.ActiveOrders.Count);
            Assert.AreEqual("Завод А", company.ActiveOrders[0].FromLocation);
            Assert.AreEqual("Магазин Б", company.ActiveOrders[0].ToLocation);
            Assert.AreEqual(CargoType.GeneralCargo, company.ActiveOrders[0].CargoType);
            Assert.AreEqual(50, company.ActiveOrders[0].CargoWeight);
            Assert.AreEqual(5000m, company.ActiveOrders[0].OfferedPrice);
            Assert.AreEqual(DeliveryStatus.Pending, company.ActiveOrders[0].Status);
        }

        /// <summary>
        /// Тест создания заказа с неподдерживаемым типом груза
        /// </summary>
        [TestMethod]
        public void TestCreateDeliveryOrderWithUnsupportedCargo()
        {
            var company = new LogisticsCompany(LogisticsType.TruckingCompany);

            // Попытка создать заказ с опасными грузами (не поддерживается автоперевозками)
            bool created = company.CreateDeliveryOrder(
                "Завод А", "Магазин Б",
                CargoType.DangerousGoods, 50, 5000m, 24);

            Assert.IsFalse(created);
            Assert.AreEqual(0, company.ActiveOrders.Count);
        }

        /// <summary>
        /// Тест выполнения заказа доставки
        /// </summary>
        [TestMethod]
        public void TestCompleteDeliveryOrder()
        {
            var company = new LogisticsCompany(LogisticsType.TruckingCompany);

            // Создаем заказ
            company.CreateDeliveryOrder("Завод А", "Магазин Б",
                CargoType.GeneralCargo, 50, 5000m, 24);

            var orderId = company.ActiveOrders[0].Id;

            // Выполняем заказ
            bool completed = company.CompleteDeliveryOrder(orderId);

            Assert.IsTrue(completed);
            Assert.AreEqual(0, company.ActiveOrders.Count);
            Assert.AreEqual(1, company.CompletedOrders.Count);
            Assert.AreEqual(DeliveryStatus.Completed, company.CompletedOrders[0].Status);
            Assert.IsNotNull(company.CompletedOrders[0].CompletionTime);
        }

        /// <summary>
        /// Тест выполнения несуществующего заказа
        /// </summary>
        [TestMethod]
        public void TestCompleteNonExistentOrder()
        {
            var company = new LogisticsCompany(LogisticsType.TruckingCompany);

            // Попытка выполнить несуществующий заказ
            bool completed = company.CompleteDeliveryOrder("NON_EXISTENT_ORDER");

            Assert.IsFalse(completed);
            Assert.AreEqual(0, company.ActiveOrders.Count);
            Assert.AreEqual(0, company.CompletedOrders.Count);
        }

        /// <summary>
        /// Тест добавления транспортного средства
        /// </summary>
        [TestMethod]
        public void TestAddVehicle()
        {
            var company = new LogisticsCompany(LogisticsType.TruckingCompany);

            var initialTruckCount = company.GetAvailableVehicles()[LogisticsVehicle.Truck];

            // Добавляем грузовик
            company.AddVehicle(LogisticsVehicle.Truck, 2);

            var vehicles = company.GetAvailableVehicles();
            Assert.AreEqual(initialTruckCount + 2, vehicles[LogisticsVehicle.Truck]);
        }

        /// <summary>
        /// Тест расчета грузоподъемности
        /// </summary>
        [TestMethod]
        public void TestCapacityCalculation()
        {
            var company = new LogisticsCompany(LogisticsType.TruckingCompany);

            var stats = company.GetCompanyStats();

            Assert.IsTrue((int)stats["TotalCapacity"] > 0);
            Assert.AreEqual(0, (int)stats["CurrentLoad"]);
            Assert.IsTrue((int)stats["AvailableCapacity"] > 0);
        }

        /// <summary>
        /// Тест загрузки компании
        /// </summary>
        [TestMethod]
        public void TestCompanyLoad()
        {
            var company = new LogisticsCompany(LogisticsType.TruckingCompany);

            // Создаем несколько заказов
            company.CreateDeliveryOrder("А", "Б", CargoType.GeneralCargo, 30, 3000m, 24);
            company.CreateDeliveryOrder("В", "Г", CargoType.GeneralCargo, 20, 2000m, 24);

            var stats = company.GetCompanyStats();

            Assert.AreEqual(50, (int)stats["CurrentLoad"]); // 30 + 20
            Assert.IsTrue((int)stats["AvailableCapacity"] > 0);
        }

        /// <summary>
        /// Тест получения статистики компании
        /// </summary>
        [TestMethod]
        public void TestGetCompanyStats()
        {
            var company = new LogisticsCompany(LogisticsType.TruckingCompany);

            var stats = company.GetCompanyStats();

            Assert.IsNotNull(stats);
            Assert.AreEqual(LogisticsType.TruckingCompany, stats["CompanyType"]);
            Assert.AreEqual(0, (int)stats["ActiveOrders"]);
            Assert.AreEqual(0, (int)stats["CompletedOrders"]);
            Assert.IsTrue((int)stats["VehicleCount"] > 0);
            Assert.IsTrue((int)stats["TotalCapacity"] > 0);
            Assert.AreEqual(0m, (decimal)stats["Revenue"]);
            Assert.AreEqual(0m, (decimal)stats["Expenses"]);
            Assert.AreEqual(0m, (decimal)stats["Profit"]);
            Assert.IsTrue((int)stats["SupportedCargoTypes"] > 0);
        }

        /// <summary>
        /// Тест финансовых операций
        /// </summary>
        [TestMethod]
        public void TestFinancialOperations()
        {
            var company = new LogisticsCompany(LogisticsType.TruckingCompany);

            // Создаем и выполняем заказ
            company.CreateDeliveryOrder("А", "Б", CargoType.GeneralCargo, 50, 5000m, 24);
            var orderId = company.ActiveOrders[0].Id;
            company.CompleteDeliveryOrder(orderId);

            var stats = company.GetCompanyStats();

            Assert.AreEqual(5000m, (decimal)stats["Revenue"]);
            Assert.IsTrue((decimal)stats["Expenses"] > 0);
            Assert.IsTrue((decimal)stats["Profit"] > 0);
        }

        /// <summary>
        /// Тест размещения здания
        /// </summary>
        [TestMethod]
        public void TestOnBuildingPlaced()
        {
            var company = new LogisticsCompany(LogisticsType.TruckingCompany);

            // Проверяем, что метод не вызывает исключений
            company.OnBuildingPlaced();

            // Компания должна быть готова к работе
            Assert.IsTrue(company.GetAvailableVehicles().Count > 0);
            Assert.IsTrue(company.SupportedCargoTypes.Count > 0);
        }

        /// <summary>
        /// Тест специализации компаний
        /// </summary>
        [TestMethod]
        public void TestCompanySpecialization()
        {
            // Тестируем разные типы логистических компаний
            var truckingCompany = new LogisticsCompany(LogisticsType.TruckingCompany);
            var refrigeratedCompany = new LogisticsCompany(LogisticsType.RefrigeratedLogistics);
            var expressCompany = new LogisticsCompany(LogisticsType.ExpressDelivery);

            // Проверяем специализацию
            Assert.AreEqual(LogisticsType.TruckingCompany, truckingCompany.CompanyType);
            Assert.AreEqual(LogisticsType.RefrigeratedLogistics, refrigeratedCompany.CompanyType);
            Assert.AreEqual(LogisticsType.ExpressDelivery, expressCompany.CompanyType);

            // Проверяем поддерживаемые типы грузов
            Assert.IsTrue(truckingCompany.SupportedCargoTypes.Contains(CargoType.GeneralCargo));
            Assert.IsTrue(refrigeratedCompany.SupportedCargoTypes.Contains(CargoType.PerishableGoods));
            Assert.IsTrue(expressCompany.SupportedCargoTypes.Contains(CargoType.Documents));
        }

        /// <summary>
        /// Тест расчета коэффициента использования
        /// </summary>
        [TestMethod]
        public void TestUtilizationRate()
        {
            var company = new LogisticsCompany(LogisticsType.TruckingCompany);

            var stats = company.GetCompanyStats();
            var utilizationRate = (double)stats["UtilizationRate"];

            // При отсутствии заказов коэффициент использования должен быть 0
            Assert.AreEqual(0.0, utilizationRate);

            // Создаем заказ
            company.CreateDeliveryOrder("А", "Б", CargoType.GeneralCargo, 50, 5000m, 24);

            stats = company.GetCompanyStats();
            utilizationRate = (double)stats["UtilizationRate"];

            // Коэффициент использования должен быть больше 0
            Assert.IsTrue(utilizationRate > 0.0);
            Assert.IsTrue(utilizationRate <= 1.0);
        }
    }
}