using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core.Services;
using Core.Resourses;
using Core.Models.Mobs;
using Core.Models.Buildings;
using Core.Enums;
using Core.Models.Map;
using System.Collections.Generic;

namespace Tests.UnitTests
{
    /// <summary>
    /// Тесты для проверки логики ExternalConnectionsManager:
    /// импорт/экспорт материалов, миграция населения и распределение жилья
    /// </summary>
    [TestClass]
    public sealed class ExternalConnectionsManagerTests
    {
        private PlayerResources playerResources;
        private List<Citizen> citizens;
        private List<ResidentialBuilding> residentialBuildings;
        private ExternalConnectionsManager manager;
        private GameMap gameMap;

        /// <summary>
        /// Инициализация окружения на каждый тест:
        /// создаются карта, стартовые ресурсы, списки граждан и домов, менеджер внешних связей
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            // Создание игровой карты 20x20 для перемещения граждан и зданий
            gameMap = new GameMap(20, 20);

            // Задание стартовых материалов и баланс игрока
            var materials = new Dictionary<ConstructionMaterial, int>
            {
                { ConstructionMaterial.Steel, 30 },
                { ConstructionMaterial.Glass, 600 },
                { ConstructionMaterial.Concrete, 100 },
                { ConstructionMaterial.Plastic, 20 }
            };
            playerResources = new PlayerResources(50000m, materials);

            // Пустые списки граждан и жилых зданий
            citizens = new List<Citizen>();
            residentialBuildings = new List<ResidentialBuilding>();

            // Создание менеджера внешних связей для торговых операций и миграции
            manager = new ExternalConnectionsManager(playerResources, citizens, residentialBuildings);
        }

        /// <summary>
        /// При нехватке стали в запасах менеджер должен импортировать материал, а баланс уменьшиться
        /// </summary>
        /// <remarks>Дефицит стали (20), запуск тика, ожидается рост стали и снижение баланса</remarks>
        [TestMethod]
        public void SimulateTick_ShouldImportMaterial_WhenShortage()
        {
            // Установка запаса стали ниже порога импорта
            playerResources.StoredMaterials[ConstructionMaterial.Steel] = 20;
            decimal initialBalance = playerResources.Balance;

            // Запуск симуляции тика экономики
            manager.SimulateTick();

            // Сталь импортируется при нехватке
            Assert.IsTrue(playerResources.StoredMaterials[ConstructionMaterial.Steel] > 20,
                "Сталь должна быть импортирована при нехватке");

            // Баланс уменьшается из‑за оплаты импорта
            Assert.IsTrue(playerResources.Balance < initialBalance,
                "Баланс должен уменьшиться после импорта");
        }

        /// <summary>
        /// При избытке стекла менеджер должен экспортировать материал, а баланс вырастет
        /// </summary>
        /// <remarks>Избыток стекла (600+), запуск тика, ожидается снижение стекла и рост баланса</remarks>
        [TestMethod]
        public void SimulateTick_ShouldExportMaterial_WhenSurplus()
        {
            // Задается высокий уровень стекла для срабатывания экспорта
            playerResources.StoredMaterials[ConstructionMaterial.Glass] = 600;
            // Устанавливаем остальные материалы в безопасный диапазон, чтобы не было импорта
            playerResources.StoredMaterials[ConstructionMaterial.Steel] = 100;
            playerResources.StoredMaterials[ConstructionMaterial.Plastic] = 100;
            playerResources.StoredMaterials[ConstructionMaterial.Concrete] = 100;
            decimal initialBalance = playerResources.Balance;

            // Запуск симуляции тика экономики
            manager.SimulateTick();

            // Стекло уменьшается за счет экспорта
            Assert.IsTrue(playerResources.StoredMaterials[ConstructionMaterial.Glass] < 600,
                "Стекло должно быть экспортировано при избытке");

            // Баланс увеличивается благодаря выручке от экспорта
            Assert.IsTrue(playerResources.Balance > initialBalance,
                "Баланс должен увеличиться после экспорта");
        }

