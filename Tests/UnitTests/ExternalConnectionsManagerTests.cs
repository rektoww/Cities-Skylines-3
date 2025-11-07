using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core.Services;
using Core.Resourses;
using Core.Enums;
using Core.Models.Buildings;
using Core.Models.Mobs;
using System.Collections.Generic;

namespace Tests.UnitTests
{
    /// <summary>
    /// Тесты для проверки логики ExternalConnectionsManager:
    /// ручной импорт/экспорт материалов и миграция населения
    /// </summary>
    [TestClass]
    public sealed class ExternalConnectionsManagerTests
    {
        private PlayerResources playerResources;
        private FinancialSystem financialSystem;
        private ExternalConnectionsManager manager;

        /// <summary>
        /// Инициализация окружения на каждый тест:
        /// создаются стартовые ресурсы, финансовая система и менеджер внешних связей
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            // Задание стартовых материалов и баланс игрока
            var materials = new Dictionary<ConstructionMaterial, int>
            {
                { ConstructionMaterial.Steel, 100 },
                { ConstructionMaterial.Glass, 100 },
                { ConstructionMaterial.Concrete, 100 },
                { ConstructionMaterial.Plastic, 100 }
            };
            playerResources = new PlayerResources(50000m, materials);
            financialSystem = new FinancialSystem(50000m);

            // Создание менеджера внешних связей для ручной торговли
            manager = new ExternalConnectionsManager(playerResources, financialSystem);
        }

        /// <summary>
        /// Ручной импорт материалов через TryImportMaterials должен списать деньги и добавить материалы
        /// </summary>
        [TestMethod]
        public void TryImportMaterials_ShouldSucceed_WhenSufficientFunds()
        {
            var quantities = new Dictionary<ConstructionMaterial, int>
            {
                { ConstructionMaterial.Steel, 10 },
                { ConstructionMaterial.Glass, 20 }
            };
            var prices = new Dictionary<ConstructionMaterial, decimal>
            {
                { ConstructionMaterial.Steel, manager.GetImportPrice(ConstructionMaterial.Steel, 100) },
                { ConstructionMaterial.Glass, manager.GetImportPrice(ConstructionMaterial.Glass, 100) }
            };

            decimal initialBalance = playerResources.Balance;
            int initialSteel = playerResources.StoredMaterials[ConstructionMaterial.Steel];
            int initialGlass = playerResources.StoredMaterials[ConstructionMaterial.Glass];

            bool result = manager.TryImportMaterials(quantities, prices, out decimal totalCost);

            Assert.IsTrue(result, "Импорт должен быть успешным");
            Assert.IsTrue(totalCost > 0, "Стоимость импорта должна быть больше 0");
            Assert.AreEqual(initialSteel + 10, playerResources.StoredMaterials[ConstructionMaterial.Steel],
                "Сталь должна добавиться в инвентарь");
            Assert.AreEqual(initialGlass + 20, playerResources.StoredMaterials[ConstructionMaterial.Glass],
                "Стекло должно добавиться в инвентарь");
        }

        /// <summary>
        /// Ручной импорт не выполняется при недостатке средств
        /// </summary>
        [TestMethod]
        public void TryImportMaterials_ShouldFail_WhenInsufficientFunds()
        {
            playerResources.Balance = 10m;
            financialSystem = new FinancialSystem(10m);
            manager = new ExternalConnectionsManager(playerResources, financialSystem);

            var quantities = new Dictionary<ConstructionMaterial, int>
            {
                { ConstructionMaterial.Steel, 1000 }
            };
            var prices = new Dictionary<ConstructionMaterial, decimal>
            {
                { ConstructionMaterial.Steel, manager.GetImportPrice(ConstructionMaterial.Steel, 100) }
            };

            int initialSteel = playerResources.StoredMaterials[ConstructionMaterial.Steel];

            bool result = manager.TryImportMaterials(quantities, prices, out decimal totalCost);

            Assert.IsFalse(result, "Импорт должен провалиться при недостатке средств");
            Assert.AreEqual(initialSteel, playerResources.StoredMaterials[ConstructionMaterial.Steel],
                "Количество стали не должно измениться");
        }

