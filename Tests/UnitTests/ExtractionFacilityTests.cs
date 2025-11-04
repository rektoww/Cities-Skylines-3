using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core.Models.Map;
using Core.Enums;

namespace Tests.UnitTests
{
    /// <summary>
    /// Тесты для добывающих предприятий
    /// </summary>
    [TestClass]
    public sealed class ExtractionFacilityTests
    {
        /// <summary>
        /// Тест создания добывающего предприятия
        /// </summary>
        [TestMethod]
        public void TestExtractionFacilityCreation()
        {
            var resource = new NaturalResource { Type = ResourceType.Iron, Amount = 100 };
            var facility = new ExtractionFacility(ResourceType.Iron, 1000, resource);

            Assert.AreEqual(ResourceType.Iron, facility.ResourceType);
            Assert.AreEqual(1000, facility.StorageCapacity);
            Assert.AreEqual(10, facility.ExtractionRate);
            Assert.AreEqual(5, facility.MaxWorkers);
            Assert.AreEqual(0, facility.WorkersCount);
            Assert.AreEqual(resource, facility.ConnectedResource);
        }

        /// <summary>
        /// Тест управления рабочими
        /// </summary>
        [TestMethod]
        public void TestWorkerManagement()
        {
            var facility = new ExtractionFacility(ResourceType.Iron, 1000);

            // Установка количества рабочих
            facility.SetWorkersCount(3);
            Assert.AreEqual(3, facility.WorkersCount);

            // Попытка установить больше рабочих, чем максимум
            facility.SetWorkersCount(10);
            Assert.AreEqual(5, facility.WorkersCount); 

            // Установка нуля рабочих
            facility.SetWorkersCount(0);
            Assert.AreEqual(0, facility.WorkersCount);
        }

        /// <summary>
        /// Тест добычи ресурсов
        /// </summary>
        [TestMethod]
        public void TestResourceExtraction()
        {
            var resource = new NaturalResource { Type = ResourceType.Iron, Amount = 100 };
            var facility = new ExtractionFacility(ResourceType.Iron, 1000, resource);

            // Добыча без рабочих
            int extracted = facility.ExtractResources();
            Assert.AreEqual(0, extracted);
            Assert.AreEqual(0, facility.CurrentStorage);
            Assert.AreEqual(100, resource.Amount);

            // Добыча с рабочими
            facility.SetWorkersCount(1);
            extracted = facility.ExtractResources();

            Assert.IsTrue(extracted > 0);
            Assert.AreEqual(extracted, facility.CurrentStorage);
            Assert.AreEqual(100 - extracted, resource.Amount);
        }

        /// <summary>
        /// Тест добычи при ограниченном хранилище
        /// </summary>
        [TestMethod]
        public void TestResourceExtractionWithLimitedStorage()
        {
            var resource = new NaturalResource { Type = ResourceType.Iron, Amount = 1000 };
            var facility = new ExtractionFacility(ResourceType.Iron, 50, resource); // Маленькое хранилище

            facility.SetWorkersCount(1);
            int extracted = facility.ExtractResources();

            // Добыча не должна превышать вместимость хранилища
            Assert.IsTrue(extracted <= 50);
            Assert.AreEqual(extracted, facility.CurrentStorage);
        }

        /// <summary>
        /// Тест добычи при истощенном ресурсе
        /// </summary>
        [TestMethod]
        public void TestResourceExtractionWithDepletedResource()
        {
            var resource = new NaturalResource { Type = ResourceType.Iron, Amount = 5 }; // Мало ресурсов
            var facility = new ExtractionFacility(ResourceType.Iron, 1000, resource);

            facility.SetWorkersCount(1);
            int extracted = facility.ExtractResources();

            // Добыча не должна превышать доступное количество ресурсов
            Assert.AreEqual(5, extracted);
            Assert.AreEqual(0, resource.Amount);
            Assert.AreEqual(5, facility.CurrentStorage);

            // Последующая добыча должна вернуть 0
            extracted = facility.ExtractResources();
            Assert.AreEqual(0, extracted);
        }

        /// <summary>
        /// Тест получения хранимых ресурсов
        /// </summary>
        [TestMethod]
        public void TestGetStoredResources()
        {
            var resource = new NaturalResource { Type = ResourceType.Iron, Amount = 100 };
            var facility = new ExtractionFacility(ResourceType.Iron, 1000, resource);

            facility.SetWorkersCount(1);
            facility.ExtractResources();

            int stored = facility.GetStoredResources();
            Assert.AreEqual(facility.CurrentStorage, stored);
        }

        /// <summary>
        /// Тест установки подключенного ресурса
        /// </summary>
        [TestMethod]
        public void TestSetConnectedResource()
        {
            var facility = new ExtractionFacility(ResourceType.Iron, 1000);
            Assert.IsNull(facility.ConnectedResource);

            var resource = new NaturalResource { Type = ResourceType.Iron, Amount = 100 };
            facility.SetConnectedResource(resource);

            Assert.AreEqual(resource, facility.ConnectedResource);
        }

        /// <summary>
        /// Тест пересчета скорости добычи
        /// </summary>
        [TestMethod]
        public void TestExtractionRateRecalculation()
        {
            var facility = new ExtractionFacility(ResourceType.Iron, 1000);

            // Начальная скорость
            Assert.AreEqual(10, facility.ExtractionRate);

            // Добавляем рабочих и проверяем скорость
            facility.SetWorkersCount(1);
            Assert.AreEqual(12, facility.ExtractionRate); // 10 + 2*1

            facility.SetWorkersCount(3);
            Assert.AreEqual(16, facility.ExtractionRate); // 10 + 2*3

            facility.SetWorkersCount(5);
            Assert.AreEqual(20, facility.ExtractionRate); // 10 + 2*5
        }

        /// <summary>
        /// Тест добычи без подключенного ресурса
        /// </summary>
        [TestMethod]
        public void TestExtractionWithoutConnectedResource()
        {
            var facility = new ExtractionFacility(ResourceType.Iron, 1000); // Без ресурса

            facility.SetWorkersCount(1);
            int extracted = facility.ExtractResources();

            Assert.AreEqual(0, extracted);
            Assert.AreEqual(0, facility.CurrentStorage);
        }
    }
}