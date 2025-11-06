using Core.Models.Buildings.SocialBuildings;
using Core.Models.Map;

[TestClass]
public class ParkTileOccupationTests
{
    [TestMethod]
    public void Park_TryPlace_ShouldOccupyCorrectTiles()
    {
        // Arrange
        var park = new Park();
        var map = new GameMap(10, 10);

        // Act
        park.TryPlace(3, 3, map);

        // Assert - парк должен занять 2x2 тайла
        Assert.AreEqual(park, map.GetBuildingAt(3, 3));
        Assert.AreEqual(park, map.GetBuildingAt(3, 4));
        Assert.AreEqual(park, map.GetBuildingAt(4, 3));
        Assert.AreEqual(park, map.GetBuildingAt(4, 4));
    }

    [TestMethod]
    public void Park_TryPlace_ShouldNotOccupyOtherTiles()
    {
        // Arrange
        var park = new Park();
        var map = new GameMap(10, 10);

        // Act
        park.TryPlace(5, 5, map);

        // Assert - соседние тайлы должны остаться пустыми
        Assert.IsNull(map.GetBuildingAt(5, 4)); // Сверху
        Assert.IsNull(map.GetBuildingAt(4, 5)); // Слева
        Assert.IsNull(map.GetBuildingAt(7, 5)); // Справа (за пределами парка)
        Assert.IsNull(map.GetBuildingAt(5, 7)); // Снизу (за пределами парка)
    }
}