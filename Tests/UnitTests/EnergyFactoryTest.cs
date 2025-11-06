using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core.Enums;
using System.Linq;
using Core.Models.Buildings.IndustrialBuildings;

namespace Tests.UnitTests
{
/// <summary>
/// Тесты для электростанции
/// </summary>   

[TestClass]
public sealed class EnergyFactoryTest
{
        /// <summary>
        /// Тест создания электростанции
        /// </summary>
        [TestMethod]
        public void TestEnergyFactoryCreation()
        {
            var factory = new EnergyFactory();

            // Проверка статических свойств строительства
            Assert.AreEqual(400000m, EnergyFactory.BuildCost);
            Assert.AreEqual(4, EnergyFactory.RequiredMaterials.Count);
            Assert.AreEqual(6, EnergyFactory.RequiredMaterials[ConstructionMaterial.Steel]);
            Assert.AreEqual(7, EnergyFactory.RequiredMaterials[ConstructionMaterial.Concrete]);
            Assert.AreEqual(3, EnergyFactory.RequiredMaterials[ConstructionMaterial.Glass]);
            Assert.AreEqual(2, EnergyFactory.RequiredMaterials[ConstructionMaterial.Plastic]);

            // Проверка базовых свойств
            Assert.AreEqual(1000, factory.MaxMaterialStorage);
            Assert.AreEqual(900, factory.MaxProductStorage);
            Assert.AreEqual(120, factory.MaxWorkers);
            Assert.AreEqual(0, factory.WorkersCount);
            Assert.AreEqual(2, factory.Workshops.Count);
        }

        /// <summary>
        /// Тест инициализации стартовых материалов
        /// </summary>
        [TestMethod]
        public void TestStartingMaterialsInitialization()
        {
            var factory = new EnergyFactory();

            var materials = factory.GetMaterialStorage();

            Assert.AreEqual(200, materials[EnergySource.Coal]);
            Assert.AreEqual(250, materials[EnergySource.Gas]);
        }

        /// <summary>
        /// Тест управления рабочими
        /// </summary>
        [TestMethod]
        public void TestWorkerManagement()
        {
            var factory = new EnergyFactory();

            // Установка количества рабочих
            factory.SetWorkersCount(100);
            Assert.AreEqual(100, factory.WorkersCount);

            // Попытка установить больше рабочих, чем максимум
            factory.SetWorkersCount(150);
            Assert.AreEqual(120, factory.WorkersCount); // Должно ограничиться MaxWorkers

            // Установка нуля рабочих
            factory.SetWorkersCount(0);
            Assert.AreEqual(0, factory.WorkersCount);
        }

        /// <summary>
        /// Тест добавления сырья
        /// </summary>
        [TestMethod]
        public void TestAddMaterials()
        {
            var factory = new EnergyFactory();

            // Успешное добавление угля
            bool addedCoal = factory.AddMaterial(EnergySource.Coal, 100);
            Assert.IsTrue(addedCoal);

            var materials = factory.GetMaterialStorage();
            Assert.AreEqual(300, materials[EnergySource.Coal]); // 200 стартовых + 100

            // Успешное добавление газа
            bool addedPlastic = factory.AddMaterial(EnergySource.Gas, 200);
            Assert.IsTrue(addedPlastic);

            materials = factory.GetMaterialStorage();
            Assert.AreEqual(450, materials[EnergySource.Gas]); // 250 стартовых + 200
        }

        /// <summary>
        /// Тест добавления сырья с превышением вместимости
        /// </summary>
        [TestMethod]
        public void TestAddMaterialsExceedingCapacity()
        {
            var factory = new EnergyFactory();

            // Попытка добавить больше, чем вмещает хранилище
            bool notAdded = factory.AddMaterial(EnergySource.Coal, 1000);
            Assert.IsFalse(notAdded); // Должно вернуть false, так как 300 + 1000 > 1000

            var materials = factory.GetMaterialStorage();
            Assert.AreEqual(200, materials[EnergySource.Coal]); // Количество не изменилось
        }

        /// <summary>
        /// Тест производства без рабочих
        /// </summary>
        [TestMethod]
        public void TestProductionWithoutWorkers()
        {
            var factory = new EnergyFactory();

            var initialMaterials = factory.GetMaterialStorage();
            var initialProducts = factory.GetConsumerOutput();

            // Запуск производства без рабочих
            factory.ProcessWorkshops();

            var finalMaterials = factory.GetMaterialStorage();
            var finalProducts = factory.GetConsumerOutput();

            // Количество топлива и объем потребления не должны измениться
            Assert.AreEqual(initialMaterials[EnergySource.Coal],
                          finalMaterials[EnergySource.Coal]);
            Assert.AreEqual(initialMaterials[EnergySource.Gas],
                          finalMaterials[EnergySource.Gas]);
            Assert.AreEqual(initialProducts.Count, finalProducts.Count);
        }

