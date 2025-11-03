using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core.Enums;

namespace Tests.UnitTests
{
    /// <summary>
    /// Тесты для перерабатывающего завода
    /// </summary>
    [TestClass]
    public sealed class ProcessingPlantTests
    {
        /// <summary>
        /// Тест создания перерабатывающего завода
        /// </summary>
        [TestMethod]
        public void TestProcessingPlantCreation()
        {
            var plant = new ProcessingPlant(ResourceType.Iron, "Сталь", 1000, 500);

            Assert.AreEqual(ResourceType.Iron, plant.InputResourceType);
            Assert.AreEqual("Сталь", plant.OutputProductType);
            Assert.AreEqual(1000, plant.InputStorageCapacity);
            Assert.AreEqual(500, plant.OutputStorageCapacity);
            Assert.AreEqual(15, plant.ProcessingRate);
            Assert.AreEqual(6, plant.MaxWorkers);
            Assert.AreEqual(0, plant.WorkersCount);
            Assert.AreEqual(0.7f, plant.ConversionRate);
            Assert.AreEqual(1, plant.Workshops.Count);
        }

        /// <summary>
        /// Тест управления рабочими
        /// </summary>
        [TestMethod]
        public void TestWorkerManagement()
        {
            var plant = new ProcessingPlant(ResourceType.Iron, "Сталь", 1000, 500);

            // Установка количества рабочих
            plant.SetWorkersCount(3);
            Assert.AreEqual(3, plant.WorkersCount);

            // Попытка установить больше рабочих, чем максимум
            plant.SetWorkersCount(10);
            Assert.AreEqual(6, plant.WorkersCount); // Должно ограничиться MaxWorkers

            // Установка нуля рабочих
            plant.SetWorkersCount(0);
            Assert.AreEqual(0, plant.WorkersCount);
        }

        /// <summary>
        /// Тест добавления сырья
        /// </summary>
        [TestMethod]
        public void TestAddRawMaterials()
        {
            var plant = new ProcessingPlant(ResourceType.Iron, "Сталь", 1000, 500);

            // Успешное добавление
            bool added = plant.AddRawMaterials(500);
            Assert.IsTrue(added);
            Assert.AreEqual(500, plant.CurrentInputStorage);

            // Попытка добавить больше, чем вмещает хранилище
            bool notAdded = plant.AddRawMaterials(600);
            Assert.IsFalse(notAdded);
            Assert.AreEqual(500, plant.CurrentInputStorage); // Количество не изменилось

            // Добавление до полного заполнения
            bool addedToMax = plant.AddRawMaterials(500);
            Assert.IsTrue(addedToMax);
            Assert.AreEqual(1000, plant.CurrentInputStorage);
        }

        /// <summary>
        /// Тест переработки материалов
        /// </summary>
        [TestMethod]
        public void TestProcessMaterials()
        {
            var plant = new ProcessingPlant(ResourceType.Iron, "Сталь", 1000, 500);

            // Переработка без рабочих
            int processed = plant.ProcessMaterials();
            Assert.AreEqual(0, processed);
            Assert.AreEqual(0, plant.CurrentInputStorage);
            Assert.AreEqual(0, plant.CurrentOutputStorage);

            // Переработка без сырья
            plant.SetWorkersCount(1);
            processed = plant.ProcessMaterials();
            Assert.AreEqual(0, processed);

            // Переработка с сырьем
            plant.AddRawMaterials(100);
            processed = plant.ProcessMaterials();

            Assert.IsTrue(processed > 0);
            Assert.IsTrue(plant.CurrentInputStorage < 100);
            Assert.AreEqual(processed, plant.CurrentOutputStorage);
        }

        /// <summary>
        /// Тест переработки с ограниченным выходным хранилищем
        /// </summary>
        [TestMethod]
        public void TestProcessMaterialsWithLimitedOutputStorage()
        {
            var plant = new ProcessingPlant(ResourceType.Iron, "Сталь", 1000, 50); // Маленькое выходное хранилище
            plant.SetWorkersCount(1);
            plant.AddRawMaterials(100);

            int processed = plant.ProcessMaterials();

            // Переработка не должна превысить вместимость выходного хранилища
            Assert.IsTrue(processed <= 50);
            Assert.AreEqual(processed, plant.CurrentOutputStorage);
        }

