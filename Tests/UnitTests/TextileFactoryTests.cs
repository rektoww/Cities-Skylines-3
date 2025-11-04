using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core.Enums;
using System.Linq;
using Core.Models.Buildings.IndustrialBuildings;

namespace Tests.UnitTests
{
    /// <summary>
    /// Тесты для текстильной фабрики
    /// </summary>
    [TestClass]
    public sealed class TextileFactoryTests
    {
        /// <summary>
        /// Тест создания текстильной фабрики
        /// </summary>
        [TestMethod]
        public void TestTextileFactoryCreation()
        {
            var factory = new TextileFactory();

            // Проверка статических свойств строительства
            Assert.AreEqual(350000m, TextileFactory.BuildCost);
            Assert.AreEqual(4, TextileFactory.RequiredMaterials.Count);
            Assert.AreEqual(12, TextileFactory.RequiredMaterials[ConstructionMaterial.Steel]);
            Assert.AreEqual(8, TextileFactory.RequiredMaterials[ConstructionMaterial.Concrete]);
            Assert.AreEqual(2, TextileFactory.RequiredMaterials[ConstructionMaterial.Glass]);
            Assert.AreEqual(6, TextileFactory.RequiredMaterials[ConstructionMaterial.Plastic]);

            // Проверка базовых свойств
            Assert.AreEqual(1200, factory.MaxMaterialStorage);
            Assert.AreEqual(800, factory.MaxProductStorage);
            Assert.AreEqual(18, factory.MaxWorkers);
            Assert.AreEqual(0, factory.WorkersCount);
            Assert.AreEqual(5, factory.Workshops.Count);
        }

        /// <summary>
        /// Тест инициализации стартовых материалов
        /// </summary>
        [TestMethod]
        public void TestStartingMaterialsInitialization()
        {
            var factory = new TextileFactory();

            var materials = factory.GetMaterialStorage();

            Assert.AreEqual(400, materials[TextileFactory.TextileMaterial.Cotton]);
            Assert.AreEqual(250, materials[TextileFactory.TextileMaterial.Wool]);
            Assert.AreEqual(150, materials[TextileFactory.TextileMaterial.Silk]);
            Assert.AreEqual(200, materials[TextileFactory.TextileMaterial.Linen]);
            Assert.AreEqual(300, materials[TextileFactory.TextileMaterial.SyntheticFiber]);
            Assert.AreEqual(100, materials[TextileFactory.TextileMaterial.Dye]);
            Assert.AreEqual(180, materials[TextileFactory.TextileMaterial.Thread]);
        }

        /// <summary>
        /// Тест управления рабочими
        /// </summary>
        [TestMethod]
        public void TestWorkerManagement()
        {
            var factory = new TextileFactory();

            // Установка количества рабочих
            factory.SetWorkersCount(12);
            Assert.AreEqual(12, factory.WorkersCount);

            // Попытка установить больше рабочих, чем максимум
            factory.SetWorkersCount(25);
            Assert.AreEqual(18, factory.WorkersCount); // Должно ограничиться MaxWorkers

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
            var factory = new TextileFactory();

            // Получаем начальное состояние
            var initialMaterials = factory.GetMaterialStorage();
            int initialCotton = initialMaterials[TextileFactory.TextileMaterial.Cotton];
            int initialTotal = factory.GetTotalMaterialStorage();

            // Вычисляем сколько можно добавить без превышения лимита
            int availableSpace = factory.MaxMaterialStorage - initialTotal;
            int cottonToAdd = System.Math.Min(100, availableSpace);

            // Успешное добавление хлопка
            bool addedCotton = factory.AddMaterial(TextileFactory.TextileMaterial.Cotton, cottonToAdd);
            Assert.IsTrue(addedCotton, "Добавление хлопка должно быть успешным");

            var materialsAfter = factory.GetMaterialStorage();
            Assert.AreEqual(initialCotton + cottonToAdd, materialsAfter[TextileFactory.TextileMaterial.Cotton]);
        }

