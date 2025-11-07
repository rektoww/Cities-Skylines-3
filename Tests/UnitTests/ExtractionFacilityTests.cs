using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core.Models.Map;
using Core.Enums;
using Core.Models.Factories;
using Core.Models.Components;
using System.Collections.Generic;

namespace Tests.UnitTests
{
    /// <summary>
    /// Тесты для производственных фабрик
    /// </summary>
    [TestClass]
    public sealed class ProductionFactoryTests
    {
        /// <summary>
        /// Тест создания производственной фабрики
        /// </summary>
        [TestMethod]
        public void TestProductionFactoryCreation()
        {
            var factory = new ProductionFactory(maxWorkers: 10, baseExtractionRate: 15);

            Assert.AreEqual(10, factory.MaxWorkers);
            Assert.AreEqual(15, factory.BaseExtractionRate);
            Assert.AreEqual(0, factory.WorkersCount);
            Assert.IsNotNull(factory.Workshops);
            Assert.IsNotNull(factory.ConnectedResources);
            Assert.IsNotNull(factory.ResourceStorage);
        }

        /// <summary>
        /// Тест управления рабочими
        /// </summary>
        [TestMethod]
        public void TestWorkerManagement()
        {
            var factory = new ProductionFactory(maxWorkers: 8, baseExtractionRate: 10);

            // Установка количества рабочих
            factory.SetWorkersCount(3);
            Assert.AreEqual(3, factory.WorkersCount);

            // Попытка установить больше рабочих, чем максимум
            factory.SetWorkersCount(10);
            Assert.AreEqual(8, factory.WorkersCount);

            // Установка нуля рабочих
            factory.SetWorkersCount(0);
            Assert.AreEqual(0, factory.WorkersCount);

            // Отрицательное количество рабочих
            factory.SetWorkersCount(-5);
            Assert.AreEqual(0, factory.WorkersCount);
        }


        /// <summary>
        /// Тест подключения ресурсов
        /// </summary>
        [TestMethod]
        public void TestResourceConnection()
        {
            var factory = new ProductionFactory();
            var ironMine = new NaturalResource { Type = ResourceType.Iron, Amount = 100 };
            var oilField = new NaturalResource { Type = ResourceType.Oil, Amount = 50 };

            factory.ConnectResource(ironMine);
            factory.ConnectResource(oilField);

            Assert.AreEqual(2, factory.ConnectedResources.Count);
            Assert.IsTrue(factory.ConnectedResources.Contains(ironMine));
            Assert.IsTrue(factory.ConnectedResources.Contains(oilField));

            // Попытка добавить тот же ресурс дважды
            factory.ConnectResource(ironMine);
            Assert.AreEqual(2, factory.ConnectedResources.Count); // Не должно добавить дубликат
        }

        /// <summary>
        /// Тест добычи ресурсов
        /// </summary>
        [TestMethod]
        public void TestResourceExtraction()
        {
            var factory = new ProductionFactory(baseExtractionRate: 20);
            var ironMine = new NaturalResource { Type = ResourceType.Iron, Amount = 100 };

            factory.ConnectResource(ironMine);
            factory.SetStorageCapacity(ResourceType.Iron, 1000);

            // Добыча без рабочих
            var extracted = factory.ExtractResources();
            Assert.AreEqual(0, extracted.Count);
            Assert.AreEqual(100, ironMine.Amount);

            // Добыча с рабочими
            factory.SetWorkersCount(2);
            extracted = factory.ExtractResources();

            Assert.IsTrue(extracted.ContainsKey(ResourceType.Iron));
            int extractedAmount = extracted[ResourceType.Iron];
            Assert.IsTrue(extractedAmount > 0);
            Assert.AreEqual(100 - extractedAmount, ironMine.Amount);
            Assert.AreEqual(extractedAmount, factory.GetResourceAmount(ResourceType.Iron));
        }

        
        /// <summary>
        /// Тест проверки возможности производства
        /// </summary>
        [TestMethod]
        public void TestCanProduce()
        {
            var factory = new ProductionFactory();

            var workshop = new Workshop
            {
                Name = "Цех проверки"
            };
            workshop.InputRequirements.Add(ResourceType.Iron, 10);
            workshop.InputRequirements.Add(ResourceType.Oil, 5);

            factory.AddWorkshop(workshop);
            factory.SetStorageCapacity(ResourceType.Iron, 100);
            factory.SetStorageCapacity(ResourceType.Oil, 100);

            // Недостаточно ресурсов
            factory.AddResource(ResourceType.Iron, 15);
            factory.AddResource(ResourceType.Oil, 3);
            Assert.IsFalse(factory.CanProduce(workshop));

            // Достаточно ресурсов
            factory.AddResource(ResourceType.Oil, 3); // Теперь 6
            Assert.IsTrue(factory.CanProduce(workshop));
        }