        /// <summary>
        /// Тест пересчета скорости переработки
        /// </summary>
        [TestMethod]
        public void TestProcessingRateRecalculation()
        {
            var plant = new ProcessingPlant(ResourceType.Iron, "Сталь", 1000, 500);

            // Начальная скорость
            Assert.AreEqual(15, plant.ProcessingRate);

            // Добавляем рабочих и проверяем скорость
            plant.SetWorkersCount(1);
            Assert.AreEqual(18, plant.ProcessingRate); // 15 + 3*1

            plant.SetWorkersCount(3);
            Assert.AreEqual(24, plant.ProcessingRate); // 15 + 3*3

            plant.SetWorkersCount(6);
            Assert.AreEqual(33, plant.ProcessingRate); // 15 + 3*6
        }

        /// <summary>
        /// Тест производственных цехов
        /// </summary>
        [TestMethod]
        public void TestWorkshopsProcessing()
        {
            var plant = new ProcessingPlant(ResourceType.Iron, "Сталь", 1000, 500);
            plant.SetWorkersCount(1);
            plant.AddRawMaterials(100);

            int initialInput = plant.CurrentInputStorage;
            int initialOutput = plant.CurrentOutputStorage;

            plant.ProcessWorkshops();

            Assert.IsTrue(plant.CurrentInputStorage <= initialInput);
            Assert.IsTrue(plant.CurrentOutputStorage >= initialOutput);
        }

        /// <summary>
        /// Тест производственных цехов без рабочих
        /// </summary>
        [TestMethod]
        public void TestWorkshopsProcessingWithoutWorkers()
        {
            var plant = new ProcessingPlant(ResourceType.Iron, "Сталь", 1000, 500);
            plant.AddRawMaterials(100);

            int initialInput = plant.CurrentInputStorage;
            int initialOutput = plant.CurrentOutputStorage;

            plant.ProcessWorkshops();

            // Без рабочих цеха не должны работать
            Assert.AreEqual(initialInput, plant.CurrentInputStorage);
            Assert.AreEqual(initialOutput, plant.CurrentOutputStorage);
        }

        /// <summary>
        /// Тест полного производственного цикла
        /// </summary>
        [TestMethod]
        public void TestFullProductionCycle()
        {
            var plant = new ProcessingPlant(ResourceType.Iron, "Сталь", 1000, 500);
            plant.SetWorkersCount(1);
            plant.AddRawMaterials(100);

            plant.FullProductionCycle();

            Assert.IsTrue(plant.CurrentInputStorage >= 0);
            Assert.IsTrue(plant.CurrentOutputStorage >= 0);
        }

        /// <summary>
        /// Тест получения статуса хранилищ
        /// </summary>
        [TestMethod]
        public void TestGetStorageStatus()
        {
            var plant = new ProcessingPlant(ResourceType.Iron, "Сталь", 1000, 500);
            plant.AddRawMaterials(300);

            var (inputMaterials, outputProducts) = plant.GetStorageStatus();

            Assert.AreEqual(300, inputMaterials);
            Assert.AreEqual(0, outputProducts);
        }

        /// <summary>
        /// Тест получения производственной информации
        /// </summary>
        [TestMethod]
        public void TestGetProductionInfo()
        {
            var plant = new ProcessingPlant(ResourceType.Iron, "Сталь", 1000, 500);
            plant.SetWorkersCount(2);

            var info = plant.GetProductionInfo();

            Assert.IsNotNull(info);
            Assert.AreEqual(ResourceType.Iron, info["InputResourceType"]);
            Assert.AreEqual("Сталь", info["OutputProductType"]);
            Assert.AreEqual(21, info["ProcessingRate"]); // 15 + 3*2
            Assert.AreEqual(2, info["WorkersCount"]);
            Assert.AreEqual(6, info["MaxWorkers"]);
            Assert.AreEqual(0.7f, info["ConversionRate"]);
        }

        /// <summary>
        /// Тест коэффициента конверсии
        /// </summary>
        [TestMethod]
        public void TestConversionRate()
        {
            var plant = new ProcessingPlant(ResourceType.Iron, "Сталь", 1000, 500);
            plant.SetWorkersCount(1);

            // Добавляем точное количество для проверки конверсии
            plant.AddRawMaterials(100);
            int processed = plant.ProcessMaterials();

            // 100 * 0.7 = 70 единиц продукции (но может быть ограничено ProcessingRate)
            int expectedOutput = (int)(Math.Min(100, plant.ProcessingRate) * plant.ConversionRate);
            Assert.AreEqual(expectedOutput, processed);
            Assert.AreEqual(expectedOutput, plant.CurrentOutputStorage);
        }