        /// <summary>
        /// Тест добавления сырья с превышением вместимости
        /// </summary>
        [TestMethod]
        public void TestAddMaterialsExceedingCapacity()
        {
            var factory = new TextileFactory();

            // Попытка добавить больше, чем вмещает хранилище
            bool notAdded = factory.AddMaterial(TextileFactory.TextileMaterial.Cotton, 900);
            Assert.IsFalse(notAdded); // Должно вернуть false, так как 400 + 900 > 1200

            var materials = factory.GetMaterialStorage();
            Assert.AreEqual(400, materials[TextileFactory.TextileMaterial.Cotton]); // Количество не изменилось
        }

        /// <summary>
        /// Тест производства без рабочих
        /// </summary>
        [TestMethod]
        public void TestProductionWithoutWorkers()
        {
            var factory = new TextileFactory();

            var initialMaterials = factory.GetMaterialStorage();
            var initialProducts = factory.GetProductionOutput();

            // Запуск производства без рабочих
            factory.ProcessWorkshops();

            var finalMaterials = factory.GetMaterialStorage();
            var finalProducts = factory.GetProductionOutput();

            // Материалы и продукция не должны измениться
            Assert.AreEqual(initialMaterials[TextileFactory.TextileMaterial.Cotton],
                          finalMaterials[TextileFactory.TextileMaterial.Cotton]);
            Assert.AreEqual(initialMaterials[TextileFactory.TextileMaterial.Wool],
                          finalMaterials[TextileFactory.TextileMaterial.Wool]);
            Assert.AreEqual(initialProducts.Count, finalProducts.Count);
        }

        /// <summary>
        /// Тест производства с рабочими
        /// </summary>
        [TestMethod]
        public void TestProductionWithWorkers()
        {
            var factory = new TextileFactory();
            factory.SetWorkersCount(18); // Максимальная эффективность

            var initialMaterials = factory.GetMaterialStorage();
            var initialCotton = initialMaterials[TextileFactory.TextileMaterial.Cotton];
            var initialWool = initialMaterials[TextileFactory.TextileMaterial.Wool];
            var initialDye = initialMaterials[TextileFactory.TextileMaterial.Dye];

            // Запуск производства
            factory.ProcessWorkshops();

            var finalMaterials = factory.GetMaterialStorage();
            var finalProducts = factory.GetProductionOutput();

            // Материалы должны быть израсходованы
            Assert.IsTrue(finalMaterials[TextileFactory.TextileMaterial.Cotton] < initialCotton);
            Assert.IsTrue(finalMaterials[TextileFactory.TextileMaterial.Wool] < initialWool);
            Assert.IsTrue(finalMaterials[TextileFactory.TextileMaterial.Dye] < initialDye);

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
            var factory = new TextileFactory();

            // Проверка эффективности при разном количестве рабочих
            factory.SetWorkersCount(0);
            Assert.AreEqual(0f, factory.ProductionEfficiency);

            factory.SetWorkersCount(9);
            Assert.AreEqual(System.Math.Round(0.675f, 3), System.Math.Round(factory.ProductionEfficiency, 3)); // 0.35 + (9/18)*0.65 = 0.675

            factory.SetWorkersCount(18);
            Assert.AreEqual(1.0f, factory.ProductionEfficiency); // 0.35 + (18/18)*0.65 = 1.0
        }

        /// <summary>
        /// Тест потребления продукции
        /// </summary>
        [TestMethod]
        public void TestProductConsumption()
        {
            var factory = new TextileFactory();
            factory.SetWorkersCount(18);
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
            var factory = new TextileFactory();
            factory.SetWorkersCount(18);
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
            var factory = new TextileFactory();
            factory.SetWorkersCount(12);

            var info = factory.GetProductionInfo();

            Assert.IsNotNull(info);
            Assert.AreEqual(12, info["WorkersCount"]);
            Assert.AreEqual(18, info["MaxWorkers"]);
            Assert.IsTrue((float)info["ProductionEfficiency"] > 0);
            Assert.IsTrue((int)info["TotalMaterialStorage"] > 0);
            Assert.AreEqual(1200, info["MaxMaterialStorage"]);
            Assert.AreEqual(800, info["MaxProductStorage"]);
            Assert.AreEqual(5, info["ActiveWorkshops"]);
        }

