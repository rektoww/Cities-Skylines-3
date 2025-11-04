using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core.Models.Buildings.CommertialBuildings;
using Core.Enums;
using System.Linq;

namespace Tests.UnitTests
{
    /// <summary>
    /// Тесты для завода по производству алкоголя
    /// </summary>
    [TestClass]
    public sealed class AlcoholFactoryTests
    {
        /// <summary>
        /// Тест создания завода алкоголя
        /// </summary>
        [TestMethod]
        public void TestAlcoholFactoryCreation()
        {
            var factory = new AlcoholFactory();

            // Проверка статических свойств строительства
            Assert.AreEqual(280000m, AlcoholFactory.BuildCost);
            Assert.AreEqual(3, AlcoholFactory.RequiredMaterials.Count); // Изменено с 4 на 3
            Assert.AreEqual(6, AlcoholFactory.RequiredMaterials[ConstructionMaterial.Steel]);
            Assert.AreEqual(7, AlcoholFactory.RequiredMaterials[ConstructionMaterial.Concrete]);
            Assert.AreEqual(5, AlcoholFactory.RequiredMaterials[ConstructionMaterial.Glass]);
            // Медь убрана из RequiredMaterials

            // Проверка базовых свойств
            Assert.AreEqual(1200, factory.MaxMaterialStorage);
            Assert.AreEqual(600, factory.MaxProductStorage);
            Assert.AreEqual(10, factory.MaxWorkers);
            Assert.AreEqual(0, factory.WorkersCount);
            Assert.AreEqual(4, factory.Workshops.Count);
        }

        /// <summary>
        /// Тест инициализации стартовых материалов
        /// </summary>
        [TestMethod]
        public void TestStartingMaterialsInitialization()
        {
            var factory = new AlcoholFactory();

            var materials = factory.GetMaterialStorage();

            Assert.AreEqual(400, materials[AlcoholFactory.AlcoholMaterial.Wheat]);
            Assert.AreEqual(500, materials[AlcoholFactory.AlcoholMaterial.Water]);
            Assert.AreEqual(200, materials[AlcoholFactory.AlcoholMaterial.Grapes]);
        }

        /// <summary>
        /// Тест управления рабочими
        /// </summary>
        [TestMethod]
        public void TestWorkerManagement()
        {
            var factory = new AlcoholFactory();

            // Установка количества рабочих
            factory.SetWorkersCount(5);
            Assert.AreEqual(5, factory.WorkersCount);

            // Попытка установить больше рабочих, чем максимум
            factory.SetWorkersCount(15);
            Assert.AreEqual(10, factory.WorkersCount); // Должно ограничиться MaxWorkers

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
            var factory = new AlcoholFactory();

            // Получаем начальное состояние
            var initialMaterials = factory.GetMaterialStorage();
            int initialWheat = initialMaterials[AlcoholFactory.AlcoholMaterial.Wheat];
            int initialTotal = factory.GetTotalMaterialStorage();

            // Вычисляем сколько можно добавить без превышения лимита
            int availableSpace = factory.MaxMaterialStorage - initialTotal;
            int wheatToAdd = Math.Min(100, availableSpace); // Добавляем разумное количество

            // Успешное добавление пшеницы
            bool addedWheat = factory.AddMaterial(AlcoholFactory.AlcoholMaterial.Wheat, wheatToAdd);
            Assert.IsTrue(addedWheat, "Добавление пшеницы должно быть успешным");

            var materialsAfter = factory.GetMaterialStorage();
            Assert.AreEqual(initialWheat + wheatToAdd, materialsAfter[AlcoholFactory.AlcoholMaterial.Wheat]);

            // Успешное добавление воды (если осталось место)
            int remainingSpace = factory.MaxMaterialStorage - factory.GetTotalMaterialStorage();
            if (remainingSpace > 0)
            {
                int waterToAdd = Math.Min(50, remainingSpace);
                bool addedWater = factory.AddMaterial(AlcoholFactory.AlcoholMaterial.Water, waterToAdd);
                Assert.IsTrue(addedWater, "Добавление воды должно быть успешным");

                materialsAfter = factory.GetMaterialStorage();
                Assert.AreEqual(500 + waterToAdd, materialsAfter[AlcoholFactory.AlcoholMaterial.Water]);
            }
        }