        /// <summary>
        /// Ручной экспорт материалов через TryExportMaterials должен добавить деньги и убрать материалы
        /// </summary>
        [TestMethod]
        public void TryExportMaterials_ShouldSucceed_WhenMaterialsAvailable()
        {
            decimal pricePerUnit = manager.GetExportPrice(ConstructionMaterial.Concrete, 100);
            decimal initialBalance = playerResources.Balance;
            int initialConcrete = playerResources.StoredMaterials[ConstructionMaterial.Concrete];

            bool result = manager.TryExportMaterials(ConstructionMaterial.Concrete, 50, pricePerUnit, out decimal totalRevenue);

            Assert.IsTrue(result, "Экспорт должен быть успешным");
            Assert.IsTrue(totalRevenue > 0, "Выручка от экспорта должна быть больше 0");
            Assert.AreEqual(initialConcrete - 50, playerResources.StoredMaterials[ConstructionMaterial.Concrete],
                "Бетон должен убраться из инвентаря");
            Assert.IsTrue(playerResources.Balance > initialBalance, "Баланс должен увеличиться");
        }

        /// <summary>
        /// Ручной экспорт не выполняется при недостатке материалов
        /// </summary>
        [TestMethod]
        public void TryExportMaterials_ShouldFail_WhenInsufficientMaterials()
        {
            decimal pricePerUnit = manager.GetExportPrice(ConstructionMaterial.Plastic, 100);
            int initialPlastic = playerResources.StoredMaterials[ConstructionMaterial.Plastic];

            bool result = manager.TryExportMaterials(ConstructionMaterial.Plastic, 1000, pricePerUnit, out decimal totalRevenue);

            Assert.IsFalse(result, "Экспорт должен провалиться при недостатке материалов");
            Assert.AreEqual(0m, totalRevenue, "Выручка должна быть 0");
            Assert.AreEqual(initialPlastic, playerResources.StoredMaterials[ConstructionMaterial.Plastic],
                "Количество пластика не должно измениться");
        }

        // ================================================================================
        // ТЕСТЫ МИГРАЦИИ
        // ================================================================================

        /// <summary>
        /// Метод AddImmigrants должен добавить указанное количество граждан
        /// </summary>
        [TestMethod]
        public void AddImmigrants_ShouldAddCitizens_WhenCalled()
        {
            var citizens = new List<Citizen>();
            var buildings = new List<ResidentialBuilding>();

            int addedCount = manager.AddImmigrants(citizens, buildings, 10);

            Assert.AreEqual(10, addedCount, "Должно быть добавлено 10 граждан");
            Assert.AreEqual(10, citizens.Count, "В списке должно быть 10 граждан");
        }

        /// <summary>
        /// Новые иммигранты должны иметь разумные атрибуты
        /// </summary>
        [TestMethod]
        public void AddImmigrants_ShouldCreateCitizensWithCorrectAttributes()
        {
            var citizens = new List<Citizen>();
            var buildings = new List<ResidentialBuilding>();

            manager.AddImmigrants(citizens, buildings, 5);

            foreach (var citizen in citizens)
            {
                Assert.AreEqual(25, citizen.Age, "Возраст иммигранта должен быть 25");
                Assert.AreEqual(EducationLevel.School, citizen.Education, "Образование должно быть School");
                Assert.IsFalse(citizen.IsEmployed, "Иммигрант не должен быть трудоустроен");
                Assert.AreEqual(100f, citizen.Health, "Здоровье должно быть 100");
                Assert.AreEqual(50f, citizen.Happiness, "Счастье должно быть 50");
            }
        }

        /// <summary>
        /// Иммигранты должны заселяться в свободное жилье
        /// </summary>
        [TestMethod]
        public void AddImmigrants_ShouldAssignHousing_WhenAvailable()
        {
            var citizens = new List<Citizen>();
            var buildings = new List<ResidentialBuilding>
            {
                new ResidentialBuilding(ResidentialType.Apartment) // Емкость 20
            };

            manager.AddImmigrants(citizens, buildings, 5);

            Assert.AreEqual(5, buildings[0].CurrentResidents.Count, "В здании должно быть 5 жильцов");
        }

        /// <summary>
        /// Метод RemoveEmigrants должен удалить указанное количество граждан
        /// </summary>
        [TestMethod]
        public void RemoveEmigrants_ShouldRemoveCitizens_WhenCalled()
        {
            var citizens = new List<Citizen>();
            for (int i = 0; i < 20; i++)
            {
                citizens.Add(new Citizen(0, 0, null) { Age = 30, IsEmployed = true, Happiness = 50f });
            }

            int removedCount = manager.RemoveEmigrants(citizens, 5);

            Assert.AreEqual(5, removedCount, "Должно быть удалено 5 граждан");
            Assert.AreEqual(15, citizens.Count, "В списке должно остаться 15 граждан");
        }