        /// <summary>
        /// Тест полного производственного цикла
        /// </summary>
        [TestMethod]
        public void TestFullProductionCycle()
        {
            var factory = new TextileFactory();
            factory.SetWorkersCount(18);

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
            var factory = new TextileFactory();
            factory.SetWorkersCount(18);

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
            var factory = new TextileFactory();

            // Вычисляем доступное место
            int availableSpace = factory.MaxMaterialStorage - factory.GetTotalMaterialStorage();

            // Добавляем материалы до полного заполнения
            bool added = factory.AddMaterial(TextileFactory.TextileMaterial.Cotton, availableSpace);
            Assert.IsTrue(added);
            Assert.AreEqual(factory.MaxMaterialStorage, factory.GetTotalMaterialStorage());

            // Попытка добавить еще должно вернуть false
            bool notAdded = factory.AddMaterial(TextileFactory.TextileMaterial.Wool, 1);
            Assert.IsFalse(notAdded);
            Assert.AreEqual(factory.MaxMaterialStorage, factory.GetTotalMaterialStorage());
        }

        /// <summary>
        /// Тест ограничения вместимости хранилища продукции
        /// </summary>
        [TestMethod]
        public void TestProductStorageCapacity()
        {
            var factory = new TextileFactory();
            factory.SetWorkersCount(18);

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
            var factory = new TextileFactory();

            Assert.AreEqual(5, factory.Workshops.Count);

            // Проверка прядильного цеха
            var spinningWorkshop = factory.Workshops[0];
            Assert.AreEqual("Прядильный цех", spinningWorkshop.Name);
            Assert.AreEqual(7, spinningWorkshop.ProductionCycleTime);
            Assert.AreEqual(3, spinningWorkshop.InputRequirements.Count);
            Assert.AreEqual(2, spinningWorkshop.OutputProducts.Count);

            // Проверка ткацкого цеха
            var weavingWorkshop = factory.Workshops[1];
            Assert.AreEqual("Ткацкий цех", weavingWorkshop.Name);
            Assert.AreEqual(9, weavingWorkshop.ProductionCycleTime);
            Assert.AreEqual(3, weavingWorkshop.InputRequirements.Count);
            Assert.AreEqual(4, weavingWorkshop.OutputProducts.Count);

            // Проверка цеха синтетических тканей
            var syntheticWorkshop = factory.Workshops[2];
            Assert.AreEqual("Цех синтетических тканей", syntheticWorkshop.Name);
            Assert.AreEqual(6, syntheticWorkshop.ProductionCycleTime);

            // Проверка швейного цеха
            var sewingWorkshop = factory.Workshops[3];
            Assert.AreEqual("Швейный цех", sewingWorkshop.Name);
            Assert.AreEqual(8, sewingWorkshop.ProductionCycleTime);

            // Проверка джинсового цеха
            var denimWorkshop = factory.Workshops[4];
            Assert.AreEqual("Джинсовый цех", denimWorkshop.Name);
            Assert.AreEqual(11, denimWorkshop.ProductionCycleTime);
        }

        /// <summary>
        /// Тест получения общего количества материалов
        /// </summary>
        [TestMethod]
        public void TestGetTotalMaterialStorage()
        {
            var factory = new TextileFactory();

            int total = factory.GetTotalMaterialStorage();

            // 400 + 250 + 150 + 200 + 300 + 100 + 180 = 1580
            // Но ограничено MaxMaterialStorage = 1200
            Assert.AreEqual(1200, total);
        }

