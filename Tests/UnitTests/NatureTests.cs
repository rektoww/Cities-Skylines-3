using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core.Enums;
using Core.Models.Map;
using Core.Services;
using Infrastructure.Services;

namespace UnitTests
{
    [TestClass]
    public class NatureTests
    {
        [TestMethod]
        public void Tile_WithForestTerrain_ShouldHaveForestProperties()
        {
            // Arrange
            var tile = new Tile();

            // Act
            tile.Terrain = TerrainType.Forest;
            tile.TreeType = TreeType.Pine;
            tile.TreeCount = 8;

            // Assert
            Assert.AreEqual(TerrainType.Forest, tile.Terrain);
            Assert.AreEqual(TreeType.Pine, tile.TreeType);
            Assert.AreEqual(8, tile.TreeCount);
            Assert.IsTrue(tile.HasForest);
        }

        [TestMethod]
        public void Tile_WithMeadowTerrain_ShouldHaveMeadowProperties()
        {
            // Arrange
            var tile = new Tile();

            // Act
            tile.Terrain = TerrainType.Meadow;
            tile.TreeType = TreeType.Oak;
            tile.TreeCount = 2;

            // Assert
            Assert.AreEqual(TerrainType.Meadow, tile.Terrain);
            Assert.AreEqual(TreeType.Oak, tile.TreeType);
            Assert.AreEqual(2, tile.TreeCount);
            Assert.IsFalse(tile.HasForest);
        }

        [TestMethod]
        public void Tile_WithoutTrees_ShouldHaveZeroTreeCount()
        {
            // Arrange & Act
            var tile = new Tile();

            // Assert
            Assert.AreEqual(0, tile.TreeCount);
            Assert.IsNull(tile.TreeType);
            Assert.IsFalse(tile.HasForest);
        }

        [TestMethod]
        public void Tile_Initialization_ShouldHaveEmptyResourcesAndZeroTrees()
        {
            // Arrange & Act
            var tile = new Tile();

            // Assert
            Assert.IsNotNull(tile.Resources);
            Assert.AreEqual(0, tile.Resources.Count);
            Assert.AreEqual(0, tile.TreeCount);
            Assert.IsNull(tile.TreeType);
        }

        [TestMethod]
        public void NatureManager_PlantTrees_ShouldSetTreeProperties()
        {
            // Arrange
            var natureManager = new NatureManager();
            var gameMap = new GameMap(5, 5);

            // Act
            natureManager.PlantTrees(gameMap, 2, 2, TreeType.Birch, 5);

            // Assert
            var tile = gameMap.Tiles[2, 2];
            Assert.AreEqual(TreeType.Birch, tile.TreeType);
            Assert.AreEqual(5, tile.TreeCount);
        }

        [TestMethod]
        public void NatureManager_PlantTrees_OutOfBounds_ShouldHandleGracefully()
        {
            // Arrange
            var natureManager = new NatureManager();
            var gameMap = new GameMap(5, 5);

            // Act & Assert - не должно быть исключения
            try
            {
                natureManager.PlantTrees(gameMap, 10, 10, TreeType.Spruce, 3);
                Assert.IsTrue(true); // Если дошли сюда - тест пройден
            }
            catch
            {
                Assert.Fail("Метод должен обрабатывать координаты за пределами карты");
            }
        }

        [TestMethod]
        public void NatureManager_ClearTrees_ShouldResetTreeProperties()
        {
            // Arrange
            var natureManager = new NatureManager();
            var gameMap = new GameMap(5, 5);
            natureManager.PlantTrees(gameMap, 2, 2, TreeType.Pine, 7);

            // Act
            natureManager.ClearTrees(gameMap, 2, 2);

            // Assert
            var tile = gameMap.Tiles[2, 2];
            Assert.IsNull(tile.TreeType);
            Assert.AreEqual(0, tile.TreeCount);
        }

        [TestMethod]
        public void NatureManager_ClearTrees_OutOfBounds_ShouldHandleGracefully()
        {
            // Arrange
            var natureManager = new NatureManager();
            var gameMap = new GameMap(5, 5);

            // Act & Assert - не должно быть исключения
            try
            {
                natureManager.ClearTrees(gameMap, 10, 10);
                Assert.IsTrue(true);
            }
            catch
            {
                Assert.Fail("Метод должен обрабатывать координаты за пределами карты");
            }
        }

        [TestMethod]
        public void NatureManager_GetTotalTreeCount_ShouldReturnCorrectCount()
        {
            // Arrange
            var natureManager = new NatureManager();
            var gameMap = new GameMap(3, 3);

            natureManager.PlantTrees(gameMap, 0, 0, TreeType.Oak, 3);
            natureManager.PlantTrees(gameMap, 1, 1, TreeType.Pine, 5);
            natureManager.PlantTrees(gameMap, 2, 2, TreeType.Birch, 2);

            // Act
            int totalTrees = natureManager.GetTotalTreeCount(gameMap);

            // Assert
            Assert.AreEqual(10, totalTrees); // 3 + 5 + 2
        }

