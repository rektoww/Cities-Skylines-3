using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            Assert.AreEqual(500, factory.MaxProcessedWoodStorage);
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
        /// Тест обработки древесины
        /// </summary>
        [TestMethod]
        public void TestWoodProcessing()
        {
            var factory = new WoodProcessingFactory("Test Factory");
            factory.IsActive = true;
            factory.AddWood(100);

            factory.ProcessWood();

            Assert.IsTrue(factory.WoodStorage < 100);
            Assert.IsTrue(factory.ProcessedWoodStorage > 0);
        }

        /// <summary>
        /// Тест производства мебели
        /// </summary>
        [TestMethod]
        public void TestFurnitureProduction()
        {
            var factory = new WoodProcessingFactory("Test Factory");
            factory.IsActive = true;
            factory.AddWood(100);
            factory.ProcessWood();

            int initialFurniture = factory.FurnitureStorage;
            factory.ProduceFurniture();

            Assert.IsTrue(factory.FurnitureStorage > initialFurniture);
        }

        /// <summary>
        /// Тест получения статуса
        /// </summary>
        [TestMethod]
        public void TestStatusReporting()
        {
            var factory = new WoodProcessingFactory("Test Factory");
            factory.AddWood(50);

            var status = factory.GetStatus();

            Assert.AreEqual(50, status.WoodAmount);
            Assert.IsTrue(status.CanProcessWood);
        }
    }
}