        /// <summary>
        /// Тест производства с рабочими
        /// </summary>
        [TestMethod]
        public void TestProductionWithWorkers()
        {
            var factory = new EnergyFactory();
            factory.SetWorkersCount(120); // Максимальная эффективность

            var initialMaterials = factory.GetMaterialStorage();
            var initialCoal = initialMaterials[EnergySource.Coal];
            var initialGas = initialMaterials[EnergySource.Gas];

            // Запуск производства
            factory.ProcessWorkshops();

            var finalMaterials = factory.GetMaterialStorage();
            var finalProducts = factory.GetConsumerOutput();

            // Материалы должны быть израсходованы
            Assert.IsTrue(finalMaterials[EnergySource.Coal] < initialCoal);
            Assert.IsTrue(finalMaterials[EnergySource.Gas] < initialGas);

            // Должна быть произведена энергия
            Assert.IsTrue(finalProducts.Count > 0);
            Assert.IsTrue(finalProducts.Values.Sum() > 0);
        }


        /// <summary>
        /// Тест потребления выработанной энергии
        /// </summary>
        [TestMethod]
        public void TestProductConsumption()
        {
            var factory = new EnergyFactory();
            factory.SetWorkersCount(120);
            factory.ProcessWorkshops(); // Производим энергию

            var initialProducts = factory.GetConsumerOutput();

            if (initialProducts.Count > 0)
            {
                var energyType = initialProducts.Keys.First();
                var initialAmount = initialProducts[energyType];

                // Успешное потребление
                bool consumed = factory.ConsumeProduct(energyType, 1);
                Assert.IsTrue(consumed);

                var finalProducts = factory.GetConsumerOutput();
                Assert.AreEqual(initialAmount - 1, finalProducts[energyType]);
            }
        }

        /// <summary>
        /// Тест потребления избыточного (сверх выработки) количества энергии
        /// </summary>
        [TestMethod]
        public void TestInsufficientProductConsumption()
        {
            var factory = new EnergyFactory();
            factory.SetWorkersCount(120);
            factory.ProcessWorkshops();

            var products = factory.GetConsumerOutput();

            if (products.Count > 0)
            {
                var energyType = products.Keys.First();

                // Попытка потребить больше, чем может выработать электростанция
                bool notConsumed = factory.ConsumeProduct(energyType, 2000);
                Assert.IsFalse(notConsumed);
            }
        }

        /// <summary>
        /// Тест получения информации о производстве
        /// </summary>
        [TestMethod]
        public void TestGetProductionInfo()
        {
            var factory = new EnergyFactory();
            factory.SetWorkersCount(100);

            var info = factory.GetProductionInfo();

            Assert.IsNotNull(info);
            Assert.AreEqual(100, info["WorkersCount"]);
            Assert.AreEqual(120, info["MaxWorkers"]);
            Assert.IsTrue((float)info["ProductionEfficiency"] > 0);
            Assert.IsTrue((int)info["TotalMaterialStorage"] > 0);
            Assert.AreEqual(1000, info["MaxMaterialStorage"]);
            Assert.AreEqual(900, info["MaxProductStorage"]);
            Assert.AreEqual(2, info["ActiveWorkshops"]);
        }

        /// <summary>
        /// Тест полного производственного цикла
        /// </summary>
        [TestMethod]
        public void TestFullProductionCycle()
        {
            var factory = new EnergyFactory();
            factory.SetWorkersCount(120);

            var initialProducts = factory.GetConsumerOutput().Values.Sum();

            factory.FullProductionCycle();

            var finalProducts = factory.GetConsumerOutput().Values.Sum();
            Assert.IsTrue(finalProducts > initialProducts);
        }

        /// <summary>
        /// Тест размещения здания
        /// </summary>
        [TestMethod]
        public void TestOnBuildingPlaced()
        {
            var factory = new EnergyFactory();
            factory.SetWorkersCount(120);

            var initialProducts = factory.GetConsumerOutput().Values.Sum();

            factory.OnBuildingPlaced();

            var finalProducts = factory.GetConsumerOutput().Values.Sum();
            Assert.IsTrue(finalProducts > initialProducts);
        }