        /// <summary>
        /// Тест получения общего количества продукции
        /// </summary>
        [TestMethod]
        public void TestGetTotalProductStorage()
        {
            var factory = new TextileFactory();
            factory.SetWorkersCount(18);
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
            Assert.AreEqual(0, (int)TextileFactory.TextileMaterial.Cotton);
            Assert.AreEqual(1, (int)TextileFactory.TextileMaterial.Wool);
            Assert.AreEqual(2, (int)TextileFactory.TextileMaterial.Silk);
            Assert.AreEqual(3, (int)TextileFactory.TextileMaterial.Linen);
            Assert.AreEqual(4, (int)TextileFactory.TextileMaterial.SyntheticFiber);
            Assert.AreEqual(5, (int)TextileFactory.TextileMaterial.Dye);
            Assert.AreEqual(6, (int)TextileFactory.TextileMaterial.Thread);

            // Проверка enum продукции
            Assert.AreEqual(0, (int)TextileFactory.TextileProduct.CottonFabric);
            Assert.AreEqual(1, (int)TextileFactory.TextileProduct.WoolFabric);
            Assert.AreEqual(2, (int)TextileFactory.TextileProduct.SilkFabric);
            Assert.AreEqual(3, (int)TextileFactory.TextileProduct.LinenFabric);
            Assert.AreEqual(4, (int)TextileFactory.TextileProduct.SyntheticFabric);
            Assert.AreEqual(5, (int)TextileFactory.TextileProduct.Denim);
            Assert.AreEqual(6, (int)TextileFactory.TextileProduct.Knitwear);
            Assert.AreEqual(7, (int)TextileFactory.TextileProduct.Yarn);
            Assert.AreEqual(8, (int)TextileFactory.TextileProduct.Clothing);
            Assert.AreEqual(9, (int)TextileFactory.TextileProduct.HomeTextile);
        }

        /// <summary>
        /// Тест производства пряжи
        /// </summary>
        [TestMethod]
        public void TestYarnProduction()
        {
            var factory = new TextileFactory();
            factory.SetWorkersCount(18);

            // Запускаем производство
            factory.ProcessWorkshops();

            var products = factory.GetProductionOutput();

            // Должна производиться пряжа
            Assert.IsTrue(products.ContainsKey(TextileFactory.TextileProduct.Yarn));
            Assert.IsTrue(products[TextileFactory.TextileProduct.Yarn] > 0);
        }

        /// <summary>
        /// Тест многоступенчатого производства (волокна -> пряжа -> ткань)
        /// </summary>
        [TestMethod]
        public void TestMultiStageProduction()
        {
            var factory = new TextileFactory();
            factory.SetWorkersCount(18);

            // Запускаем несколько циклов производства
            for (int i = 0; i < 3; i++)
            {
                factory.ProcessWorkshops();
            }

            var products = factory.GetProductionOutput();

            // Должны производиться ткани из различных материалов
            Assert.IsTrue(products.ContainsKey(TextileFactory.TextileProduct.CottonFabric) ||
                         products.ContainsKey(TextileFactory.TextileProduct.WoolFabric) ||
                         products.ContainsKey(TextileFactory.TextileProduct.SilkFabric));
        }

        /// <summary>
        /// Тест последовательного производства
        /// </summary>
        [TestMethod]
        public void TestSequentialProduction()
        {
            var factory = new TextileFactory();
            factory.SetWorkersCount(18);

            var productsAfterFirstCycle = factory.GetProductionOutput().Values.Sum();
            factory.ProcessWorkshops();
            var productsAfterSecondCycle = factory.GetProductionOutput().Values.Sum();

            // После второго цикла должно быть больше продукции
            Assert.IsTrue(productsAfterSecondCycle > productsAfterFirstCycle);
        }

        /// <summary>
        /// Тест производства всех видов текстильной продукции
        /// </summary>
        [TestMethod]
        public void TestAllTextileTypesProduction()
        {
            var factory = new TextileFactory();
            factory.SetWorkersCount(18);

            // Запускаем несколько циклов для производства всех видов продукции
            for (int i = 0; i < 8; i++)
            {
                factory.ProcessWorkshops();
            }

            var products = factory.GetProductionOutput();

            // Должны производиться различные виды текстильной продукции
            Assert.IsTrue(products.Count >= 5); // Как минимум 5 разных продуктов
        }

        /// <summary>
        /// Тест эффективности при неполной загрузке рабочими
        /// </summary>
        [TestMethod]
        public void TestEfficiencyWithPartialWorkforce()
        {
            var factory = new TextileFactory();
            factory.SetWorkersCount(6); // 33% от максимальной workforce

            var initialMaterials = factory.GetMaterialStorage();
            factory.ProcessWorkshops();

            var finalMaterials = factory.GetMaterialStorage();
            var finalProducts = factory.GetProductionOutput();

            // При неполной загрузке все равно должно происходить производство, но с меньшей эффективностью
            Assert.IsTrue(finalMaterials[TextileFactory.TextileMaterial.Cotton] < initialMaterials[TextileFactory.TextileMaterial.Cotton]);
            Assert.IsTrue(finalProducts.Values.Sum() > 0);
        }

