using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core.Models.Map;
using Core.Models.Mobs;

namespace Tests.UnitTests
{
    /// <summary>
    /// Тесты для деревообрабатывающей фабрики
    /// </summary>
    [TestClass]
    public sealed class WoodProcessingFactoryTests
    {
        /// <summary>
        /// Тест создания фабрики
        /// </summary>
        [TestMethod]
        public void TestWoodProcessingFactoryCreation()
        {
            var factory = new WoodProcessingFactory("Test Factory");

            Assert.AreEqual("Test Factory", factory.Name);
            Assert.AreEqual(1000, factory.MaxWoodStorage);
            Assert.IsTrue(factory.IsActive);
            Assert.AreEqual(8, factory.MaxWorkers);
        }

        /// <summary>
        /// Тест добавления древесины
        /// </summary>
        [TestMethod]
        public void TestWoodAdding()
        {
            var factory = new WoodProcessingFactory("Test Factory");

            bool added = factory.AddWood(500);
            Assert.IsTrue(added);
            Assert.AreEqual(500, factory.WoodStorage);

            bool notAdded = factory.AddWood(600);
            Assert.IsFalse(notAdded);
        }

        /// <summary>
        /// Тест добычи древесины из леса
        /// </summary>
        [TestMethod]
        public void TestWoodExtraction()
        {
            var forest = new NaturalResource { Amount = 100 };
            var factory = new WoodProcessingFactory("Test Factory", forest);
            factory.IsActive = true;

            int extracted = factory.ExtractWood();

            Assert.IsTrue(extracted > 0);
            Assert.AreEqual(extracted, factory.WoodStorage);
        }

        /// <summary>
        /// Тест добавления рабочих
        /// </summary>
        [TestMethod]
        public void TestWorkerManagement()
        {
            var factory = new WoodProcessingFactory("Test Factory");
            var citizen = new Citizen(0, 0, null) { Age = 25, IsEmployed = false };

            bool added = factory.AddWorker(citizen);

            Assert.IsTrue(added);
            Assert.AreEqual(1, factory.WorkersCount);
            Assert.IsTrue(citizen.IsEmployed);
            Assert.IsTrue(factory.IsWorker(citizen));
        }

        /// <summary>
        /// Тест производственного цикла
        /// </summary>
        [TestMethod]
        public void TestProductionCycle()
        {
            var factory = new WoodProcessingFactory("Test Factory");
            factory.IsActive = true;
            factory.AddWood(100);

            int initialWood = factory.WoodStorage;
            factory.ProcessWorkshops();

            Assert.IsTrue(factory.WoodStorage <= initialWood);
        }

        /// <summary>
        /// Тест полного цикла фабрики
        /// </summary>
        [TestMethod]
        public void TestFullProductionCycle()
        {
            var forest = new NaturalResource { Amount = 200 };
            var factory = new WoodProcessingFactory("Test Factory", forest);
            factory.IsActive = true;

            factory.FullProductionCycle();

            Assert.IsTrue(factory.WoodStorage >= 0);
        }

        /// <summary>
        /// Тест проверки возможности добычи
        /// </summary>
        [TestMethod]
        public void TestCanExtractWood()
        {
            var forest = new NaturalResource { Amount = 100 };
            var factory = new WoodProcessingFactory("Test Factory", forest);
            factory.IsActive = true;

            bool canExtract = factory.CanExtractWood();
            Assert.IsFalse(canExtract); // Нет рабочих

            var citizen = new Citizen(0, 0, null) { Age = 25, IsEmployed = false };
            factory.AddWorker(citizen);

            canExtract = factory.CanExtractWood();
            Assert.IsTrue(canExtract); // Есть рабочие и место
        }

        /// <summary>
        /// Тест получения списка рабочих
        /// </summary>
        [TestMethod]
        public void TestGetWorkers()
        {
            var factory = new WoodProcessingFactory("Test Factory");
            var citizen1 = new Citizen(0, 0, null) { Age = 25, IsEmployed = false };
            var citizen2 = new Citizen(1, 1, null) { Age = 30, IsEmployed = false };

            factory.AddWorker(citizen1);
            factory.AddWorker(citizen2);

            var workers = factory.GetWorkers();

            Assert.AreEqual(2, workers.Count);
            Assert.IsTrue(workers.Contains(citizen1));
            Assert.IsTrue(workers.Contains(citizen2));
        }
    }
}