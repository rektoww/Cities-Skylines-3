using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core.Models.Map;

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
            var factory = new WoodProcessingFactory();

            Assert.AreEqual(1000, factory.MaxWoodStorage);
            Assert.AreEqual(0, factory.WoodStorage);
            Assert.AreEqual(8, factory.MaxWorkers);
            Assert.AreEqual(0, factory.WorkersCount);
            Assert.IsNull(factory.ConnectedForest);
            Assert.AreEqual(3, factory.Workshops.Count);
        }

        /// <summary>
        /// Тест создания фабрики с подключенным лесом
        /// </summary>
        [TestMethod]
        public void TestWoodProcessingFactoryCreationWithForest()
        {
            var forest = new NaturalResource { Amount = 500 };
            var factory = new WoodProcessingFactory(forest);

            Assert.AreEqual(forest, factory.ConnectedForest);
            Assert.AreEqual(1000, factory.MaxWoodStorage);
        }

        /// <summary>
        /// Тест добавления древесины
        /// </summary>
        [TestMethod]
        public void TestWoodAdding()
        {
            var factory = new WoodProcessingFactory();

            // Успешное добавление
            bool added = factory.AddWood(500);
            Assert.IsTrue(added);
            Assert.AreEqual(500, factory.WoodStorage);

            // Попытка добавить больше, чем вмещает хранилище
            bool notAdded = factory.AddWood(600);
            Assert.IsFalse(notAdded);
            Assert.AreEqual(500, factory.WoodStorage); // Количество не изменилось

            // Добавление до полного заполнения
            bool addedToMax = factory.AddWood(500);
            Assert.IsTrue(addedToMax);
            Assert.AreEqual(1000, factory.WoodStorage);
        }

        /// <summary>
        /// Тест добычи древесины из леса
        /// </summary>
        [TestMethod]
        public void TestWoodExtraction()
        {
            var forest = new NaturalResource { Amount = 100 };
            var factory = new WoodProcessingFactory(forest);

            // Добыча без рабочих
            int extracted = factory.ExtractWood();
            Assert.AreEqual(0, extracted);
            Assert.AreEqual(0, factory.WoodStorage);
            Assert.AreEqual(100, forest.Amount);

            // Добыча с рабочими
            factory.SetWorkersCount(1);
            extracted = factory.ExtractWood();

            Assert.IsTrue(extracted > 0);
            Assert.AreEqual(extracted, factory.WoodStorage);
            Assert.AreEqual(100 - extracted, forest.Amount);
        }

        /// <summary>
        /// Тест добычи древесины при ограниченном хранилище
        /// </summary>
        [TestMethod]
        public void TestWoodExtractionWithLimitedStorage()
        {
            var forest = new NaturalResource { Amount = 1000 };
            var factory = new WoodProcessingFactory(forest);

            // Заполняем хранилище частично
            factory.AddWood(950);
            factory.SetWorkersCount(1);

            int extracted = factory.ExtractWood();

            // Добыча не должна превышать доступное место
            Assert.IsTrue(extracted <= 50);
            Assert.AreEqual(950 + extracted, factory.WoodStorage);
        }


        /// <summary>
        /// Тест управления рабочими
        /// </summary>
        [TestMethod]
        public void TestWorkerManagement()
        {
            var factory = new WoodProcessingFactory();

            // Установка количества рабочих
            factory.SetWorkersCount(3);
            Assert.AreEqual(3, factory.WorkersCount);

            // Попытка установить больше рабочих, чем максимум
            factory.SetWorkersCount(10);
            Assert.AreEqual(8, factory.WorkersCount); // Должно ограничиться MaxWorkers

            // Установка нуля рабочих
            factory.SetWorkersCount(0);
            Assert.AreEqual(0, factory.WorkersCount);
        }

        /// <summary>
        /// Тест производственного цикла без древесины
        /// </summary>
        [TestMethod]
        public void TestProductionCycleWithoutWood()
        {
            var factory = new WoodProcessingFactory();
            factory.SetWorkersCount(1);

            int initialWood = factory.WoodStorage;
            factory.ProcessWorkshops();

            // Без древесины производство не должно изменить состояние
            Assert.AreEqual(initialWood, factory.WoodStorage);
        }

        /// <summary>
        /// Тест производственного цикла с древесиной
        /// </summary>
        [TestMethod]
        public void TestProductionCycleWithWood()
        {
            var factory = new WoodProcessingFactory();
            factory.SetWorkersCount(1);
            factory.AddWood(100);

            int initialWood = factory.WoodStorage;
            factory.ProcessWorkshops();

            // После производства количество древесины должно уменьшиться
            Assert.IsTrue(factory.WoodStorage < initialWood);
        }

        /// <summary>
        /// Тест полного цикла фабрики
        /// </summary>
        [TestMethod]
        public void TestFullProductionCycle()
        {
            var forest = new NaturalResource { Amount = 200 };
            var factory = new WoodProcessingFactory(forest);
            factory.SetWorkersCount(1);

            factory.FullProductionCycle();

            // После полного цикла должно быть какое-то количество древесины
            Assert.IsTrue(factory.WoodStorage >= 0);
        }

        /// <summary>
        /// Тест получения выходной продукции
        /// </summary>
        [TestMethod]
        public void TestGetProductionOutput()
        {
            var factory = new WoodProcessingFactory();

            var output = factory.GetProductionOutput();

            Assert.IsNotNull(output);
            Assert.AreEqual(3, output.Count);
            Assert.IsTrue(output.ContainsKey("Обработанная древесина"));
            Assert.IsTrue(output.ContainsKey("Мебель"));
            Assert.IsTrue(output.ContainsKey("Бумага"));
        }

        /// <summary>
        /// Тест инициализации цехов
        /// </summary>
        [TestMethod]
        public void TestWorkshopsInitialization()
        {
            var factory = new WoodProcessingFactory();

            Assert.AreEqual(3, factory.Workshops.Count);

            // Проверяем цех обработки древесины
            var woodProcessing = factory.Workshops[0];
            Assert.AreEqual("Цех обработки древесины", woodProcessing.Name);
            Assert.AreEqual(5, woodProcessing.ProductionCycleTime);
            Assert.IsTrue(woodProcessing.InputRequirements.ContainsKey("Древесина"));
            Assert.IsTrue(woodProcessing.OutputProducts.ContainsKey("Обработанная древесина"));

            // Проверяем мебельный цех
            var furnitureWorkshop = factory.Workshops[1];
            Assert.AreEqual("Мебельный цех", furnitureWorkshop.Name);
            Assert.AreEqual(8, furnitureWorkshop.ProductionCycleTime);

            // Проверяем цех производства бумаги
            var paperWorkshop = factory.Workshops[2];
            Assert.AreEqual("Цех производства бумаги", paperWorkshop.Name);
            Assert.AreEqual(6, paperWorkshop.ProductionCycleTime);
        }

        /// <summary>
        /// Тест добычи с истощенным лесом
        /// </summary>
        [TestMethod]
        public void TestWoodExtractionWithDepletedForest()
        {
            var forest = new NaturalResource { Amount = 5 }; // Мало древесины
            var factory = new WoodProcessingFactory(forest);
            factory.SetWorkersCount(1);

            int extracted = factory.ExtractWood();

            // Добыча не должна превышать доступное количество древесины
            Assert.AreEqual(5, extracted);
            Assert.AreEqual(0, forest.Amount);
            Assert.AreEqual(5, factory.WoodStorage);

            // Последующая добыча должна вернуть 0
            extracted = factory.ExtractWood();
            Assert.AreEqual(0, extracted);
        }
    }
}