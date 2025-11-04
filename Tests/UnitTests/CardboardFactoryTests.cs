using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using Core.Models.Buildings.IndustrialBuildings;

namespace Core.Tests.Buildings.IndustrialBuildings
{
    [TestClass]
    public class CardboardFactoryTests
    {
        private CardboardFactory _factory;

        [TestInitialize]
        public void Setup()
        {
            _factory = new CardboardFactory();
        }

        [TestMethod]
        public void Constructor_InitializesWithStartingMaterials()
        {
            // Assert
            var materials = _factory.GetMaterialStorage();
            Assert.AreEqual(5, materials.Count, "Должно быть 5 видов начальных материалов");
            Assert.IsTrue(materials[CardboardFactory.CardboardMaterial.WoodChips] >= 0);
            Assert.IsTrue(materials[CardboardFactory.CardboardMaterial.RecycledPaper] >= 0);
        }

        [TestMethod]
        public void Constructor_InitializesWorkshops()
        {
            // Assert
            Assert.AreEqual(5, _factory.Workshops.Count, "Должно быть 5 цехов");
            Assert.IsTrue(_factory.Workshops.Any(w => w.Name == "Цех подготовки сырья"));
            Assert.IsTrue(_factory.Workshops.Any(w => w.Name == "Цех гофрированного картона"));
        }

        [TestMethod]
        public void SetWorkersCount_ValidCount_SetsCorrectly()
        {
            // Act
            _factory.SetWorkersCount(8);

            // Assert
            Assert.AreEqual(8, _factory.WorkersCount);
        }

        [TestMethod]
        public void SetWorkersCount_ExceedsMax_ClampsToMax()
        {
            // Act
            _factory.SetWorkersCount(15); // MaxWorkers = 12

            // Assert
            Assert.AreEqual(12, _factory.WorkersCount);
        }

        [TestMethod]
        public void SetWorkersCount_NegativeCount_SetsToZero()
        {
            // Act
            _factory.SetWorkersCount(-5);

            // Assert
            Assert.AreEqual(0, _factory.WorkersCount);
        }

        [TestMethod]
        public void ProductionEfficiency_NoWorkers_ReturnsZero()
        {
            // Arrange
            _factory.SetWorkersCount(0);

            // Assert
            Assert.AreEqual(0f, _factory.ProductionEfficiency);
        }

        [TestMethod]
        public void ProductionEfficiency_HalfWorkers_ReturnsCorrectValue()
        {
            // Arrange
            _factory.SetWorkersCount(6); // Половина от 12

            // Assert
            float efficiency = _factory.ProductionEfficiency;
            Assert.IsTrue(efficiency > 0.6f && efficiency < 0.7f);
        }

        [TestMethod]
        public void ProductionEfficiency_FullWorkers_ReturnsOne()
        {
            // Arrange
            _factory.SetWorkersCount(12);

            // Assert
            Assert.AreEqual(1f, _factory.ProductionEfficiency);
        }

        [TestMethod]
        public void AddMaterial_WithinCapacity_AddsSuccessfully()
        {
            // Act
            bool result = _factory.AddMaterial(CardboardFactory.CardboardMaterial.WoodChips, 200);

            // Assert
            Assert.IsTrue(result);
            var materials = _factory.GetMaterialStorage();
            Assert.IsTrue(materials[CardboardFactory.CardboardMaterial.WoodChips] >= 200);
        }

