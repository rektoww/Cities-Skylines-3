using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using Core.Models.Buildings.IndustrialBuildings;

namespace Core.Tests.Buildings.IndustrialBuildings
{
    [TestClass]
    public class PackagingFactoryTests
    {
        private PackagingFactory _factory;

        [TestInitialize]
        public void Setup()
        {
            _factory = new PackagingFactory();
        }

        [TestMethod]
        public void Constructor_InitializesWithStartingMaterials()
        {
            // Assert
            var materials = _factory.GetMaterialStorage();
            Assert.AreEqual(6, materials.Count, "Должно быть 6 видов начальных материалов");
            Assert.IsTrue(materials[PackagingFactory.PackagingMaterial.Cardboard] >= 0);
            Assert.IsTrue(materials[PackagingFactory.PackagingMaterial.Plastic] >= 0);
        }

        [TestMethod]
        public void Constructor_InitializesWorkshops()
        {
            // Assert
            Assert.AreEqual(5, _factory.Workshops.Count, "Должно быть 5 цехов");
            Assert.IsTrue(_factory.Workshops.Any(w => w.Name == "Цех картонной упаковки"));
            Assert.IsTrue(_factory.Workshops.Any(w => w.Name == "Цех пластиковой упаковки"));
        }

        [TestMethod]
        public void SetWorkersCount_ValidCount_SetsCorrectly()
        {
            // Act
            _factory.SetWorkersCount(10);

            // Assert
            Assert.AreEqual(10, _factory.WorkersCount);
        }

        [TestMethod]
        public void SetWorkersCount_ExceedsMax_ClampsToMax()
        {
            // Act
            _factory.SetWorkersCount(20); // MaxWorkers = 15

            // Assert
            Assert.AreEqual(15, _factory.WorkersCount);
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
            _factory.SetWorkersCount(7); // Примерно половина от 15

            // Assert
            Assert.IsTrue(_factory.ProductionEfficiency > 0.6f && _factory.ProductionEfficiency < 0.8f);
        }

        [TestMethod]
        public void ProductionEfficiency_FullWorkers_ReturnsOne()
        {
            // Arrange
            _factory.SetWorkersCount(15);

            // Assert
            Assert.AreEqual(1f, _factory.ProductionEfficiency);
        }

        [TestMethod]
        public void AddMaterial_WithinCapacity_AddsSuccessfully()
        {
            // Act
            bool result = _factory.AddMaterial(PackagingFactory.PackagingMaterial.Cardboard, 100);

            // Assert
            Assert.IsTrue(result);
            var materials = _factory.GetMaterialStorage();
            Assert.IsTrue(materials[PackagingFactory.PackagingMaterial.Cardboard] >= 100);
        }

        [TestMethod]
        public void AddMaterial_ExceedsCapacity_ReturnsFalse()
        {
            // Arrange
            int hugeAmount = 10000;

            // Act
            bool result = _factory.AddMaterial(PackagingFactory.PackagingMaterial.Cardboard, hugeAmount);

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
            _factory.SetWorkersCount(10);

            // Добавляем достаточно материалов для производства
            _factory.AddMaterial(PackagingFactory.PackagingMaterial.Cardboard, 100);
            _factory.AddMaterial(PackagingFactory.PackagingMaterial.Ink, 50);
            _factory.AddMaterial(PackagingFactory.PackagingMaterial.Plastic, 100);

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
            _factory.SetWorkersCount(10);
            var initialMaterials = _factory.GetMaterialStorage();
            int initialCardboard = initialMaterials[PackagingFactory.PackagingMaterial.Cardboard];

            // Act
            _factory.ProcessWorkshops();

            // Assert
            var finalMaterials = _factory.GetMaterialStorage();
            int finalCardboard = finalMaterials[PackagingFactory.PackagingMaterial.Cardboard];
            Assert.IsTrue(finalCardboard < initialCardboard, "Картон должен расходоваться в процессе производства");
        }

