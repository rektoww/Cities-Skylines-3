using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core.Models.Buildings.CommertialBuildings;
using Core.Enums;
using System.Linq;

namespace Tests.UnitTests
{
    /// <summary>
    /// Тесты для фабрики косметики
    /// </summary>
    [TestClass]
    public sealed class CosmeticsFactoryTests
    {
        /// <summary>
        /// Тест создания фабрики косметики
        /// </summary>
        [TestMethod]
        public void TestCosmeticsFactoryCreation()
        {
            var factory = new CosmeticsFactory();

            // Проверка статических свойств строительства
            Assert.AreEqual(300000m, CosmeticsFactory.BuildCost);
            Assert.AreEqual(4, CosmeticsFactory.RequiredMaterials.Count);
            Assert.AreEqual(5, CosmeticsFactory.RequiredMaterials[ConstructionMaterial.Steel]);
            Assert.AreEqual(8, CosmeticsFactory.RequiredMaterials[ConstructionMaterial.Concrete]);
            Assert.AreEqual(4, CosmeticsFactory.RequiredMaterials[ConstructionMaterial.Glass]);
            Assert.AreEqual(3, CosmeticsFactory.RequiredMaterials[ConstructionMaterial.Plastic]);

            // Проверка базовых свойств
            Assert.AreEqual(1000, factory.MaxMaterialStorage);
            Assert.AreEqual(800, factory.MaxProductStorage);
            Assert.AreEqual(12, factory.MaxWorkers);
            Assert.AreEqual(0, factory.WorkersCount);
            Assert.AreEqual(4, factory.Workshops.Count);
        }

        /// <summary>
        /// Тест инициализации стартовых материалов
        /// </summary>
        [TestMethod]
        public void TestStartingMaterialsInitialization()
        {
            var factory = new CosmeticsFactory();

            var materials = factory.GetMaterialStorage();

            Assert.AreEqual(200, materials[CosmeticsFactory.CosmeticMaterial.Alcohol]);
            Assert.AreEqual(150, materials[CosmeticsFactory.CosmeticMaterial.Plastic]);
        }

        /// <summary>
        /// Тест управления рабочими
        /// </summary>
        [TestMethod]
        public void TestWorkerManagement()
        {
            var factory = new CosmeticsFactory();

            // Установка количества рабочих
            factory.SetWorkersCount(5);
            Assert.AreEqual(5, factory.WorkersCount);

            // Попытка установить больше рабочих, чем максимум
            factory.SetWorkersCount(15);
            Assert.AreEqual(12, factory.WorkersCount); // Должно ограничиться MaxWorkers

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
            var factory = new CosmeticsFactory();

            // Успешное добавление алкоголя
            bool addedAlcohol = factory.AddMaterial(CosmeticsFactory.CosmeticMaterial.Alcohol, 300);
            Assert.IsTrue(addedAlcohol);

            var materials = factory.GetMaterialStorage();
            Assert.AreEqual(500, materials[CosmeticsFactory.CosmeticMaterial.Alcohol]); // 200 стартовых + 300

            // Успешное добавление пластика
            bool addedPlastic = factory.AddMaterial(CosmeticsFactory.CosmeticMaterial.Plastic, 200);
            Assert.IsTrue(addedPlastic);

            materials = factory.GetMaterialStorage();
            Assert.AreEqual(350, materials[CosmeticsFactory.CosmeticMaterial.Plastic]); // 150 стартовых + 200
        }

        /// <summary>
        /// Тест добавления сырья с превышением вместимости
        /// </summary>
        [TestMethod]
        public void TestAddMaterialsExceedingCapacity()
        {
            var factory = new CosmeticsFactory();

            // Попытка добавить больше, чем вмещает хранилище
            bool notAdded = factory.AddMaterial(CosmeticsFactory.CosmeticMaterial.Alcohol, 900);
            Assert.IsFalse(notAdded); // Должно вернуть false, так как 200 + 900 > 1000

            var materials = factory.GetMaterialStorage();
            Assert.AreEqual(200, materials[CosmeticsFactory.CosmeticMaterial.Alcohol]); // Количество не изменилось
        }

        /// <summary>
        /// Тест производства без рабочих
        /// </summary>
        [TestMethod]
        public void TestProductionWithoutWorkers()
        {
            var factory = new CosmeticsFactory();

            var initialMaterials = factory.GetMaterialStorage();
            var initialProducts = factory.GetProductionOutput();

            // Запуск производства без рабочих
            factory.ProcessWorkshops();

            var finalMaterials = factory.GetMaterialStorage();
            var finalProducts = factory.GetProductionOutput();

            // Материалы и продукция не должны измениться
            Assert.AreEqual(initialMaterials[CosmeticsFactory.CosmeticMaterial.Alcohol],
                          finalMaterials[CosmeticsFactory.CosmeticMaterial.Alcohol]);
            Assert.AreEqual(initialMaterials[CosmeticsFactory.CosmeticMaterial.Plastic],
                          finalMaterials[CosmeticsFactory.CosmeticMaterial.Plastic]);
            Assert.AreEqual(initialProducts.Count, finalProducts.Count);
        }