        [TestMethod]
        public void NatureManager_GetTilesWithTreesCount_ShouldReturnCorrectCount()
        {
            // Arrange
            var natureManager = new NatureManager();
            var gameMap = new GameMap(3, 3);

            natureManager.PlantTrees(gameMap, 0, 0, TreeType.Oak, 3);
            natureManager.PlantTrees(gameMap, 1, 1, TreeType.Pine, 5);
            // Тайл [2,2] без деревьев

            // Act
            int tilesWithTrees = natureManager.GetTilesWithTreesCount(gameMap);

            // Assert
            Assert.AreEqual(2, tilesWithTrees);
        }

        [TestMethod]
        public void NatureManager_GetTreeTypeStatistics_ShouldReturnCorrectStatistics()
        {
            // Arrange
            var natureManager = new NatureManager();
            var gameMap = new GameMap(3, 3);

            natureManager.PlantTrees(gameMap, 0, 0, TreeType.Oak, 3);
            natureManager.PlantTrees(gameMap, 1, 1, TreeType.Pine, 5);
            natureManager.PlantTrees(gameMap, 2, 2, TreeType.Oak, 2); // Еще дубы

            // Act
            var statistics = natureManager.GetTreeTypeStatistics(gameMap);

            // Assert
            Assert.AreEqual(5, statistics[TreeType.Oak]); // 3 + 2
            Assert.AreEqual(5, statistics[TreeType.Pine]);
            Assert.AreEqual(0, statistics[TreeType.Birch]);
            Assert.AreEqual(0, statistics[TreeType.Spruce]);
        }

        [TestMethod]
        public void NatureManager_PlantTrees_WithInvalidCount_ShouldClampToValidRange()
        {
            // Arrange
            var natureManager = new NatureManager();
            var gameMap = new GameMap(5, 5);

            // Act - попытка посадить 0 деревьев (должно стать 1)
            natureManager.PlantTrees(gameMap, 2, 2, TreeType.Spruce, 0);

            // Assert
            var tile = gameMap.Tiles[2, 2];
            Assert.AreEqual(1, tile.TreeCount); // Должно быть минимум 1

            // Act - попытка посадить 15 деревьев (должно стать 10)
            natureManager.PlantTrees(gameMap, 3, 3, TreeType.Birch, 15);

            // Assert
            tile = gameMap.Tiles[3, 3];
            Assert.AreEqual(10, tile.TreeCount); // Должно быть максимум 10
        }

        [TestMethod]
        public void StaticMapProvider_ShouldGenerateForestTilesWithTrees()
        {
            // Arrange & Act
            var map = StaticMapProvider.Build();
            bool foundForestWithTrees = false;

            // Assert - ищем хотя бы один лес с деревьями
            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    var tile = map.Tiles[x, y];
                    if (tile.Terrain == TerrainType.Forest &&
                        tile.TreeType.HasValue &&
                        tile.TreeCount > 0)
                    {
                        foundForestWithTrees = true;
                        break;
                    }
                }
                if (foundForestWithTrees) break;
            }

            Assert.IsTrue(foundForestWithTrees, "Должен быть хотя бы один лес с деревьями");
        }

        [TestMethod]
        public void StaticMapProvider_ShouldGenerateMeadowTiles()
        {
            // Arrange & Act
            var map = StaticMapProvider.Build();
            bool foundMeadow = false;

            // Assert - ищем хотя бы один луг
            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    if (map.Tiles[x, y].Terrain == TerrainType.Meadow)
                    {
                        foundMeadow = true;
                        break;
                    }
                }
                if (foundMeadow) break;
            }

            Assert.IsTrue(foundMeadow, "Должен быть хотя бы один луг");
        }

        [TestMethod]
        public void TreeType_Enum_ShouldHaveFourTreeTypes()
        {
            // Arrange & Act
            var treeTypes = Enum.GetValues(typeof(TreeType));

            // Assert
            Assert.AreEqual(4, treeTypes.Length);
            Assert.IsTrue(Enum.IsDefined(typeof(TreeType), TreeType.Oak));
            Assert.IsTrue(Enum.IsDefined(typeof(TreeType), TreeType.Pine));
            Assert.IsTrue(Enum.IsDefined(typeof(TreeType), TreeType.Birch));
            Assert.IsTrue(Enum.IsDefined(typeof(TreeType), TreeType.Spruce));
        }

        [TestMethod]
        public void TerrainType_Enum_ShouldHaveFiveTerrainTypes()
        {
            // Arrange & Act
            var terrainTypes = Enum.GetValues(typeof(TerrainType));

            // Assert
            Assert.AreEqual(5, terrainTypes.Length);
            Assert.IsTrue(Enum.IsDefined(typeof(TerrainType), TerrainType.Water));
            Assert.IsTrue(Enum.IsDefined(typeof(TerrainType), TerrainType.Plain));
            Assert.IsTrue(Enum.IsDefined(typeof(TerrainType), TerrainType.Mountain));
            Assert.IsTrue(Enum.IsDefined(typeof(TerrainType), TerrainType.Forest));
            Assert.IsTrue(Enum.IsDefined(typeof(TerrainType), TerrainType.Meadow));
        }
    }
}