        [TestMethod]
        public void ProcessWorkshops_RespectsProductionEfficiency()
        {
            // Arrange
            _factory.SetWorkersCount(5); // Средняя эффективность
            _factory.AddMaterial(PackagingFactory.PackagingMaterial.Cardboard, 200);
            _factory.AddMaterial(PackagingFactory.PackagingMaterial.Ink, 100);

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
            _factory.SetWorkersCount(10);
            _factory.AddMaterial(PackagingFactory.PackagingMaterial.Cardboard, 100);
            _factory.AddMaterial(PackagingFactory.PackagingMaterial.Ink, 50);
            _factory.ProcessWorkshops();

            var initialProducts = _factory.GetProductionOutput();
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
            var productType = PackagingFactory.PackagingProduct.CardboardBox;

            // Act
            bool result = _factory.ConsumeProduct(productType, 1000);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ConsumeProduct_ExactQuantity_RemovesProductFromStorage()
        {
            // Arrange
            _factory.SetWorkersCount(10);
            _factory.AddMaterial(PackagingFactory.PackagingMaterial.Cardboard, 50);
            _factory.AddMaterial(PackagingFactory.PackagingMaterial.Ink, 20);
            _factory.ProcessWorkshops();

            var initialProducts = _factory.GetProductionOutput();
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
            Assert.AreEqual(15, info["MaxWorkers"]);
            Assert.IsTrue((float)info["ProductionEfficiency"] > 0);
            Assert.IsTrue((int)info["TotalMaterialStorage"] >= 0);
            Assert.AreEqual(2000, info["MaxMaterialStorage"]);
            Assert.AreEqual(5, info["ActiveWorkshops"]);
        }

        [TestMethod]
        public void MultipleProductionCycles_AccumulateProducts()
        {
            // Arrange
            _factory.SetWorkersCount(10);
            _factory.AddMaterial(PackagingFactory.PackagingMaterial.Cardboard, 300);
            _factory.AddMaterial(PackagingFactory.PackagingMaterial.Ink, 100);

            // Act
            _factory.ProcessWorkshops(); // Первый цикл
            var productsAfterFirstCycle = _factory.GetProductionOutput();
            int firstCycleTotal = productsAfterFirstCycle.Values.Sum();

            _factory.ProcessWorkshops(); // Второй цикл
            var productsAfterSecondCycle = _factory.GetProductionOutput();
            int secondCycleTotal = productsAfterSecondCycle.Values.Sum();

            // Assert
            Assert.IsTrue(secondCycleTotal > firstCycleTotal, "Второй цикл должен увеличить общее производство");
        }

        [TestMethod]
        public void Production_RespectsStorageLimits()
        {
            // Arrange
            _factory.SetWorkersCount(15); // Максимальная эффективность

            // Добавляем огромное количество материалов
            _factory.AddMaterial(PackagingFactory.PackagingMaterial.Cardboard, 5000);
            _factory.AddMaterial(PackagingFactory.PackagingMaterial.Plastic, 5000);
            _factory.AddMaterial(PackagingFactory.PackagingMaterial.Ink, 5000);

            // Act
            _factory.ProcessWorkshops();

            // Assert
            var products = _factory.GetProductionOutput();
            int totalProducts = products.Values.Sum();
            Assert.IsTrue(totalProducts <= 1500, "Общее производство не должно превышать MaxProductStorage (1500)");
        }

        [TestMethod]
        public void AllProductTypes_CanBeProduced()
        {
            // Arrange
            _factory.SetWorkersCount(15);

            // Добавляем все необходимые материалы
            foreach (var material in System.Enum.GetValues(typeof(PackagingFactory.PackagingMaterial)).Cast<PackagingFactory.PackagingMaterial>())
            {
                _factory.AddMaterial(material, 500);
            }

            // Act
            _factory.ProcessWorkshops();

            // Assert
            var products = _factory.GetProductionOutput();
            var allProductTypes = System.Enum.GetValues(typeof(PackagingFactory.PackagingProduct)).Cast<PackagingFactory.PackagingProduct>();

            // Проверяем, что производится хотя бы несколько типов продукции
            int producedTypesCount = products.Count(p => p.Value > 0);
            Assert.IsTrue(producedTypesCount >= 3, "Должно производиться несколько типов продукции");
        }
    }
}