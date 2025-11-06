using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core.Enums;
using System.Linq;
using Core.Models.Buildings.IndustrialBuildings;

namespace Tests.UnitTests
{
    /// <summary>
    /// Тесты для химического завода
    /// </summary>
    [TestClass]
    public sealed class ChemicalPlantTest
    {
        /// <summary>
        /// Тест создания химического завода
        /// </summary>
        [TestMethod]
        public void TestChemicalPlantCreation()
        {
            var plant = new ChemicalPlant(ChemicalIndustryType.PetrochemicalPlant);

            // Проверка статических свойств строительства
            Assert.AreEqual(450000m, ChemicalPlant.BuildCost);
            Assert.AreEqual(4, ChemicalPlant.RequiredMaterials.Count);
            Assert.AreEqual(15, ChemicalPlant.RequiredMaterials[ConstructionMaterial.Steel]);
            Assert.AreEqual(12, ChemicalPlant.RequiredMaterials[ConstructionMaterial.Concrete]);
            Assert.AreEqual(8, ChemicalPlant.RequiredMaterials[ConstructionMaterial.Glass]);
            Assert.AreEqual(5, ChemicalPlant.RequiredMaterials[ConstructionMaterial.Plastic]);

            // Проверка базовых свойств
            Assert.AreEqual(ChemicalIndustryType.PetrochemicalPlant, plant.PlantType);
            Assert.AreEqual(2000, plant.MaxRawMaterialStorage);
            Assert.AreEqual(1500, plant.MaxProductStorage);
            Assert.AreEqual(20, plant.MaxChemists);
            Assert.AreEqual(0, plant.ChemistsCount);
            Assert.IsTrue(plant.ProductionUnits.Count > 0);
        }

        /// <summary>
        /// Тест инициализации стартовых материалов
        /// </summary>
        [TestMethod]
        public void TestStartingMaterialsInitialization()
        {
            var plant = new ChemicalPlant(ChemicalIndustryType.PetrochemicalPlant);

            var materials = plant.GetRawMaterials();

            Assert.AreEqual(500, materials[ChemicalMaterial.CrudeOil]);
            Assert.AreEqual(300, materials[ChemicalMaterial.NaturalGas]);
        }

        /// <summary>
        /// Тест управления химиками-технологами
        /// </summary>
        [TestMethod]
        public void TestChemistsManagement()
        {
            var plant = new ChemicalPlant(ChemicalIndustryType.PetrochemicalPlant);

            // Установка количества химиков
            plant.SetChemistsCount(15);
            Assert.AreEqual(15, plant.ChemistsCount);

            // Попытка установить больше химиков, чем максимум
            plant.SetChemistsCount(25);
            Assert.AreEqual(20, plant.ChemistsCount); // Должно ограничиться MaxChemists

            // Установка нуля химиков
            plant.SetChemistsCount(0);
            Assert.AreEqual(0, plant.ChemistsCount);
        }

        /// <summary>
        /// Тест добавления сырья
        /// </summary>
        [TestMethod]
        public void TestAddRawMaterials()
        {
            var plant = new ChemicalPlant(ChemicalIndustryType.PetrochemicalPlant);

            // Успешное добавление сырой нефти
            bool addedOil = plant.AddRawMaterial(ChemicalMaterial.CrudeOil, 200);
            Assert.IsTrue(addedOil);

            var materials = plant.GetRawMaterials();
            Assert.AreEqual(700, materials[ChemicalMaterial.CrudeOil]); // 500 стартовых + 200

            // Успешное добавление природного газа
            bool addedGas = plant.AddRawMaterial(ChemicalMaterial.NaturalGas, 150);
            Assert.IsTrue(addedGas);

            materials = plant.GetRawMaterials();
            Assert.AreEqual(450, materials[ChemicalMaterial.NaturalGas]); // 300 стартовых + 150
        }

