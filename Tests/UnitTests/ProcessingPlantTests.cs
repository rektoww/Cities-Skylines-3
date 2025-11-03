//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Core.Models.Map;
//using Core.Models.Mobs;
//using Core.Enums;

//namespace Tests.UnitTests
//{
//    /// <summary>
//    /// Тесты для перерабатывающего завода
//    /// </summary>
//    [TestClass]
//    public sealed class ProcessingPlantTests
//    {
//        /// <summary>
//        /// Тест создания перерабатывающего завода
//        /// </summary>
//        [TestMethod]
//        public void TestProcessingPlantCreation()
//        {
//            var plant = new ProcessingPlant("Test Plant", ResourceType.Iron, "Сталь", 1000, 500);

//            Assert.AreEqual("Test Plant", plant.Name);
//            Assert.AreEqual(ResourceType.Iron, plant.InputResourceType);
//            Assert.AreEqual("Сталь", plant.OutputProductType);
//            Assert.AreEqual(1000, plant.InputStorageCapacity);
//            Assert.AreEqual(500, plant.OutputStorageCapacity);
//            Assert.AreEqual(15, plant.ProcessingRate);
//            Assert.AreEqual(6, plant.MaxWorkers);
//            Assert.IsTrue(plant.IsActive);
//        }

//        /// <summary>
//        /// Тест управления рабочими
//        /// </summary>
//        [TestMethod]
//        public void TestWorkerManagement()
//        {
//            var plant = new ProcessingPlant("Test Plant", ResourceType.Iron, "Сталь", 1000, 500);
//            var citizen = new Citizen(0, 0, null) { Age = 25, IsEmployed = false };

//            bool added = plant.AddWorker(citizen);
//            Assert.IsTrue(added);
//            Assert.AreEqual(1, plant.WorkersCount);
//            Assert.IsTrue(citizen.IsEmployed);
//            Assert.IsTrue(plant.IsWorker(citizen));

//            bool removed = plant.RemoveWorker(citizen);
//            Assert.IsTrue(removed);
//            Assert.AreEqual(0, plant.WorkersCount);
//            Assert.IsFalse(citizen.IsEmployed);
//        }

//        /// <summary>
//        /// Тест добавления сырья
//        /// </summary>
//        [TestMethod]
//        public void TestAddRawMaterials()
//        {
//            var plant = new ProcessingPlant("Test Plant", ResourceType.Iron, "Сталь", 1000, 500);

//            bool added = plant.AddRawMaterials(500);
//            Assert.IsTrue(added);
//            Assert.AreEqual(500, plant.CurrentInputStorage);

//            bool notAdded = plant.AddRawMaterials(600);
//            Assert.IsFalse(notAdded);
//            Assert.AreEqual(500, plant.CurrentInputStorage);
//        }

//        /// <summary>
//        /// Тест переработки материалов
//        /// </summary>
//        [TestMethod]
//        public void TestProcessMaterials()
//        {
//            var plant = new ProcessingPlant("Test Plant", ResourceType.Iron, "Сталь", 1000, 500);
//            plant.IsActive = true;

//            var citizen = new Citizen(0, 0, null) { Age = 25, IsEmployed = false };
//            plant.AddWorker(citizen);
//            plant.AddRawMaterials(100);

//            int processed = plant.ProcessMaterials();

//            Assert.IsTrue(processed > 0);
//            Assert.IsTrue(plant.CurrentInputStorage < 100);
//            Assert.AreEqual(processed, plant.CurrentOutputStorage);
//        }

//        /// <summary>
//        /// Тест сбора продукции
//        /// </summary>
//        [TestMethod]
//        public void TestCollectProducts()
//        {
//            var plant = new ProcessingPlant("Test Plant", ResourceType.Iron, "Сталь", 1000, 500);
//            plant.IsActive = true;

//            var citizen = new Citizen(0, 0, null) { Age = 25, IsEmployed = false };
//            plant.AddWorker(citizen);
//            plant.AddRawMaterials(100);
//            plant.ProcessMaterials();

//            int collected = plant.CollectProducts();

//            Assert.IsTrue(collected > 0);
//            Assert.AreEqual(0, plant.CurrentOutputStorage);
//        }

//        /// <summary>
//        /// Тест проверки возможности переработки
//        /// </summary>
//        [TestMethod]
//        public void TestCanProcess()
//        {
//            var plant = new ProcessingPlant("Test Plant", ResourceType.Iron, "Сталь", 1000, 500);
//            plant.IsActive = true;

