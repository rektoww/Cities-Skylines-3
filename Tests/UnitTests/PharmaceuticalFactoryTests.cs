using Core.Enums;
using Core.Models.Buildings.IndustrialBuildings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.UnitTests
{
    [TestClass]
    public class PharmaceuticalFactoryTests
    {
        /// <summary>
        /// Тест статических свойств стоимости строительства
        /// </summary>
        [TestMethod]
        public void TestStaticConstructionProperties()
        {
            // Проверка стоимости строительства
            Assert.AreEqual(350000m, PharmaceuticalFactory.BuildCost);

            // Проверка необходимых материалов
            var requiredMaterials = PharmaceuticalFactory.RequiredMaterials;
            Assert.AreEqual(3, requiredMaterials.Count);
            Assert.AreEqual(8, requiredMaterials[ConstructionMaterial.Steel]);
            Assert.AreEqual(10, requiredMaterials[ConstructionMaterial.Concrete]);
            Assert.AreEqual(12, requiredMaterials[ConstructionMaterial.Glass]);
        }

        /// <summary>
        /// Тест инициализации завода
        /// </summary>
        [TestMethod]
        public void TestFactoryInitialization()
        {
            var factory = new PharmaceuticalFactory();

            // Проверка базовых параметров
            Assert.AreEqual(1500, factory.MaxMaterialStorage);
            Assert.AreEqual(800, factory.MaxProductStorage);
            Assert.AreEqual(15, factory.MaxWorkers);
            Assert.AreEqual(0, factory.WorkersCount);
            Assert.AreEqual(0.8f, factory.CleanlinessLevel);
        }

        /// <summary>
        /// Тест инициализации цехов
        /// </summary>
        [TestMethod]
        public void TestWorkshopsInitialization()
        {
            var factory = new PharmaceuticalFactory();

            Assert.AreEqual(5, factory.Workshops.Count);

            // Проверка цеха производства таблеток
            var tabletWorkshop = factory.Workshops[0];
            Assert.AreEqual("Цех производства таблеток", tabletWorkshop.Name);
            Assert.AreEqual(6, tabletWorkshop.ProductionCycleTime);
            Assert.AreEqual(10, tabletWorkshop.InputRequirements["Chemicals"]);
            Assert.AreEqual(5, tabletWorkshop.InputRequirements["Minerals"]);
            Assert.AreEqual(15, tabletWorkshop.OutputProducts["Tablets"]);
            Assert.AreEqual(8, tabletWorkshop.OutputProducts["Vitamins"]);

            // Проверка цеха производства антибиотиков
            var antibioticsWorkshop = factory.Workshops[1];
            Assert.AreEqual("Цех производства антибиотиков", antibioticsWorkshop.Name);
            Assert.AreEqual(10, antibioticsWorkshop.ProductionCycleTime);
            Assert.AreEqual(15, antibioticsWorkshop.InputRequirements["Chemicals"]);
            Assert.AreEqual(8, antibioticsWorkshop.InputRequirements["Herbs"]);
            Assert.AreEqual(5, antibioticsWorkshop.InputRequirements["Water"]);
            Assert.AreEqual(6, antibioticsWorkshop.OutputProducts["Antibiotics"]);

            // Проверка цеха производства обезболивающих
            var painkillersWorkshop = factory.Workshops[2];
            Assert.AreEqual("Цех производства обезболивающих", painkillersWorkshop.Name);
            Assert.AreEqual(8, painkillersWorkshop.ProductionCycleTime);
            Assert.AreEqual(12, painkillersWorkshop.InputRequirements["Chemicals"]);
            Assert.AreEqual(6, painkillersWorkshop.InputRequirements["Plants"]);
            Assert.AreEqual(10, painkillersWorkshop.OutputProducts["Painkillers"]);

            // Проверка цеха производства сиропов
            var syrupWorkshop = factory.Workshops[3];
            Assert.AreEqual("Цех производства сиропов", syrupWorkshop.Name);
            Assert.AreEqual(7, syrupWorkshop.ProductionCycleTime);
            Assert.AreEqual(10, syrupWorkshop.InputRequirements["Herbs"]);
            Assert.AreEqual(8, syrupWorkshop.InputRequirements["Water"]);
            Assert.AreEqual(4, syrupWorkshop.InputRequirements["Plants"]);
            Assert.AreEqual(12, syrupWorkshop.OutputProducts["Syrups"]);

            // Проверка цеха медицинских материалов
            var medicalWorkshop = factory.Workshops[4];
            Assert.AreEqual("Цех медицинских материалов", medicalWorkshop.Name);
            Assert.AreEqual(5, medicalWorkshop.ProductionCycleTime);
            Assert.AreEqual(8, medicalWorkshop.InputRequirements["Chemicals"]);
            Assert.AreEqual(3, medicalWorkshop.InputRequirements["Plants"]);
            Assert.AreEqual(10, medicalWorkshop.OutputProducts["Antiseptics"]);
            Assert.AreEqual(20, medicalWorkshop.OutputProducts["Bandages"]);
        }


        /// <summary>
        /// Тест установки уровня чистоты
        /// </summary>
        [TestMethod]
        public void TestSetCleanlinessLevel()
        {
            var factory = new PharmaceuticalFactory();

            // Установка нормального уровня
            factory.SetCleanlinessLevel(0.95f);
            Assert.AreEqual(0.95f, factory.CleanlinessLevel);

            // Установка максимального уровня
            factory.SetCleanlinessLevel(1.0f);
            Assert.AreEqual(1.0f, factory.CleanlinessLevel);

            // Установка минимального уровня
            factory.SetCleanlinessLevel(0.1f);
            Assert.AreEqual(0.1f, factory.CleanlinessLevel);

            // Установка превышающего максимальный уровень
            factory.SetCleanlinessLevel(1.5f);
            Assert.AreEqual(1.0f, factory.CleanlinessLevel);

            // Установка ниже минимального уровня
            factory.SetCleanlinessLevel(-0.1f);
            Assert.AreEqual(0.1f, factory.CleanlinessLevel);
        }

        /// <summary>
        /// Тест расчета эффективности производства
        /// </summary>
        [TestMethod]
        public void TestProductionEfficiencyCalculation()
        {
            var factory = new PharmaceuticalFactory();

            // Без рабочих - эффективность 0
            factory.SetWorkersCount(0);
            Assert.AreEqual(0f, factory.ProductionEfficiency);

            // Половина рабочих, полная чистота
            factory.SetWorkersCount(7);
            factory.SetCleanlinessLevel(1.0f);
            float expectedEfficiency1 = (0.3f + (7f / 15f) * 0.5f) * 1.0f;
            Assert.AreEqual(expectedEfficiency1, factory.ProductionEfficiency);

            // Полные рабочие, полная чистота
            factory.SetWorkersCount(15);
            factory.SetCleanlinessLevel(1.0f);
            float expectedEfficiency2 = (0.3f + 1.0f * 0.5f) * 1.0f;
            Assert.AreEqual(expectedEfficiency2, factory.ProductionEfficiency);

            // Полные рабочие, низкая чистота
            factory.SetWorkersCount(15);
            factory.SetCleanlinessLevel(0.5f);
            float expectedEfficiency3 = (0.3f + 1.0f * 0.5f) * 0.5f;
            Assert.AreEqual(expectedEfficiency3, factory.ProductionEfficiency);

            // Малое количество рабочих, средняя чистота
            factory.SetWorkersCount(3);
            factory.SetCleanlinessLevel(0.7f);
            float expectedEfficiency4 = (0.3f + (3f / 15f) * 0.5f) * 0.7f;
            Assert.AreEqual(expectedEfficiency4, factory.ProductionEfficiency);
        }

        /// <summary>
        /// Тест производственного процесса без рабочих
        /// </summary>
        [TestMethod]
        public void TestProcessWorkshopsWithNoWorkers()
        {
            var factory = new PharmaceuticalFactory();

            // Убеждаемся, что рабочих нет
            factory.SetWorkersCount(0);

            var initialMaterials = factory.GetTotalMaterialStorage();
            var initialProducts = factory.GetTotalProductStorage();

            // Запускаем производственный процесс
            factory.ProcessWorkshops();

            var finalMaterials = factory.GetTotalMaterialStorage();
            var finalProducts = factory.GetTotalProductStorage();

            // При отсутствии рабочих материалы и продукция не должны измениться
            Assert.AreEqual(initialMaterials, finalMaterials);
            Assert.AreEqual(initialProducts, finalProducts);
        }

        /// <summary>
        /// Тест производственного процесса с рабочими
        /// </summary>
        [TestMethod]
        public void TestProcessWorkshopsWithWorkers()
        {
            var factory = new PharmaceuticalFactory();
            factory.SetWorkersCount(10);
            factory.SetCleanlinessLevel(1.0f);

            var initialMaterials = factory.GetTotalMaterialStorage();
            var initialProducts = factory.GetTotalProductStorage();

            // Запускаем производственный процесс
            factory.ProcessWorkshops();

            var finalMaterials = factory.GetTotalMaterialStorage();
            var finalProducts = factory.GetTotalProductStorage();

            // При наличии рабочих материалы должны уменьшиться, а продукция - увеличиться
            Assert.IsTrue(finalMaterials < initialMaterials);
            Assert.IsTrue(finalProducts > initialProducts);
        }

        /// <summary>
        /// Тест потребления продукции
        /// </summary>
        [TestMethod]
        public void TestConsumeProductFunctionality()
        {
            var factory = new PharmaceuticalFactory();

            // Сначала производим продукцию
            factory.SetWorkersCount(10);
            factory.ProcessWorkshops();

            var initialProducts = factory.GetProductionOutput();

            if (initialProducts.Count > 0)
            {
                var productType = initialProducts.First().Key;
                var initialAmount = initialProducts.First().Value;

                // Успешное потребление продукции
                bool result1 = factory.ConsumeProduct(productType, 1);
                Assert.IsTrue(result1);
                Assert.AreEqual(initialAmount - 1, factory.GetProductionOutput()[productType]);

                // Попытка потребить больше чем есть
                bool result2 = factory.ConsumeProduct(productType, initialAmount + 100);
                Assert.IsFalse(result2);

                // Потребление до нуля - продукт должен удалиться из словаря
                factory.ConsumeProduct(productType, initialAmount - 1);
                Assert.IsFalse(factory.GetProductionOutput().ContainsKey(productType));
            }
        }

        /// <summary>
        /// Тест потребления несуществующей продукции
        /// </summary>
        [TestMethod]
        public void TestConsumeNonExistentProduct()
        {
            var factory = new PharmaceuticalFactory();

            // Попытка потребить продукцию, которой нет на складе
            bool result = factory.ConsumeProduct(PharmaceuticalFactory.PharmaceuticalProduct.Antibiotics, 1);
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Тест полного производственного цикла
        /// </summary>
        [TestMethod]
        public void TestFullProductionCycle()
        {
            var factory = new PharmaceuticalFactory();
            factory.SetWorkersCount(10);

            var initialMaterials = factory.GetTotalMaterialStorage();
            var initialProducts = factory.GetTotalProductStorage();

            // Запускаем полный цикл
            factory.FullProductionCycle();

            var finalMaterials = factory.GetTotalMaterialStorage();
            var finalProducts = factory.GetTotalProductStorage();

            // Проверяем, что производство произошло
            Assert.IsTrue(finalMaterials < initialMaterials);
            Assert.IsTrue(finalProducts > initialProducts);
        }

        /// <summary>
        /// Тест получения информации о производстве
        /// </summary>
        [TestMethod]
        public void TestGetProductionInfo()
        {
            var factory = new PharmaceuticalFactory();
            factory.SetWorkersCount(8);
            factory.SetCleanlinessLevel(0.9f);

            var info = factory.GetProductionInfo();

            // Проверка всех полей информации
            Assert.AreEqual(8, info["WorkersCount"]);
            Assert.AreEqual(15, info["MaxWorkers"]);
            Assert.AreEqual(0.9f, info["CleanlinessLevel"]);
            Assert.AreEqual(1500, info["MaxMaterialStorage"]);
            Assert.AreEqual(800, info["MaxProductStorage"]);
            Assert.AreEqual(5, info["ActiveWorkshops"]);

            // Проверка расчетных полей
            Assert.IsTrue((float)info["ProductionEfficiency"] > 0);
            Assert.IsTrue((int)info["TotalMaterialStorage"] > 0);
            Assert.IsTrue((int)info["TotalProductStorage"] >= 0);
        }

        /// <summary>
        /// Тест метода OnBuildingPlaced
        /// </summary>
        [TestMethod]
        public void TestOnBuildingPlaced()
        {
            var factory = new PharmaceuticalFactory();
            factory.SetWorkersCount(5);

            var initialProducts = factory.GetTotalProductStorage();

            // Вызываем метод размещения здания
            factory.OnBuildingPlaced();

            var finalProducts = factory.GetTotalProductStorage();

            // Проверяем, что производство запустилось
            Assert.IsTrue(finalProducts > initialProducts);
        }


        /// <summary>
        /// Тест получения словаря продукции
        /// </summary>
        [TestMethod]
        public void TestGetProductionOutput()
        {
            var factory = new PharmaceuticalFactory();
            var products = factory.GetProductionOutput();

            // Изначально склад продукции пуст
            Assert.IsNotNull(products);
            Assert.AreEqual(0, products.Count);
            Assert.IsInstanceOfType(products, typeof(Dictionary<PharmaceuticalFactory.PharmaceuticalProduct, int>));

            // После производства продукция должна появиться
            factory.SetWorkersCount(10);
            factory.ProcessWorkshops();

            var productsAfterProduction = factory.GetProductionOutput();
            Assert.IsTrue(productsAfterProduction.Count > 0);
        }

        /// <summary>
        /// Тест ограничения вместимости склада продукции
        /// </summary>
        [TestMethod]
        public void TestProductStorageCapacityLimit()
        {
            var factory = new PharmaceuticalFactory();
            factory.SetWorkersCount(15);
            factory.SetCleanlinessLevel(1.0f);

            // Многократно запускаем производство до заполнения склада
            for (int i = 0; i < 10; i++)
            {
                factory.ProcessWorkshops();
            }

            var totalProducts = factory.GetTotalProductStorage();

            // Проверяем, что не превышена максимальная вместимость
            Assert.IsTrue(totalProducts <= factory.MaxProductStorage);
        }

        /// <summary>
        /// Тест эффективности при граничных значениях
        /// </summary>
        [TestMethod]
        public void TestEfficiencyEdgeCases()
        {
            var factory = new PharmaceuticalFactory();

            // Минимальное количество рабочих
            factory.SetWorkersCount(1);
            factory.SetCleanlinessLevel(0.1f);
            float minEfficiency = factory.ProductionEfficiency;
            Assert.IsTrue(minEfficiency > 0f);

            // Максимальное количество рабочих и чистота
            factory.SetWorkersCount(15);
            factory.SetCleanlinessLevel(1.0f);
            float maxEfficiency = factory.ProductionEfficiency;
            Assert.AreEqual(0.8f, maxEfficiency);
        }
    }
}
