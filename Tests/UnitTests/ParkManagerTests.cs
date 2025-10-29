using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core.Services;
using Core.Models.Map;

namespace UnitTests
{
    [TestClass]
    public class ParkManagerTests
    {
        [TestMethod]
        public void CreatePark_ShouldSetParkAndPedestrianFlags()
        {
            // Arrange
            var parkManager = new ParkManager();
            var gameMap = new GameMap(10, 10);

            // Act
            parkManager.CreatePark(gameMap, 2, 2, 3, 3);

            // Assert
            for (int x = 2; x < 5; x++)
            {
                for (int y = 2; y < 5; y++)
                {
                    Assert.IsTrue(gameMap.Tiles[x, y].HasPark);
                    Assert.IsTrue(gameMap.Tiles[x, y].HasPedestrianPath);
                }
            }
        }

        [TestMethod]
        public void CreatePark_OutOfBounds_ShouldHandleGracefully()
        {
            // Arrange
            var parkManager = new ParkManager();
            var gameMap = new GameMap(5, 5);

            // Act & Assert - проверяем что метод не падает, а обрабатывает границы
            try
            {
                parkManager.CreatePark(gameMap, 4, 4, 3, 3);
                // Если не упало - проверяем что создалось только в пределах карты
                int createdParks = 0;
                for (int x = 4; x < 7; x++)
                {
                    for (int y = 4; y < 7; y++)
                    {
                        if (x < 5 && y < 5 && gameMap.Tiles[x, y].HasPark)
                            createdParks++;
                    }
                }
                Assert.IsTrue(createdParks > 0); // Хотя бы некоторые тайлы созданы
            }
            catch
            {
                Assert.Fail("Method should handle out of bounds gracefully");
            }
        }
        [TestMethod]
        public void CreateBikeLane_OutOfBounds_ShouldHandleGracefully()
        {
            // Arrange
            var parkManager = new ParkManager();
            var gameMap = new GameMap(5, 5);

            // Act & Assert
            try
            {
                parkManager.CreateBikeLane(gameMap, 4, 4, 3, true);
                // Проверяем что создалось в пределах карты
                int createdLanes = 0;
                for (int x = 4; x < 7; x++)
                {
                    if (x < 5 && gameMap.Tiles[x, 4].HasBikeLane)
                        createdLanes++;
                }
                Assert.IsTrue(createdLanes > 0);
            }
            catch
            {
                Assert.Fail("Method should handle out of bounds gracefully");
            }
        }


        [TestMethod]
        public void CreateBikeLane_Horizontal_ShouldSetBikeLaneFlags()
        {
            // Arrange
            var parkManager = new ParkManager();
            var gameMap = new GameMap(10, 10);

            // Act
            parkManager.CreateBikeLane(gameMap, 1, 1, 5, true);

            // Assert
            for (int x = 1; x < 6; x++)
            {
                Assert.IsTrue(gameMap.Tiles[x, 1].HasBikeLane);
                Assert.IsTrue(gameMap.Tiles[x, 1].HasPedestrianPath);
            }
        }

        [TestMethod]
        public void CreateBikeLane_Vertical_ShouldSetBikeLaneFlags()
        {
            // Arrange
            var parkManager = new ParkManager();
            var gameMap = new GameMap(10, 10);

            // Act
            parkManager.CreateBikeLane(gameMap, 1, 1, 5, false);

            // Assert
            for (int y = 1; y < 6; y++)
            {
                Assert.IsTrue(gameMap.Tiles[1, y].HasBikeLane);
                Assert.IsTrue(gameMap.Tiles[1, y].HasPedestrianPath);
            }
        }
        [TestMethod]
        public void GetHappyTilesCount_ShouldReturnCorrectCount()
        {
            // Arrange
            var parkManager = new ParkManager();
            var gameMap = new GameMap(5, 5);

            parkManager.CreatePark(gameMap, 0, 0, 2, 2); // 4 тайла с парком
            parkManager.CreateBikeLane(gameMap, 3, 0, 2, true); // 2 тайла с велодорожкой

            // Act
            int happyTilesCount = parkManager.GetHappyTilesCount(gameMap);

            // Assert
            Assert.AreEqual(6, happyTilesCount); // 4 парка + 2 велодорожки
        }

        [TestMethod]
        public void GetHappyTilesCount_NoInfrastructure_ShouldReturnZero()
        {
            // Arrange
            var parkManager = new ParkManager();
            var gameMap = new GameMap(5, 5);

            // Act
            int happyTilesCount = parkManager.GetHappyTilesCount(gameMap);

            // Assert
            Assert.AreEqual(0, happyTilesCount);
        }

        [TestMethod]
        public void CreatePark_And_BikeLane_Overlap_ShouldSetAllFlags()
        {
            // Arrange
            var parkManager = new ParkManager();
            var gameMap = new GameMap(10, 10);

            // Act
            parkManager.CreatePark(gameMap, 1, 1, 3, 3);
            parkManager.CreateBikeLane(gameMap, 2, 0, 3, false); // Пересекает парк

            // Assert - проверяем пересекающийся тайл
            Assert.IsTrue(gameMap.Tiles[2, 1].HasPark);
            Assert.IsTrue(gameMap.Tiles[2, 1].HasBikeLane);
            Assert.IsTrue(gameMap.Tiles[2, 1].HasPedestrianPath);
        }

        [TestMethod]
        public void CreatePark_SingleTile_ShouldWork()
        {
            // Arrange
            var parkManager = new ParkManager();
            var gameMap = new GameMap(5, 5);

            // Act
            parkManager.CreatePark(gameMap, 2, 2, 1, 1);

            // Assert
            Assert.IsTrue(gameMap.Tiles[2, 2].HasPark);
            Assert.IsTrue(gameMap.Tiles[2, 2].HasPedestrianPath);
        }

        [TestMethod]
        public void CreateBikeLane_SingleTile_ShouldWork()
        {
            // Arrange
            var parkManager = new ParkManager();
            var gameMap = new GameMap(5, 5);

            // Act
            parkManager.CreateBikeLane(gameMap, 2, 2, 1, true);

            // Assert
            Assert.IsTrue(gameMap.Tiles[2, 2].HasBikeLane);
            Assert.IsTrue(gameMap.Tiles[2, 2].HasPedestrianPath);
        }
    }
}