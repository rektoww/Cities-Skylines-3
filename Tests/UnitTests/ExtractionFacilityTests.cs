using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

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
            var facility = new ExtractionFacility("Test Mine", ResourceType.Coal, 1000);

            Assert.AreEqual("Test Mine", facility.Name);
            Assert.AreEqual(ResourceType.Coal, facility.ResourceType);
            Assert.AreEqual(1000, facility.StorageCapacity);
        }

        /// <summary>
        /// Тест управления рабочими
        /// </summary>
        [TestMethod]
        public void TestWorkerManagement()
        {
            var facility = new ExtractionFacility("Test Mine", ResourceType.Coal, 1000);
            var worker = new Worker { Name = "John", SkillLevel = 3 };

            bool added = facility.AddWorker(worker);
            Assert.IsTrue(added);
            Assert.AreEqual(1, facility.WorkersCount);

            bool removed = facility.RemoveWorker(worker);
            Assert.IsTrue(removed);
            Assert.AreEqual(0, facility.WorkersCount);
        }

        /// <summary>
        /// Тест добычи ресурсов
        /// </summary>
        [TestMethod]
        public void TestResourceExtraction()
        {
            var facility = new ExtractionFacility("Test Mine", ResourceType.Coal, 1000);
            facility.IsActive = true;
            facility.AddWorker(new Worker { Name = "Worker" });

            int extracted = facility.ExtractResources();

            Assert.IsTrue(extracted > 0);
            Assert.AreEqual(extracted, facility.CurrentStorage);
        }

        /// <summary>
        /// Тест сбора ресурсов
        /// </summary>
        [TestMethod]
        public void TestResourceCollection()
        {
            var facility = new ExtractionFacility("Test Mine", ResourceType.Coal, 1000);
            facility.IsActive = true;
            facility.AddWorker(new Worker { Name = "Worker" });
            facility.ExtractResources();

            int collected = facility.CollectResources();

            Assert.IsTrue(collected > 0);
            Assert.AreEqual(0, facility.CurrentStorage);
        }
    }
}