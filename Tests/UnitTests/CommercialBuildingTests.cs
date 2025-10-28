using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Tests.UnitTests
{
    /// <summary>
    /// Тесты для коммерческих зданий
    /// </summary>
    [TestClass]
    public sealed class CommercialBuildingTests
    {
        /// <summary>
        /// Тест создания магазина
        /// </summary>
        [TestMethod]
        public void TestStoreCreation()
        {
            var store = new Store("Test Store");
            Assert.AreEqual("Test Store", store.Name);
            Assert.AreEqual(CommercialType.Store, store.Type);
            Assert.AreEqual(20, store.CustomerCapacity);
        }

        /// <summary>
        /// Тест управления клиентами магазина
        /// </summary>
        [TestMethod]
        public void TestStoreCustomerManagement()
        {
            var store = new Store("Test Store");
            store.Open();

            bool added = store.AddCustomer();
            Assert.IsTrue(added);
            Assert.AreEqual(1, store.CurrentCustomers);

            store.RemoveCustomer();
            Assert.AreEqual(0, store.CurrentCustomers);
        }

        /// <summary>
        /// Тест выручки кафе
        /// </summary>
        [TestMethod]
        public void TestCafeRevenue()
        {
            var cafe = new Cafe("Test Cafe");
            cafe.Open();

            cafe.AddCustomer();
            cafe.AddCustomer();
            decimal initialRevenue = cafe.Revenue;

            cafe.ProcessBusiness();

            Assert.IsTrue(cafe.Revenue > initialRevenue);
        }

        /// <summary>
        /// Тест заправки топлива
        /// </summary>
        [TestMethod]
        public void TestGasStationRefueling()
        {
            var station = new GasStation("Test Station");
            station.CurrentFuel = 5000;

            bool result = station.Refuel(2000);

            Assert.IsTrue(result);
            Assert.AreEqual(7000, station.CurrentFuel);
        }

        /// <summary>
        /// Тест управления товарами
        /// </summary>
        [TestMethod]
        public void TestProductManagement()
        {
            var store = new Store("Test Store");
            var product = new Product { Name = "Test Product", Price = 10.0m };

            bool added = store.AddProduct(product);
            Assert.IsTrue(added);

            bool removed = store.RemoveProduct("Test Product");
            Assert.IsTrue(removed);
        }
    }
}