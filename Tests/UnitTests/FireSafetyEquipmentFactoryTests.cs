using Core.Models.Buildings.IndustrialBuildings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Tests.UnitTests
{
    [TestClass]
    public class FireSafetyEquipmentFactoryTests
    {
        private FireSafetyEquipmentFactory _factory;

        [TestInitialize]
        public void Setup()
        {
            _factory = new FireSafetyEquipmentFactory();
        }

        /// <summary>
        /// Тест инициализации стартовых компонентов
        /// </summary>
        [TestMethod]
        public void TestStartingComponentsInitialization()
        {
            // Arrange & Act
            var components = _factory.GetComponentStorage();

            // Assert - проверяем ВСЕ компоненты
            Assert.AreEqual(150, components[FireSafetyEquipmentFactory.FireSafetyComponent.Steel],
                "Сталь должна быть инициализирована значением 150");

            Assert.AreEqual(120, components[FireSafetyEquipmentFactory.FireSafetyComponent.Plastic],
                "Пластик должен быть инициализирован значением 120");

            Assert.AreEqual(80, components[FireSafetyEquipmentFactory.FireSafetyComponent.Aluminum],
                "Алюминий должен быть инициализирован значением 80");

            Assert.AreEqual(50, components[FireSafetyEquipmentFactory.FireSafetyComponent.Copper],
                "Медь должна быть инициализирована значением 50");

            Assert.AreEqual(60, components[FireSafetyEquipmentFactory.FireSafetyComponent.Chemicals],
                "Химикаты должны быть инициализированы значением 60");

            Assert.AreEqual(40, components[FireSafetyEquipmentFactory.FireSafetyComponent.Electronics],
                "Электроника должна быть инициализирована значением 40");

            Assert.AreEqual(70, components[FireSafetyEquipmentFactory.FireSafetyComponent.Rubber],
                "Резина должна быть инициализирована значением 70");

            Assert.AreEqual(90, components[FireSafetyEquipmentFactory.FireSafetyComponent.Textile],
                "Текстиль должен быть инициализирован значением 90");

            // Проверка общего количества
            int totalComponents = components.Values.Sum();
            Assert.AreEqual(660, totalComponents,
                "Общее количество компонентов должно быть 660");
        }

        /// <summary>
        /// Тест управления рабочими
        /// </summary>
        [TestMethod]
        public void TestWorkersManagement()
        {
            // Arrange
            var factory = new FireSafetyEquipmentFactory();

            // Act 1 - Установка валидного количества рабочих
            factory.SetWorkersCount(6);

            // Assert 1
            Assert.AreEqual(6, factory.WorkersCount, "Должно быть установлено 6 рабочих");

            // Act 2 - Попытка установить больше максимума
            factory.SetWorkersCount(15); // Максимум 12

            // Assert 2
            Assert.AreEqual(12, factory.WorkersCount, "Не должно превышать максимум в 12 рабочих");

            // Act 3 - Попытка установить отрицательное значение
            factory.SetWorkersCount(-3);

            // Assert 3
            Assert.AreEqual(0, factory.WorkersCount, "Не должно быть отрицательного количества рабочих");

            // Act 4 - Проверка эффективности при разных количествах рабочих
            factory.SetWorkersCount(6); // 50% от максимума

            // Assert 4
            float expectedEfficiency = 0.5f + (6 / 12f * 0.4f) + (1 * 0.02f); // 0.5 + 0.2 + 0.02 = 0.72
            Assert.AreEqual(expectedEfficiency, factory.ProductionEfficiency, 0.01f,
                "Эффективность должна корректно рассчитываться");
        }

        /// <summary>
        /// Тест производства с рабочими
        /// </summary>
        [TestMethod]
        public void TestProductionWithWorkers()
        {
            // Arrange
            _factory.SetWorkersCount(12); // Максимум рабочих
            var initialComponents = _factory.GetComponentStorage();

            // Act
            _factory.FullProductionCycle();
            var productionOutput = _factory.GetProductionOutput();
            var finalComponents = _factory.GetComponentStorage();

            // Assert
            Assert.IsTrue(productionOutput.Count > 0, "Должна быть произведена хоть какая-то продукция");
            Assert.IsTrue(_factory.ProductionEfficiency > 0.9f, "Эффективность должна быть высокой при полной загрузке");

            // Проверяем, что компоненты были израсходованы
            foreach (var component in initialComponents)
            {
                Assert.IsTrue(finalComponents[component.Key] < component.Value,
                    $"Компонент {component.Key} должен был быть израсходован. Было: {component.Value}, Стало: {finalComponents[component.Key]}");
            }
        }

        /// <summary>
        /// Тест отсутствия производства без рабочих
        /// </summary>
        [TestMethod]
        public void TestNoProductionWithoutWorkers()
        {
            // Arrange
            _factory.SetWorkersCount(0);

            // Act
            _factory.FullProductionCycle();
            var productionOutput = _factory.GetProductionOutput();

            // Assert
            Assert.AreEqual(0, productionOutput.Count, "Не должно быть продукции без рабочих");
            Assert.AreEqual(0f, _factory.ProductionEfficiency, "Эффективность должна быть 0 без рабочих");
        }

        /// <summary>
        /// Тест инициализации производственных линий
        /// </summary>
        [TestMethod]
        public void TestProductionLinesInitialization()
        {
            // Arrange & Act
            var productionLines = _factory.ProductionLines;

            // Assert
            Assert.AreEqual(5, productionLines.Count, "Должно быть 5 производственных линий");

            // Проверяем основные линии
            var extinguisherLine = productionLines[0];
            Assert.AreEqual("Линия огнетушителей и рукавов", extinguisherLine.Name);
            Assert.AreEqual(8, extinguisherLine.ProductionCycleTime);

            var alarmSystemLine = productionLines[1];
            Assert.AreEqual("Линия систем сигнализации", alarmSystemLine.Name);
            Assert.AreEqual(12, alarmSystemLine.ProductionCycleTime);

            var suppressionLine = productionLines[2];
            Assert.AreEqual("Линия систем пожаротушения", suppressionLine.Name);
            Assert.AreEqual(16, suppressionLine.ProductionCycleTime);
        }

        /// <summary>
        /// Тест добавления компонентов
        /// </summary>
        [TestMethod]
        public void TestAddComponent()
        {
            // Arrange
            var component = FireSafetyEquipmentFactory.FireSafetyComponent.Steel;
            var initialAmount = _factory.GetComponentStorage()[component];

            // Act
            bool added = _factory.AddComponent(component, 100);

            // Assert
            Assert.IsTrue(added, "Компоненты должны быть успешно добавлены");
            var finalAmount = _factory.GetComponentStorage()[component];
            Assert.AreEqual(initialAmount + 100, finalAmount, "Количество компонентов должно увеличиться");
        }

        /// <summary>
        /// Тест добавления компонентов сверх лимита
        /// </summary>
        [TestMethod]
        public void TestAddComponentOverLimit()
        {
            // Arrange
            var component = FireSafetyEquipmentFactory.FireSafetyComponent.Steel;

            // Act - попытка добавить слишком много компонентов
            bool added = _factory.AddComponent(component, 2000);

            // Assert
            Assert.IsFalse(added, "Нельзя добавить компоненты сверх лимита склада");
        }

        /// <summary>
        /// Тест улучшения стандартов безопасности
        /// </summary>
        [TestMethod]
        public void TestUpgradeSafetyStandards()
        {
            // Arrange
            var initialLevel = _factory.SafetyStandardsLevel;

            // Act
            bool upgraded = _factory.UpgradeSafetyStandards();

            // Assert
            Assert.IsTrue(upgraded, "Улучшение должно быть успешным");
            Assert.AreEqual(initialLevel + 1, _factory.SafetyStandardsLevel, "Уровень должен увеличиться на 1");
        }

        /// <summary>
        /// Тест максимального уровня стандартов безопасности
        /// </summary>
        [TestMethod]
        public void TestMaxSafetyStandardsLevel()
        {
            // Arrange
            // Устанавливаем максимальный уровень
            for (int i = 0; i < 4; i++)
            {
                _factory.UpgradeSafetyStandards();
            }

            // Act - попытка улучшить сверх максимума
            bool upgraded = _factory.UpgradeSafetyStandards();

            // Assert
            Assert.IsFalse(upgraded, "Нельзя улучшить сверх максимального уровня");
            Assert.AreEqual(5, _factory.SafetyStandardsLevel, "Должен быть достигнут максимальный уровень");
        }

        /// <summary>
        /// Тест соответствия стандартам безопасности
        /// </summary>
        [TestMethod]
        public void TestMeetsSafetyRegulations()
        {
            // Arrange
            var factory = new FireSafetyEquipmentFactory(); // Уровень 1 по умолчанию

            // Act & Assert - уровень 1 не соответствует
            Assert.IsFalse(factory.MeetsSafetyRegulations(), "Уровень 1 не соответствует стандартам");

            // Повышаем уровень
            factory.UpgradeSafetyStandards(); // Уровень 2

            // Assert - уровень 2 соответствует
            Assert.IsTrue(factory.MeetsSafetyRegulations(), "Уровень 2 соответствует стандартам");
        }

        /// <summary>
        /// Тест рейтинга безопасности продукции
        /// </summary>
        [TestMethod]
        public void TestProductSafetyRating()
        {
            // Arrange
            var factory = new FireSafetyEquipmentFactory();

            // Act
            float ratingLevel1 = factory.GetProductSafetyRating();
            factory.UpgradeSafetyStandards();
            float ratingLevel2 = factory.GetProductSafetyRating();

            // Assert
            Assert.AreEqual(0.76f, ratingLevel1, 0.01f, "Рейтинг при уровне 1 должен быть 0.76");
            Assert.AreEqual(0.82f, ratingLevel2, 0.01f, "Рейтинг при уровне 2 должен быть 0.82");
            Assert.IsTrue(ratingLevel2 > ratingLevel1, "Рейтинг должен увеличиваться с уровнем");
        }

        /// <summary>
        /// Тест отгрузки продукции
        /// </summary>
        [TestMethod]
        public void TestProductShipping()
        {
            // Arrange
            _factory.SetWorkersCount(12);
            _factory.FullProductionCycle(); // Производим продукцию

            var initialProducts = _factory.GetProductionOutput();

            if (initialProducts.Count > 0)
            {
                var productType = initialProducts.Keys.First();
                var initialAmount = initialProducts[productType];

                // Act - успешная отгрузка
                bool shipped = _factory.ShipProduct(productType, 1);

                // Assert
                Assert.IsTrue(shipped, "Отгрузка должна быть успешной");

                var finalProducts = _factory.GetProductionOutput();
                Assert.AreEqual(initialAmount - 1, finalProducts[productType],
                    "Количество продукции должно уменьшиться на 1 после отгрузки");
            }
            else
            {
                Assert.Inconclusive("Нет произведенной продукции для тестирования отгрузки");
            }
        }

        /// <summary>
        /// Тест расчета стоимости обслуживания
        /// </summary>
        [TestMethod]
        public void TestMaintenanceCostCalculation()
        {
            // Arrange
            _factory.SetWorkersCount(8);
            _factory.UpgradeSafetyStandards(); // Уровень 2

            // Act
            decimal cost = _factory.CalculateMaintenanceCost();

            // Assert
            decimal expectedCost = 12000m + (8 * 2000m) + (2 * 3000m); // 12000 + 16000 + 6000 = 34000
            Assert.AreEqual(expectedCost, cost, "Стоимость обслуживания должна корректно рассчитываться");
        }

        /// <summary>
        /// Тест получения информации о производстве
        /// </summary>
        [TestMethod]
        public void TestGetProductionInfo()
        {
            // Arrange
            _factory.SetWorkersCount(6);

            // Act
            var info = _factory.GetProductionInfo();

            // Assert
            Assert.IsTrue(info.ContainsKey("WorkersCount"), "Должна содержать информацию о рабочих");
            Assert.IsTrue(info.ContainsKey("ProductionEfficiency"), "Должна содержать эффективность");
            Assert.IsTrue(info.ContainsKey("SafetyStandardsLevel"), "Должна содержать уровень стандартов");
            Assert.IsTrue(info.ContainsKey("ProductSafetyRating"), "Должна содержать рейтинг безопасности");

            Assert.AreEqual(6, info["WorkersCount"], "Информация о рабочих должна быть корректной");
            Assert.IsTrue((float)info["ProductionEfficiency"] > 0, "Эффективность должна быть положительной");
        }
    }
}