        /// <summary>
        /// Тест инициализации цехов
        /// </summary>
        [TestMethod]
        public void TestWorkshopsInitialization()
        {
            var plant = new ProcessingPlant(ResourceType.Oil, "Топливо", 800, 400);

            Assert.AreEqual(1, plant.Workshops.Count);

            var workshop = plant.Workshops[0];
            Assert.AreEqual("Цех переработки Oil", workshop.Name);
            Assert.AreEqual(8, workshop.ProductionCycleTime);
            Assert.IsTrue(workshop.InputRequirements.ContainsKey("Oil"));
            Assert.IsTrue(workshop.OutputProducts.ContainsKey("Топливо"));
        }

        /// <summary>
        /// Тест переработки с недостаточным количеством сырья
        /// </summary>
        [TestMethod]
        public void TestProcessMaterialsWithInsufficientInput()
        {
            var plant = new ProcessingPlant(ResourceType.Iron, "Сталь", 1000, 500);
            plant.SetWorkersCount(1);

            // Добавляем меньше сырья, чем требуется для полного цикла
            plant.AddRawMaterials(5);
            int processed = plant.ProcessMaterials();

            // Должна произойти переработка только доступного сырья
            Assert.IsTrue(processed > 0);
            Assert.AreEqual(0, plant.CurrentInputStorage);
            Assert.AreEqual(processed, plant.CurrentOutputStorage);
        }

        /// <summary>
        /// Тест создания завода с другим типом ресурса
        /// </summary>
        [TestMethod]
        public void TestProcessingPlantWithDifferentResource()
        {
            var plant = new ProcessingPlant(ResourceType.Gas, "Сжиженный газ", 800, 300);

            Assert.AreEqual(ResourceType.Gas, plant.InputResourceType);
            Assert.AreEqual("Сжиженный газ", plant.OutputProductType);
            Assert.AreEqual(800, plant.InputStorageCapacity);
            Assert.AreEqual(300, plant.OutputStorageCapacity);
        }

        /// <summary>
        /// Тест точного расчета конверсии
        /// </summary>
        [TestMethod]
        public void TestExactConversionCalculation()
        {
            var plant = new ProcessingPlant(ResourceType.Iron, "Сталь", 1000, 500);
            plant.SetWorkersCount(1);

            // Устанавливаем точное количество сырья для предсказуемого результата
            int inputAmount = 20; // Должно быть достаточно для обработки
            plant.AddRawMaterials(inputAmount);

            int processed = plant.ProcessMaterials();

            // Проверяем точный расчет: inputAmount * ConversionRate
            int expectedOutput = (int)(inputAmount * plant.ConversionRate);
            Assert.AreEqual(expectedOutput, processed);
            Assert.AreEqual(0, plant.CurrentInputStorage); // Все сырье должно быть переработано
            Assert.AreEqual(expectedOutput, plant.CurrentOutputStorage);
        }

        /// <summary>
        /// Тест создания завода с Oil ресурсом
        /// </summary>
        [TestMethod]
        public void TestProcessingPlantWithOilResource()
        {
            var plant = new ProcessingPlant(ResourceType.Oil, "Бензин", 1200, 600);

            Assert.AreEqual(ResourceType.Oil, plant.InputResourceType);
            Assert.AreEqual("Бензин", plant.OutputProductType);
            Assert.AreEqual(1200, plant.InputStorageCapacity);
            Assert.AreEqual(600, plant.OutputStorageCapacity);
        }

        /// <summary>
        /// Тест создания завода с Gas ресурсом
        /// </summary>
        [TestMethod]
        public void TestProcessingPlantWithGasResource()
        {
            var plant = new ProcessingPlant(ResourceType.Gas, "Пропан", 900, 450);

            Assert.AreEqual(ResourceType.Gas, plant.InputResourceType);
            Assert.AreEqual("Пропан", plant.OutputProductType);
            Assert.AreEqual(900, plant.InputStorageCapacity);
            Assert.AreEqual(450, plant.OutputStorageCapacity);
        }
    }
}