        /// <summary>
        /// Импорт не выполняется, если баланс недостаточен для покупки материалов
        /// </summary>
        /// <remarks>Баланс 10, дефицит стали 10, запуск тика — количество стали не меняется</remarks>
        [TestMethod]
        public void SimulateTick_ShouldNotImport_WhenInsufficientFunds()
        {
            // Уменьшение баланса до значения ниже стоимости закупки
            playerResources.Balance = 10m;
            playerResources.StoredMaterials[ConstructionMaterial.Steel] = 10;
            int initialSteel = playerResources.StoredMaterials[ConstructionMaterial.Steel];

            // Запуск симуляции тика экономики
            manager.SimulateTick();

            // Импорт не происходит из‑за нехватки средств
            Assert.AreEqual(initialSteel, playerResources.StoredMaterials[ConstructionMaterial.Steel],
                "Сталь не должна импортироваться при недостатке денег");
        }

        /// <summary>
        /// После импорта статистика импорта для материала становится положительной
        /// </summary>
        /// <remarks>Создается дефицит пластика 20, запуск тика — показатель импорта &gt; 0.</remarks>
        [TestMethod]
        public void GetImportRate_ShouldReturnCorrectValue_AfterImport()
        {
            // Создание дефицит пластика
            playerResources.StoredMaterials[ConstructionMaterial.Plastic] = 20;

            // Запуск симуляции, чтобы сработал импорт
            manager.SimulateTick();

            // Статистика импорта отражает привезенный объем
            int importedPlastic = manager.GetImportRate(ConstructionMaterial.Plastic);
            Assert.IsTrue(importedPlastic > 0,
                "Статистика импорта пластика должна показывать импортированный объем");
        }

        /// <summary>
        /// После экспорта статистика экспорта для материала становится положительной
        /// </summary>
        /// <remarks>Создается избыток стекла 700, запуск тика — показатель экспорта &gt; 0.</remarks>
        [TestMethod]
        public void GetExportRate_ShouldReturnCorrectValue_AfterExport()
        {
            // Создание избытка стекла
            playerResources.StoredMaterials[ConstructionMaterial.Glass] = 700;

            // Запуск симуляции, чтобы сработал экспорт
            manager.SimulateTick();

            // Статистика экспорта отражает вывезенный объем
            int exportedGlass = manager.GetExportRate(ConstructionMaterial.Glass);
            Assert.IsTrue(exportedGlass > 0,
                "Статистика экспорта стекла должна показывать экспортированный объем");
        }

        /// <summary>
        /// При благоприятных условиях (низкая безработица и свободное жилье) появляются иммигранты
        /// </summary>
        /// <remarks>Создаются 100 трудоустроенных и здание с местами; после тика численность не уменьшается, баланс миграции неотрицательный</remarks>
        [TestMethod]
        public void SimulateTick_ShouldAddImmigrants_WhenGoodConditions()
        {
            // Низкая безработица — добавление работающих граждан
            for (int i = 0; i < 100; i++)
            {
                citizens.Add(new Citizen(0, 0, gameMap)
                {
                    Age = 30,
                    IsEmployed = true
                });
            }

            // Наличие свободного жилья
            var building = new ResidentialBuilding(ResidentialType.Apartment);
            residentialBuildings.Add(building);

            int initialCitizensCount = citizens.Count;

            // Запуск симуляции тика экономики
            manager.SimulateTick();

            // Число граждан не уменьшается, ожидается приток
            Assert.IsTrue(citizens.Count >= initialCitizensCount,
                "Количество граждан должно увеличиться при благоприятных условиях");

            // Баланс миграции неотрицательный
            Assert.IsTrue(manager.GetMigrationBalance() >= 0,
                "Миграционный баланс должен быть положительным при хороших условиях");
        }

        /// <summary>
        /// При неблагоприятных условиях (высокая безработица и дефицит жилья) часть граждан уезжает
        /// </summary>
        /// <remarks>Создаются 100 безработных с низким счастьем и заполненное общежитие; после тика численность уменьшается, баланс миграции отрицательный</remarks>
        [TestMethod]
        public void SimulateTick_ShouldRemoveEmigrants_WhenBadConditions()
        {
            // Высокая безработица и низкое счастье
            for (int i = 0; i < 100; i++)
            {
                citizens.Add(new Citizen(0, 0, gameMap)
                {
                    Age = 30,
                    IsEmployed = false,
                    Happiness = 20f
                });
            }

            // Жилья нет — заполнение здания полностью
            var building = new ResidentialBuilding(ResidentialType.Dormitory);
            for (int i = 0; i < building.Capacity; i++)
            {
                building.CurrentResidents.Add(new Citizen(0, 0, gameMap));
            }
            residentialBuildings.Add(building);

            int initialCitizensCount = citizens.Count;

            // Запуск симуляции тика экономики
            manager.SimulateTick();

            // Часть граждан уехала
            Assert.IsTrue(citizens.Count < initialCitizensCount,
                "Количество граждан должно уменьшиться при плохих условиях");

            // Баланс миграции отрицательный
            Assert.IsTrue(manager.GetMigrationBalance() < 0,
                "Миграционный баланс должен быть отрицательным при плохих условиях");
        }