        /// <summary>
        /// Тест добавления сырья с превышением вместимости
        /// </summary>
        [TestMethod]
        public void TestAddRawMaterialsExceedingCapacity()
        {
            var plant = new ChemicalPlant(ChemicalIndustryType.PetrochemicalPlant);

            // Попытка добавить больше, чем вмещает хранилище
            bool notAdded = plant.AddRawMaterial(ChemicalMaterial.CrudeOil, 2000);
            Assert.IsFalse(notAdded); // Должно вернуть false, так как 500 + 2000 > 2000

            var materials = plant.GetRawMaterials();
            Assert.AreEqual(500, materials[ChemicalMaterial.CrudeOil]); // Количество не изменилось
        }

        /// <summary>
        /// Тест производства без химиков
        /// </summary>
        [TestMethod]
        public void TestProductionWithoutChemists()
        {
            var plant = new ChemicalPlant(ChemicalIndustryType.PetrochemicalPlant);

            var initialMaterials = plant.GetRawMaterials();
            var initialProducts = plant.GetChemicalProducts();

            // Запуск производства без химиков
            plant.RunChemicalProcesses();

            var finalMaterials = plant.GetRawMaterials();
            var finalProducts = plant.GetChemicalProducts();

            // Количество сырья и продукции не должно измениться
            Assert.AreEqual(initialMaterials[ChemicalMaterial.CrudeOil],
                          finalMaterials[ChemicalMaterial.CrudeOil]);
            Assert.AreEqual(initialMaterials[ChemicalMaterial.NaturalGas],
                          finalMaterials[ChemicalMaterial.NaturalGas]);
            Assert.AreEqual(initialProducts.Count, finalProducts.Count);
        }

        /// <summary>
        /// Тест продажи химической продукции
        /// </summary>
        [TestMethod]
        public void TestProductSales()
        {
            var plant = new ChemicalPlant(ChemicalIndustryType.PetrochemicalPlant);
            plant.SetChemistsCount(20);
            plant.RunChemicalProcesses(); // Производим химикаты

            var initialProducts = plant.GetChemicalProducts();

            if (initialProducts.Count > 0)
            {
                var productType = initialProducts.Keys.First();
                var initialAmount = initialProducts[productType];

                // Успешная продажа
                bool sold = plant.SellProduct(productType, 1);
                Assert.IsTrue(sold);

                var finalProducts = plant.GetChemicalProducts();
                Assert.AreEqual(initialAmount - 1, finalProducts[productType]);
            }
        }

        /// <summary>
        /// Тест продажи избыточного количества продукции
        /// </summary>
        [TestMethod]
        public void TestInsufficientProductSales()
        {
            var plant = new ChemicalPlant(ChemicalIndustryType.PetrochemicalPlant);
            plant.SetChemistsCount(20);
            plant.RunChemicalProcesses();

            var products = plant.GetChemicalProducts();

            if (products.Count > 0)
            {
                var productType = products.Keys.First();

                // Попытка продать больше, чем произведено
                bool notSold = plant.SellProduct(productType, 2000);
                Assert.IsFalse(notSold);
            }
        }

        /// <summary>
        /// Тест улучшения системы безопасности
        /// </summary>
        [TestMethod]
        public void TestSafetySystemUpgrade()
        {
            var plant = new ChemicalPlant(ChemicalIndustryType.PetrochemicalPlant);

            var initialSafety = plant.SafetyLevel;
            var initialEnvironmentalSafety = plant.EnvironmentalSafety;

            // Улучшаем систему безопасности
            plant.UpgradeSafetySystem();

            Assert.IsTrue(plant.SafetyLevel > initialSafety);
            Assert.IsTrue(plant.EnvironmentalSafety > initialEnvironmentalSafety);
        }