//            bool canProcess = plant.CanProcess();
//            Assert.IsFalse(canProcess); // Нет рабочих

//            var citizen = new Citizen(0, 0, null) { Age = 25, IsEmployed = false };
//            plant.AddWorker(citizen);

//            canProcess = plant.CanProcess();
//            Assert.IsFalse(canProcess); // Нет сырья

//            plant.AddRawMaterials(100);
//            canProcess = plant.CanProcess();
//            Assert.IsTrue(canProcess); // Есть рабочие и сырье
//        }

//        /// <summary>
//        /// Тест пересчета скорости переработки
//        /// </summary>
//        [TestMethod]
//        public void TestProcessingRateRecalculation()
//        {
//            var plant = new ProcessingPlant("Test Plant", ResourceType.Iron, "Сталь", 1000, 500);
//            int initialRate = plant.ProcessingRate;

//            var citizen1 = new Citizen(0, 0, null) { Age = 25, IsEmployed = false };
//            var citizen2 = new Citizen(1, 1, null) { Age = 30, IsEmployed = false };

//            plant.AddWorker(citizen1);
//            Assert.AreEqual(18, plant.ProcessingRate); // 15 + 3*1

//            plant.AddWorker(citizen2);
//            Assert.AreEqual(21, plant.ProcessingRate); // 15 + 3*2
//        }

//        /// <summary>
//        /// Тест производственных цехов
//        /// </summary>
//        [TestMethod]
//        public void TestWorkshopsProcessing()
//        {
//            var plant = new ProcessingPlant("Test Plant", ResourceType.Iron, "Сталь", 1000, 500);
//            plant.IsActive = true;

//            var citizen = new Citizen(0, 0, null) { Age = 25, IsEmployed = false };
//            plant.AddWorker(citizen);
//            plant.AddRawMaterials(100);

//            int initialInput = plant.CurrentInputStorage;
//            int initialOutput = plant.CurrentOutputStorage;

//            plant.ProcessWorkshops();

//            Assert.IsTrue(plant.CurrentInputStorage < initialInput);
//            Assert.IsTrue(plant.CurrentOutputStorage > initialOutput);
//        }

//        /// <summary>
//        /// Тест полного производственного цикла
//        /// </summary>
//        [TestMethod]
//        public void TestFullProductionCycle()
//        {
//            var plant = new ProcessingPlant("Test Plant", ResourceType.Iron, "Сталь", 1000, 500);
//            plant.IsActive = true;

//            var citizen = new Citizen(0, 0, null) { Age = 25, IsEmployed = false };
//            plant.AddWorker(citizen);
//            plant.AddRawMaterials(100);

//            plant.FullProductionCycle();

//            Assert.IsTrue(plant.CurrentInputStorage >= 0);
//            Assert.IsTrue(plant.CurrentOutputStorage >= 0);
//        }

//        /// <summary>
//        /// Тест получения списка рабочих
//        /// </summary>
//        [TestMethod]
//        public void TestGetWorkers()
//        {
//            var plant = new ProcessingPlant("Test Plant", ResourceType.Iron, "Сталь", 1000, 500);
//            var citizen1 = new Citizen(0, 0, null) { Age = 25, IsEmployed = false };
//            var citizen2 = new Citizen(1, 1, null) { Age = 30, IsEmployed = false };

//            plant.AddWorker(citizen1);
//            plant.AddWorker(citizen2);

//            var workers = plant.GetWorkers();

//            Assert.AreEqual(2, workers.Count);
//            Assert.IsTrue(workers.Contains(citizen1));
//            Assert.IsTrue(workers.Contains(citizen2));
//        }

//        /// <summary>
//        /// Тест коэффициента конверсии
//        /// </summary>
//        [TestMethod]
//        public void TestConversionRate()
//        {
//            var plant = new ProcessingPlant("Test Plant", ResourceType.Iron, "Сталь", 1000, 500);
//            plant.IsActive = true;

//            var citizen = new Citizen(0, 0, null) { Age = 25, IsEmployed = false };
//            plant.AddWorker(citizen);

//            // Добавляем точное количество для проверки конверсии
//            plant.AddRawMaterials(100);
//            plant.ProcessMaterials();

//            // 100 * 0.7 = 70 единиц продукции
//            Assert.IsTrue(plant.CurrentOutputStorage <= 70);
//        }
//    }
//}