        /// <summary>
        /// Тест синтетического производства
        /// </summary>
        [TestMethod]
        public void TestSyntheticProduction()
        {
            var factory = new TextileFactory();
            factory.SetWorkersCount(18);

            // Запускаем производство
            factory.ProcessWorkshops();

            var products = factory.GetProductionOutput();

            // Должны производиться синтетические ткани
            Assert.IsTrue(products.ContainsKey(TextileFactory.TextileProduct.SyntheticFabric) ||
                         products.ContainsKey(TextileFactory.TextileProduct.Knitwear));
        }

        /// <summary>
        /// Тест швейного производства
        /// </summary>
        [TestMethod]
        public void TestSewingProduction()
        {
            var factory = new TextileFactory();
            factory.SetWorkersCount(18);

            // Запускаем несколько циклов для накопления тканей
            for (int i = 0; i < 4; i++)
            {
                factory.ProcessWorkshops();
            }

            var products = factory.GetProductionOutput();

            // Должна производиться готовая одежда и домашний текстиль
            Assert.IsTrue(products.ContainsKey(TextileFactory.TextileProduct.Clothing) ||
                         products.ContainsKey(TextileFactory.TextileProduct.HomeTextile));
        }

        /// <summary>
        /// Тест джинсового производства
        /// </summary>
        [TestMethod]
        public void TestDenimProduction()
        {
            var factory = new TextileFactory();
            factory.SetWorkersCount(18);

            // Запускаем производство
            factory.ProcessWorkshops();

            var products = factory.GetProductionOutput();

            // Должна производиться джинсовая ткань
            Assert.IsTrue(products.ContainsKey(TextileFactory.TextileProduct.Denim));
        }

        /// <summary>
        /// Тест потребления всех видов продукции
        /// </summary>
        [TestMethod]
        public void TestConsumeAllProducts()
        {
            var factory = new TextileFactory();
            factory.SetWorkersCount(18);
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
            var factory = new TextileFactory();
            factory.SetWorkersCount(18);

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

        /// <summary>
        /// Тест сложной производственной цепочки
        /// </summary>
        [TestMethod]
        public void TestComplexProductionChain()
        {
            var factory = new TextileFactory();
            factory.SetWorkersCount(18);

            // Запускаем несколько циклов для полной производственной цепочки
            for (int i = 0; i < 6; i++)
            {
                factory.ProcessWorkshops();
            }

            var products = factory.GetProductionOutput();

            // Проверяем наличие продукции на разных этапах цепочки
            bool hasRawMaterials = products.ContainsKey(TextileFactory.TextileProduct.Yarn);
            bool hasFabrics = products.ContainsKey(TextileFactory.TextileProduct.CottonFabric) ||
                             products.ContainsKey(TextileFactory.TextileProduct.WoolFabric);
            bool hasFinishedProducts = products.ContainsKey(TextileFactory.TextileProduct.Clothing) ||
                                      products.ContainsKey(TextileFactory.TextileProduct.HomeTextile);

            // Должны присутствовать продукты разных этапов производства
            Assert.IsTrue(hasRawMaterials || hasFabrics || hasFinishedProducts);
        }

        /// <summary>
        /// Тест специализированных тканей
        /// </summary>
        [TestMethod]
        public void TestSpecializedFabrics()
        {
            var factory = new TextileFactory();
            factory.SetWorkersCount(18);

            // Запускаем производство
            for (int i = 0; i < 5; i++)
            {
                factory.ProcessWorkshops();
            }

            var products = factory.GetProductionOutput();

            // Проверяем производство специализированных тканей
            bool hasLuxuryFabrics = products.ContainsKey(TextileFactory.TextileProduct.SilkFabric);
            bool hasSpecialFabrics = products.ContainsKey(TextileFactory.TextileProduct.Denim);
            bool hasSyntheticFabrics = products.ContainsKey(TextileFactory.TextileProduct.SyntheticFabric);

            // Должны производиться различные специализированные ткани
            Assert.IsTrue(hasLuxuryFabrics || hasSpecialFabrics || hasSyntheticFabrics);
        }
    }
}