        /// <summary>
        /// Эмиграция должна приоритетно удалять безработных с низким счастьем
        /// </summary>
        [TestMethod]
        public void RemoveEmigrants_ShouldPrioritizeUnemployedUnhappy()
        {
            var citizens = new List<Citizen>();

            // Добавляем счастливого работающего
            var happyEmployed = new Citizen(0, 0, null)
            {
                Age = 30,
                IsEmployed = true,
                Happiness = 80f
            };
            citizens.Add(happyEmployed);

            // Добавляем несчастного безработного
            var unhappyUnemployed = new Citizen(0, 0, null)
            {
                Age = 30,
                IsEmployed = false,
                Happiness = 20f
            };
            citizens.Add(unhappyUnemployed);

            manager.RemoveEmigrants(citizens, 1);

            Assert.IsTrue(citizens.Contains(happyEmployed), "Счастливый работающий должен остаться");
            Assert.IsFalse(citizens.Contains(unhappyUnemployed), "Несчастный безработный должен уехать первым");
        }

        /// <summary>
        /// Эмиграция должна освобождать жилье
        /// </summary>
        [TestMethod]
        public void RemoveEmigrants_ShouldFreeHousing()
        {
            var citizens = new List<Citizen>();
            var building = new ResidentialBuilding(ResidentialType.Apartment);

            // Заселяем 10 человек
            for (int i = 0; i < 10; i++)
            {
                var citizen = new Citizen(0, 0, null)
                {
                    Age = 30,
                    IsEmployed = false,
                    Happiness = 30f,
                    Home = building
                };
                citizens.Add(citizen);
                building.CurrentResidents.Add(citizen);
            }

            int initialResidents = building.CurrentResidents.Count;
            manager.RemoveEmigrants(citizens, 3);

            Assert.AreEqual(initialResidents - 3, building.CurrentResidents.Count,
                "Жилье должно освободиться после эмиграции");
        }

        /// <summary>
        /// Расчет безработицы должен возвращать правильное значение
        /// </summary>
        [TestMethod]
        public void CalculateUnemploymentRate_ShouldReturnCorrectValue()
        {
            var citizens = new List<Citizen>();

            // 10 работающих
            for (int i = 0; i < 10; i++)
            {
                citizens.Add(new Citizen(0, 0, null) { Age = 30, IsEmployed = true });
            }

            // 5 безработных
            for (int i = 0; i < 5; i++)
            {
                citizens.Add(new Citizen(0, 0, null) { Age = 30, IsEmployed = false });
            }

            float rate = manager.CalculateUnemploymentRate(citizens);

            // 5 из 15 = 0.333...
            Assert.AreEqual(5f / 15f, rate, 0.01f, "Безработица должна быть ~33%");
        }

        /// <summary>
        /// Расчет свободного жилья должен возвращать правильное значение
        /// </summary>
        [TestMethod]
        public void CalculateAvailableHousing_ShouldReturnCorrectValue()
        {
            var buildings = new List<ResidentialBuilding>
            {
                new ResidentialBuilding(ResidentialType.Apartment) // Емкость 20
            };

            // Заселяем 5 человек
            for (int i = 0; i < 5; i++)
            {
                buildings[0].CurrentResidents.Add(new Citizen(0, 0, null));
            }

            int available = manager.CalculateAvailableHousing(buildings);

            Assert.AreEqual(15, available, "Должно быть 15 свободных мест");
        }

        /// <summary>
        /// Методы миграции должны корректно обрабатывать null и пустые списки
        /// </summary>
        [TestMethod]
        public void MigrationMethods_ShouldHandleNullAndEmptyLists()
        {
            // AddImmigrants с null
            int added = manager.AddImmigrants(null, null, 5);
            Assert.AreEqual(0, added, "AddImmigrants должен вернуть 0 для null");

            // RemoveEmigrants с null
            int removed = manager.RemoveEmigrants(null, 5);
            Assert.AreEqual(0, removed, "RemoveEmigrants должен вернуть 0 для null");

            // RemoveEmigrants с пустым списком
            removed = manager.RemoveEmigrants(new List<Citizen>(), 5);
            Assert.AreEqual(0, removed, "RemoveEmigrants должен вернуть 0 для пустого списка");

            // CalculateUnemploymentRate с null
            float rate = manager.CalculateUnemploymentRate(null);
            Assert.AreEqual(0f, rate, "CalculateUnemploymentRate должен вернуть 0 для null");

            // CalculateAvailableHousing с null
            int housing = manager.CalculateAvailableHousing(null);
            Assert.AreEqual(0, housing, "CalculateAvailableHousing должен вернуть 0 для null");
        }
    }
}
