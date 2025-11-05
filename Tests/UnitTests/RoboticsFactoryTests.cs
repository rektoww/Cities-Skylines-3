using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core.Enums;
using Core.Models.Buildings.IndustrialBuildings;
using System.Linq;

namespace Tests.UnitTests
{
    /// <summary>
    /// Тесты для фабрики робототехники
    /// </summary>
    [TestClass]
    public sealed class RoboticsFactoryTests
    {
        /// <summary>
        /// Тест создания фабрики робототехники
        /// </summary>
        [TestMethod]
        public void TestRoboticsFactoryCreation()
        {
            var factory = new RoboticsFactory();

            // Проверка статических свойств строительства
            Assert.AreEqual(850000m, RoboticsFactory.BuildCost);
            Assert.AreEqual(4, RoboticsFactory.RequiredMaterials.Count);
            Assert.AreEqual(15, RoboticsFactory.RequiredMaterials[ConstructionMaterial.Steel]);
            Assert.AreEqual(12, RoboticsFactory.RequiredMaterials[ConstructionMaterial.Concrete]);
            Assert.AreEqual(6, RoboticsFactory.RequiredMaterials[ConstructionMaterial.Plastic]);
            Assert.AreEqual(3, RoboticsFactory.RequiredMaterials[ConstructionMaterial.Glass]);

            // Проверка базовых свойств
            Assert.AreEqual(800, factory.MaxComponentStorage);
            Assert.AreEqual(50, factory.MaxRobotStorage);
            Assert.AreEqual(8, factory.MaxEngineers);
            Assert.AreEqual(0, factory.EngineersCount);
            Assert.AreEqual(3, factory.AssemblyLines.Count);
            Assert.AreEqual(1, factory.ResearchLevel);
        }

        /// <summary>
        /// Тест получения общего количества компонентов
        /// </summary>
        [TestMethod]
        public void TestGetTotalComponentStorage()
        {
            // Arrange
            var factory = new RoboticsFactory();

            // Act
            int total = factory.GetTotalComponentStorage();

            // Assert - 50 + 30 + 25 + 100 + 80 = 285
            Assert.AreEqual(285, total);
        }

        /// <summary>
        /// Тест управления инженерами
        /// </summary>
        [TestMethod]
        public void TestEngineerManagement()
        {
            var factory = new RoboticsFactory();

            // Установка количества инженеров
            factory.SetEngineersCount(5);
            Assert.AreEqual(5, factory.EngineersCount);

            // Попытка установить больше инженеров, чем максимум
            factory.SetEngineersCount(15);
            Assert.AreEqual(8, factory.EngineersCount); // Должно ограничиться MaxEngineers

            // Установка нуля инженеров
            factory.SetEngineersCount(0);
            Assert.AreEqual(0, factory.EngineersCount);
        }

        /// <summary>
        /// Тест добавления компонентов
        /// </summary>
        [TestMethod]
        public void TestAddComponents()
        {
            var factory = new RoboticsFactory();

            // Получаем начальное состояние
            var initialComponents = factory.GetComponentStorage();
            int initialMicrochips = initialComponents[RoboticsFactory.RoboticsComponent.Microchips];
            int initialTotal = factory.GetTotalComponentStorage();

            // Вычисляем сколько можно добавить без превышения лимита
            int availableSpace = factory.MaxComponentStorage - initialTotal;
            int microchipsToAdd = System.Math.Min(50, availableSpace);

            // Успешное добавление микрочипов
            bool addedMicrochips = factory.AddComponent(RoboticsFactory.RoboticsComponent.Microchips, microchipsToAdd);
            Assert.IsTrue(addedMicrochips, "Добавление микрочипов должно быть успешным");

            var componentsAfter = factory.GetComponentStorage();
            Assert.AreEqual(initialMicrochips + microchipsToAdd, componentsAfter[RoboticsFactory.RoboticsComponent.Microchips]);
        }

