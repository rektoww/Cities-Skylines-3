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
    public class RecyclingPlantTests
    {
        /// <summary>
        /// Тест статических свойств стоимости строительства
        /// </summary>
        [TestMethod]
        public void TestStaticConstructionProperties()
        {
            // Проверка стоимости строительства
            Assert.AreEqual(280000m, RecyclingPlant.BuildCost);

            // Проверка необходимых материалов
            var requiredMaterials = RecyclingPlant.RequiredMaterials;
            Assert.AreEqual(3, requiredMaterials.Count);
            Assert.AreEqual(6, requiredMaterials[ConstructionMaterial.Steel]);
            Assert.AreEqual(8, requiredMaterials[ConstructionMaterial.Concrete]);
            Assert.AreEqual(4, requiredMaterials[ConstructionMaterial.Glass]);
        }

        /// <summary>
        /// Тест инициализации завода
        /// </summary>
        [TestMethod]
        public void TestFactoryInitialization()
        {
            var plant = new RecyclingPlant();

            // Проверка базовых параметров
            Assert.AreEqual(2000, plant.MaxWasteStorage);
            Assert.AreEqual(1200, plant.MaxProductStorage);
            Assert.AreEqual(12, plant.MaxWorkers);
            Assert.AreEqual(0, plant.WorkersCount);
            Assert.AreEqual(0.7f, plant.EcoEfficiency);
        }

        /// <summary>
        /// Тест инициализации цехов
        /// </summary>
        [TestMethod]
        public void TestWorkshopsInitialization()
        {
            var plant = new RecyclingPlant();

            Assert.AreEqual(5, plant.Workshops.Count);

            // Проверка цеха переработки пластика
            var plasticWorkshop = plant.Workshops[0];
            Assert.AreEqual("Цех переработки пластика", plasticWorkshop.Name);
            Assert.AreEqual(5, plasticWorkshop.ProductionCycleTime);
            Assert.AreEqual(8, plasticWorkshop.InputRequirements["Plastic"]);
            Assert.AreEqual(6, plasticWorkshop.OutputProducts["RecycledPlastic"]);

            // Проверка цеха переработки бумаги
            var paperWorkshop = plant.Workshops[1];
            Assert.AreEqual("Цех переработки бумаги", paperWorkshop.Name);
            Assert.AreEqual(4, paperWorkshop.ProductionCycleTime);
            Assert.AreEqual(10, paperWorkshop.InputRequirements["Paper"]);
            Assert.AreEqual(8, paperWorkshop.OutputProducts["RecycledPaper"]);

            // Проверка цеха переработки стекла
            var glassWorkshop = plant.Workshops[2];
            Assert.AreEqual("Цех переработки стекла", glassWorkshop.Name);
            Assert.AreEqual(6, glassWorkshop.ProductionCycleTime);
            Assert.AreEqual(12, glassWorkshop.InputRequirements["Glass"]);
            Assert.AreEqual(10, glassWorkshop.OutputProducts["RecycledGlass"]);

            // Проверка цеха переработки металла
            var metalWorkshop = plant.Workshops[3];
            Assert.AreEqual("Цех переработки металла", metalWorkshop.Name);
            Assert.AreEqual(7, metalWorkshop.ProductionCycleTime);
            Assert.AreEqual(15, metalWorkshop.InputRequirements["Metal"]);
            Assert.AreEqual(12, metalWorkshop.OutputProducts["RecycledMetal"]);

            // Проверка цеха переработки органики
            var organicWorkshop = plant.Workshops[4];
            Assert.AreEqual("Цех переработки органики", organicWorkshop.Name);
            Assert.AreEqual(8, organicWorkshop.ProductionCycleTime);
            Assert.AreEqual(20, organicWorkshop.InputRequirements["Organic"]);
            Assert.AreEqual(15, organicWorkshop.OutputProducts["Compost"]);
            Assert.AreEqual(8, organicWorkshop.OutputProducts["Biogas"]);
        }

        /// <summary>
        /// Тест установки экологической эффективности
        /// </summary>
        [TestMethod]
        public void TestSetEcoEfficiency()
        {
            var plant = new RecyclingPlant();

            // Установка нормального уровня
            plant.SetEcoEfficiency(0.85f);
            Assert.AreEqual(0.85f, plant.EcoEfficiency);

            // Установка максимального уровня
            plant.SetEcoEfficiency(1.0f);
            Assert.AreEqual(1.0f, plant.EcoEfficiency);

            // Установка минимального уровня
            plant.SetEcoEfficiency(0.1f);
            Assert.AreEqual(0.1f, plant.EcoEfficiency);

            // Установка превышающего максимальный уровень
            plant.SetEcoEfficiency(1.5f);
            Assert.AreEqual(1.0f, plant.EcoEfficiency);

            // Установка ниже минимального уровня
            plant.SetEcoEfficiency(-0.1f);
            Assert.AreEqual(0.1f, plant.EcoEfficiency);
        }

        /// <summary>
        /// Тест эффективности при граничных значениях
        /// </summary>
        [TestMethod]
        public void TestEfficiencyEdgeCases()
        {
            var plant = new RecyclingPlant();

            // Минимальное количество рабочих
            plant.SetWorkersCount(1);
            plant.SetEcoEfficiency(0.1f);
            float minEfficiency = plant.RecyclingEfficiency;
            Assert.IsTrue(minEfficiency > 0f);

            // Максимальное количество рабочих и эффективность
            plant.SetWorkersCount(12);
            plant.SetEcoEfficiency(1.0f);
            float maxEfficiency = plant.RecyclingEfficiency;
            Assert.AreEqual(0.8f, maxEfficiency);
        }

        /// <summary>
        /// Тест расчета эффективности переработки
        /// </summary>
        [TestMethod]
        public void TestRecyclingEfficiencyCalculation()
        {
            var plant = new RecyclingPlant();

            // Без рабочих - эффективность 0
            plant.SetWorkersCount(0);
            Assert.AreEqual(0f, plant.RecyclingEfficiency);

            // Половина рабочих, полная эффективность
            plant.SetWorkersCount(6);
            plant.SetEcoEfficiency(1.0f);
            float expectedEfficiency1 = (0.4f + (6f / 12f) * 0.4f) * 1.0f;
            Assert.AreEqual(expectedEfficiency1, plant.RecyclingEfficiency);

            // Полные рабочие, полная эффективность
            plant.SetWorkersCount(12);
            plant.SetEcoEfficiency(1.0f);
            float expectedEfficiency2 = (0.4f + 1.0f * 0.4f) * 1.0f;
            Assert.AreEqual(expectedEfficiency2, plant.RecyclingEfficiency);

            // Полные рабочие, низкая эффективность
            plant.SetWorkersCount(12);
            plant.SetEcoEfficiency(0.5f);
            float expectedEfficiency3 = (0.4f + 1.0f * 0.4f) * 0.5f;
            Assert.AreEqual(expectedEfficiency3, plant.RecyclingEfficiency);
        }

        /// <summary>
        /// Тест процесса переработки без рабочих
        /// </summary>
        [TestMethod]
        public void TestProcessWorkshopsWithNoWorkers()
        {
            var plant = new RecyclingPlant();

            // Убеждаемся, что рабочих нет
            plant.SetWorkersCount(0);

            var initialWaste = plant.GetTotalWasteStorage();
            var initialProducts = plant.GetTotalProductStorage();

            // Запускаем процесс переработки
            plant.ProcessWorkshops();

            var finalWaste = plant.GetTotalWasteStorage();
            var finalProducts = plant.GetTotalProductStorage();

            // При отсутствии рабочих отходы и продукция не должны измениться
            Assert.AreEqual(initialWaste, finalWaste);
            Assert.AreEqual(initialProducts, finalProducts);
        }

        /// <summary>
        /// Тест процесса переработки с рабочими
        /// </summary>
        [TestMethod]
        public void TestProcessWorkshopsWithWorkers()
        {
            var plant = new RecyclingPlant();
            plant.SetWorkersCount(8);
            plant.SetEcoEfficiency(1.0f);

            var initialWaste = plant.GetTotalWasteStorage();
            var initialProducts = plant.GetTotalProductStorage();

            // Запускаем процесс переработки
            plant.ProcessWorkshops();

            var finalWaste = plant.GetTotalWasteStorage();
            var finalProducts = plant.GetTotalProductStorage();

            // При наличии рабочих отходы должны уменьшиться, а продукция - увеличиться
            Assert.IsTrue(finalWaste < initialWaste);
            Assert.IsTrue(finalProducts > initialProducts);
        }

        /// <summary>
        /// Тест потребления переработанной продукции
        /// </summary>
        [TestMethod]
        public void TestConsumeProductFunctionality()
        {
            var plant = new RecyclingPlant();

            // Сначала запускаем переработку
            plant.SetWorkersCount(8);
            plant.ProcessWorkshops();

            var initialProducts = plant.GetRecyclingOutput();

            if (initialProducts.Count > 0)
            {
                var productType = initialProducts.First().Key;
                var initialAmount = initialProducts.First().Value;

                // Успешное потребление продукции
                bool result1 = plant.ConsumeProduct(productType, 1);
                Assert.IsTrue(result1);
                Assert.AreEqual(initialAmount - 1, plant.GetRecyclingOutput()[productType]);

                // Попытка потребить больше чем есть
                bool result2 = plant.ConsumeProduct(productType, initialAmount + 100);
                Assert.IsFalse(result2);

                // Потребление до нуля - продукт должен удалиться из словаря
                plant.ConsumeProduct(productType, initialAmount - 1);
                Assert.IsFalse(plant.GetRecyclingOutput().ContainsKey(productType));
            }
        }

        /// <summary>
        /// Тест добавления отходов
        /// </summary>
        [TestMethod]
        public void TestAddWasteFunctionality()
        {
            var plant = new RecyclingPlant();

            var initialWaste = plant.GetTotalWasteStorage();

            // Успешное добавление отходов
            bool result1 = plant.AddWaste(RecyclingPlant.WasteMaterial.Plastic, 100);
            Assert.IsTrue(result1);
            Assert.AreEqual(initialWaste + 100, plant.GetTotalWasteStorage());

            // Попытка добавить слишком много отходов
            bool result2 = plant.AddWaste(RecyclingPlant.WasteMaterial.Plastic, 5000);
            Assert.IsFalse(result2);
        }

        /// <summary>
        /// Тест полного цикла переработки
        /// </summary>
        [TestMethod]
        public void TestFullRecyclingCycle()
        {
            var plant = new RecyclingPlant();
            plant.SetWorkersCount(8);

            var initialWaste = plant.GetTotalWasteStorage();
            var initialProducts = plant.GetTotalProductStorage();

            // Запускаем полный цикл
            plant.FullRecyclingCycle();

            var finalWaste = plant.GetTotalWasteStorage();
            var finalProducts = plant.GetTotalProductStorage();

            // Проверяем, что переработка произошла
            Assert.IsTrue(finalWaste < initialWaste);
            Assert.IsTrue(finalProducts > initialProducts);
        }

        /// <summary>
        /// Тест получения информации о переработке
        /// </summary>
        [TestMethod]
        public void TestGetRecyclingInfo()
        {
            var plant = new RecyclingPlant();
            plant.SetWorkersCount(6);
            plant.SetEcoEfficiency(0.9f);

            var info = plant.GetRecyclingInfo();

            // Проверка всех полей информации
            Assert.AreEqual(6, info["WorkersCount"]);
            Assert.AreEqual(12, info["MaxWorkers"]);
            Assert.AreEqual(0.9f, info["EcoEfficiency"]);
            Assert.AreEqual(2000, info["MaxWasteStorage"]);
            Assert.AreEqual(1200, info["MaxProductStorage"]);
            Assert.AreEqual(5, info["ActiveWorkshops"]);

            // Проверка расчетных полей
            Assert.IsTrue((float)info["RecyclingEfficiency"] > 0);
            Assert.IsTrue((int)info["TotalWasteStorage"] > 0);
            Assert.IsTrue((int)info["TotalProductStorage"] >= 0);
        }

        /// <summary>
        /// Тест метода OnBuildingPlaced
        /// </summary>
        [TestMethod]
        public void TestOnBuildingPlaced()
        {
            var plant = new RecyclingPlant();
            plant.SetWorkersCount(5);

            var initialProducts = plant.GetTotalProductStorage();

            // Вызываем метод размещения здания
            plant.OnBuildingPlaced();

            var finalProducts = plant.GetTotalProductStorage();

            // Проверяем, что переработка запустилась
            Assert.IsTrue(finalProducts > initialProducts);
        }

        /// <summary>
        /// Тест ограничения вместимости склада
        /// </summary>
        [TestMethod]
        public void TestStorageCapacityLimit()
        {
            var plant = new RecyclingPlant();
            plant.SetWorkersCount(12);
            plant.SetEcoEfficiency(1.0f);

            // Многократно запускаем переработку до заполнения склада
            for (int i = 0; i < 10; i++)
            {
                plant.ProcessWorkshops();
            }

            var totalProducts = plant.GetTotalProductStorage();

            // Проверяем, что не превышена максимальная вместимость
            Assert.IsTrue(totalProducts <= plant.MaxProductStorage);
        }
    }
}