        /// <summary>
        /// Тест добавления сырья с превышением вместимости
        /// </summary>
        [TestMethod]
        public void TestAddMaterialsExceedingCapacity()
        {
            var factory = new AlcoholFactory();

            // Попытка добавить больше, чем вмещает хранилище
            bool notAdded = factory.AddMaterial(AlcoholFactory.AlcoholMaterial.Wheat, 900);
            Assert.IsFalse(notAdded); // Должно вернуть false, так как 400 + 900 > 1200

            var materials = factory.GetMaterialStorage();
            Assert.AreEqual(400, materials[AlcoholFactory.AlcoholMaterial.Wheat]); // Количество не изменилось
        }

        /// <summary>
        /// Тест производства без рабочих
        /// </summary>
        [TestMethod]
        public void TestProductionWithoutWorkers()
        {
            var factory = new AlcoholFactory();

            var initialMaterials = factory.GetMaterialStorage();
            var initialProducts = factory.GetProductionOutput();

            // Запуск производства без рабочих
            factory.ProcessWorkshops();

            var finalMaterials = factory.GetMaterialStorage();
            var finalProducts = factory.GetProductionOutput();

            // Материалы и продукция не должны измениться
            Assert.AreEqual(initialMaterials[AlcoholFactory.AlcoholMaterial.Wheat],
                          finalMaterials[AlcoholFactory.AlcoholMaterial.Wheat]);
            Assert.AreEqual(initialMaterials[AlcoholFactory.AlcoholMaterial.Water],
                          finalMaterials[AlcoholFactory.AlcoholMaterial.Water]);
            Assert.AreEqual(initialProducts.Count, finalProducts.Count);
        }

        /// <summary>
        /// Тест производства с рабочими
        /// </summary>
        [TestMethod]
        public void TestProductionWithWorkers()
        {
            var factory = new AlcoholFactory();
            factory.SetWorkersCount(10); // Максимальная эффективность

            var initialMaterials = factory.GetMaterialStorage();
            var initialWheat = initialMaterials[AlcoholFactory.AlcoholMaterial.Wheat];
            var initialWater = initialMaterials[AlcoholFactory.AlcoholMaterial.Water];
            var initialGrapes = initialMaterials[AlcoholFactory.AlcoholMaterial.Grapes];

            // Запуск производства
            factory.ProcessWorkshops();

            var finalMaterials = factory.GetMaterialStorage();
            var finalProducts = factory.GetProductionOutput();

            // Материалы должны быть израсходованы
            Assert.IsTrue(finalMaterials[AlcoholFactory.AlcoholMaterial.Wheat] < initialWheat);
            Assert.IsTrue(finalMaterials[AlcoholFactory.AlcoholMaterial.Water] < initialWater);
            Assert.IsTrue(finalMaterials[AlcoholFactory.AlcoholMaterial.Grapes] < initialGrapes);

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
            var factory = new AlcoholFactory();

            // Проверка эффективности при разном количестве рабочих
            factory.SetWorkersCount(0);
            Assert.AreEqual(0f, factory.ProductionEfficiency);

            factory.SetWorkersCount(5);
            Assert.AreEqual(Math.Round(0.7f, 3), Math.Round(factory.ProductionEfficiency,3)); // 0.4 + (5/10)*0.6 = 0.7

            factory.SetWorkersCount(10);
            Assert.AreEqual(1.0f, factory.ProductionEfficiency); // 0.4 + (10/10)*0.6 = 1.0
        }

        /// <summary>
        /// Тест потребления продукции
        /// </summary>
        [TestMethod]
        public void TestProductConsumption()
        {
            var factory = new AlcoholFactory();
            factory.SetWorkersCount(10);
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
            var factory = new AlcoholFactory();
            factory.SetWorkersCount(10);
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
            var factory = new AlcoholFactory();
            factory.SetWorkersCount(6);

            var info = factory.GetProductionInfo();

            Assert.IsNotNull(info);
            Assert.AreEqual(6, info["WorkersCount"]);
            Assert.AreEqual(10, info["MaxWorkers"]);
            Assert.IsTrue((float)info["ProductionEfficiency"] > 0);
            Assert.IsTrue((int)info["TotalMaterialStorage"] > 0);
            Assert.AreEqual(1200, info["MaxMaterialStorage"]);
            Assert.AreEqual(600, info["MaxProductStorage"]);
            Assert.AreEqual(4, info["ActiveWorkshops"]);
        }

