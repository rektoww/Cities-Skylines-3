using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core.Enums;
using System.Linq;
using Core.Models.Buildings.IndustrialBuildings;

namespace Tests.UnitTests
{
    /// <summary>
    /// Тесты для фабрики пищевой промышленности
    /// </summary>
    [TestClass]
    public sealed class FoodProcessingFactoryTests
    {
        /// <summary>
        /// Тест создания пищевой фабрики
        /// </summary>
        [TestMethod]
        public void TestFoodProcessingFactoryCreation()
        {
            var factory = new FoodProcessingFactory();

            // Проверка статических свойств строительства
            Assert.AreEqual(320000m, FoodProcessingFactory.BuildCost);
            Assert.AreEqual(4, FoodProcessingFactory.RequiredMaterials.Count);
            Assert.AreEqual(8, FoodProcessingFactory.RequiredMaterials[ConstructionMaterial.Steel]);
            Assert.AreEqual(10, FoodProcessingFactory.RequiredMaterials[ConstructionMaterial.Concrete]);
            Assert.AreEqual(3, FoodProcessingFactory.RequiredMaterials[ConstructionMaterial.Glass]);
            Assert.AreEqual(4, FoodProcessingFactory.RequiredMaterials[ConstructionMaterial.Plastic]);

            // Проверка базовых свойств
            Assert.AreEqual(1500, factory.MaxMaterialStorage);
            Assert.AreEqual(1000, factory.MaxProductStorage);
            Assert.AreEqual(15, factory.MaxWorkers);
            Assert.AreEqual(0, factory.WorkersCount);
            Assert.AreEqual(5, factory.Workshops.Count);
        }

        /// <summary>
        /// Тест инициализации стартовых материалов
        /// </summary>
        [TestMethod]
        public void TestStartingMaterialsInitialization()
        {
            var factory = new FoodProcessingFactory();

            var materials = factory.GetMaterialStorage();

            Assert.AreEqual(300, materials[FoodProcessingFactory.FoodMaterial.Wheat]);
            Assert.AreEqual(200, materials[FoodProcessingFactory.FoodMaterial.Milk]);
            Assert.AreEqual(150, materials[FoodProcessingFactory.FoodMaterial.Meat]);
            Assert.AreEqual(250, materials[FoodProcessingFactory.FoodMaterial.Vegetables]);
            Assert.AreEqual(180, materials[FoodProcessingFactory.FoodMaterial.Fruits]);
            Assert.AreEqual(100, materials[FoodProcessingFactory.FoodMaterial.Sugar]);
            Assert.AreEqual(120, materials[FoodProcessingFactory.FoodMaterial.Eggs]);
        }

        /// <summary>
        /// Тест управления рабочими
        /// </summary>
        [TestMethod]
        public void TestWorkerManagement()
        {
            var factory = new FoodProcessingFactory();

            // Установка количества рабочих
            factory.SetWorkersCount(8);
            Assert.AreEqual(8, factory.WorkersCount);

            // Попытка установить больше рабочих, чем максимум
            factory.SetWorkersCount(20);
            Assert.AreEqual(15, factory.WorkersCount); // Должно ограничиться MaxWorkers

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
            var factory = new FoodProcessingFactory();

            // Получаем начальное состояние
            var initialMaterials = factory.GetMaterialStorage();
            int initialWheat = initialMaterials[FoodProcessingFactory.FoodMaterial.Wheat];
            int initialTotal = factory.GetTotalMaterialStorage();

            // Вычисляем сколько можно добавить без превышения лимита
            int availableSpace = factory.MaxMaterialStorage - initialTotal;
            int wheatToAdd = System.Math.Min(100, availableSpace);

            // Успешное добавление пшеницы
            bool addedWheat = factory.AddMaterial(FoodProcessingFactory.FoodMaterial.Wheat, wheatToAdd);
            Assert.IsTrue(addedWheat, "Добавление пшеницы должно быть успешным");

            var materialsAfter = factory.GetMaterialStorage();
            Assert.AreEqual(initialWheat + wheatToAdd, materialsAfter[FoodProcessingFactory.FoodMaterial.Wheat]);
        }

