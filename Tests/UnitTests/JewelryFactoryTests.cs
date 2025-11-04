using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Core.Models.Buildings.IndustrialBuildings;

namespace Tests.UnitTests
{
    [TestClass]
    public class JewelryFactoryTests
    {
        private JewelryFactory _factory;

        [TestInitialize]
        public void Init()
        {
            _factory = new JewelryFactory();
        }

        [TestMethod]
        public void Constructor_InitializesWithStartingMaterials()
        {
            // Проверка, что в хранилище есть основные виды сырья и они неотрицательны
            var materials = _factory.GetMaterialStorage();
            Assert.AreEqual(10, materials.Count);
            Assert.IsTrue(materials[JewelryFactory.JewelryMaterial.Gold] >= 0);
            Assert.IsTrue(materials[JewelryFactory.JewelryMaterial.Silver] >= 0);
            Assert.IsTrue(materials[JewelryFactory.JewelryMaterial.Diamond] >= 0);
        }

        [TestMethod]
        public void Constructor_InitializesWorkshops()
        {
            // Проверка количества и названий цехов
            Assert.AreEqual(5, _factory.Workshops.Count);
            Assert.IsTrue(_factory.Workshops.Any(w => w.Name == "Цех колец"));
            Assert.IsTrue(_factory.Workshops.Any(w => w.Name == "Цех ожерелий и цепей"));
            Assert.IsTrue(_factory.Workshops.Any(w => w.Name == "Цех серёг и брошей"));
            Assert.IsTrue(_factory.Workshops.Any(w => w.Name == "Цех браслетов и подвесок"));
            Assert.IsTrue(_factory.Workshops.Any(w => w.Name == "Цех корпусов часов и упаковки"));
        }

        [TestMethod]
        public void SetWorkersCount_BoundsRespected()
        {
            // Установка допустимого количества
            _factory.SetWorkersCount(12);
            Assert.AreEqual(12, _factory.WorkersCount);

            // Попытка установить больше максимума
            _factory.SetWorkersCount(100);
            Assert.AreEqual(18, _factory.WorkersCount);

            // Попытка установить отрицательное количество
            _factory.SetWorkersCount(-5);
            Assert.AreEqual(0, _factory.WorkersCount);
        }

        [TestMethod]
        public void AddMaterial_RespectsCapacity()
        {
            // Подготовка: вычисляем свободное место и добавляем допустимое количество
            var initialTotal = _factory.GetProductionInfo()["TotalMaterialStorage"];
            int free = _factory.MaxMaterialStorage - (int)initialTotal;
            bool ok = _factory.AddMaterial(JewelryFactory.JewelryMaterial.Gold, free > 0 ? 1 : 0);
            Assert.AreEqual(free > 0, ok);

            // Попытка переполнить хранилище
            bool overflow = _factory.AddMaterial(JewelryFactory.JewelryMaterial.Gold, 100000);
            Assert.IsFalse(overflow);
        }

        [TestMethod]
        public void ProcessWorkshops_NoWorkers_NoProduction()
        {
            // Проверка, что без рабочих продукция не создаётся
            _factory.SetWorkersCount(0);
            var before = _factory.GetProductionOutput();

            _factory.ProcessWorkshops();

            var after = _factory.GetProductionOutput();
            Assert.AreEqual(before.Count, after.Count);
            Assert.AreEqual(before.Values.Sum(), after.Values.Sum());
        }

        [TestMethod]
        public void ProcessWorkshops_WithWorkers_ProducesAndConsumes()
        {
            // Подготовка: добавляем материалы и рабочих
            _factory.SetWorkersCount(12);
            _factory.AddMaterial(JewelryFactory.JewelryMaterial.Gold, 50);
            _factory.AddMaterial(JewelryFactory.JewelryMaterial.Platinum, 30);
            _factory.AddMaterial(JewelryFactory.JewelryMaterial.Gemstone, 40);
            _factory.AddMaterial(JewelryFactory.JewelryMaterial.Silver, 60);
            _factory.AddMaterial(JewelryFactory.JewelryMaterial.Diamond, 10);
            var materialsBefore = _factory.GetMaterialStorage();

            _factory.ProcessWorkshops();

            // Проверка: есть произведённая продукция и сырьё уменьшилось
            var products = _factory.GetProductionOutput();
            Assert.IsTrue(products.Count > 0);
            Assert.IsTrue(products.Values.Sum() > 0);

            var materialsAfter = _factory.GetMaterialStorage();
            Assert.IsTrue(materialsAfter[JewelryFactory.JewelryMaterial.Gold] <= materialsBefore[JewelryFactory.JewelryMaterial.Gold]);
        }

        [TestMethod]
        public void Production_RespectsProductStorageLimit()
        {
            // Подготовка: максимальная эффективность и очень много сырья
            _factory.SetWorkersCount(18);
            foreach (var m in System.Enum.GetValues(typeof(JewelryFactory.JewelryMaterial)).Cast<JewelryFactory.JewelryMaterial>())
            {
                _factory.AddMaterial(m, 10000);
            }

            _factory.ProcessWorkshops();

            // Проверка: общий объём продукции не превышает предел хранилища
            var products = _factory.GetProductionOutput();
            int total = products.Values.Sum();
            Assert.IsTrue(total <= _factory.MaxProductStorage);
        }

        [TestMethod]
        public void SeveralProductTypes_AreProduced()
        {
            // Подготовка достаточного количества сырья и рабочих
            _factory.SetWorkersCount(18);
            foreach (var m in System.Enum.GetValues(typeof(JewelryFactory.JewelryMaterial)).Cast<JewelryFactory.JewelryMaterial>())
            {
                _factory.AddMaterial(m, 500);
            }

            _factory.ProcessWorkshops();

            // Проверка: производится несколько видов изделий
            var products = _factory.GetProductionOutput();
            int kinds = products.Count(p => p.Value > 0);
            Assert.IsTrue(kinds >= 3);
        }
    }
}
