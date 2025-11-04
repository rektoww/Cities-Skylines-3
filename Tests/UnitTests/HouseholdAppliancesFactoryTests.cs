using Core.Enums;
using Core.Models.Buildings.CommertialBuildings;
using Core.Models.Buildings.IndustrialBuildings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Tests.UnitTests
{
    /// <summary>
    /// Тесты для фабрики бытовой техники
    /// </summary>
    [TestClass]
    public sealed class HouseholdAppliancesFactoryTests
    {
        private HouseholdAppliancesFactory _factory;

        [TestInitialize]
        public void TestInitialize()
        {
            _factory = new HouseholdAppliancesFactory();
        }

        /// <summary>
        /// Тест создания фабрики бытовой техники
        /// </summary>
        [TestMethod]
        public void TestHouseholdAppliancesFactoryCreation()
        {
            // Assert
            Assert.AreEqual(2000, _factory.MaxComponentsStorage);
            Assert.AreEqual(500, _factory.MaxProductsStorage);
            Assert.AreEqual(20, _factory.MaxWorkers);
            Assert.AreEqual(0, _factory.WorkersCount);
            Assert.AreEqual(5, _factory.AssemblyLines.Count);
            Assert.AreEqual(10, _factory.ProductCategories.Count);
            Assert.AreEqual(10, _factory.ProductPrices.Count);
            Assert.AreEqual(0.5f, _factory.AutomationLevel);
        }

        /// <summary>
        /// Тест статических свойств строительства
        /// </summary>
        [TestMethod]
        public void TestStaticProperties()
        {
            // Assert
            Assert.AreEqual(400000m, HouseholdAppliancesFactory.BuildCost);
            Assert.AreEqual(8, HouseholdAppliancesFactory.RequiredMaterials[ConstructionMaterial.Steel]);
            Assert.AreEqual(12, HouseholdAppliancesFactory.RequiredMaterials[ConstructionMaterial.Concrete]);
            Assert.AreEqual(6, HouseholdAppliancesFactory.RequiredMaterials[ConstructionMaterial.Glass]);
            Assert.AreEqual(10, HouseholdAppliancesFactory.RequiredMaterials[ConstructionMaterial.Plastic]);
        }

        /// <summary>
        /// Тест инициализации стартовых комплектующих
        /// </summary>
        [TestMethod]
        public void TestStartingComponentsInitialization()
        {
            // Act
            var components = _factory.GetComponentsStorage();

            // Assert
            Assert.AreEqual(300, components[HouseholdAppliancesFactory.ApplianceComponent.SteelSheets]);
            Assert.AreEqual(250, components[HouseholdAppliancesFactory.ApplianceComponent.PlasticParts]);
            Assert.AreEqual(50, components[HouseholdAppliancesFactory.ApplianceComponent.ElectricMotors]);
            Assert.AreEqual(30, components[HouseholdAppliancesFactory.ApplianceComponent.Compressors]);
            Assert.AreEqual(80, components[HouseholdAppliancesFactory.ApplianceComponent.ElectronicBoards]);
            Assert.AreEqual(60, components[HouseholdAppliancesFactory.ApplianceComponent.GlassPanels]);
            Assert.AreEqual(40, components[HouseholdAppliancesFactory.ApplianceComponent.CopperTubes]);
            Assert.AreEqual(70, components[HouseholdAppliancesFactory.ApplianceComponent.InsulationMaterials]);
        }

        /// <summary>
        /// Тест управления рабочими
        /// </summary>
        [TestMethod]
        public void TestWorkerManagement()
        {
            // Act & Assert
            _factory.SetWorkersCount(10);
            Assert.AreEqual(10, _factory.WorkersCount);

            _factory.SetWorkersCount(25); // Превышение максимума
            Assert.AreEqual(20, _factory.WorkersCount);

            _factory.SetWorkersCount(0);
            Assert.AreEqual(0, _factory.WorkersCount);
        }

        /// <summary>
        /// Тест управления автоматизацией
        /// </summary>
        [TestMethod]
        public void TestAutomationManagement()
        {
            // Act & Assert
            _factory.SetAutomationLevel(0.8f);
            Assert.AreEqual(0.8f, _factory.AutomationLevel);

            _factory.SetAutomationLevel(1.5f); // Превышение максимума
            Assert.AreEqual(1.0f, _factory.AutomationLevel);

            _factory.SetAutomationLevel(-0.5f); // Ниже минимума
            Assert.AreEqual(0.0f, _factory.AutomationLevel);
        }

        /// <summary>
        /// Тест добавления комплектующих
        /// </summary>
        [TestMethod]
        public void TestAddComponents()
        {
            // Act
            bool addedSteel = _factory.AddComponent(HouseholdAppliancesFactory.ApplianceComponent.SteelSheets, 50);
            bool addedPlastic = _factory.AddComponent(HouseholdAppliancesFactory.ApplianceComponent.PlasticParts, 30);

            // Assert
            Assert.IsTrue(addedSteel);
            Assert.IsTrue(addedPlastic);

            var components = _factory.GetComponentsStorage();
            Assert.AreEqual(350, components[HouseholdAppliancesFactory.ApplianceComponent.SteelSheets]); // 300 + 50
            Assert.AreEqual(280, components[HouseholdAppliancesFactory.ApplianceComponent.PlasticParts]); // 250 + 30
        }

        /// <summary>
        /// Тест добавления комплектующих с превышением лимита
        /// </summary>
        [TestMethod]
        public void TestAddComponentsExceedingCapacity()
        {
            // Act - пытаемся добавить больше чем вместимость
            bool notAdded = _factory.AddComponent(HouseholdAppliancesFactory.ApplianceComponent.SteelSheets, 1800);

            // Assert
            Assert.IsFalse(notAdded);
            Assert.AreEqual(300, _factory.GetComponentsStorage()[HouseholdAppliancesFactory.ApplianceComponent.SteelSheets]);
        }

        /// <summary>
        /// Тест эффективности сборки
        /// </summary>
        [TestMethod]
        public void TestAssemblyEfficiency()
        {
            // Act & Assert
            _factory.SetWorkersCount(0);
            Assert.AreEqual(0f, _factory.AssemblyEfficiency);

            _factory.SetWorkersCount(10);
            Assert.IsTrue(_factory.AssemblyEfficiency > 0.6f); // 0.3 + (10/20)*0.7 = 0.65

            _factory.SetWorkersCount(20);
            Assert.AreEqual(1.0f, _factory.AssemblyEfficiency); // 0.3 + (20/20)*0.7 = 1.0
        }

        /// <summary>
        /// Тест сборки без рабочих
        /// </summary>
        [TestMethod]
        public void TestAssemblyWithoutWorkers()
        {
            // Arrange
            var initialComponents = _factory.GetComponentsStorage();
            var initialProducts = _factory.GetProductionOutput();

            // Act
            _factory.RunAssemblyLines();

            // Assert - ничего не должно измениться
            var finalComponents = _factory.GetComponentsStorage();
            var finalProducts = _factory.GetProductionOutput();

            Assert.AreEqual(initialComponents[HouseholdAppliancesFactory.ApplianceComponent.SteelSheets],
                          finalComponents[HouseholdAppliancesFactory.ApplianceComponent.SteelSheets]);
            Assert.AreEqual(initialComponents[HouseholdAppliancesFactory.ApplianceComponent.PlasticParts],
                          finalComponents[HouseholdAppliancesFactory.ApplianceComponent.PlasticParts]);
            Assert.AreEqual(initialProducts.Count, finalProducts.Count);
        }

        /// <summary>
        /// Тест сборки с рабочими
        /// </summary>
        [TestMethod]
        public void TestAssemblyWithWorkers()
        {
            // Arrange
            _factory.SetWorkersCount(20); // Максимальная эффективность
            var initialComponents = _factory.GetComponentsStorage();
            var initialSteel = initialComponents[HouseholdAppliancesFactory.ApplianceComponent.SteelSheets];
            var initialPlastic = initialComponents[HouseholdAppliancesFactory.ApplianceComponent.PlasticParts];

            // Act
            _factory.RunAssemblyLines();

            // Assert
            var finalComponents = _factory.GetComponentsStorage();
            var finalProducts = _factory.GetProductionOutput();

            // Комплектующие должны быть израсходованы
            Assert.IsTrue(finalComponents[HouseholdAppliancesFactory.ApplianceComponent.SteelSheets] < initialSteel);
            Assert.IsTrue(finalComponents[HouseholdAppliancesFactory.ApplianceComponent.PlasticParts] < initialPlastic);

            // Должна быть произведена техника
            Assert.IsTrue(finalProducts.Count > 0);
            Assert.IsTrue(finalProducts.Values.Sum() > 0);
        }

        /// <summary>
        /// Тест продажи продукции
        /// </summary>
        [TestMethod]
        public void TestProductSelling()
        {
            // Arrange
            _factory.SetWorkersCount(20);
            _factory.RunAssemblyLines();
            var initialProducts = _factory.GetProductionOutput();

            if (initialProducts.Count > 0)
            {
                var product = initialProducts.First();
                var productType = product.Key;
                var initialAmount = product.Value;

                // Act
                bool sold = _factory.SellProduct(productType, 1);

                // Assert
                Assert.IsTrue(sold);
                var finalProducts = _factory.GetProductionOutput();
                Assert.AreEqual(initialAmount - 1, finalProducts[productType]);
            }
        }

        /// <summary>
        /// Тест продажи недостающего количества продукции
        /// </summary>
        [TestMethod]
        public void TestInsufficientProductSelling()
        {
            // Arrange
            _factory.SetWorkersCount(20);
            _factory.RunAssemblyLines();

            // Act - пытаемся продать несуществующий продукт в большом количестве
            bool notSold = _factory.SellProduct(HouseholdAppliancesFactory.ApplianceProduct.Refrigerator, 1000);

            // Assert
            Assert.IsFalse(notSold);
        }

        /// <summary>
        /// Тест получения продуктов по категории
        /// </summary>
        [TestMethod]
        public void TestGetProductsByCategory()
        {
            // Arrange
            _factory.SetWorkersCount(20);
            _factory.RunAssemblyLines();

            // Act
            var kitchenProducts = _factory.GetProductsByCategory(HouseholdAppliancesFactory.ApplianceCategory.Kitchen);
            var refrigerationProducts = _factory.GetProductsByCategory(HouseholdAppliancesFactory.ApplianceCategory.Refrigeration);

            // Assert
            Assert.IsNotNull(kitchenProducts);
            Assert.IsNotNull(refrigerationProducts);
        }

        /// <summary>
        /// Тест расчета стоимости инвентаря
        /// </summary>
        [TestMethod]
        public void TestCalculateInventoryValue()
        {
            // Arrange
            _factory.SetWorkersCount(20);
            _factory.RunAssemblyLines();

            // Act
            decimal inventoryValue = _factory.CalculateInventoryValue();

            // Assert
            Assert.IsTrue(inventoryValue >= 0);
        }

        /// <summary>
        /// Тест получения статистики по категориям
        /// </summary>
        [TestMethod]
        public void TestGetCategoryStatistics()
        {
            // Arrange
            _factory.SetWorkersCount(20);
            _factory.RunAssemblyLines();

            // Act
            var statistics = _factory.GetCategoryStatistics();

            // Assert
            Assert.AreEqual(5, statistics.Count); // 5 категорий
            Assert.IsTrue(statistics.ContainsKey(HouseholdAppliancesFactory.ApplianceCategory.Kitchen));
            Assert.IsTrue(statistics.ContainsKey(HouseholdAppliancesFactory.ApplianceCategory.Refrigeration));
            Assert.IsTrue(statistics.ContainsKey(HouseholdAppliancesFactory.ApplianceCategory.Laundry));
            Assert.IsTrue(statistics.ContainsKey(HouseholdAppliancesFactory.ApplianceCategory.Climate));
            Assert.IsTrue(statistics.ContainsKey(HouseholdAppliancesFactory.ApplianceCategory.SmallAppliances));
        }

        /// <summary>
        /// Тест получения информации о производстве
        /// </summary>
        [TestMethod]
        public void TestGetProductionInfo()
        {
            // Arrange
            _factory.SetWorkersCount(15);
            _factory.SetAutomationLevel(0.7f);

            // Act
            var info = _factory.GetProductionInfo();

            // Assert
            Assert.IsNotNull(info);
            Assert.AreEqual(15, info["WorkersCount"]);
            Assert.AreEqual(20, info["MaxWorkers"]);
            Assert.IsTrue((float)info["AssemblyEfficiency"] > 0);
            Assert.AreEqual(0.7f, info["AutomationLevel"]);
            Assert.AreEqual(2000, info["MaxComponentsStorage"]);
            Assert.AreEqual(500, info["MaxProductsStorage"]);
            Assert.AreEqual(5, info["ActiveAssemblyLines"]);
            Assert.IsTrue((decimal)info["InventoryValue"] >= 0);
        }

        /// <summary>
        /// Тест получения наиболее прибыльных продуктов
        /// </summary>
        [TestMethod]
        public void TestGetMostProfitableProducts()
        {
            // Act
            var profitableProducts = _factory.GetMostProfitableProducts();

            // Assert
            Assert.AreEqual(10, profitableProducts.Count);
            // Холодильник должен быть самым дорогим
            Assert.AreEqual(HouseholdAppliancesFactory.ApplianceProduct.Refrigerator, profitableProducts[0].Key);
            Assert.AreEqual(25000m, profitableProducts[0].Value);
        }

        /// <summary>
        /// Тест инициализации категорий продукции
        /// </summary>
        [TestMethod]
        public void TestProductCategoriesInitialization()
        {
            // Assert
            Assert.AreEqual(10, _factory.ProductCategories.Count);

            // Проверка категорий для конкретных продуктов
            Assert.AreEqual(HouseholdAppliancesFactory.ApplianceCategory.Refrigeration,
                          _factory.ProductCategories[HouseholdAppliancesFactory.ApplianceProduct.Refrigerator]);
            Assert.AreEqual(HouseholdAppliancesFactory.ApplianceCategory.Laundry,
                          _factory.ProductCategories[HouseholdAppliancesFactory.ApplianceProduct.WashingMachine]);
            Assert.AreEqual(HouseholdAppliancesFactory.ApplianceCategory.Kitchen,
                          _factory.ProductCategories[HouseholdAppliancesFactory.ApplianceProduct.Oven]);
        }

        /// <summary>
        /// Тест инициализации цен на продукцию
        /// </summary>
        [TestMethod]
        public void TestProductPricesInitialization()
        {
            // Assert
            Assert.AreEqual(10, _factory.ProductPrices.Count);

            // Проверка цен для конкретных продуктов
            Assert.AreEqual(25000m, _factory.ProductPrices[HouseholdAppliancesFactory.ApplianceProduct.Refrigerator]);
            Assert.AreEqual(20000m, _factory.ProductPrices[HouseholdAppliancesFactory.ApplianceProduct.WashingMachine]);
            Assert.AreEqual(5000m, _factory.ProductPrices[HouseholdAppliancesFactory.ApplianceProduct.Microwave]);
            Assert.AreEqual(6000m, _factory.ProductPrices[HouseholdAppliancesFactory.ApplianceProduct.VacuumCleaner]);
        }

        /// <summary>
        /// Тест инициализации сборочных линий
        /// </summary>
        [TestMethod]
        public void TestAssemblyLinesInitialization()
        {
            // Assert
            Assert.AreEqual(5, _factory.AssemblyLines.Count);

            var lineNames = _factory.AssemblyLines.Select(w => w.Name).ToList();
            Assert.IsTrue(lineNames.Contains("Линия сборки холодильного оборудования"));
            Assert.IsTrue(lineNames.Contains("Линия сборки стирального оборудования"));
            Assert.IsTrue(lineNames.Contains("Линия сборки кухонной техники"));
            Assert.IsTrue(lineNames.Contains("Линия сборки климатической техники"));
            Assert.IsTrue(lineNames.Contains("Линия сборки мелкой бытовой техники"));
        }

        /// <summary>
        /// Тест полного производственного цикла
        /// </summary>
        [TestMethod]
        public void TestFullProductionCycle()
        {
            // Arrange
            _factory.SetWorkersCount(20);
            var initialProducts = _factory.GetProductionOutput().Values.Sum();

            // Act
            _factory.FullProductionCycle();

            // Assert
            var finalProducts = _factory.GetProductionOutput().Values.Sum();
            Assert.IsTrue(finalProducts > initialProducts);
        }

        /// <summary>
        /// Тест размещения здания
        /// </summary>
        [TestMethod]
        public void TestOnBuildingPlaced()
        {
            // Arrange
            _factory.SetWorkersCount(20);
            var initialProducts = _factory.GetProductionOutput().Values.Sum();

            // Act
            _factory.OnBuildingPlaced();

            // Assert
            var finalProducts = _factory.GetProductionOutput().Values.Sum();
            Assert.IsTrue(finalProducts > initialProducts);
        }

        /// <summary>
        /// Тест ограничения хранилища продукции
        /// </summary>
        [TestMethod]
        public void TestProductStorageCapacity()
        {
            // Arrange
            _factory.SetWorkersCount(20);

            // Act - многократный запуск сборки
            for (int i = 0; i < 15; i++)
            {
                _factory.RunAssemblyLines();
            }

            // Assert - продукция не должна превысить лимит
            Assert.IsTrue(_factory.GetTotalProductsStorage() <= _factory.MaxProductsStorage);
        }

        /// <summary>
        /// Тест влияния автоматизации на производство
        /// </summary>
        [TestMethod]
        public void TestAutomationImpact()
        {
            // Arrange
            _factory.SetWorkersCount(10);

            // Act с низкой автоматизацией
            _factory.SetAutomationLevel(0.2f);
            _factory.RunAssemblyLines();
            var productsLowAutomation = _factory.GetProductionOutput().Values.Sum();

            // Сбросим и проверим с высокой автоматизацией
            _factory = new HouseholdAppliancesFactory();
            _factory.SetWorkersCount(10);
            _factory.SetAutomationLevel(0.9f);
            _factory.RunAssemblyLines();
            var productsHighAutomation = _factory.GetProductionOutput().Values.Sum();

            // Assert - высокая автоматизация должна давать больше продукции
            Assert.IsTrue(productsHighAutomation >= productsLowAutomation);
        }

        /// <summary>
        /// Тест всех enum значений
        /// </summary>
        [TestMethod]
        public void TestAllEnumValues()
        {
            // Assert
            Assert.AreEqual(8, typeof(HouseholdAppliancesFactory.ApplianceComponent).GetEnumValues().Length);
            Assert.AreEqual(10, typeof(HouseholdAppliancesFactory.ApplianceProduct).GetEnumValues().Length);
            Assert.AreEqual(5, typeof(HouseholdAppliancesFactory.ApplianceCategory).GetEnumValues().Length);
        }

        /// <summary>
        /// Тест конфигурации сборочных линий
        /// </summary>
        [TestMethod]
        public void TestAssemblyLinesConfiguration()
        {
            // Arrange
            var refrigerationLine = _factory.AssemblyLines[0];

            // Assert
            Assert.AreEqual("Линия сборки холодильного оборудования", refrigerationLine.Name);
            Assert.AreEqual(12, refrigerationLine.ProductionCycleTime);
            Assert.AreEqual(5, refrigerationLine.InputRequirements.Count);
            Assert.AreEqual(2, refrigerationLine.OutputProducts.Count);

            // Проверяем что линия производит холодильники и морозильные камеры
            Assert.IsTrue(refrigerationLine.OutputProducts.ContainsKey("Refrigerator"));
            Assert.IsTrue(refrigerationLine.OutputProducts.ContainsKey("Freezer"));
        }

        /// <summary>
        /// Тест последовательной сборки
        /// </summary>
        [TestMethod]
        public void TestSequentialAssembly()
        {
            // Arrange
            _factory.SetWorkersCount(20);

            var productsAfterFirstRun = _factory.GetProductionOutput().Values.Sum();
            _factory.RunAssemblyLines();
            var productsAfterSecondRun = _factory.GetProductionOutput().Values.Sum();

            // Assert - после второго запуска должно быть больше продукции
            Assert.IsTrue(productsAfterSecondRun > productsAfterFirstRun);
        }

        /// <summary>
        /// Тест эффективности при различном количестве рабочих
        /// </summary>
        [TestMethod]
        public void TestEfficiencyWithDifferentWorkforce()
        {
            // Arrange & Act
            _factory.SetWorkersCount(5);
            float efficiencyLow = _factory.AssemblyEfficiency;

            _factory.SetWorkersCount(15);
            float efficiencyMedium = _factory.AssemblyEfficiency;

            _factory.SetWorkersCount(20);
            float efficiencyHigh = _factory.AssemblyEfficiency;

            // Assert - эффективность должна увеличиваться с количеством рабочих
            Assert.IsTrue(efficiencyLow < efficiencyMedium);
            Assert.IsTrue(efficiencyMedium < efficiencyHigh);
            Assert.AreEqual(1.0f, efficiencyHigh);
        }
    }
}