        /// <summary>
        /// Тест добавления сырья с превышением вместимости
        /// </summary>
        [TestMethod]
        public void TestAddMaterialsExceedingCapacity()
        {
            var factory = new FoodProcessingFactory();

            // Попытка добавить больше, чем вмещает хранилище
            bool notAdded = factory.AddMaterial(FoodProcessingFactory.FoodMaterial.Wheat, 1300);
            Assert.IsFalse(notAdded); // Должно вернуть false, так как 300 + 1300 > 1500

            var materials = factory.GetMaterialStorage();
            Assert.AreEqual(300, materials[FoodProcessingFactory.FoodMaterial.Wheat]); // Количество не изменилось
        }

        /// <summary>
        /// Тест производства без рабочих
        /// </summary>
        [TestMethod]
        public void TestProductionWithoutWorkers()
        {
            var factory = new FoodProcessingFactory();

            var initialMaterials = factory.GetMaterialStorage();
            var initialProducts = factory.GetProductionOutput();

            // Запуск производства без рабочих
            factory.ProcessWorkshops();

            var finalMaterials = factory.GetMaterialStorage();
            var finalProducts = factory.GetProductionOutput();

            // Материалы и продукция не должны измениться
            Assert.AreEqual(initialMaterials[FoodProcessingFactory.FoodMaterial.Wheat],
                          finalMaterials[FoodProcessingFactory.FoodMaterial.Wheat]);
            Assert.AreEqual(initialMaterials[FoodProcessingFactory.FoodMaterial.Milk],
                          finalMaterials[FoodProcessingFactory.FoodMaterial.Milk]);
            Assert.AreEqual(initialProducts.Count, finalProducts.Count);
        }

        /// <summary>
        /// Тест производства с рабочими
        /// </summary>
        [TestMethod]
        public void TestProductionWithWorkers()
        {
            var factory = new FoodProcessingFactory();
            factory.SetWorkersCount(15); // Максимальная эффективность

            var initialMaterials = factory.GetMaterialStorage();
            var initialWheat = initialMaterials[FoodProcessingFactory.FoodMaterial.Wheat];
            var initialMilk = initialMaterials[FoodProcessingFactory.FoodMaterial.Milk];
            var initialMeat = initialMaterials[FoodProcessingFactory.FoodMaterial.Meat];

            // Запуск производства
            factory.ProcessWorkshops();

            var finalMaterials = factory.GetMaterialStorage();
            var finalProducts = factory.GetProductionOutput();

            // Материалы должны быть израсходованы
            Assert.IsTrue(finalMaterials[FoodProcessingFactory.FoodMaterial.Wheat] < initialWheat);
            Assert.IsTrue(finalMaterials[FoodProcessingFactory.FoodMaterial.Milk] < initialMilk);
            Assert.IsTrue(finalMaterials[FoodProcessingFactory.FoodMaterial.Meat] < initialMeat);

            // Должна быть произведена продукция
            Assert.IsTrue(finalProducts.Count > 0);
            Assert.IsTrue(finalProducts.Values.Sum() > 0);
        }

        /// <summary>
        /// Тест эффективности производства
        /// </summary>
        [TestMethod]
        public void TestProductionEfficiency()
        {
            var factory = new FoodProcessingFactory();

            // Проверка эффективности при разном количестве рабочих
            factory.SetWorkersCount(0);
            Assert.AreEqual(0f, factory.ProductionEfficiency);

            factory.SetWorkersCount(8);
            Assert.AreEqual(System.Math.Round(0.72f, 2), System.Math.Round(factory.ProductionEfficiency, 2)); // 0.4 + (8/15)*0.6 ≈ 0.72

            factory.SetWorkersCount(15);
            Assert.AreEqual(1.0f, factory.ProductionEfficiency); // 0.4 + (15/15)*0.6 = 1.0
        }

        /// <summary>
        /// Тест потребления продукции
        /// </summary>
        [TestMethod]
        public void TestProductConsumption()
        {
            var factory = new FoodProcessingFactory();
            factory.SetWorkersCount(15);
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
            var factory = new FoodProcessingFactory();
            factory.SetWorkersCount(15);
            factory.ProcessWorkshops();

            var products = factory.GetProductionOutput();

            if (products.Count > 0)
            {
                var productType = products.Keys.First();

                // Попытка потребить больше, чем есть
                bool notConsumed = factory.ConsumeProduct(productType, 10000);
                Assert.IsFalse(notConsumed);
            }
        }