        /// <summary>
        /// Тест полного производственного цикла
        /// </summary>
        [TestMethod]
        public void TestFullProductionCycle()
        {
            var factory = new AlcoholFactory();
            factory.SetWorkersCount(10);

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
            var factory = new AlcoholFactory();
            factory.SetWorkersCount(10);

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
            var factory = new AlcoholFactory();

            // Вычисляем доступное место
            int availableSpace = factory.MaxMaterialStorage - factory.GetTotalMaterialStorage();

            // Добавляем материалы до полного заполнения
            bool added = factory.AddMaterial(AlcoholFactory.AlcoholMaterial.Wheat, availableSpace);
            Assert.IsTrue(added);
            Assert.AreEqual(factory.MaxMaterialStorage, factory.GetTotalMaterialStorage());

            // Попытка добавить еще должно вернуть false
            bool notAdded = factory.AddMaterial(AlcoholFactory.AlcoholMaterial.Water, 1);
            Assert.IsFalse(notAdded);
            Assert.AreEqual(factory.MaxMaterialStorage, factory.GetTotalMaterialStorage());
        }

        /// <summary>
        /// Тест ограничения вместимости хранилища продукции
        /// </summary>
        [TestMethod]
        public void TestProductStorageCapacity()
        {
            var factory = new AlcoholFactory();
            factory.SetWorkersCount(10);

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
            var factory = new AlcoholFactory();

            Assert.AreEqual(4, factory.Workshops.Count);

            // Проверка цеха брожения
            var fermentationWorkshop = factory.Workshops[0];
            Assert.AreEqual("Цех брожения", fermentationWorkshop.Name);
            Assert.AreEqual(8, fermentationWorkshop.ProductionCycleTime);
            Assert.AreEqual(3, fermentationWorkshop.InputRequirements.Count);
            Assert.AreEqual(2, fermentationWorkshop.OutputProducts.Count);

            // Проверка цеха дистилляции
            var distillationWorkshop = factory.Workshops[1];
            Assert.AreEqual("Цех дистилляции", distillationWorkshop.Name);
            Assert.AreEqual(12, distillationWorkshop.ProductionCycleTime);

            // Проверка цеха производства спирта
            var alcoholWorkshop = factory.Workshops[2];
            Assert.AreEqual("Цех производства спирта", alcoholWorkshop.Name);
            Assert.AreEqual(10, alcoholWorkshop.ProductionCycleTime);

            // Проверка цеха выдержки
            var agingWorkshop = factory.Workshops[3];
            Assert.AreEqual("Цех выдержки", agingWorkshop.Name);
            Assert.AreEqual(15, agingWorkshop.ProductionCycleTime);
        }

        /// <summary>
        /// Тест получения общего количества материалов
        /// </summary>
        [TestMethod]
        public void TestGetTotalMaterialStorage()
        {
            var factory = new AlcoholFactory();

            // Добавляем дополнительные материалы
            factory.AddMaterial(AlcoholFactory.AlcoholMaterial.Wheat, 100);
            factory.AddMaterial(AlcoholFactory.AlcoholMaterial.Water, 50);
            factory.AddMaterial(AlcoholFactory.AlcoholMaterial.Grapes, 30);

            int total = factory.GetTotalMaterialStorage();

            // 400 (стартовая пшеница) + 500 (стартовая вода) + 200 (стартовый виноград) + 100 + 50 + 30 = 1280
            // Но ограничено MaxMaterialStorage = 1200
            Assert.AreEqual(1200, total);
        }

        /// <summary>
        /// Тест получения общего количества продукции
        /// </summary>
        [TestMethod]
        public void TestGetTotalProductStorage()
        {
            var factory = new AlcoholFactory();
            factory.SetWorkersCount(10);
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
            Assert.AreEqual(0, (int)AlcoholFactory.AlcoholMaterial.Wheat);
            Assert.AreEqual(1, (int)AlcoholFactory.AlcoholMaterial.Water);
            Assert.AreEqual(2, (int)AlcoholFactory.AlcoholMaterial.Grapes);

            // Проверка enum продукции
            Assert.AreEqual(0, (int)AlcoholFactory.AlcoholProduct.Beer);
            Assert.AreEqual(1, (int)AlcoholFactory.AlcoholProduct.Vodka);
            Assert.AreEqual(2, (int)AlcoholFactory.AlcoholProduct.Whiskey);
            Assert.AreEqual(3, (int)AlcoholFactory.AlcoholProduct.Wine);
            Assert.AreEqual(4, (int)AlcoholFactory.AlcoholProduct.Brandy);
            Assert.AreEqual(5, (int)AlcoholFactory.AlcoholProduct.Alcohol);
        }