        /// <summary>
        /// Тест получения информации о производстве
        /// </summary>
        [TestMethod]
        public void TestGetProductionInfo()
        {
            var plant = new ChemicalPlant(ChemicalIndustryType.PetrochemicalPlant);
            plant.SetChemistsCount(15);

            var info = plant.GetProductionInfo();

            Assert.IsNotNull(info);
            Assert.AreEqual(ChemicalIndustryType.PetrochemicalPlant, info["PlantType"]);
            Assert.AreEqual(15, info["ChemistsCount"]);
            Assert.AreEqual(20, info["MaxChemists"]);
            Assert.IsTrue((float)info["ProductionEfficiency"] > 0);
            Assert.IsTrue((float)info["SafetyLevel"] > 0);
            Assert.IsTrue((float)info["EnvironmentalSafety"] > 0);
            Assert.IsTrue((int)info["TotalRawMaterialStorage"] > 0);
            Assert.AreEqual(2000, info["MaxRawMaterialStorage"]);
            Assert.AreEqual(1500, info["MaxProductStorage"]);
            Assert.IsTrue((int)info["ActiveReactors"] > 0);
        }

        /// <summary>
        /// Тест размещения здания
        /// </summary>
        [TestMethod]
        public void TestOnBuildingPlaced()
        {
            var plant = new ChemicalPlant(ChemicalIndustryType.PetrochemicalPlant);
            plant.SetChemistsCount(20);

            var initialProducts = plant.GetChemicalProducts().Values.Sum();

            plant.OnBuildingPlaced();

            var finalProducts = plant.GetChemicalProducts().Values.Sum();
            Assert.IsTrue(finalProducts > initialProducts);
        }

        /// <summary>
        /// Тест ограничения хранения продукции
        /// </summary>
        [TestMethod]
        public void TestProductStorageCapacity()
        {
            var plant = new ChemicalPlant(ChemicalIndustryType.PetrochemicalPlant);
            plant.SetChemistsCount(20);

            // Выполняем много циклов для производства химикатов
            for (int i = 0; i < 10; i++)
            {
                plant.RunChemicalProcesses();
            }

            // Количество произведенной продукции не должно превысить максимальное потребление
            Assert.IsTrue(plant.GetTotalProductStorage() <= plant.MaxProductStorage);
        }

        /// <summary>
        /// Тест инициализации производственных установок
        /// </summary>
        [TestMethod]
        public void TestProductionUnitsInitialization()
        {
            var plant = new ChemicalPlant(ChemicalIndustryType.PetrochemicalPlant);

            Assert.AreEqual(1, plant.ProductionUnits.Count);

            // Проверка установки крекинга
            var crackingUnit = plant.ProductionUnits[0];
            Assert.AreEqual("Установка крекинга", crackingUnit.Name);
            Assert.AreEqual(ChemicalProcess.Cracking, crackingUnit.ProcessType);
            Assert.AreEqual(100, crackingUnit.Capacity);
            Assert.IsTrue(crackingUnit.InputMaterials.Contains(ChemicalMaterial.CrudeOil));
            Assert.IsTrue(crackingUnit.OutputProducts.Contains(ChemicalProduct.Ethylene));
            Assert.IsTrue(crackingUnit.OutputProducts.Contains(ChemicalProduct.Propylene));
        }

        /// <summary>
        /// Тест получения общего количества сырья
        /// </summary>
        [TestMethod]
        public void TestGetTotalRawMaterialStorage()
        {
            var plant = new ChemicalPlant(ChemicalIndustryType.PetrochemicalPlant);

            // Добавляем дополнительное сырье
            plant.AddRawMaterial(ChemicalMaterial.CrudeOil, 100);
            plant.AddRawMaterial(ChemicalMaterial.NaturalGas, 50);

            int total = plant.GetTotalRawMaterialStorage();

            // 500 (стартовая нефть) + 300 (стартовый газ) + 100 + 50 = 950
            Assert.AreEqual(950, total);
        }