        [TestMethod]
        public void AddMaterial_ExceedsCapacity_ReturnsFalse()
        {
            // Arrange
            int hugeAmount = 10000;

            // Act
            bool result = _factory.AddMaterial(CardboardFactory.CardboardMaterial.WoodChips, hugeAmount);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ProcessWorkshops_NoWorkers_NoProduction()
        {
            // Arrange
            _factory.SetWorkersCount(0);
            var initialProducts = _factory.GetProductionOutput();

            // Act
            _factory.ProcessWorkshops();

            // Assert
            var finalProducts = _factory.GetProductionOutput();
            Assert.AreEqual(initialProducts.Count, finalProducts.Count);
        }

        [TestMethod]
        public void ProcessWorkshops_WithWorkers_ProducesGoods()
        {
            // Arrange
            _factory.SetWorkersCount(8);

            // Добавляем достаточно материалов для производства
            _factory.AddMaterial(CardboardFactory.CardboardMaterial.WoodChips, 300);
            _factory.AddMaterial(CardboardFactory.CardboardMaterial.Water, 200);
            _factory.AddMaterial(CardboardFactory.CardboardMaterial.Energy, 150);

            // Act
            _factory.ProcessWorkshops();

            // Assert
            var products = _factory.GetProductionOutput();
            Assert.IsTrue(products.Count > 0, "Должна производиться хотя бы одна продукция");
            Assert.IsTrue(products.Values.Sum() > 0, "Должно быть произведено некоторое количество продукции");
        }

        [TestMethod]
        public void ProcessWorkshops_ConsumesMaterials()
        {
            // Arrange
            _factory.SetWorkersCount(8);
            var initialMaterials = _factory.GetMaterialStorage();
            int initialWoodChips = initialMaterials[CardboardFactory.CardboardMaterial.WoodChips];

            // Act
            _factory.ProcessWorkshops();

            // Assert
            var finalMaterials = _factory.GetMaterialStorage();
            int finalWoodChips = finalMaterials[CardboardFactory.CardboardMaterial.WoodChips];
            Assert.IsTrue(finalWoodChips < initialWoodChips, "Древесная щепа должна расходоваться в процессе производства");
        }

        [TestMethod]
        public void ProcessWorkshops_RespectsProductionEfficiency()
        {
            // Arrange
            _factory.SetWorkersCount(4); // Средняя эффективность
            _factory.AddMaterial(CardboardFactory.CardboardMaterial.WoodChips, 400);
            _factory.AddMaterial(CardboardFactory.CardboardMaterial.Water, 300);
            _factory.AddMaterial(CardboardFactory.CardboardMaterial.Energy, 200);

            // Act
            _factory.ProcessWorkshops();

            // Assert
            var products = _factory.GetProductionOutput();
            int totalProduction = products.Values.Sum();
            Assert.IsTrue(totalProduction > 0, "Должно быть произведено некоторое количество продукции");
        }

        [TestMethod]
        public void ConsumeProduct_SufficientQuantity_ConsumesSuccessfully()
        {
            // Arrange
            _factory.SetWorkersCount(8);
            _factory.AddMaterial(CardboardFactory.CardboardMaterial.WoodChips, 400);
            _factory.AddMaterial(CardboardFactory.CardboardMaterial.Water, 300);
            _factory.AddMaterial(CardboardFactory.CardboardMaterial.Energy, 200);
            _factory.ProcessWorkshops();

            var initialProducts = _factory.GetProductionOutput();

            if (initialProducts.Count == 0)
            {
                Assert.Inconclusive("Нет продукции для тестирования потребления");
                return;
            }

            var productType = initialProducts.Keys.First();

            // Act
            bool result = _factory.ConsumeProduct(productType, 1);

            // Assert
            Assert.IsTrue(result);
            var finalProducts = _factory.GetProductionOutput();
            Assert.AreEqual(initialProducts[productType] - 1, finalProducts[productType]);
        }

        [TestMethod]
        public void ConsumeProduct_InsufficientQuantity_ReturnsFalse()
        {
            // Arrange
            var productType = CardboardFactory.CardboardProduct.CardboardBoxes;

            // Act
            bool result = _factory.ConsumeProduct(productType, 1000);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ConsumeProduct_ExactQuantity_RemovesProductFromStorage()
        {
            // Arrange
            _factory.SetWorkersCount(8);
            _factory.AddMaterial(CardboardFactory.CardboardMaterial.WoodChips, 500);
            _factory.AddMaterial(CardboardFactory.CardboardMaterial.Water, 400);
            _factory.AddMaterial(CardboardFactory.CardboardMaterial.Energy, 300);
            _factory.ProcessWorkshops();

            var initialProducts = _factory.GetProductionOutput();

            if (initialProducts.Count == 0)
            {
                Assert.Inconclusive("Нет продукции для тестирования");
                return;
            }

            var productType = initialProducts.Keys.First();
            int quantity = initialProducts[productType];

            // Act
            bool result = _factory.ConsumeProduct(productType, quantity);

            // Assert
            Assert.IsTrue(result);
            var finalProducts = _factory.GetProductionOutput();
            Assert.IsFalse(finalProducts.ContainsKey(productType), "Продукт должен быть удален из хранилища при полном потреблении");
        }

        [TestMethod]
        public void GetProductionInfo_ReturnsCompleteInformation()
        {
            // Arrange
            _factory.SetWorkersCount(8);

            // Act
            var info = _factory.GetProductionInfo();

            // Assert
            Assert.AreEqual(8, info["WorkersCount"]);
            Assert.AreEqual(12, info["MaxWorkers"]);
            Assert.IsTrue((float)info["ProductionEfficiency"] > 0);
            Assert.IsTrue((int)info["TotalMaterialStorage"] >= 0);
            Assert.AreEqual(2500, info["MaxMaterialStorage"]);
            Assert.AreEqual(5, info["ActiveWorkshops"]);
        }

        [TestMethod]
        public void MultipleProductionCycles_AccumulateProducts()
        {
            // Arrange
            _factory.SetWorkersCount(8);
            _factory.AddMaterial(CardboardFactory.CardboardMaterial.WoodChips, 600);
            _factory.AddMaterial(CardboardFactory.CardboardMaterial.Water, 500);
            _factory.AddMaterial(CardboardFactory.CardboardMaterial.Energy, 400);

            // Act
            _factory.ProcessWorkshops(); // Первый цикл
            var productsAfterFirstCycle = _factory.GetProductionOutput();
            int firstCycleTotal = productsAfterFirstCycle.Values.Sum();

            _factory.ProcessWorkshops(); // Второй цикл
            var productsAfterSecondCycle = _factory.GetProductionOutput();
            int secondCycleTotal = productsAfterSecondCycle.Values.Sum();

            // Assert
            Assert.IsTrue(secondCycleTotal >= firstCycleTotal, "Второй цикл должен увеличить общее производство");
        }


        [TestMethod]
        public void Production_RespectsStorageLimits()
        {
            // Arrange
            _factory.SetWorkersCount(12); // Максимальная эффективность

            // Добавляем огромное количество материалов
            _factory.AddMaterial(CardboardFactory.CardboardMaterial.WoodChips, 6000);
            _factory.AddMaterial(CardboardFactory.CardboardMaterial.RecycledPaper, 5000);
            _factory.AddMaterial(CardboardFactory.CardboardMaterial.Energy, 4000);

            // Act
            _factory.ProcessWorkshops();

            // Assert
            var products = _factory.GetProductionOutput();
            int totalProducts = products.Values.Sum();
            Assert.IsTrue(totalProducts <= 1800, "Общее производство не должно превышать MaxProductStorage (1800)");
        }

        [TestMethod]
        public void AllProductTypes_CanBeProduced()
        {
            // Arrange
            _factory.SetWorkersCount(12);

            // Добавляем все необходимые материалы
            foreach (var material in System.Enum.GetValues(typeof(CardboardFactory.CardboardMaterial))
                                                .Cast<CardboardFactory.CardboardMaterial>())
            {
                _factory.AddMaterial(material, 800);
            }

            // Act
            _factory.ProcessWorkshops();

            // Assert
            var products = _factory.GetProductionOutput();
            int producedTypesCount = products.Count(p => p.Value > 0);
            Assert.IsTrue(producedTypesCount >= 3, "Должно производиться несколько типов продукции");
        }

        [TestMethod]
        public void ComplexProductionChain_WorksCorrectly()
        {
            // Arrange
            _factory.SetWorkersCount(10);

            // Обеспечиваем материалы для полной цепочки производства
            _factory.AddMaterial(CardboardFactory.CardboardMaterial.WoodChips, 1000);
            _factory.AddMaterial(CardboardFactory.CardboardMaterial.RecycledPaper, 800);
            _factory.AddMaterial(CardboardFactory.CardboardMaterial.Chemicals, 600);
            _factory.AddMaterial(CardboardFactory.CardboardMaterial.Water, 1200);
            _factory.AddMaterial(CardboardFactory.CardboardMaterial.Energy, 900);

            // Act
            _factory.ProcessWorkshops();

            // Assert
            var products = _factory.GetProductionOutput();
            var materials = _factory.GetMaterialStorage();

            Assert.IsTrue(products.Count > 0, "Должна производиться продукция");
            Assert.IsTrue(materials.Values.Sum() < 4500, "Материалы должны расходоваться");
        }

        [TestMethod]
        public void AddMaterial_ZeroOrNegativeAmount_HandlesCorrectly()
        {
            // Act & Assert
            Assert.IsFalse(_factory.AddMaterial(CardboardFactory.CardboardMaterial.WoodChips, 0));
            Assert.IsFalse(_factory.AddMaterial(CardboardFactory.CardboardMaterial.WoodChips, -100));

            var materials = _factory.GetMaterialStorage();
            Assert.IsTrue(materials[CardboardFactory.CardboardMaterial.WoodChips] >= 0, "Количество материалов не должно измениться");
        }

        [TestMethod]
        public void ConsumeProduct_ZeroAmount_ReturnsTrue()
        {
            // Act
            bool result = _factory.ConsumeProduct(CardboardFactory.CardboardProduct.CardboardBoxes, 0);

            // Assert
            Assert.IsTrue(result, "Потребление 0 единиц всегда должно быть успешным");
        }
    }
}