        /// <summary>
        /// Тест производства спирта
        /// </summary>
        [TestMethod]
        public void TestAlcoholProduction()
        {
            var factory = new AlcoholFactory();
            factory.SetWorkersCount(10);

            // Запускаем производство
            factory.ProcessWorkshops();

            var products = factory.GetProductionOutput();

            // Должен производиться спирт
            Assert.IsTrue(products.ContainsKey(AlcoholFactory.AlcoholProduct.Alcohol));
            Assert.IsTrue(products[AlcoholFactory.AlcoholProduct.Alcohol] > 0);
        }

        /// <summary>
        /// Тест многоступенчатого производства (вино -> бренди)
        /// </summary>
        [TestMethod]
        public void TestMultiStageProduction()
        {
            var factory = new AlcoholFactory();
            factory.SetWorkersCount(10);

            // Запускаем несколько циклов производства
            for (int i = 0; i < 3; i++)
            {
                factory.ProcessWorkshops();
            }

            var products = factory.GetProductionOutput();

            // Если производится вино, то должен производиться и бренди
            if (products.ContainsKey(AlcoholFactory.AlcoholProduct.Wine) && products[AlcoholFactory.AlcoholProduct.Wine] > 0)
            {
                Assert.IsTrue(products.ContainsKey(AlcoholFactory.AlcoholProduct.Brandy));
            }
        }

        /// <summary>
        /// Тест последовательного производства
        /// </summary>
        [TestMethod]
        public void TestSequentialProduction()
        {
            var factory = new AlcoholFactory();
            factory.SetWorkersCount(10);

            var productsAfterFirstCycle = factory.GetProductionOutput().Values.Sum();
            factory.ProcessWorkshops();
            var productsAfterSecondCycle = factory.GetProductionOutput().Values.Sum();

            // После второго цикла должно быть больше продукции
            Assert.IsTrue(productsAfterSecondCycle > productsAfterFirstCycle);
        }

        /// <summary>
        /// Тест производства всех видов алкоголя
        /// </summary>
        [TestMethod]
        public void TestAllAlcoholTypesProduction()
        {
            var factory = new AlcoholFactory();
            factory.SetWorkersCount(10);

            // Запускаем несколько циклов для производства всех видов алкоголя
            for (int i = 0; i < 5; i++)
            {
                factory.ProcessWorkshops();
            }

            var products = factory.GetProductionOutput();

            // Должны производиться различные виды алкоголя
            Assert.IsTrue(products.Count >= 3); // Как минимум 3 разных продукта
        }

        /// <summary>
        /// Тест эффективности при неполной загрузке рабочими
        /// </summary>
        [TestMethod]
        public void TestEfficiencyWithPartialWorkforce()
        {
            var factory = new AlcoholFactory();
            factory.SetWorkersCount(3); // 30% от максимальной workforce

            var initialMaterials = factory.GetMaterialStorage();
            factory.ProcessWorkshops();

            var finalMaterials = factory.GetMaterialStorage();
            var finalProducts = factory.GetProductionOutput();

            // При неполной загрузке все равно должно происходить производство, но с меньшей эффективностью
            Assert.IsTrue(finalMaterials[AlcoholFactory.AlcoholMaterial.Wheat] < initialMaterials[AlcoholFactory.AlcoholMaterial.Wheat]);
            Assert.IsTrue(finalProducts.Values.Sum() > 0);
        }

        /// <summary>
        /// Тест строительных материалов без меди
        /// </summary>
        [TestMethod]
        public void TestConstructionMaterialsWithoutCopper()
        {
            var requiredMaterials = AlcoholFactory.RequiredMaterials;

            // Проверяем, что присутствуют только сталь, бетон и стекло
            Assert.AreEqual(3, requiredMaterials.Count);
            Assert.IsTrue(requiredMaterials.ContainsKey(ConstructionMaterial.Steel));
            Assert.IsTrue(requiredMaterials.ContainsKey(ConstructionMaterial.Concrete));
            Assert.IsTrue(requiredMaterials.ContainsKey(ConstructionMaterial.Glass));
        }
    }
}