        /// <summary>
        /// Тест добавления компонентов с превышением вместимости
        /// </summary>
        [TestMethod]
        public void TestAddComponentsExceedingCapacity()
        {
            var factory = new RoboticsFactory();

            // Попытка добавить больше, чем вмещает хранилище
            bool notAdded = factory.AddComponent(RoboticsFactory.RoboticsComponent.Aluminum, 900);
            Assert.IsFalse(notAdded); // Должно вернуть false, так как 100 + 900 > 800

            var components = factory.GetComponentStorage();
            Assert.AreEqual(100, components[RoboticsFactory.RoboticsComponent.Aluminum]); // Количество не изменилось
        }

        /// <summary>
        /// Тест производства без инженеров
        /// </summary>
        [TestMethod]
        public void TestProductionWithoutEngineers()
        {
            var factory = new RoboticsFactory();

            var initialComponents = factory.GetComponentStorage();
            var initialRobots = factory.GetProductionOutput();

            // Запуск производства без инженеров
            factory.ProcessAssemblyLines();

            var finalComponents = factory.GetComponentStorage();
            var finalRobots = factory.GetProductionOutput();

            // Компоненты и роботы не должны измениться
            Assert.AreEqual(initialComponents[RoboticsFactory.RoboticsComponent.Microchips],
                          finalComponents[RoboticsFactory.RoboticsComponent.Microchips]);
            Assert.AreEqual(initialComponents[RoboticsFactory.RoboticsComponent.Aluminum],
                          finalComponents[RoboticsFactory.RoboticsComponent.Aluminum]);
            Assert.AreEqual(initialRobots.Count, finalRobots.Count);
        }

        /// <summary>
        /// Тест производства с инженерами
        /// </summary>
        [TestMethod]
        public void TestProductionWithEngineers()
        {
            var factory = new RoboticsFactory();
            factory.SetEngineersCount(8); // Максимальная эффективность

            var initialComponents = factory.GetComponentStorage();
            var initialMicrochips = initialComponents[RoboticsFactory.RoboticsComponent.Microchips];
            var initialAluminum = initialComponents[RoboticsFactory.RoboticsComponent.Aluminum];
            var initialCopper = initialComponents[RoboticsFactory.RoboticsComponent.CopperWiring];

            // Запуск производства
            factory.ProcessAssemblyLines();

            var finalComponents = factory.GetComponentStorage();
            var finalRobots = factory.GetProductionOutput();

            // Компоненты должны быть израсходованы
            Assert.IsTrue(finalComponents[RoboticsFactory.RoboticsComponent.Microchips] < initialMicrochips);
            Assert.IsTrue(finalComponents[RoboticsFactory.RoboticsComponent.Aluminum] < initialAluminum);
            Assert.IsTrue(finalComponents[RoboticsFactory.RoboticsComponent.CopperWiring] < initialCopper);

            // Должны быть произведены роботы
            Assert.IsTrue(finalRobots.Count > 0);
            Assert.IsTrue(finalRobots.Values.Sum() > 0);
        }

        /// <summary>
        /// Тест эффективности производства
        /// </summary>
        [TestMethod]
        public void TestProductionEfficiency()
        {
            var factory = new RoboticsFactory();

            // Проверка эффективности при разном количестве инженеров
            factory.SetEngineersCount(0);
            Assert.AreEqual(0f, factory.ProductionEfficiency);

            factory.SetEngineersCount(4);
            Assert.AreEqual(System.Math.Round(0.65f, 3), System.Math.Round(factory.ProductionEfficiency, 3)); // 0.4 + (4/8)*0.4 + 1*0.05 = 0.65

            factory.SetEngineersCount(8);
            Assert.AreEqual(0.85f, factory.ProductionEfficiency); // 0.4 + (8/8)*0.4 + 1*0.05 = 0.85
        }

        [TestMethod]
        public void TestInsufficientRobotShipping()
        {
            var factory = new RoboticsFactory();
            factory.SetEngineersCount(8);
            factory.ProcessAssemblyLines();

            var robots = factory.GetProductionOutput();

            if (robots.Count > 0)
            {
                var robotType = robots.Keys.First();

                // Попытка отгрузить больше, чем есть
                bool notShipped = factory.ShipRobot(robotType, 10000);
                Assert.IsFalse(notShipped);
            }
        }