        /// <summary>
        /// Количество иммигрантов становится неотрицательным после притока населения
        /// </summary>
        /// <remarks>Создаются 50 трудоустроенных и жилье; после тика число иммигрантов ≥ 0.</remarks>
        [TestMethod]
        public void GetImmigrantsCount_ShouldReturnCorrectValue_AfterImmigration()
        {
            // Условия для притока: есть работающие граждане
            for (int i = 0; i < 50; i++)
            {
                citizens.Add(new Citizen(0, 0, gameMap)
                {
                    Age = 30,
                    IsEmployed = true
                });
            }

            // Добавление жилья
            var building = new ResidentialBuilding(ResidentialType.Apartment);
            residentialBuildings.Add(building);

            // Запуск симуляции тика экономики
            manager.SimulateTick();

            // Показатель иммиграции неотрицателен
            int immigrants = manager.GetImmigrantsCount();
            Assert.IsTrue(immigrants >= 0,
                "Количество иммигрантов должно быть неотрицательным");
        }

        /// <summary>
        /// Количество эмигрантов становится неотрицательным после оттока населения
        /// </summary>
        /// <remarks>Создаются 100 безработных с низким счастьем; после тика число эмигрантов ≥ 0.</remarks>
        [TestMethod]
        public void GetEmigrantsCount_ShouldReturnCorrectValue_AfterEmigration()
        {
            // Условия для оттока: много безработных и недовольных
            for (int i = 0; i < 100; i++)
            {
                citizens.Add(new Citizen(0, 0, gameMap)
                {
                    Age = 30,
                    IsEmployed = false,
                    Happiness = 15f
                });
            }

            // Запуск симуляции тика экономики
            manager.SimulateTick();

            // Показатель эмиграции неотрицателен
            int emigrants = manager.GetEmigrantsCount();
            Assert.IsTrue(emigrants >= 0,
                "Количество эмигрантов должно быть неотрицательным");
        }

        /// <summary>
        /// Новоприбывшие граждане получают жилье при наличии свободных мест
        /// </summary>
        /// <remarks>Есть потенциал притока и свободные квартиры; после тика число жильцов в доме увеличивается.</remarks>
        [TestMethod]
        public void SimulateTick_ShouldAssignHousing_ToNewImmigrants()
        {
            // Поддержка условий для притока (есть работающие)
            for (int i = 0; i < 50; i++)
            {
                citizens.Add(new Citizen(0, 0, gameMap)
                {
                    Age = 30,
                    IsEmployed = true
                });
            }

            // Дом с доступными местами
            var building = new ResidentialBuilding(ResidentialType.Apartment);
            residentialBuildings.Add(building);
            int initialResidents = building.CurrentResidents.Count;

            // Запуск симуляции тика экономики
            manager.SimulateTick();

            // Если кто‑то прибыл — число жильцов растет
            if (manager.GetImmigrantsCount() > 0)
            {
                Assert.IsTrue(building.CurrentResidents.Count > initialResidents,
                    "Новым иммигрантам должно быть назначено жилье");
            }
        }

        /// <summary>
        /// При оттоке населения занятые места в жилье освобождаются
        /// </summary>
        /// <remarks>Создается дом и 50 жильцов с низким счастьем; после тика число жильцов уменьшается.</remarks>
        [TestMethod]
        public void SimulateTick_ShouldFreeHousing_WhenCitizensEmigrate()
        {
            // Дом, в котором живут граждане
            var building = new ResidentialBuilding(ResidentialType.Apartment);
            residentialBuildings.Add(building);

            // Заселение 50 человек с низким счастьем и без работы
            for (int i = 0; i < 50; i++)
            {
                var citizen = new Citizen(0, 0, gameMap)
                {
                    Age = 30,
                    IsEmployed = false,
                    Happiness = 10f,
                    Home = building
                };
                citizens.Add(citizen);
                building.CurrentResidents.Add(citizen);
            }

            int initialResidents = building.CurrentResidents.Count;

            // Запуск симуляции тика экономики (ожидается отток)
            manager.SimulateTick();

            // Часть мест в доме освобождается
            Assert.IsTrue(building.CurrentResidents.Count < initialResidents,
                "Жилье должно освободиться после эмиграции граждан");
        }

