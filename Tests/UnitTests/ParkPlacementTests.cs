using Core.Enums;
using Core.Models.Buildings.SocialBuildings;
using Core.Models.Map;

[TestClass]
public class ParkPlacementTests
{
    [TestMethod]
    public void Park_CanPlace_OnSuitableTerrain_ShouldReturnTrue()
    {
        // Arrange
        var park = new Park();
        var map = new GameMap(10, 10);

        // Act & Assert - парк можно размещать на подходящей местности
        Assert.IsTrue(park.CanPlace(0, 0, map)); // Равнина
    }

    [TestMethod]
    public void Park_CanPlace_OnWater_ShouldReturnFalse()
    {
        // Arrange
        var park = new Park();
        var map = new GameMap(10, 10);
        map.Tiles[2, 2].Terrain = TerrainType.Water;

        // Act & Assert - парк нельзя размещать на воде
        Assert.IsFalse(park.CanPlace(2, 2, map));
    }

    [TestMethod]
    public void Park_CanPlace_OnMountain_ShouldReturnFalse()
    {
        // Arrange
        var park = new Park();
        var map = new GameMap(10, 10);
        map.Tiles[3, 3].Terrain = TerrainType.Mountain;

        // Act & Assert - парк нельзя размещать на горах
        Assert.IsFalse(park.CanPlace(3, 3, map));
    }

    [TestMethod]
    public void Park_CanPlace_OutOfBounds_ShouldReturnFalse()
    {
        // Arrange
        var park = new Park();
        var map = new GameMap(5, 5);

        // Act & Assert - парк нельзя размещать за границами карты
        Assert.IsFalse(park.CanPlace(4, 4, map)); // Парк 2x2 не влезет в позицию 4,4
    }

    [TestMethod]
    public void Park_TryPlace_Successful_ShouldSetCoordinatesAndGameMap()
    {
        // Arrange
        var park = new Park();
        var map = new GameMap(10, 10);

        // Act
        bool result = park.TryPlace(2, 2, map);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(2, park.X);
        Assert.AreEqual(2, park.Y);
        Assert.AreEqual(map, park.GameMap);
    }

    [TestMethod]
    public void Park_TryPlace_OnOccupiedTile_ShouldReturnFalse()
    {
        // Arrange
        var park1 = new Park();
        var park2 = new Park();
        var map = new GameMap(10, 10);
        park1.TryPlace(2, 2, map); // Размещаем первый парк

        // Act & Assert - второй парк нельзя разместить на занятом месте
        Assert.IsFalse(park2.CanPlace(2, 2, map));
        Assert.IsFalse(park2.TryPlace(2, 2, map));
    }
}