        /// <summary>
        /// Тест получения информации о производстве
        /// </summary>
        [TestMethod]
        public void TestGetProductionInfo()
        {
            var factory = new RoboticsFactory();
            factory.SetEngineersCount(6);

            var info = factory.GetProductionInfo();

            Assert.IsNotNull(info);
            Assert.AreEqual(6, info["EngineersCount"]);
            Assert.AreEqual(8, info["MaxEngineers"]);
            Assert.IsTrue((float)info["ProductionEfficiency"] > 0);
            Assert.AreEqual(1, info["ResearchLevel"]);
            Assert.AreEqual(0m, info["ResearchBudget"]);
            Assert.IsTrue((int)info["TotalComponentStorage"] > 0);
            Assert.AreEqual(800, info["MaxComponentStorage"]);
            Assert.AreEqual(50, info["MaxRobotStorage"]);
            Assert.AreEqual(3, info["ActiveAssemblyLines"]);
        }

        /// <summary>
        /// Тест полного производственного цикла
        /// </summary>
        [TestMethod]
        public void TestFullProductionCycle()
        {
            var factory = new RoboticsFactory();
            factory.SetEngineersCount(8);

            var initialRobots = factory.GetProductionOutput().Values.Sum();

            factory.FullProductionCycle();

            var finalRobots = factory.GetProductionOutput().Values.Sum();
            Assert.IsTrue(finalRobots > initialRobots);
        }

        /// <summary>
        /// Тест размещения здания
        /// </summary>
        [TestMethod]
        public void TestOnBuildingPlaced()
        {
            var factory = new RoboticsFactory();
            factory.SetEngineersCount(8);

            var initialRobots = factory.GetProductionOutput().Values.Sum();

            factory.OnBuildingPlaced();

            var finalRobots = factory.GetProductionOutput().Values.Sum();
            Assert.IsTrue(finalRobots > initialRobots);
        }

        /// <summary>
        /// Тест ограничения вместимости хранилища компонентов
        /// </summary>
        [TestMethod]
        public void TestComponentStorageCapacity()
        {
            var factory = new RoboticsFactory();

            // Вычисляем доступное место
            int availableSpace = factory.MaxComponentStorage - factory.GetTotalComponentStorage();

            // Добавляем компоненты до полного заполнения
            bool added = factory.AddComponent(RoboticsFactory.RoboticsComponent.Aluminum, availableSpace);
            Assert.IsTrue(added);
            Assert.AreEqual(factory.MaxComponentStorage, factory.GetTotalComponentStorage());

            // Попытка добавить еще должно вернуть false
            bool notAdded = factory.AddComponent(RoboticsFactory.RoboticsComponent.Microchips, 1);
            Assert.IsFalse(notAdded);
            Assert.AreEqual(factory.MaxComponentStorage, factory.GetTotalComponentStorage());
        }

        /// <summary>
        /// Тест ограничения вместимости хранилища роботов
        /// </summary>
        [TestMethod]
        public void TestRobotStorageCapacity()
        {
            var factory = new RoboticsFactory();
            factory.SetEngineersCount(8);

            // Выполняем много циклов для заполнения склада
            for (int i = 0; i < 20; i++)
            {
                factory.ProcessAssemblyLines();
            }

            // Роботы не должны превысить максимальную вместимость
            Assert.IsTrue(factory.GetTotalRobotStorage() <= factory.MaxRobotStorage);
        }