        /// <summary>
        /// Тест производства с рабочими
        /// </summary>
        [TestMethod]
        public void TestProductionWithWorkers()
        {
            var factory = new CosmeticsFactory();
            factory.SetWorkersCount(12); // Максимальная эффективность

            var initialMaterials = factory.GetMaterialStorage();
            var initialAlcohol = initialMaterials[CosmeticsFactory.CosmeticMaterial.Alcohol];
            var initialPlastic = initialMaterials[CosmeticsFactory.CosmeticMaterial.Plastic];

            // Запуск производства
            factory.ProcessWorkshops();

            var finalMaterials = factory.GetMaterialStorage();
            var finalProducts = factory.GetProductionOutput();

            // Материалы должны быть израсходованы
            Assert.IsTrue(finalMaterials[CosmeticsFactory.CosmeticMaterial.Alcohol] < initialAlcohol);
            Assert.IsTrue(finalMaterials[CosmeticsFactory.CosmeticMaterial.Plastic] < initialPlastic);

            // Должна быть произведена продукция
            Assert.IsTrue(finalProducts.Count > 0);
            Assert.IsTrue(finalProducts.Values.Sum() > 0);
        }

   
        /// <summary>
        /// Тест потребления продукции
        /// </summary>
        [TestMethod]
        public void TestProductConsumption()
        {
            var factory = new CosmeticsFactory();
            factory.SetWorkersCount(12);
            factory.ProcessWorkshops(); // Производим продукцию

            var initialProducts = factory.GetProductionOutput();

            if (initialProducts.Count > 0)
            {
                var productType = initialProducts.Keys.First();
                var initialAmount = initialProducts[productType];

                // Успешное потребление
                bool consumed = factory.ConsumeProduct(productType, 1);
                Assert.IsTrue(consumed);

                var finalProducts = factory.GetProductionOutput();
                Assert.AreEqual(initialAmount - 1, finalProducts[productType]);
            }
        }

        /// <summary>
        /// Тест потребления недостаточного количества продукции
        /// </summary>
        [TestMethod]
        public void TestInsufficientProductConsumption()
        {
            var factory = new CosmeticsFactory();
            factory.SetWorkersCount(12);
            factory.ProcessWorkshops();

            var products = factory.GetProductionOutput();

            if (products.Count > 0)
            {
                var productType = products.Keys.First();

                // Попытка потребить больше, чем есть
                bool notConsumed = factory.ConsumeProduct(productType, 1000);
                Assert.IsFalse(notConsumed);
            }
        }

        /// <summary>
        /// Тест получения информации о производстве
        /// </summary>
        [TestMethod]
        public void TestGetProductionInfo()
        {
            var factory = new CosmeticsFactory();
            factory.SetWorkersCount(8);

            var info = factory.GetProductionInfo();

            Assert.IsNotNull(info);
            Assert.AreEqual(8, info["WorkersCount"]);
            Assert.AreEqual(12, info["MaxWorkers"]);
            Assert.IsTrue((float)info["ProductionEfficiency"] > 0);
            Assert.IsTrue((int)info["TotalMaterialStorage"] > 0);
            Assert.AreEqual(1000, info["MaxMaterialStorage"]);
            Assert.AreEqual(800, info["MaxProductStorage"]);
            Assert.AreEqual(4, info["ActiveWorkshops"]);
        }

        /// <summary>
        /// Тест полного производственного цикла
        /// </summary>
        [TestMethod]
        public void TestFullProductionCycle()
        {
            var factory = new CosmeticsFactory();
            factory.SetWorkersCount(12);

            var initialProducts = factory.GetProductionOutput().Values.Sum();

            factory.FullProductionCycle();

            var finalProducts = factory.GetProductionOutput().Values.Sum();
            Assert.IsTrue(finalProducts > initialProducts);
        }

        /// <summary>
        /// Тест размещения здания
        /// </summary>
        [TestMethod]
        public void TestOnBuildingPlaced()
        {
            var factory = new CosmeticsFactory();
            factory.SetWorkersCount(12);

            var initialProducts = factory.GetProductionOutput().Values.Sum();

            factory.OnBuildingPlaced();

            var finalProducts = factory.GetProductionOutput().Values.Sum();
            Assert.IsTrue(finalProducts > initialProducts);
        }



        /// <summary>
        /// Тест ограничения вместимости хранилища продукции
        /// </summary>
        [TestMethod]
        public void TestProductStorageCapacity()
        {
            var factory = new CosmeticsFactory();
            factory.SetWorkersCount(12);

            // Выполняем много циклов для заполнения склада
            for (int i = 0; i < 10; i++)
            {
                factory.ProcessWorkshops();
            }

            // Продукция не должна превысить максимальную вместимость
            Assert.IsTrue(factory.GetTotalProductStorage() <= factory.MaxProductStorage);
        }