        /// <summary>
        /// Тест получения информации о производстве
        /// </summary>
        [TestMethod]
        public void TestGetProductionInfo()
        {
            var factory = new FoodProcessingFactory();
            factory.SetWorkersCount(10);

            var info = factory.GetProductionInfo();

            Assert.IsNotNull(info);
            Assert.AreEqual(10, info["WorkersCount"]);
            Assert.AreEqual(15, info["MaxWorkers"]);
            Assert.IsTrue((float)info["ProductionEfficiency"] > 0);
            Assert.IsTrue((int)info["TotalMaterialStorage"] > 0);
            Assert.AreEqual(1500, info["MaxMaterialStorage"]);
            Assert.AreEqual(1000, info["MaxProductStorage"]);
            Assert.AreEqual(5, info["ActiveWorkshops"]);
        }

        /// <summary>
        /// Тест полного производственного цикла
        /// </summary>
        [TestMethod]
        public void TestFullProductionCycle()
        {
            var factory = new FoodProcessingFactory();
            factory.SetWorkersCount(15);

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
            var factory = new FoodProcessingFactory();
            factory.SetWorkersCount(15);

            var initialProducts = factory.GetProductionOutput().Values.Sum();

            factory.OnBuildingPlaced();

            var finalProducts = factory.GetProductionOutput().Values.Sum();
            Assert.IsTrue(finalProducts > initialProducts);
        }

        /// <summary>
        /// Тест ограничения вместимости хранилища материалов
        /// </summary>
        [TestMethod]
        public void TestMaterialStorageCapacity()
        {
            var factory = new FoodProcessingFactory();

            // Вычисляем доступное место
            int availableSpace = factory.MaxMaterialStorage - factory.GetTotalMaterialStorage();

            // Добавляем материалы до полного заполнения
            bool added = factory.AddMaterial(FoodProcessingFactory.FoodMaterial.Wheat, availableSpace);
            Assert.IsTrue(added);
            Assert.AreEqual(factory.MaxMaterialStorage, factory.GetTotalMaterialStorage());

            // Попытка добавить еще должно вернуть false
            bool notAdded = factory.AddMaterial(FoodProcessingFactory.FoodMaterial.Milk, 1);
            Assert.IsFalse(notAdded);
            Assert.AreEqual(factory.MaxMaterialStorage, factory.GetTotalMaterialStorage());
        }

        /// <summary>
        /// Тест ограничения вместимости хранилища продукции
        /// </summary>
        [TestMethod]
        public void TestProductStorageCapacity()
        {
            var factory = new FoodProcessingFactory();
            factory.SetWorkersCount(15);

            // Выполняем много циклов для заполнения склада
            for (int i = 0; i < 15; i++)
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
            var factory = new FoodProcessingFactory();

            Assert.AreEqual(5, factory.Workshops.Count);

            // Проверка хлебопекарного цеха
            var bakeryWorkshop = factory.Workshops[0];
            Assert.AreEqual("Хлебопекарный цех", bakeryWorkshop.Name);
            Assert.AreEqual(6, bakeryWorkshop.ProductionCycleTime);
            Assert.AreEqual(2, bakeryWorkshop.InputRequirements.Count);
            Assert.AreEqual(2, bakeryWorkshop.OutputProducts.Count);

            // Проверка молочного цеха
            var dairyWorkshop = factory.Workshops[1];
            Assert.AreEqual("Молочный цех", dairyWorkshop.Name);
            Assert.AreEqual(8, dairyWorkshop.ProductionCycleTime);
            Assert.AreEqual(2, dairyWorkshop.InputRequirements.Count);
            Assert.AreEqual(3, dairyWorkshop.OutputProducts.Count);

            // Проверка мясного цеха
            var meatWorkshop = factory.Workshops[2];
            Assert.AreEqual("Мясной цех", meatWorkshop.Name);
            Assert.AreEqual(10, meatWorkshop.ProductionCycleTime);

            // Проверка консервного цеха
            var canningWorkshop = factory.Workshops[3];
            Assert.AreEqual("Консервный цех", canningWorkshop.Name);
            Assert.AreEqual(12, canningWorkshop.ProductionCycleTime);

            // Проверка цеха упаковки яиц
            var eggsWorkshop = factory.Workshops[4];
            Assert.AreEqual("Цех упаковки яиц", eggsWorkshop.Name);
            Assert.AreEqual(4, eggsWorkshop.ProductionCycleTime);
        }

