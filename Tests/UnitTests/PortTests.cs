using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core.Models.Buildings;
using Core.Models.Map;
using Core.Resourses;
using Core.Enums;
using System.Collections.Generic;

namespace Tests.UnitTests
{
    [TestClass]
    public sealed class PortTests
    {
        private PlayerResources CreateEmptyResources() =>
            new PlayerResources(0m, new Dictionary<ConstructionMaterial, int>());

        // SeaPort: можно ставить на суше, если есть соседний водный тайл
        [TestMethod]
        public void SeaPort_CanPlace_ReturnsTrue_WhenAdjacentToWater()
        {
            var map = new GameMap(5, 5);
            // Центр — суша
            map.Tiles[2, 2].Terrain = TerrainType.Plain;
            // Сосед справа — вода
            map.Tiles[3, 2].Terrain = TerrainType.Water;

            var port = new SeaPort(CreateEmptyResources());
            bool canPlace = port.CanPlace(2, 2, map);

            Assert.IsTrue(canPlace);
        }

        [TestMethod]
        public void SeaPort_CanPlace_ReturnsFalse_WhenNoWaterNearby()
        {
            var map = new GameMap(5, 5);
            // Все клетки — равнина
            for (int x = 0; x < map.Width; x++)
                for (int y = 0; y < map.Height; y++)
                    map.Tiles[x, y].Terrain = TerrainType.Plain;

            var port = new SeaPort(CreateEmptyResources());
            bool canPlace = port.CanPlace(2, 2, map);

            Assert.IsFalse(canPlace);
        }

        [TestMethod]
        public void SeaPort_TryPlace_PlacesBuilding_OnMap()
        {
            var map = new GameMap(5, 5);
            map.Tiles[2, 2].Terrain = TerrainType.Plain;
            map.Tiles[3, 2].Terrain = TerrainType.Water;

            var port = new SeaPort(CreateEmptyResources());
            bool placed = port.TryPlace(2, 2, map);

            Assert.IsTrue(placed);
            Assert.AreSame(port, map.GetBuildingAt(2, 2));
            // занятие всех тайлов в footprint
            for (int tx = 2; tx < 2 + port.Width; tx++)
                for (int ty = 2; ty < 2 + port.Height; ty++)
                    Assert.AreSame(port, map.GetBuildingAt(tx, ty));
        }

        [TestMethod]
        public void SeaPort_Units_AreInitialized_AfterPlacement()
        {
            var map = new GameMap(6, 6);
            map.Tiles[2, 2].Terrain = TerrainType.Plain;
            map.Tiles[3, 2].Terrain = TerrainType.Water;

            var port = new SeaPort(CreateEmptyResources());
            bool placed = port.TryPlace(2, 2, map);

            Assert.IsTrue(placed);
            // После OnBuildingPlaced юниты должны быть созданы
            Assert.IsNotNull(port.Units);
            Assert.AreEqual(10, port.Units.Count); // SeaPort.MaxUnits == 10
        }

        // AirPort: нельзя ставить близко к лесу (радиус 2)
        [TestMethod]
        public void AirPort_CanPlace_ReturnsFalse_WhenNearbyForest()
        {
            var map = new GameMap(7, 7);
            // Площадка для аэропорта — суша
            map.Tiles[3, 3].Terrain = TerrainType.Plain;
            // В радиусе 2 есть лес
            map.Tiles[5, 3].Terrain = TerrainType.Forest;
            map.Tiles[5, 3].TreeCount = 6;

            var port = new AirPort(CreateEmptyResources());
            bool canPlace = port.CanPlace(3, 3, map);

            Assert.IsFalse(canPlace);
        }

        [TestMethod]
        public void AirPort_CanPlace_ReturnsTrue_WhenNoForestNearby()
        {
            var map = new GameMap(7, 7);
            map.Tiles[3, 3].Terrain = TerrainType.Plain;
            // лес далеко
            map.Tiles[0, 0].Terrain = TerrainType.Forest;
            map.Tiles[0, 0].TreeCount = 8;

            var port = new AirPort(CreateEmptyResources());
            bool canPlace = port.CanPlace(3, 3, map);

            Assert.IsTrue(canPlace);
        }

        [TestMethod]
        public void AirPort_TryPlace_PlacesBuilding_OnMap()
        {
            var map = new GameMap(8, 8);
            map.Tiles[4, 4].Terrain = TerrainType.Plain;
            // рядом нет леса
            map.Tiles[0, 0].Terrain = TerrainType.Forest;
            map.Tiles[0, 0].TreeCount = 6;

            var port = new AirPort(CreateEmptyResources());
            bool placed = port.TryPlace(4, 4, map);

            Assert.IsTrue(placed);
            Assert.AreSame(port, map.GetBuildingAt(4, 4));
        }

        [TestMethod]
        public void AirPort_Units_AreInitialized_AfterPlacement()
        {
            var map = new GameMap(8, 8);
            map.Tiles[4, 4].Terrain = TerrainType.Plain;

            var port = new AirPort(CreateEmptyResources());
            bool placed = port.TryPlace(4, 4, map);

            Assert.IsTrue(placed);
            Assert.IsNotNull(port.Units);
            Assert.AreEqual(5, port.Units.Count); // AirPort.MaxUnits == 5
        }
    }
}