        /// <summary>
        /// Тест инициализации линий сборки
        /// </summary>
        [TestMethod]
        public void TestAssemblyLinesInitialization()
        {
            var factory = new RoboticsFactory();

            Assert.AreEqual(3, factory.AssemblyLines.Count);

            // Проверка линии промышленных роботов
            var industrialLine = factory.AssemblyLines[0];
            Assert.AreEqual("Линия промышленных роботов", industrialLine.Name);
            Assert.AreEqual(24, industrialLine.ProductionCycleTime);
            Assert.AreEqual(5, industrialLine.InputRequirements.Count);
            Assert.AreEqual(2, industrialLine.OutputProducts.Count);

            // Проверка линии специализированных роботов
            var specializedLine = factory.AssemblyLines[1];
            Assert.AreEqual("Линия специализированных роботов", specializedLine.Name);
            Assert.AreEqual(32, specializedLine.ProductionCycleTime);
            Assert.AreEqual(5, specializedLine.InputRequirements.Count);
            Assert.AreEqual(3, specializedLine.OutputProducts.Count);

            // Проверка линии продвинутых роботов
            var advancedLine = factory.AssemblyLines[2];
            Assert.AreEqual("Линия продвинутых роботов", advancedLine.Name);
            Assert.AreEqual(48, advancedLine.ProductionCycleTime);
            Assert.AreEqual(5, advancedLine.InputRequirements.Count);
            Assert.AreEqual(3, advancedLine.OutputProducts.Count);
        }

        [TestMethod]
        public void TestGetTotalRobotStorage()
        {
            var factory = new RoboticsFactory();
            factory.SetEngineersCount(8);
            factory.ProcessAssemblyLines();

            int total = factory.GetTotalRobotStorage();

            Assert.IsTrue(total >= 0);
            Assert.IsTrue(total <= factory.MaxRobotStorage);
        }

        /// <summary>
        /// Тест конфигурации компонентов и типов роботов
        /// </summary>
        [TestMethod]
        public void TestComponentAndRobotConfiguration()
        {
            // Проверка enum компонентов
            Assert.AreEqual(0, (int)RoboticsFactory.RoboticsComponent.Microchips);
            Assert.AreEqual(1, (int)RoboticsFactory.RoboticsComponent.Actuators);
            Assert.AreEqual(2, (int)RoboticsFactory.RoboticsComponent.Sensors);
            Assert.AreEqual(3, (int)RoboticsFactory.RoboticsComponent.Aluminum);
            Assert.AreEqual(4, (int)RoboticsFactory.RoboticsComponent.CopperWiring);

            // Проверка enum типов роботов
            Assert.AreEqual(0, (int)RoboticsFactory.RobotType.AssemblyRobot);
            Assert.AreEqual(1, (int)RoboticsFactory.RobotType.WeldingRobot);
            Assert.AreEqual(2, (int)RoboticsFactory.RobotType.PaintingRobot);
            Assert.AreEqual(3, (int)RoboticsFactory.RobotType.PackagingRobot);
            Assert.AreEqual(4, (int)RoboticsFactory.RobotType.InspectionRobot);
            Assert.AreEqual(5, (int)RoboticsFactory.RobotType.LogisticsRobot);
            Assert.AreEqual(6, (int)RoboticsFactory.RobotType.MedicalRobot);
            Assert.AreEqual(7, (int)RoboticsFactory.RobotType.ResearchRobot);
        }

        /// <summary>
        /// Тест производства промышленных роботов
        /// </summary>
        [TestMethod]
        public void TestIndustrialRobotsProduction()
        {
            var factory = new RoboticsFactory();
            factory.SetEngineersCount(8);

            // Запускаем производство
            factory.ProcessAssemblyLines();

            var robots = factory.GetProductionOutput();

            // Должны производиться промышленные роботы
            Assert.IsTrue(robots.ContainsKey(RoboticsFactory.RobotType.AssemblyRobot) ||
                         robots.ContainsKey(RoboticsFactory.RobotType.WeldingRobot));
        }