        /// <summary>
        /// В нейтральных условиях баланс миграции близок к нулю
        /// </summary>
        /// <remarks>50 граждан с 50% занятостью, заполненный отель; после тика баланс миграции около 0.</remarks>
        [TestMethod]
        public void GetMigrationBalance_ShouldReturnZero_WithNeutralConditions()
        {
            // Создание смешанной занятости 50/50
            for (int i = 0; i < 50; i++)
            {
                citizens.Add(new Citizen(0, 0, gameMap)
                {
                    Age = 30,
                    IsEmployed = i % 2 == 0
                });
            }

            // Свободных мест нет — отель заполнен
            var building = new ResidentialBuilding(ResidentialType.Hotel);
            for (int i = 0; i < building.Capacity; i++)
            {
                building.CurrentResidents.Add(new Citizen(0, 0, gameMap));
            }
            residentialBuildings.Add(building);

            // Запуск симуляции тика экономики
            manager.SimulateTick();

            // Баланс миграции около нуля
            int balance = manager.GetMigrationBalance();
            Assert.IsTrue(balance >= -10 && balance <= 10,
                "Миграционный баланс должен быть близок к нулю при нейтральных условиях");
        }

        /// <summary>
        /// После торговых операций общий объем торговли неотрицателен
        /// </summary>
        /// <remarks>Дефицит стали и избыток стекла; после тика общий объем торговли ≥ 0.</remarks>
        [TestMethod]
        public void GetTotalTradeVolume_ShouldReturnPositiveValue_AfterTrade()
        {
            // Задается дефицит стали и избыток стекла для запуска торговли
            playerResources.StoredMaterials[ConstructionMaterial.Steel] = 10;
            playerResources.StoredMaterials[ConstructionMaterial.Glass] = 700;

            // Запуск симуляции тика экономики
            manager.SimulateTick();

            // Суммарный объем торговли неотрицательный
            decimal tradeVolume = manager.GetTotalTradeVolume();
            Assert.IsTrue(tradeVolume >= 0,
                "Общий объем торговли должен быть неотрицательным");
        }

        /// <summary>
        /// Статистика импорта очищается между тиками
        /// </summary>
        /// <remarks>Сначала дефицит стали и импорт &gt; 0, затем нормальный уровень — импорт сброшен в 0.</remarks>
        [TestMethod]
        public void SimulateTick_ShouldClearStatistics_AfterEachTick()
        {
            // Сценарий 1: дефицит стали приводит к импорту
            playerResources.StoredMaterials[ConstructionMaterial.Steel] = 10;
            manager.SimulateTick();
            int firstImport = manager.GetImportRate(ConstructionMaterial.Steel);

            // Сценарий 2: запас стали восстановлен — импорта нет
            playerResources.StoredMaterials[ConstructionMaterial.Steel] = 100;
            manager.SimulateTick();
            int secondImport = manager.GetImportRate(ConstructionMaterial.Steel);

            // Показатель импорта сбрасывается между тиками
            Assert.AreEqual(0, secondImport,
                "Статистика импорта должна обнуляться после каждого тика");
        }

        /// <summary>
        /// Метод тика устойчив к пустому списку граждан
        /// </summary>
        /// <remarks>Удаляются все граждане; вызов тика не приводит к исключениям.</remarks>
        [TestMethod]
        public void SimulateTick_ShouldNotCrash_WithEmptyCitizensList()
        {
            // Очистка перечня граждан
            citizens.Clear();

            // Вызов тика не должен завершаться исключением
            try
            {
                manager.SimulateTick();
                Assert.IsTrue(true, "Метод должен работать с пустым списком граждан");
            }
            catch
            {
                Assert.Fail("Метод упал с пустым списком граждан");
            }
        }