        /// <summary>
        /// Тест ограничения потребления выработанной энергии
        /// </summary>
        [TestMethod]
        public void TestProductStorageCapacity()
        {
            var factory = new EnergyFactory();
            factory.SetWorkersCount(120);

            // Выполняем много циклов для выработки энергии
            for (int i = 0; i < 10; i++)
            {
                factory.ProcessWorkshops();
            }

            // Количество выработанной энергии не должна превысить максимальное потребление
            Assert.IsTrue(factory.GetTotalProductStorage() <= factory.MaxProductStorage);
        }

        /// <summary>
        /// Тест инициализации цехов
        /// </summary>
        [TestMethod]
        public void TestWorkshopsInitialization()
        {
            var factory = new EnergyFactory();

            Assert.AreEqual(2, factory.Workshops.Count);

            // Проверка цеха производства электроэнергии
            var electricityWorkshop = factory.Workshops[0];
            Assert.AreEqual("Цех производства электроэнергии", electricityWorkshop.Name);
            Assert.AreEqual(3, electricityWorkshop.ProductionCycleTime);
            Assert.AreEqual(2, electricityWorkshop.InputRequirements.Count);
            Assert.AreEqual(1, electricityWorkshop.OutputProducts.Count);

            // Проверка цеха производства горячей воды
            var hotwaterWorkshop = factory.Workshops[1];
            Assert.AreEqual("Цех производства горячей воды", hotwaterWorkshop.Name);
            Assert.AreEqual(5, hotwaterWorkshop.ProductionCycleTime);
            Assert.AreEqual(1, hotwaterWorkshop.InputRequirements.Count);
            Assert.AreEqual(1, hotwaterWorkshop.OutputProducts.Count);
        }

        /// <summary>
        /// Тест получения общего количества материалов
        /// </summary>
        [TestMethod]
        public void TestGetTotalMaterialStorage()
        {
            var factory = new EnergyFactory();

            // Добавляем дополнительное топливо
            factory.AddMaterial(EnergySource.Coal, 100);
            factory.AddMaterial(EnergySource.Gas, 50);

            int total = factory.GetTotalMaterialStorage();

            // 200 (стартовый уголь) + 250 (стартовый газ) + 100 + 50 = 600
            Assert.AreEqual(600, total);
        }

        /// <summary>
        /// Тест получения общего количества произведенной энергии
        /// </summary>
        [TestMethod]
        public void TestGetTotalProductStorage()
        {
            var factory = new EnergyFactory();
            factory.SetWorkersCount(120);
            factory.ProcessWorkshops();

            int total = factory.GetTotalProductStorage();

            Assert.IsTrue(total >= 0);
            Assert.IsTrue(total <= factory.MaxProductStorage);
        }

        /// <summary>
        /// Тест конфигурации топлива и энергии
        /// </summary>
        [TestMethod]
        public void TestMaterialAndProductConfiguration()
        {
            // Проверка enum топлива
            Assert.AreEqual(0, (int)EnergySource.Coal);
            Assert.AreEqual(1, (int)EnergySource.Gas);

            // Проверка enum энергии
            Assert.AreEqual(0, (int)EnergyType.Electricity);
            Assert.AreEqual(1, (int)EnergyType.ThermalEnergy);
        }

        /// <summary>
        /// Тест производства при неполной загрузке рабочими
        /// </summary>
        [TestMethod]
        public void TestProductionWithPartialWorkforce()
        {
            var factory = new EnergyFactory();
            factory.SetWorkersCount(30); // 25% от максимальной workforce

            var initialMaterials = factory.GetMaterialStorage();
            factory.ProcessWorkshops();

            var finalMaterials = factory.GetMaterialStorage();
            var finalProducts = factory.GetConsumerOutput();

            // При неполной загрузке все равно должно происходить производство, но с меньшей эффективностью
            Assert.IsTrue(finalMaterials[EnergySource.Coal] < initialMaterials[EnergySource.Coal]);
            Assert.IsTrue(finalProducts.Values.Sum() > 0);
        }

        /// <summary>
        /// Тест последовательного производства
        /// </summary>
        [TestMethod]
        public void TestSequentialProduction()
        {
            var factory = new EnergyFactory();
            factory.SetWorkersCount(120);

            var productsAfterFirstCycle = factory.GetConsumerOutput().Values.Sum();
            factory.ProcessWorkshops();
            var productsAfterSecondCycle = factory.GetConsumerOutput().Values.Sum();

            // После второго цикла должно быть больше продукции
            Assert.IsTrue(productsAfterSecondCycle > productsAfterFirstCycle);
        }
    }
}