        /// <summary>
        /// Тест многоступенчатого производства
        /// </summary>
        [TestMethod]
        public void TestMultiStageProduction()
        {
            var factory = new RoboticsFactory();
            factory.SetEngineersCount(8);

            // Запускаем несколько циклов производства
            for (int i = 0; i < 3; i++)
            {
                factory.ProcessAssemblyLines();
            }

            var robots = factory.GetProductionOutput();

            // Должны производиться роботы разных категорий
            bool hasIndustrial = robots.ContainsKey(RoboticsFactory.RobotType.AssemblyRobot) ||
                               robots.ContainsKey(RoboticsFactory.RobotType.WeldingRobot);
            bool hasSpecialized = robots.ContainsKey(RoboticsFactory.RobotType.PaintingRobot) ||
                                robots.ContainsKey(RoboticsFactory.RobotType.PackagingRobot);
            bool hasAdvanced = robots.ContainsKey(RoboticsFactory.RobotType.LogisticsRobot) ||
                             robots.ContainsKey(RoboticsFactory.RobotType.MedicalRobot);

            Assert.IsTrue(hasIndustrial || hasSpecialized || hasAdvanced);
        }

        /// <summary>
        /// Тест последовательного производства
        /// </summary>
        [TestMethod]
        public void TestSequentialProduction()
        {
            var factory = new RoboticsFactory();
            factory.SetEngineersCount(8);

            var robotsAfterFirstCycle = factory.GetProductionOutput().Values.Sum();
            factory.ProcessAssemblyLines();
            var robotsAfterSecondCycle = factory.GetProductionOutput().Values.Sum();

            // После второго цикла должно быть больше роботов
            Assert.IsTrue(robotsAfterSecondCycle > robotsAfterFirstCycle);
        }

        /// <summary>
        /// Тест производства всех типов роботов
        /// </summary>
        [TestMethod]
        public void TestAllRobotTypesProduction()
        {
            var factory = new RoboticsFactory();
            factory.SetEngineersCount(8);

            // Запускаем несколько циклов для производства всех типов роботов
            for (int i = 0; i < 10; i++)
            {
                factory.ProcessAssemblyLines();
            }

            var robots = factory.GetProductionOutput();

            // Должны производиться различные типы роботов
            Assert.IsTrue(robots.Count >= 4); // Как минимум 4 разных типа роботов
        }

        /// <summary>
        /// Тест эффективности при неполной загрузке инженерами
        /// </summary>
        [TestMethod]
        public void TestEfficiencyWithPartialWorkforce()
        {
            var factory = new RoboticsFactory();
            factory.SetEngineersCount(2); // 25% от максимальной workforce

            var initialComponents = factory.GetComponentStorage();
            factory.ProcessAssemblyLines();

            var finalComponents = factory.GetComponentStorage();
            var finalRobots = factory.GetProductionOutput();

            // При неполной загрузке все равно должно происходить производство, но с меньшей эффективностью
            Assert.IsTrue(finalComponents[RoboticsFactory.RoboticsComponent.Microchips] < initialComponents[RoboticsFactory.RoboticsComponent.Microchips]);
            Assert.IsTrue(finalRobots.Values.Sum() > 0);
        }

        /// <summary>
        /// Тест исследований и улучшений
        /// </summary>
        [TestMethod]
        public void TestResearchUpgrades()
        {
            var factory = new RoboticsFactory();

            // Изначально уровень исследований 1
            Assert.AreEqual(1, factory.ResearchLevel);

            // TODO: После реализации системы финансов добавить тесты на успешное улучшение
            // bool upgraded = factory.UpgradeResearch();
            // Assert.IsTrue(upgraded);
            // Assert.AreEqual(2, factory.ResearchLevel);
        }

        /// <summary>
        /// Тест бюджета исследований
        /// </summary>
        [TestMethod]
        public void TestResearchBudget()
        {
            var factory = new RoboticsFactory();

            // Установка бюджета исследований
            factory.SetResearchBudget(500000m);

            // Проверка через GetProductionInfo()
            var info = factory.GetProductionInfo();
            Assert.AreEqual(500000m, info["ResearchBudget"]);
        }

        /// <summary>
        /// Тест стоимости обслуживания
        /// </summary>
        [TestMethod]
        public void TestMaintenanceCost()
        {
            var factory = new RoboticsFactory();
            factory.SetEngineersCount(6);

            decimal cost = factory.CalculateMaintenanceCost();

            // Базовая стоимость + инженеры + исследования
            // 15000 + 6*2500 + 1*5000 = 15000 + 15000 + 5000 = 35000
            Assert.AreEqual(35000m, cost);
        }