        /// <summary>
        /// Тест получения общего количества материалов
        /// </summary>
        [TestMethod]
        public void TestGetTotalMaterialStorage()
        {
            var factory = new FoodProcessingFactory();

            int total = factory.GetTotalMaterialStorage();

            // 300 + 200 + 150 + 250 + 180 + 100 + 120 = 1300
            Assert.AreEqual(1300, total);
        }

        /// <summary>
        /// Тест получения общего количества продукции
        /// </summary>
        [TestMethod]
        public void TestGetTotalProductStorage()
        {
            var factory = new FoodProcessingFactory();
            factory.SetWorkersCount(15);
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
            Assert.AreEqual(0, (int)FoodProcessingFactory.FoodMaterial.Wheat);
            Assert.AreEqual(1, (int)FoodProcessingFactory.FoodMaterial.Milk);
            Assert.AreEqual(2, (int)FoodProcessingFactory.FoodMaterial.Meat);
            Assert.AreEqual(3, (int)FoodProcessingFactory.FoodMaterial.Vegetables);
            Assert.AreEqual(4, (int)FoodProcessingFactory.FoodMaterial.Fruits);
            Assert.AreEqual(5, (int)FoodProcessingFactory.FoodMaterial.Sugar);
            Assert.AreEqual(6, (int)FoodProcessingFactory.FoodMaterial.Eggs);

            // Проверка enum продукции
            Assert.AreEqual(0, (int)FoodProcessingFactory.FoodProduct.Bread);
            Assert.AreEqual(1, (int)FoodProcessingFactory.FoodProduct.Pasta);
            Assert.AreEqual(2, (int)FoodProcessingFactory.FoodProduct.Cheese);
            Assert.AreEqual(3, (int)FoodProcessingFactory.FoodProduct.Yogurt);
            Assert.AreEqual(4, (int)FoodProcessingFactory.FoodProduct.Sausages);
            Assert.AreEqual(5, (int)FoodProcessingFactory.FoodProduct.CannedVegetables);
            Assert.AreEqual(6, (int)FoodProcessingFactory.FoodProduct.Juice);
            Assert.AreEqual(7, (int)FoodProcessingFactory.FoodProduct.Jam);
            Assert.AreEqual(8, (int)FoodProcessingFactory.FoodProduct.Butter);
            Assert.AreEqual(9, (int)FoodProcessingFactory.FoodProduct.Eggs);
        }

        /// <summary>
        /// Тест производства хлеба
        /// </summary>
        [TestMethod]
        public void TestBreadProduction()
        {
            var factory = new FoodProcessingFactory();
            factory.SetWorkersCount(15);

            // Запускаем производство
            factory.ProcessWorkshops();

            var products = factory.GetProductionOutput();

            // Должен производиться хлеб
            Assert.IsTrue(products.ContainsKey(FoodProcessingFactory.FoodProduct.Bread));
            Assert.IsTrue(products[FoodProcessingFactory.FoodProduct.Bread] > 0);
        }

        /// <summary>
        /// Тест многоступенчатого производства (молоко -> сыр/йогурт)
        /// </summary>
        [TestMethod]
        public void TestDairyProduction()
        {
            var factory = new FoodProcessingFactory();
            factory.SetWorkersCount(15);

            // Запускаем несколько циклов производства
            for (int i = 0; i < 3; i++)
            {
                factory.ProcessWorkshops();
            }

            var products = factory.GetProductionOutput();

            // Должны производиться молочные продукты
            Assert.IsTrue(products.ContainsKey(FoodProcessingFactory.FoodProduct.Cheese) ||
                         products.ContainsKey(FoodProcessingFactory.FoodProduct.Yogurt) ||
                         products.ContainsKey(FoodProcessingFactory.FoodProduct.Butter));
        }

        /// <summary>
        /// Тест последовательного производства
        /// </summary>
        [TestMethod]
        public void TestSequentialProduction()
        {
            var factory = new FoodProcessingFactory();
            factory.SetWorkersCount(15);

            var productsAfterFirstCycle = factory.GetProductionOutput().Values.Sum();
            factory.ProcessWorkshops();
            var productsAfterSecondCycle = factory.GetProductionOutput().Values.Sum();

            // После второго цикла должно быть больше продукции
            Assert.IsTrue(productsAfterSecondCycle > productsAfterFirstCycle);
        }

