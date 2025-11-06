using Core.Models.Buildings.SocialBuildings;
using Core.Models.Map;

[TestClass]
public class ParkIntegrationTests
{
    [TestMethod]
    public void Park_ShouldBeInstanceOfBuilding()
    {
        // Arrange & Act
        var park = new Park();

        // Assert - парк должен быть экземпляром Building
        Assert.IsInstanceOfType(park, typeof(Core.Models.Base.Building));
    }

    [TestMethod]
    public void Park_OnBuildingPlaced_ShouldNotThrowException()
    {
        // Arrange
        var park = new Park();
        var map = new GameMap(10, 10);

        // Act & Assert - метод не должен бросать исключений
        try
        {
            park.TryPlace(2, 2, map);
            park.OnBuildingPlaced(); // Должен выполниться без ошибок
        }
        catch
        {
            Assert.Fail("OnBuildingPlaced should not throw exceptions");
        }
    }
}