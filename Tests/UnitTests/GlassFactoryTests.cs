using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Core.Models.Buildings.IndustrialBuildings;

namespace Tests.UnitTests
{
    [TestClass]
    public class GlassFactoryTests
    {
        private GlassFactory _factory;

        [TestInitialize]
        public void Init()
        {
            _factory = new GlassFactory();
        }

        [TestMethod]
        public void Constructor_InitializesWithStartingMaterials()
        {
            // Проверка наличия основных видов сырья и неотрицательных значений
            var materials = _factory.GetMaterialStorage();
            Assert.AreEqual(10, materials.Count);
            Assert.IsTrue(materials[GlassFactory.GlassMaterial.Sand] >= 0);
            Assert.IsTrue(materials[GlassFactory.GlassMaterial.RecycledGlass] >= 0);
        }

        [TestMethod]
        public void Constructor_InitializesWorkshops()
        {
            // Проверка количества и названий цехов
            Assert.AreEqual(5, _factory.Workshops.Count);
            Assert.IsTrue(_factory.Workshops.Any(w => w.Name == "Плавильная печь"));
            Assert.IsTrue(_factory.Workshops.Any(w => w.Name == "Линия тарного стекла"));
            Assert.IsTrue(_factory.Workshops.Any(w => w.Name == "Флоат-линия"));
            Assert.IsTrue(_factory.Workshops.Any(w => w.Name == "Закалка и ламинирование"));
            Assert.IsTrue(_factory.Workshops.Any(w => w.Name == "Вытяжка волокна и трубок"));
        }

        [TestMethod]
        public void SetWorkersCount_BoundsRespected()
        {
            // Установка допустимого количества
            _factory.SetWorkersCount(12);
            Assert.AreEqual(12, _factory.WorkersCount);

            // Попытка установить больше максимума
            _factory.SetWorkersCount(100);
            Assert.AreEqual(20, _factory.WorkersCount);

            // Попытка установить отрицательное количество
            _factory.SetWorkersCount(-5);
            Assert.AreEqual(0, _factory.WorkersCount);
        }

        [TestMethod]
        public void AddMaterial_RespectsCapacity()
        {
            // Подсчёт свободного места и добавление допустимого количества
            var initialTotal = _factory.GetProductionInfo()["TotalMaterialStorage"];
            int free = _factory.MaxMaterialStorage - (int)initialTotal;
            bool ok = _factory.AddMaterial(GlassFactory.GlassMaterial.Sand, free > 0 ? 1 : 0);
            Assert.AreEqual(free > 0, ok);

            // Попытка переполнить хранилище
            bool overflow = _factory.AddMaterial(GlassFactory.GlassMaterial.Sand, 100000);
            Assert.IsFalse(overflow);
        }

        [TestMethod]
        public void ProcessWorkshops_NoWorkers_NoProduction()
        {
            // Проверка, что без рабочих нет выпуска продукции
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
            // Подготовка достаточного сырья для всех линий и установка рабочих
            _factory.SetWorkersCount(16);
            _factory.AddMaterial(GlassFactory.GlassMaterial.Sand, 200);
            _factory.AddMaterial(GlassFactory.GlassMaterial.SodaAsh, 100);
            _factory.AddMaterial(GlassFactory.GlassMaterial.Limestone, 80);
            _factory.AddMaterial(GlassFactory.GlassMaterial.RecycledGlass, 200);
            _factory.AddMaterial(GlassFactory.GlassMaterial.Colorant, 50);
            _factory.AddMaterial(GlassFactory.GlassMaterial.Gas, 100);
            _factory.AddMaterial(GlassFactory.GlassMaterial.Resin, 50);
            _factory.AddMaterial(GlassFactory.GlassMaterial.Tin, 20);
            _factory.AddMaterial(GlassFactory.GlassMaterial.Silica, 60);
            _factory.AddMaterial(GlassFactory.GlassMaterial.Clay, 40);

            var materialsBefore = _factory.GetMaterialStorage();

            _factory.ProcessWorkshops();

            // Проверка: продукция произведена, сырьё расходуется
            var products = _factory.GetProductionOutput();
            Assert.IsTrue(products.Count > 0);
            Assert.IsTrue(products.Values.Sum() > 0);

            var materialsAfter = _factory.GetMaterialStorage();
            Assert.IsTrue(materialsAfter[GlassFactory.GlassMaterial.Sand] <= materialsBefore[GlassFactory.GlassMaterial.Sand]);
        }

        [TestMethod]
        public void Production_RespectsProductStorageLimit()
        {
            // Максимальная эффективность и большой запас сырья
            _factory.SetWorkersCount(20);
            foreach (var m in System.Enum.GetValues(typeof(GlassFactory.GlassMaterial)).Cast<GlassFactory.GlassMaterial>())
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
            // Достаточное количество сырья и рабочих для разнообразия продукции
            _factory.SetWorkersCount(20);
            foreach (var m in System.Enum.GetValues(typeof(GlassFactory.GlassMaterial)).Cast<GlassFactory.GlassMaterial>())
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
