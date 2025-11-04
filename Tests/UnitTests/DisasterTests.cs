﻿using Core.Enums;
using Core.Models.Base;
using Core.Models.Buildings;
using Core.Models.Map;
using Core.Models.Mobs;
using Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnitTests
{
    /// <summary>
    /// Набор тестов для проверки функциональности системы бедствий.
    /// </summary>
    [TestClass]
    public class DisasterTests
    {
        // Константы для размеров карты
        private const int MAP_WIDTH = 10;
        private const int MAP_HEIGHT = 10;

        // Константы для координат
        private const int CENTER_X = 5;
        private const int CENTER_Y = 5;
        private const int TEST_X_1 = 1;
        private const int TEST_Y_1 = 1;
        private const int TEST_X_2 = 2;
        private const int TEST_Y_2 = 2;
        private const int TEST_X_3 = 3;
        private const int TEST_Y_3 = 3;
        private const int TEST_X_6 = 6;
        private const int TEST_Y_6 = 6;

        // Константы для параметров бедствий
        private const float HIGH_INTENSITY = 1.0f;
        private const float MEDIUM_INTENSITY = 0.8f;
        private const float LOW_INTENSITY = 0.5f;
        private const float LOW_INTENSITY_2 = 0.7f;
        private const int LONG_DURATION = 10;
        private const int MEDIUM_DURATION = 5;
        private const int SHORT_DURATION = 3;
        private const int VERY_SHORT_DURATION = 1;
        private const float LARGE_RADIUS = 2.5f;
        private const float MEDIUM_RADIUS = 2.0f;
        private const float SMALL_RADIUS = 1.5f;
        private const float VERY_SMALL_RADIUS = 1.0f;

        // Константы для условий зданий
        private const float FULL_CONDITION = 100f;
        private const float MIN_CONDITION = 0f;
        private const float EXPECTED_CONDITION_AFTER_EARTHQUAKE = 80f;
        private const float EXPECTED_CONDITION_AFTER_INDUSTRIAL = 70f;

        // Константы для тестовых данных
        private const int TREE_COUNT = 5;
        private const int NEIGHBOR_TREE_COUNT = 3;
        private const int MAX_UPDATE_ITERATIONS = 20;
        private const int UPDATE_ITERATIONS_FOR_COMPLETION = 3;

        // Сообщения для утверждений
        private const string FIRE_SHOULD_SPREAD_MESSAGE = "Пожар должен распространиться хотя бы на одного соседа";
        private const string FIRE_SHOULD_BE_ON_NEIGHBOR_MESSAGE = "Новый пожар должен быть на соседней клетке, а создан на ({0}, {1})";
        private const string BUILDING_SHOULD_TAKE_DAMAGE_MESSAGE = "Здание должно получить урон";
        private const string DAMAGE_SHOULD_BE_WITHIN_RANGE_MESSAGE = "Урон должен быть в пределах расчетного";
        private const string TREES_SHOULD_BE_DESTROYED_MESSAGE = "Деревья должны быть уничтожены";
        private const string TREE_TYPE_SHOULD_BE_RESET_MESSAGE = "Тип дерева должен быть сброшен";
        private const string PARK_SHOULD_BE_DESTROYED_MESSAGE = "Парк должен быть уничтожен";
        private const string ELECTRICITY_SHOULD_BE_DISABLED_MESSAGE = "Электричество должно быть отключено";
        private const string FIRE_SHOULD_BE_CREATED_MESSAGE = "Должен создаться пожар при взрыве газа";
        private const string FACTORY_SHOULD_TAKE_MORE_DAMAGE_MESSAGE = "Фабрика должна получить больше урона чем дом";

        private GameMap _map;
        private UtilityManager _utilityManager;
        private DisasterManager _disasterManager;

        /// <summary>
        /// Инициализация тестового окружения перед каждым тестом.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            // Создаем тестовую карту 10x10
            _map = new GameMap(MAP_WIDTH, MAP_HEIGHT);
            _utilityManager = new UtilityManager();
            _disasterManager = new DisasterManager(_map, _utilityManager);

            // Инициализируем тайлы
            for (int x = 0; x < _map.Width; x++)
            {
                for (int y = 0; y < _map.Height; y++)
                {
                    _map.Tiles[x, y] = new Tile
                    {
                        X = x,
                        Y = y,
                        Terrain = TerrainType.Plain,
                        Building = null,
                        HasRoad = true,
                        TreeCount = 0
                    };
                }
            }
        }

        /// <summary>
        /// Проверка корректной инициализации события бедствия.
        /// </summary>
        [TestMethod]
        public void DisasterEvent_Constructor_ShouldInitializeCorrectly()
        {
            // Arrange & Act
            var disaster = new DisasterEvent(
                DisasterType.Fire,
                MEDIUM_INTENSITY,
                MEDIUM_DURATION,
                TEST_X_3, TEST_Y_3,
                LARGE_RADIUS,
                _map,
                _disasterManager
            );

            // Assert
            Assert.AreEqual(DisasterType.Fire, disaster.Type);
            Assert.AreEqual(MEDIUM_INTENSITY, disaster.Intensity);
            Assert.AreEqual(MEDIUM_DURATION, disaster.DurationTicks);
            Assert.AreEqual(TEST_X_3, disaster.EpicenterX);
            Assert.AreEqual(TEST_Y_3, disaster.EpicenterY);
            Assert.AreEqual(LARGE_RADIUS, disaster.Radius);
            Assert.AreEqual(_map, disaster.Map);
            Assert.AreEqual(_disasterManager, disaster.Manager);
            Assert.IsTrue(disaster.IsActive);
            Assert.AreEqual(0, disaster.CurrentTick);
        }

        /// <summary>
        /// Проверка изменения состояния активности события бедствия при превышении длительности.
        /// </summary>
        [TestMethod]
        public void DisasterEvent_IsActive_ShouldReturnFalse_WhenCurrentTickExceedsDuration()
        {
            // Arrange
            var disaster = new DisasterEvent(DisasterType.Fire, LOW_INTENSITY, SHORT_DURATION, TEST_X_1, TEST_Y_1, MEDIUM_RADIUS, _map);

            // Act & Assert - проверяем состояние после каждого Update

            // После 0 обновлений
            Assert.IsTrue(disaster.IsActive);
            Assert.AreEqual(0, disaster.CurrentTick);

            disaster.Update(); // Tick 0 -> 1
            Assert.IsTrue(disaster.IsActive);
            Assert.AreEqual(1, disaster.CurrentTick);

            disaster.Update(); // Tick 1 -> 2  
            Assert.IsTrue(disaster.IsActive);
            Assert.AreEqual(2, disaster.CurrentTick);

            disaster.Update(); // Tick 2 -> 3
                               // Теперь CurrentTick = 3, DurationTicks = 3, поэтому IsActive = false
            Assert.IsFalse(disaster.IsActive);
            Assert.AreEqual(SHORT_DURATION, disaster.CurrentTick);

            // Дополнительный вызов - ничего не должно происходить
            disaster.Update();
            Assert.IsFalse(disaster.IsActive);
            Assert.AreEqual(SHORT_DURATION, disaster.CurrentTick); // Не увеличивается, т.к. неактивно
        }

        /// <summary>
        /// Проверка распространения пожара на соседние клетки при выполнении условий.
        /// </summary>
        [TestMethod]
        public void DisasterEvent_FireSpread_ShouldCreateNewFireEvents_WhenConditionsMet()
        {
            // Arrange - создаем горючие материалы во ВСЕХ соседних клетках
            var neighbors = new (int, int)[] { (CENTER_X, CENTER_Y + 1), (CENTER_X, CENTER_Y - 1), (CENTER_X - 1, CENTER_Y), (CENTER_X + 1, CENTER_Y) };

            foreach (var (x, y) in neighbors)
            {
                _map.Tiles[x, y].Building = new TestResidentialBuilding(ResidentialType.Apartment);
                _map.Tiles[x, y].TreeCount = NEIGHBOR_TREE_COUNT; // Добавляем деревья для гарантии
            }

            var fire = new DisasterEvent(DisasterType.Fire, HIGH_INTENSITY, LONG_DURATION, CENTER_X, CENTER_Y, MEDIUM_RADIUS, _map, _disasterManager);

            var spreadFires = new List<DisasterEvent>();
            fire.OnFireSpread += (newFire) => spreadFires.Add(newFire);

            // Act - выполняем достаточно обновлений для гарантированного распространения
            for (int i = 0; i < MAX_UPDATE_ITERATIONS; i++)
            {
                fire.Update();
            }

            // Assert
            Assert.IsTrue(spreadFires.Count > 0, FIRE_SHOULD_SPREAD_MESSAGE);

            // Проверяем, что новые пожары созданы на соседних клетках
            foreach (var spreadFire in spreadFires)
            {
                bool isNeighbor = neighbors.Any(n =>
                    n.Item1 == spreadFire.EpicenterX && n.Item2 == spreadFire.EpicenterY);
                Assert.IsTrue(isNeighbor, string.Format(FIRE_SHOULD_BE_ON_NEIGHBOR_MESSAGE, spreadFire.EpicenterX, spreadFire.EpicenterY));
            }
        }

        /// <summary>
        /// Проверка увеличения текущего такта при обновлении события бедствия.
        /// </summary>
        [TestMethod]
        public void DisasterEvent_Update_ShouldIncrementCurrentTick()
        {
            // Arrange
            var disaster = new DisasterEvent(DisasterType.Fire, LOW_INTENSITY, MEDIUM_DURATION, TEST_X_1, TEST_Y_1, MEDIUM_RADIUS, _map);

            // Act
            disaster.Update();

            // Assert
            Assert.AreEqual(1, disaster.CurrentTick);
            Assert.IsTrue(disaster.IsActive);
        }

        /// <summary>
        /// Проверка нанесения урона зданиям при землетрясении.
        /// </summary>
        [TestMethod]
        public void DisasterEvent_Earthquake_ShouldDamageBuildings()
        {
            // Arrange
            var building = new TestResidentialBuilding(ResidentialType.Apartment)
            {
                Condition = FULL_CONDITION,
                HasElectricity = true,
                HasWater = true,
                HasGas = true,
                HasSewage = true
            };

            _map.Tiles[CENTER_X, CENTER_Y].Building = building;
            var earthquake = new DisasterEvent(DisasterType.Earthquake, MEDIUM_INTENSITY, VERY_SHORT_DURATION, CENTER_X, CENTER_Y, VERY_SMALL_RADIUS, _map);

            // Act
            earthquake.Update();

            // Assert
            Assert.IsTrue(building.Condition < FULL_CONDITION, BUILDING_SHOULD_TAKE_DAMAGE_MESSAGE);
            Assert.IsTrue(building.Condition >= EXPECTED_CONDITION_AFTER_EARTHQUAKE, DAMAGE_SHOULD_BE_WITHIN_RANGE_MESSAGE);
        }

        /// <summary>
        /// Проверка уничтожения деревьев и парков при пожаре.
        /// </summary>
        [TestMethod]
        public void DisasterEvent_Fire_ShouldDestroyTreesAndParks()
        {
            // Arrange
            var tile = _map.Tiles[CENTER_X, CENTER_Y];
            tile.TreeCount = TREE_COUNT;
            tile.TreeType = TreeType.Oak;
            tile.HasPark = true;

            var fire = new DisasterEvent(DisasterType.Fire, HIGH_INTENSITY, VERY_SHORT_DURATION, CENTER_X, CENTER_Y, VERY_SMALL_RADIUS, _map);

            // Act
            fire.Update();

            // Assert
            Assert.AreEqual(0, tile.TreeCount, TREES_SHOULD_BE_DESTROYED_MESSAGE);
            Assert.IsNull(tile.TreeType, TREE_TYPE_SHOULD_BE_RESET_MESSAGE);
            Assert.IsFalse(tile.HasPark, PARK_SHOULD_BE_DESTROYED_MESSAGE);
        }

        /// <summary>
        /// Проверка отключения электричества при аварии в энергосети.
        /// </summary>
        [TestMethod]
        public void DisasterEvent_PowerGridFailure_ShouldDisableElectricity()
        {
            // Arrange
            var building = new TestCommercialBuilding(CommercialBuildingType.Shop)
            {
                HasElectricity = true
            };

            _map.Tiles[CENTER_X, CENTER_Y].Building = building;
            var powerFailure = new DisasterEvent(DisasterType.PowerGridFailure, HIGH_INTENSITY, VERY_SHORT_DURATION, CENTER_X, CENTER_Y, VERY_SMALL_RADIUS, _map);

            // Act
            powerFailure.Update();

            // Assert
            Assert.IsFalse(building.HasElectricity, ELECTRICITY_SHOULD_BE_DISABLED_MESSAGE);
        }

        /// <summary>
        /// Проверка создания пожара при взрыве газа.
        /// </summary>
        [TestMethod]
        public void DisasterEvent_GasLeak_ShouldCreateFire_WhenExplosionOccurs()
        {
            // Arrange
            var building = new TestResidentialBuilding(ResidentialType.Apartment)
            {
                Condition = FULL_CONDITION,
                HasGas = true
            };

            _map.Tiles[CENTER_X, CENTER_Y].Building = building;

            // Подписываемся на событие создания нового бедствия
            DisasterEvent newFireEvent = null;
            _disasterManager.OnDisasterStarted += (disaster) =>
            {
                if (disaster.Type == DisasterType.Fire)
                    newFireEvent = disaster;
            };

            var gasLeak = new DisasterEvent(DisasterType.GasLeak, HIGH_INTENSITY, VERY_SHORT_DURATION, CENTER_X, CENTER_Y, VERY_SMALL_RADIUS, _map, _disasterManager);

            // Act
            gasLeak.Update();

            // Assert
            Assert.IsNotNull(newFireEvent, FIRE_SHOULD_BE_CREATED_MESSAGE);
            Assert.AreEqual(DisasterType.Fire, newFireEvent.Type);
            Assert.AreEqual(CENTER_X, newFireEvent.EpicenterX);
            Assert.AreEqual(CENTER_Y, newFireEvent.EpicenterY);
        }

        /// <summary>
        /// Проверка большего урона промышленным зданиям при промышленной аварии.
        /// </summary>
        [TestMethod]
        public void DisasterEvent_IndustrialAccident_ShouldDamageFactoryMore()
        {
            // Arrange
            var factory = new TestCommercialBuilding(CommercialBuildingType.Factory)
            {
                Condition = FULL_CONDITION
            };

            var house = new TestResidentialBuilding(ResidentialType.Apartment)
            {
                Condition = FULL_CONDITION
            };

            _map.Tiles[CENTER_X, CENTER_Y].Building = factory;
            _map.Tiles[TEST_X_6, TEST_Y_6].Building = house;

            var accident = new DisasterEvent(DisasterType.IndustrialAccident, LOW_INTENSITY, VERY_SHORT_DURATION, CENTER_X, CENTER_Y, MEDIUM_RADIUS, _map);

            // Act
            accident.Update();

            // Assert
            Assert.IsTrue(factory.Condition < house.Condition, FACTORY_SHOULD_TAKE_MORE_DAMAGE_MESSAGE);
            Assert.IsTrue(factory.Condition <= EXPECTED_CONDITION_AFTER_INDUSTRIAL, DAMAGE_SHOULD_BE_WITHIN_RANGE_MESSAGE);
        }

        /// <summary>
        /// Проверка добавления события бедствия в список активных при запуске.
        /// </summary>
        [TestMethod]
        public void DisasterManager_TriggerDisaster_ShouldAddToActiveEvents()
        {
            // Arrange & Act
            _disasterManager.TriggerDisaster(DisasterType.Fire, LOW_INTENSITY, SHORT_DURATION, TEST_X_2, TEST_Y_2, SMALL_RADIUS);

            // Assert
            Assert.AreEqual(1, _disasterManager.ActiveEvents.Count);
            var disaster = _disasterManager.ActiveEvents[0];
            Assert.AreEqual(DisasterType.Fire, disaster.Type);
            Assert.AreEqual(TEST_X_2, disaster.EpicenterX);
            Assert.AreEqual(TEST_Y_2, disaster.EpicenterY);
        }

        /// <summary>
        /// Проверка удаления завершенных событий из списка активных при обновлении.
        /// </summary>
        [TestMethod]
        public void DisasterManager_Update_ShouldRemoveFinishedEvents()
        {
            // Arrange
            _disasterManager.TriggerDisaster(DisasterType.Fire, LOW_INTENSITY, VERY_SHORT_DURATION, TEST_X_2, TEST_Y_2, SMALL_RADIUS);

            // Act - выполняем достаточно обновлений для завершения события
            for (int i = 0; i < UPDATE_ITERATIONS_FOR_COMPLETION; i++)
            {
                _disasterManager.Update();
            }

            // Assert
            Assert.AreEqual(0, _disasterManager.ActiveEvents.Count);
        }

        /// <summary>
        /// Проверка очистки всех активных событий при остановке бедствий.
        /// </summary>
        [TestMethod]
        public void DisasterManager_StopAllDisasters_ShouldClearActiveEvents()
        {
            // Arrange
            _disasterManager.TriggerDisaster(DisasterType.Fire, LOW_INTENSITY, MEDIUM_DURATION, TEST_X_2, TEST_Y_2, SMALL_RADIUS);
            _disasterManager.TriggerDisaster(DisasterType.Earthquake, LOW_INTENSITY_2, SHORT_DURATION, TEST_X_3, TEST_Y_3, MEDIUM_RADIUS);

            // Act
            _disasterManager.StopAllDisasters();

            // Assert
            Assert.AreEqual(0, _disasterManager.ActiveEvents.Count);
        }

        /// <summary>
        /// Проверка генерации отчета о бедствиях при наличии активных событий.
        /// </summary>
        [TestMethod]
        public void DisasterManager_GetDisasterReport_ShouldReturnReport_WhenActiveEventsExist()
        {
            // Arrange
            _disasterManager.TriggerDisaster(DisasterType.Fire, LOW_INTENSITY, MEDIUM_DURATION, TEST_X_2, TEST_Y_2, SMALL_RADIUS);
            _disasterManager.TriggerDisaster(DisasterType.Earthquake, LOW_INTENSITY_2, SHORT_DURATION, TEST_X_3, TEST_Y_3, MEDIUM_RADIUS);

            // Act
            var report = _disasterManager.GetDisasterReport();

            // Assert
            Assert.IsTrue(report.Contains("Active Disasters: 2"));
            Assert.IsTrue(report.Contains("Fire"));
            Assert.IsTrue(report.Contains("Earthquake"));
        }

        /// <summary>
        /// Проверка генерации отчета о бедствиях при отсутствии активных событий.
        /// </summary>
        [TestMethod]
        public void DisasterManager_GetDisasterReport_ShouldReturnNoDisasters_WhenNoActiveEvents()
        {
            // Act
            var report = _disasterManager.GetDisasterReport();

            // Assert
            Assert.AreEqual("No active disasters.", report);
        }

        // Вспомогательные тестовые классы
        private class TestResidentialBuilding : ResidentialBuilding
        {
            public TestResidentialBuilding(ResidentialType type) : base(type) { }
        }

        private class TestCommercialBuilding : CommercialBuilding
        {
            public TestCommercialBuilding(CommercialBuildingType type) : base(type) { }
        }
    }
}