        /// <summary>
        /// Тест отгрузки всех типов роботов
        /// </summary>
        [TestMethod]
        public void TestShipAllRobots()
        {
            var factory = new RoboticsFactory();
            factory.SetEngineersCount(8);
            factory.ProcessAssemblyLines();

            var robots = factory.GetProductionOutput();

            // Отгружаем по одному роботу каждого типа
            foreach (var robot in robots)
            {
                bool shipped = factory.ShipRobot(robot.Key, 1);
                Assert.IsTrue(shipped, $"Не удалось отгрузить робота {robot.Key}");
            }
        }

        /// <summary>
        /// Тест полного цикла производства и отгрузки
        /// </summary>
        [TestMethod]
        public void TestFullProductionAndShippingCycle()
        {
            var factory = new RoboticsFactory();
            factory.SetEngineersCount(8);

            // Производим роботов
            factory.FullProductionCycle();
            var robotsBefore = factory.GetProductionOutput();
            int totalBefore = factory.GetTotalRobotStorage();

            // Отгружаем часть роботов
            if (robotsBefore.Count > 0)
            {
                var firstRobot = robotsBefore.Keys.First();
                int amountToShip = System.Math.Min(3, robotsBefore[firstRobot]);

                bool shipped = factory.ShipRobot(firstRobot, amountToShip);
                Assert.IsTrue(shipped);

                var robotsAfter = factory.GetProductionOutput();
                int totalAfter = factory.GetTotalRobotStorage();

                Assert.AreEqual(totalBefore - amountToShip, totalAfter);
            }
        }

        /// <summary>
        /// Тест сложной производственной цепочки роботов
        /// </summary>
        [TestMethod]
        public void TestComplexRobotProductionChain()
        {
            var factory = new RoboticsFactory();
            factory.SetEngineersCount(8);

            // Запускаем несколько циклов для полной производственной цепочки
            for (int i = 0; i < 8; i++)
            {
                factory.ProcessAssemblyLines();
            }

            var robots = factory.GetProductionOutput();

            // Проверяем наличие роботов разных категорий
            bool hasIndustrial = robots.ContainsKey(RoboticsFactory.RobotType.AssemblyRobot) ||
                               robots.ContainsKey(RoboticsFactory.RobotType.WeldingRobot);
            bool hasSpecialized = robots.ContainsKey(RoboticsFactory.RobotType.PaintingRobot) ||
                                robots.ContainsKey(RoboticsFactory.RobotType.PackagingRobot) ||
                                robots.ContainsKey(RoboticsFactory.RobotType.InspectionRobot);
            bool hasAdvanced = robots.ContainsKey(RoboticsFactory.RobotType.LogisticsRobot) ||
                             robots.ContainsKey(RoboticsFactory.RobotType.MedicalRobot) ||
                             robots.ContainsKey(RoboticsFactory.RobotType.ResearchRobot);

            // Должны присутствовать роботы разных категорий
            Assert.IsTrue(hasIndustrial || hasSpecialized || hasAdvanced);
        }

        /// <summary>
        /// Тест специализированных роботов
        /// </summary>
        [TestMethod]
        public void TestSpecializedRobots()
        {
            var factory = new RoboticsFactory();
            factory.SetEngineersCount(8);

            // Запускаем производство
            for (int i = 0; i < 6; i++)
            {
                factory.ProcessAssemblyLines();
            }

            var robots = factory.GetProductionOutput();

            // Проверяем производство специализированных роботов
            bool hasMedical = robots.ContainsKey(RoboticsFactory.RobotType.MedicalRobot);
            bool hasResearch = robots.ContainsKey(RoboticsFactory.RobotType.ResearchRobot);
            bool hasLogistics = robots.ContainsKey(RoboticsFactory.RobotType.LogisticsRobot);

            // Должны производиться различные специализированные роботы
            Assert.IsTrue(hasMedical || hasResearch || hasLogistics);
        }
    }
}