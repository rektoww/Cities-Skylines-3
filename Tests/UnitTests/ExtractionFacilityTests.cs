using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core.Models.Map;
using Core.Models.Mobs;
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
            var facility = new ExtractionFacility("Test Mine", ResourceType.Iron, 1000);

            Assert.AreEqual("Test Mine", facility.Name);
            Assert.AreEqual(ResourceType.Iron, facility.ResourceType);
            Assert.AreEqual(1000, facility.StorageCapacity);
            Assert.AreEqual(10, facility.ExtractionRate);
            Assert.AreEqual(5, facility.MaxWorkers);
            Assert.IsTrue(facility.IsActive);
        }

        /// <summary>
        /// Тест управления рабочими
        /// </summary>
        [TestMethod]
        public void TestWorkerManagement()
        {
            var facility = new ExtractionFacility("Test Mine", ResourceType.Iron, 1000);
            var citizen = new Citizen(0, 0, null) { Age = 25, IsEmployed = false };

            bool added = facility.AddWorker(citizen);
            Assert.IsTrue(added);
            Assert.AreEqual(1, facility.WorkersCount);
            Assert.IsTrue(citizen.IsEmployed);
            Assert.IsTrue(facility.IsWorker(citizen));

            bool removed = facility.RemoveWorker(citizen);
            Assert.IsTrue(removed);
            Assert.AreEqual(0, facility.WorkersCount);
            Assert.IsFalse(citizen.IsEmployed);
        }

        /// <summary>
        /// Тест добычи ресурсов
        /// </summary>
        [TestMethod]
        public void TestResourceExtraction()
        {
            var resource = new NaturalResource { Type = ResourceType.Iron, Amount = 100 };
            var facility = new ExtractionFacility("Test Mine", ResourceType.Iron, 1000, resource);
            facility.IsActive = true;

            var citizen = new Citizen(0, 0, null) { Age = 25, IsEmployed = false };
            facility.AddWorker(citizen);

            int extracted = facility.ExtractResources();

            Assert.IsTrue(extracted > 0);
            Assert.AreEqual(extracted, facility.CurrentStorage);
            Assert.IsTrue(resource.Amount < 100);
        }

        /// <summary>
        /// Тест сбора ресурсов
        /// </summary>
        [TestMethod]
        public void TestResourceCollection()
        {
            var resource = new NaturalResource { Type = ResourceType.Iron, Amount = 100 };
            var facility = new ExtractionFacility("Test Mine", ResourceType.Iron, 1000, resource);
            facility.IsActive = true;

            var citizen = new Citizen(0, 0, null) { Age = 25, IsEmployed = false };
            facility.AddWorker(citizen);

            facility.ExtractResources();

            int collected = facility.CollectResources();

            Assert.IsTrue(collected > 0);
            Assert.AreEqual(0, facility.CurrentStorage);
        }

        /// <summary>
        /// Тест проверки возможности добычи
        /// </summary>
        [TestMethod]
        public void TestCanExtract()
        {
            var resource = new NaturalResource { Type = ResourceType.Iron, Amount = 100 };
            var facility = new ExtractionFacility("Test Mine", ResourceType.Iron, 1000, resource);
            facility.IsActive = true;

            bool canExtract = facility.CanExtract();
            Assert.IsFalse(canExtract); // Нет рабочих

            var citizen = new Citizen(0, 0, null) { Age = 25, IsEmployed = false };
            facility.AddWorker(citizen);

            canExtract = facility.CanExtract();
            Assert.IsTrue(canExtract); // Есть рабочие и ресурсы
        }

        /// <summary>
        /// Тест пересчета скорости добычи
        /// </summary>
        [TestMethod]
        public void TestExtractionRateRecalculation()
        {
            var facility = new ExtractionFacility("Test Mine", ResourceType.Iron, 1000);
            int initialRate = facility.ExtractionRate;

            var citizen1 = new Citizen(0, 0, null) { Age = 25, IsEmployed = false };
            var citizen2 = new Citizen(1, 1, null) { Age = 30, IsEmployed = false };

            facility.AddWorker(citizen1);
            Assert.AreEqual(12, facility.ExtractionRate); // 10 + 2*1

            facility.AddWorker(citizen2);
            Assert.AreEqual(14, facility.ExtractionRate); // 10 + 2*2
        }

        /// <summary>
        /// Тест получения списка рабочих
        /// </summary>
        [TestMethod]
        public void TestGetWorkers()
        {
            var facility = new ExtractionFacility("Test Mine", ResourceType.Iron, 1000);
            var citizen1 = new Citizen(0, 0, null) { Age = 25, IsEmployed = false };
            var citizen2 = new Citizen(1, 1, null) { Age = 30, IsEmployed = false };

            facility.AddWorker(citizen1);
            facility.AddWorker(citizen2);

            var workers = facility.GetWorkers();

            Assert.AreEqual(2, workers.Count);
            Assert.IsTrue(workers.Contains(citizen1));
            Assert.IsTrue(workers.Contains(citizen2));
        }
    }
}