        /// <summary>
        /// Тест полного производственного цикла
        /// </summary>
        [TestMethod]
        public void TestFullProductionCycle()
        {
            var factory = new ProductionFactory(baseExtractionRate: 25);
            var ironMine = new NaturalResource { Type = ResourceType.Iron, Amount = 200 };

            // Настраиваем фабрику
            factory.ConnectResource(ironMine);
            factory.SetStorageCapacity(ResourceType.Iron, 100);
            factory.SetStorageCapacity(ResourceType.Oil, 50);
            factory.SetWorkersCount(3);

            // Добавляем производственный цех
            var workshop = new Workshop
            {
                Name = "Перерабатывающий цех"
            };
            workshop.InputRequirements.Add(ResourceType.Iron, 20);
            workshop.OutputProducts.Add(ResourceType.Oil, 10);

            factory.AddWorkshop(workshop);

            // Запускаем полный цикл
            factory.FullProductionCycle();

            // Проверяем, что ресурсы добылись и переработались
            var storage = factory.GetResourceStorage();
            Assert.IsTrue(storage.ContainsKey(ResourceType.Iron) || storage.ContainsKey(ResourceType.Oil));
        }

        /// <summary>
        /// Тест получения запасов ресурсов
        /// </summary>
        [TestMethod]
        public void TestGetResourceStorage()
        {
            var factory = new ProductionFactory();

            factory.SetStorageCapacity(ResourceType.Iron, 100);
            factory.SetStorageCapacity(ResourceType.Oil, 50);

            factory.AddResource(ResourceType.Iron, 30);
            factory.AddResource(ResourceType.Oil, 20);

            var storage = factory.GetResourceStorage();

            Assert.AreEqual(2, storage.Count);
            Assert.AreEqual(30, storage[ResourceType.Iron]);
            Assert.AreEqual(20, storage[ResourceType.Oil]);
        }

        /// <summary>
        /// Тест забора ресурсов из хранилища
        /// </summary>
        [TestMethod]
        public void TestTakeResource()
        {
            var factory = new ProductionFactory();

            factory.SetStorageCapacity(ResourceType.Iron, 100);
            factory.AddResource(ResourceType.Iron, 50);

            // Успешный забор
            bool result = factory.TakeResource(ResourceType.Iron, 30);
            Assert.IsTrue(result);
            Assert.AreEqual(20, factory.GetResourceAmount(ResourceType.Iron));

            // Попытка забрать больше, чем есть
            result = factory.TakeResource(ResourceType.Iron, 25);
            Assert.IsFalse(result);
            Assert.AreEqual(20, factory.GetResourceAmount(ResourceType.Iron));

            // Попытка забрать несуществующий ресурс
            result = factory.TakeResource(ResourceType.Gas, 10);
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Тест инициализации цехов
        /// </summary>
        [TestMethod]
        public void TestWorkshopInitialization()
        {
            var factory = new ProductionFactory();

            var workshops = new List<Workshop>
            {
                new Workshop { Name = "Цех 1" },
                new Workshop { Name = "Цех 2" },
                new Workshop { Name = "Цех 3" }
            };

            factory.InitializeWorkshops(workshops);

            Assert.AreEqual(3, factory.Workshops.Count);
            Assert.AreEqual("Цех 1", factory.Workshops[0].Name);
            Assert.AreEqual("Цех 2", factory.Workshops[1].Name);
            Assert.AreEqual("Цех 3", factory.Workshops[2].Name);
        }
    }
}