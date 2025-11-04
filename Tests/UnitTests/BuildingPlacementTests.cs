using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core.Models.Buildings;
using Core.Models.Map;
using Core.Enums;
using System.Collections.Generic;

namespace Tests.UnitTests
{
    [TestClass]
    public sealed class BuildingPlacementTests
    {
        private GameMap map;

        [TestInitialize]
        public void Setup()
        {
            map = new GameMap(10, 10);
            for (int x = 0; x < map.Width; x++)
                for (int y = 0; y < map.Height; y++)
                    map.Tiles[x, y].Terrain = TerrainType.Plain; // все тайлы пригодны
        }

        private ServiceBuilding CreateDefaultBuilding()
        {
            var materials = new Dictionary<ConstructionMaterial, int>
            {
                { ConstructionMaterial.Steel, 5 },
                { ConstructionMaterial.Concrete, 5 }
            };

            return new ServiceBuilding(ServiceBuildingType.School, 100);
        }

        // Успешное размещение на свободных тайлах
        [TestMethod]
        public void TryPlace_ShouldSucceed_OnFreeTiles()
        {
            var building = CreateDefaultBuilding();

            bool placed = building.TryPlace(2, 2, map);

            Assert.IsTrue(placed, "Здание должно успешно разместиться на свободных тайлах.");
            Assert.AreEqual(2, building.X);
            Assert.AreEqual(2, building.Y);
            Assert.AreEqual(building, map.GetBuildingAt(2, 2), "Тайл должен быть занят зданием.");
        }

        // Попытка построить за пределами карты
        [TestMethod]
        public void TryPlace_ShouldFail_WhenOutOfBounds()
        {
            var building = CreateDefaultBuilding();

            bool placed = building.TryPlace(9, 9, map);

            Assert.IsFalse(placed, "Здание не должно размещаться за границей карты.");
        }

        // Попытка построить на воде
        [TestMethod]
        public void TryPlace_ShouldFail_OnWater()
        {
            map.Tiles[3, 3].Terrain = TerrainType.Water;

            var building = CreateDefaultBuilding();
            bool placed = building.TryPlace(3, 3, map);

            Assert.IsFalse(placed, "Здание не должно размещаться на воде.");
        }

        // Попытка построить на горе
        [TestMethod]
        public void TryPlace_ShouldFail_OnMountain()
        {
            map.Tiles[1, 1].Terrain = TerrainType.Mountain;

            var building = CreateDefaultBuilding();
            bool placed = building.TryPlace(1, 1, map);

            Assert.IsFalse(placed, "Здание не должно размещаться на горной местности.");
        }

        // Попытка построить поверх другого здания
        [TestMethod]
        public void TryPlace_ShouldFail_WhenTileOccupied()
        {
            var b1 = CreateDefaultBuilding();
            b1.TryPlace(2, 2, map);

            var b2 = CreateDefaultBuilding();
            bool placed2 = b2.TryPlace(2, 2, map);

            Assert.IsFalse(placed2, "Второе здание не должно размещаться поверх первого.");
        }

        // Проверка, что здание занимает все свои тайлы
        [TestMethod]
        public void TryPlace_ShouldOccupyAllTiles()
        {
            var building = CreateDefaultBuilding();
            bool placed = building.TryPlace(1, 1, map);

            Assert.IsTrue(placed);

            for (int x = 1; x < 1 + building.Width; x++)
            {
                for (int y = 1; y < 1 + building.Height; y++)
                {
                    Assert.AreEqual(building, map.GetBuildingAt(x, y),
                        $"Тайл ({x},{y}) должен быть занят зданием.");
                }
            }
        }
    }
}