        /// <summary>
        /// Тест инициализации цехов
        /// </summary>
        [TestMethod]
        public void TestWorkshopsInitialization()
        {
            var factory = new CosmeticsFactory();

            Assert.AreEqual(4, factory.Workshops.Count);

            // Проверка цеха кремов и лосьонов
            var creamWorkshop = factory.Workshops[0];
            Assert.AreEqual("Цех кремов и лосьонов", creamWorkshop.Name);
            Assert.AreEqual(6, creamWorkshop.ProductionCycleTime);
            Assert.AreEqual(2, creamWorkshop.InputRequirements.Count);
            Assert.AreEqual(2, creamWorkshop.OutputProducts.Count);

            // Проверка цеха декоративной косметики
            var makeupWorkshop = factory.Workshops[1];
            Assert.AreEqual("Цех декоративной косметики", makeupWorkshop.Name);
            Assert.AreEqual(8, makeupWorkshop.ProductionCycleTime);

            // Проверка цеха парфюмерии
            var perfumeWorkshop = factory.Workshops[2];
            Assert.AreEqual("Цех парфюмерии", perfumeWorkshop.Name);
            Assert.AreEqual(10, perfumeWorkshop.ProductionCycleTime);

            // Проверка цеха ухода за волосами
            var hairCareWorkshop = factory.Workshops[3];
            Assert.AreEqual("Цех ухода за волосами", hairCareWorkshop.Name);
            Assert.AreEqual(7, hairCareWorkshop.ProductionCycleTime);
        }

        /// <summary>
        /// Тест получения общего количества материалов
        /// </summary>
        [TestMethod]
        public void TestGetTotalMaterialStorage()
        {
            var factory = new CosmeticsFactory();

            // Добавляем дополнительные материалы
            factory.AddMaterial(CosmeticsFactory.CosmeticMaterial.Alcohol, 100);
            factory.AddMaterial(CosmeticsFactory.CosmeticMaterial.Plastic, 50);

            int total = factory.GetTotalMaterialStorage();

            // 200 (стартовый алкоголь) + 150 (стартовый пластик) + 100 + 50 = 500
            Assert.AreEqual(500, total);
        }

        /// <summary>
        /// Тест получения общего количества продукции
        /// </summary>
        [TestMethod]
        public void TestGetTotalProductStorage()
        {
            var factory = new CosmeticsFactory();
            factory.SetWorkersCount(12);
            factory.ProcessWorkshops();

            int total = factory.GetTotalProductStorage();

            Assert.IsTrue(total >= 0);
            Assert.IsTrue(total <= factory.MaxProductStorage);
        }

        /// <summary>
        /// Тест конфигурации сырья и продукции
        /// </summary>
        [TestMethod]
        public void TestMaterialAndProductConfiguration()
        {
            // Проверка enum сырья
            Assert.AreEqual(0, (int)CosmeticsFactory.CosmeticMaterial.Alcohol);
            Assert.AreEqual(1, (int)CosmeticsFactory.CosmeticMaterial.Plastic);

            // Проверка enum продукции
            Assert.AreEqual(0, (int)CosmeticsFactory.CosmeticProduct.FaceCream);
            Assert.AreEqual(1, (int)CosmeticsFactory.CosmeticProduct.BodyLotion);
            Assert.AreEqual(2, (int)CosmeticsFactory.CosmeticProduct.Lipstick);
            Assert.AreEqual(3, (int)CosmeticsFactory.CosmeticProduct.EyeShadow);
            Assert.AreEqual(4, (int)CosmeticsFactory.CosmeticProduct.Perfume);
            Assert.AreEqual(5, (int)CosmeticsFactory.CosmeticProduct.EauDeToilette);
            Assert.AreEqual(6, (int)CosmeticsFactory.CosmeticProduct.Shampoo);
            Assert.AreEqual(7, (int)CosmeticsFactory.CosmeticProduct.Conditioner);
        }

        /// <summary>
        /// Тест производства при неполной загрузке рабочими
        /// </summary>
        [TestMethod]
        public void TestProductionWithPartialWorkforce()
        {
            var factory = new CosmeticsFactory();
            factory.SetWorkersCount(3); // 25% от максимальной workforce

            var initialMaterials = factory.GetMaterialStorage();
            factory.ProcessWorkshops();

            var finalMaterials = factory.GetMaterialStorage();
            var finalProducts = factory.GetProductionOutput();

            // При неполной загрузке все равно должно происходить производство, но с меньшей эффективностью
            Assert.IsTrue(finalMaterials[CosmeticsFactory.CosmeticMaterial.Alcohol] < initialMaterials[CosmeticsFactory.CosmeticMaterial.Alcohol]);
            Assert.IsTrue(finalProducts.Values.Sum() > 0);
        }

        /// <summary>
        /// Тест последовательного производства
        /// </summary>
        [TestMethod]
        public void TestSequentialProduction()
        {
            var factory = new CosmeticsFactory();
            factory.SetWorkersCount(12);

            var productsAfterFirstCycle = factory.GetProductionOutput().Values.Sum();
            factory.ProcessWorkshops();
            var productsAfterSecondCycle = factory.GetProductionOutput().Values.Sum();

            // После второго цикла должно быть больше продукции
            Assert.IsTrue(productsAfterSecondCycle > productsAfterFirstCycle);
        }
    }
}