        /// <summary>
        /// Тест производства всех видов пищевой продукции
        /// </summary>
        [TestMethod]
        public void TestAllFoodTypesProduction()
        {
            var factory = new FoodProcessingFactory();
            factory.SetWorkersCount(15);

            // Запускаем несколько циклов для производства всех видов продукции
            for (int i = 0; i < 8; i++)
            {
                factory.ProcessWorkshops();
            }

            var products = factory.GetProductionOutput();

            // Должны производиться различные виды пищевой продукции
            Assert.IsTrue(products.Count >= 4); // Как минимум 4 разных продукта
        }

        /// <summary>
        /// Тест эффективности при неполной загрузке рабочими
        /// </summary>
        [TestMethod]
        public void TestEfficiencyWithPartialWorkforce()
        {
            var factory = new FoodProcessingFactory();
            factory.SetWorkersCount(5); // ~33% от максимальной workforce

            var initialMaterials = factory.GetMaterialStorage();
            factory.ProcessWorkshops();

            var finalMaterials = factory.GetMaterialStorage();
            var finalProducts = factory.GetProductionOutput();

            // При неполной загрузке все равно должно происходить производство, но с меньшей эффективностью
            Assert.IsTrue(finalMaterials[FoodProcessingFactory.FoodMaterial.Wheat] < initialMaterials[FoodProcessingFactory.FoodMaterial.Wheat]);
            Assert.IsTrue(finalProducts.Values.Sum() > 0);
        }

        /// <summary>
        /// Тест консервного производства
        /// </summary>
        [TestMethod]
        public void TestCanningProduction()
        {
            var factory = new FoodProcessingFactory();
            factory.SetWorkersCount(15);

            // Запускаем производство
            factory.ProcessWorkshops();

            var products = factory.GetProductionOutput();

            // Должны производиться консервированные продукты
            Assert.IsTrue(products.ContainsKey(FoodProcessingFactory.FoodProduct.CannedVegetables) ||
                         products.ContainsKey(FoodProcessingFactory.FoodProduct.Juice) ||
                         products.ContainsKey(FoodProcessingFactory.FoodProduct.Jam));
        }

        /// <summary>
        /// Тест упаковки яиц
        /// </summary>
        [TestMethod]
        public void TestEggPackaging()
        {
            var factory = new FoodProcessingFactory();
            factory.SetWorkersCount(15);

            // Запускаем производство
            factory.ProcessWorkshops();

            var products = factory.GetProductionOutput();

            // Должны производиться упакованные яйца
            Assert.IsTrue(products.ContainsKey(FoodProcessingFactory.FoodProduct.Eggs));
        }

        /// <summary>
        /// Тест потребления всех видов продукции
        /// </summary>
        [TestMethod]
        public void TestConsumeAllProducts()
        {
            var factory = new FoodProcessingFactory();
            factory.SetWorkersCount(15);
            factory.ProcessWorkshops();

            var products = factory.GetProductionOutput();

            // Потребляем по одной единице каждого продукта
            foreach (var product in products)
            {
                bool consumed = factory.ConsumeProduct(product.Key, 1);
                Assert.IsTrue(consumed, $"Не удалось потребить продукт {product.Key}");
            }
        }

        /// <summary>
        /// Тест полного цикла производства и потребления
        /// </summary>
        [TestMethod]
        public void TestFullProductionAndConsumptionCycle()
        {
            var factory = new FoodProcessingFactory();
            factory.SetWorkersCount(15);

            // Производим продукцию
            factory.FullProductionCycle();
            var productsBefore = factory.GetProductionOutput();
            int totalBefore = factory.GetTotalProductStorage();

            // Потребляем часть продукции
            if (productsBefore.Count > 0)
            {
                var firstProduct = productsBefore.Keys.First();
                int amountToConsume = System.Math.Min(5, productsBefore[firstProduct]);

                bool consumed = factory.ConsumeProduct(firstProduct, amountToConsume);
                Assert.IsTrue(consumed);

                var productsAfter = factory.GetProductionOutput();
                int totalAfter = factory.GetTotalProductStorage();

                Assert.AreEqual(totalBefore - amountToConsume, totalAfter);
            }
        }
    }
}