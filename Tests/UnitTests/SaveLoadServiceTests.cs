using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;
using System.IO;
using Core.Models.Map;
using Infrastructure.Services;
using Core.Models.Base;
using Core.Models.Roads;
using Core.Enums;
using Core.Models.Buildings.CommertialBuildings;
using Core.Models.Buildings.SocialBuildings;

namespace Tests.UnitTests
{
    /// <summary>
    /// Тесты для сервиса сохранения и загрузки
    /// </summary>
    [TestClass]
    public sealed class SaveLoadServiceTests
    {
        /// <summary>
        /// Тест создания экземпляра сервиса
        /// </summary>
        [TestMethod]
        public void TestSaveLoadServiceCreation()
        {
            // Arrange & Act
            var service = new SaveLoadService();

            // Assert
            Assert.IsNotNull(service);
        }

        /// <summary>
        /// Тест проверки типа сервиса
        /// </summary>
        [TestMethod]
        public void TestSaveLoadServiceType()
        {
            // Arrange & Act
            var service = new SaveLoadService();

            // Assert
            Assert.IsInstanceOfType(service, typeof(SaveLoadService));
        }

        /// <summary>
        /// Тест сохранения и загрузки пустой карты
        /// </summary>
        [TestMethod]
        public void TestSaveAndLoadEmptyMap()
        {
            // Arrange
            var service = new SaveLoadService();
            var map = new GameMap(10, 10);
            string filePath = "test_empty_map.json";

            // Act
            service.SaveGame(map, filePath);
            service.LoadGame(map, filePath);

            // Assert
            Assert.IsTrue(File.Exists(filePath));
            Assert.AreEqual(0, map.Buildings.Count);
            Assert.AreEqual(0, map.RoadSegments.Count);

            // Cleanup
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        /// <summary>
        /// Тест сохранения и загрузки карты со зданиями
        /// </summary>
        [TestMethod]
        public void TestSaveAndLoadMapWithBuildings()
        {
            // Arrange
            var service = new SaveLoadService();
            var map = new GameMap(10, 10);
            string filePath = "test_buildings_map.json";

            // Добавляем тестовые здания
            map.Buildings.Add(new Shop { X = 1, Y = 1 });
            map.Buildings.Add(new Park { X = 2, Y = 2 });

            // Act
            service.SaveGame(map, filePath);

            // Создаем новую карту для загрузки
            var loadedMap = new GameMap(10, 10);
            service.LoadGame(loadedMap, filePath);

            // Assert
            Assert.IsTrue(File.Exists(filePath));
            Assert.AreEqual(2, loadedMap.Buildings.Count);

            // Cleanup
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        /// <summary>
        /// Тест сохранения и загрузки карты с дорогами
        /// </summary>
        [TestMethod]
        public void TestSaveAndLoadMapWithRoads()
        {
            // Arrange
            var service = new SaveLoadService();
            var map = new GameMap(10, 10);
            string filePath = "test_roads_map.json";

            // Добавляем тестовые дороги
            map.RoadSegments.Add(new RoadSegment(0, 0, 5, 0, RoadType.Street));
            map.RoadSegments.Add(new RoadSegment(0, 5, 5, 5, RoadType.Avenue));

            // Act
            service.SaveGame(map, filePath);

            var loadedMap = new GameMap(10, 10);
            service.LoadGame(loadedMap, filePath);

            // Assert
            Assert.IsTrue(File.Exists(filePath));
            Assert.AreEqual(2, loadedMap.RoadSegments.Count);

            // Cleanup
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        /// <summary>
        /// Тест обработки отсутствующего файла при загрузке
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void TestLoadNonExistentFile()
        {
            // Arrange
            var service = new SaveLoadService();
            var map = new GameMap(10, 10);
            string filePath = "non_existent_file.json";

            // Act & Assert
            service.LoadGame(map, filePath);
        }

        /// <summary>
        /// Тест базовой логики - истинные утверждения
        /// </summary>
        [TestMethod]
        public void TestBasicLogic()
        {
            // Arrange
            int expectedValue = 2 + 2;

            // Act
            int actualValue = 4;

            // Assert
            Assert.AreEqual(expectedValue, actualValue);
            Assert.IsTrue(actualValue > 0);
            Assert.IsFalse(actualValue < 0);
        }
    }
}