        /// <summary>
        /// Тест получения общего количества продукции
        /// </summary>
        [TestMethod]
        public void TestGetTotalProductStorage()
        {
            var plant = new ChemicalPlant(ChemicalIndustryType.PetrochemicalPlant);
            plant.SetChemistsCount(20);
            plant.RunChemicalProcesses();

            int total = plant.GetTotalProductStorage();

            Assert.IsTrue(total >= 0);
            Assert.IsTrue(total <= plant.MaxProductStorage);
        }

        /// <summary>
        /// Тест конфигурации сырья и продукции
        /// </summary>
        [TestMethod]
        public void TestMaterialAndProductConfiguration()
        {
            // Проверка enum сырья
            Assert.AreEqual(0, (int)ChemicalMaterial.CrudeOil);
            Assert.AreEqual(1, (int)ChemicalMaterial.NaturalGas);

            // Проверка enum продукции
            Assert.AreEqual(0, (int)ChemicalProduct.Ammonia);
            Assert.AreEqual(1, (int)ChemicalProduct.Urea);

            // Проверка enum процессов
            Assert.AreEqual(0, (int)ChemicalProcess.Distillation);
            Assert.AreEqual(1, (int)ChemicalProcess.Cracking);
        }

        /// <summary>
        /// Тест производства при неполной загрузке химиками
        /// </summary>
        [TestMethod]
        public void TestProductionWithPartialChemists()
        {
            var plant = new ChemicalPlant(ChemicalIndustryType.PetrochemicalPlant);
            plant.SetChemistsCount(5); // 25% от максимальной workforce

            var initialMaterials = plant.GetRawMaterials();
            plant.RunChemicalProcesses();

            var finalMaterials = plant.GetRawMaterials();
            var finalProducts = plant.GetChemicalProducts();

            // При неполной загрузке все равно должно происходить производство, но с меньшей эффективностью
            Assert.IsTrue(finalMaterials[ChemicalMaterial.CrudeOil] < initialMaterials[ChemicalMaterial.CrudeOil]);
            Assert.IsTrue(finalProducts.Values.Sum() > 0);
        }

        /// <summary>
        /// Тест последовательного производства
        /// </summary>
        [TestMethod]
        public void TestSequentialProduction()
        {
            var plant = new ChemicalPlant(ChemicalIndustryType.PetrochemicalPlant);
            plant.SetChemistsCount(20);

            var productsAfterFirstCycle = plant.GetChemicalProducts().Values.Sum();
            plant.RunChemicalProcesses();
            var productsAfterSecondCycle = plant.GetChemicalProducts().Values.Sum();

            // После второго цикла должно быть больше продукции
            Assert.IsTrue(productsAfterSecondCycle > productsAfterFirstCycle);
        }

        /// <summary>
        /// Тест специализации химических заводов
        /// </summary>
        [TestMethod]
        public void TestPlantSpecialization()
        {
            // Тестируем разные типы химических заводов
            var petrochemicalPlant = new ChemicalPlant(ChemicalIndustryType.PetrochemicalPlant);
            var fertilizerPlant = new ChemicalPlant(ChemicalIndustryType.FertilizerPlant);
            var polymerPlant = new ChemicalPlant(ChemicalIndustryType.PolymerPlant);

            // Проверяем специализацию
            Assert.AreEqual(ChemicalIndustryType.PetrochemicalPlant, petrochemicalPlant.PlantType);
            Assert.AreEqual(ChemicalIndustryType.FertilizerPlant, fertilizerPlant.PlantType);
            Assert.AreEqual(ChemicalIndustryType.PolymerPlant, polymerPlant.PlantType);

            // Проверяем различные стартовые материалы
            Assert.AreEqual(500, petrochemicalPlant.GetRawMaterials()[ChemicalMaterial.CrudeOil]);
            Assert.AreEqual(400, fertilizerPlant.GetRawMaterials()[ChemicalMaterial.NaturalGas]);
            Assert.AreEqual(350, polymerPlant.GetRawMaterials()[ChemicalMaterial.Ethylene]);
        }
    }
}