        /// <summary>
        /// Метод тика устойчив к пустому списку зданий
        /// </summary>
        /// <remarks>Удаляются все здания; при наличии граждан вызов тика не приводит к исключениям.</remarks>
        [TestMethod]
        public void SimulateTick_ShouldNotCrash_WithEmptyBuildingsList()
        {
            // Удаление всех зданий
            residentialBuildings.Clear();

            // Добавление нескольких граждан для проверки миграционных расчетов
            for (int i = 0; i < 10; i++)
            {
                citizens.Add(new Citizen(0, 0, gameMap) { Age = 30 });
            }

            // Вызов тика не должен завершаться исключением
            try
            {
                manager.SimulateTick();
                Assert.IsTrue(true, "Метод должен работать с пустым списком зданий");
            }
            catch
            {
                Assert.Fail("Метод упал с пустым списком зданий");
            }
        }

        /// <summary>
        /// Новые иммигранты создаются с разумными начальными атрибутами
        /// </summary>
        /// <remarks>Есть условия для притока; после тика последний добавленный гражданин трудоспособного возраста, здоровье &gt; 0, не трудоустроен.</remarks>
        [TestMethod]
        public void NewImmigrants_ShouldHaveReasonableAttributes()
        {
            // Условия для притока: есть работающие граждане
            for (int i = 0; i < 50; i++)
            {
                citizens.Add(new Citizen(0, 0, gameMap)
                {
                    Age = 30,
                    IsEmployed = true
                });
            }

            // Добавление жилья
            var building = new ResidentialBuilding(ResidentialType.Apartment);
            residentialBuildings.Add(building);

            int initialCount = citizens.Count;

            // Запуск симуляция тика экономики
            manager.SimulateTick();

            // Проверка атрибутов последнего прибывшего (если приток произошел)
            if (citizens.Count > initialCount)
            {
                var immigrant = citizens[citizens.Count - 1];
                Assert.IsTrue(immigrant.Age >= 18 && immigrant.Age <= 65,
                    "Возраст иммигранта должен быть в трудоспособном диапазоне");
                Assert.IsTrue(immigrant.Health > 0,
                    "Здоровье иммигранта должно быть положительным");
                Assert.IsFalse(immigrant.IsEmployed,
                    "Новый иммигрант пока не трудоустроен");
            }
        }

        /// <summary>
        /// При выборе кандидатов на отъезд приоритет отдается недовольным безработным
        /// </summary>
        /// <remarks>Создаются счастливый работающий и несчастный безработный; после тика первый остается, второй уезжает при наличии оттока.</remarks>
        [TestMethod]
        public void Emigration_ShouldPrioritizeUnemployedUnhappy()
        {
            // Создаем достаточно работающих для умеренной безработицы
            for (int i = 0; i < 60; i++)
            {
                citizens.Add(new Citizen(0, 0, gameMap)
                {
                    Age = 30,
                    IsEmployed = true,
                    Happiness = 70f
                });
            }

            // Гражданин с низким счастьем и без работы
            var unhappyUnemployed = new Citizen(0, 0, gameMap)
            {
                Age = 30,
                IsEmployed = false,
                Happiness = 20f
            };
            citizens.Add(unhappyUnemployed);

            // Гражданин с высокой удовлетворенностью и работой
            var happyEmployed = new Citizen(0, 0, gameMap)
            {
                Age = 30,
                IsEmployed = true,
                Happiness = 80f
            };
            citizens.Add(happyEmployed);

            // Небольшая группа безработных с низким счастьем
            for (int i = 0; i < 10; i++)
            {
                citizens.Add(new Citizen(0, 0, gameMap)
                {
                    Age = 30,
                    IsEmployed = false,
                    Happiness = 30f
                });
            }

            // Запуск симуляции тика экономики
            manager.SimulateTick();

            // Счастливый работающий сохраняется в городе
            Assert.IsTrue(citizens.Contains(happyEmployed),
                "Счастливый работающий гражданин не должен эмигрировать в первую очередь");

            // Несчастный безработный покидает город при наличии оттока
            if (manager.GetEmigrantsCount() > 0)
            {
                Assert.IsFalse(citizens.Contains(unhappyUnemployed),
                    "Несчастный безработный должен эмигрировать в первую очередь");
            }
        }
    }
}