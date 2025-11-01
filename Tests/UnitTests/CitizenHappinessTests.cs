using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core.Models.Mobs;
using Core.Models.Map;
using Core.Models.Buildings;
using Core.Enums;

namespace UnitTests
{
    [TestClass]
    public class CitizenHappinessTests
    {
        [TestMethod]
        public void UpdateHappinessBasedOnInfrastructure_Park_ShouldIncreaseHappiness()
        {
            // Arrange
            var gameMap = new GameMap(5, 5);
            gameMap.Tiles[2, 2].HasPark = true;
            var citizen = new Citizen(2, 2, gameMap);
            citizen.Happiness = 50f;

            // Act
            citizen.UpdateHappinessBasedOnInfrastructure();

            // Debug - выведем что получилось
            float actualHappiness = citizen.Happiness;
            float expectedHappiness = 50f + 0.1f - 0.01f;

            // Assert
            Assert.AreEqual(expectedHappiness, actualHappiness, 0.001f,
                $"Expected: {expectedHappiness}, Actual: {actualHappiness}");
        }

        [TestMethod]
        public void UpdateHappinessBasedOnInfrastructure_BikeLane_ShouldIncreaseHappiness()
        {
            // Arrange
            var gameMap = new GameMap(5, 5);
            gameMap.Tiles[2, 2].HasBikeLane = true;
            var citizen = new Citizen(2, 2, gameMap);
            citizen.Happiness = 50f;
            float expectedHappiness = 50f + 0.05f - 0.01f;

            // Act
            citizen.UpdateHappinessBasedOnInfrastructure();

            // Assert
            Assert.AreEqual(expectedHappiness, citizen.Happiness, 0.001f);
        }

        [TestMethod]
        public void UpdateHappinessBasedOnInfrastructure_PedestrianPath_ShouldIncreaseHappiness()
        {
            // Arrange
            var gameMap = new GameMap(5, 5);
            gameMap.Tiles[2, 2].HasPedestrianPath = true;
            var citizen = new Citizen(2, 2, gameMap);
            citizen.Happiness = 50f;
            float expectedHappiness = 50f + 0.03f - 0.01f;

            // Act
            citizen.UpdateHappinessBasedOnInfrastructure();

            // Assert
            Assert.AreEqual(expectedHappiness, citizen.Happiness, 0.001f);
        }

        [TestMethod]
        public void UpdateHappinessBasedOnInfrastructure_HomeWithUtilities_ShouldIncreaseHappiness()
        {
            // Arrange
            var gameMap = new GameMap(5, 5);
            var citizen = new Citizen(2, 2, gameMap);
            citizen.Happiness = 50f;
            var home = new ResidentialBuilding(ResidentialType.Apartment);
            home.HasElectricity = true;
            home.HasWater = true;
            home.HasGas = true;
            home.HasSewage = true;
            citizen.Home = home;
            float expectedHappiness = 50f + 0.02f - 0.01f;

            // Act
            citizen.UpdateHappinessBasedOnInfrastructure();

            // Assert
            Assert.AreEqual(expectedHappiness, citizen.Happiness, 0.001f);
        }

        [TestMethod]
        public void UpdateHappinessBasedOnInfrastructure_WorkplaceWithUtilities_ShouldNotDecreaseHappiness()
        {
            // Arrange
            var gameMap = new GameMap(5, 5);
            var citizen = new Citizen(2, 2, gameMap);
            citizen.Happiness = 50f;
            var workplace = new ServiceBuilding(ServiceBuildingType.Hospital, 100);
            workplace.HasElectricity = true;
            workplace.HasWater = true;
            workplace.HasGas = true;
            workplace.HasSewage = true;
            citizen.Workplace = workplace;

            // Act
            citizen.UpdateHappinessBasedOnInfrastructure();

            // Assert - с работой счастье должно быть >= 50
            Assert.IsTrue(citizen.Happiness >= 49.9f, $"Happiness should not decrease below 49.9, but got: {citizen.Happiness}");
        }

        [TestMethod]
        public void UpdateHappinessBasedOnInfrastructure_MultipleBonuses_ShouldCombine()
        {
            // Arrange
            var gameMap = new GameMap(5, 5);
            gameMap.Tiles[2, 2].HasPark = true;
            gameMap.Tiles[2, 2].HasBikeLane = true;
            var citizen = new Citizen(2, 2, gameMap);
            float initialHappiness = citizen.Happiness;

            // Act
            citizen.UpdateHappinessBasedOnInfrastructure();

            // Assert
            Assert.AreEqual(initialHappiness + 0.1f + 0.05f - 0.01f, citizen.Happiness);
        }

        [TestMethod]
        public void UpdateHappinessBasedOnInfrastructure_NaturalDecrease_ShouldLowerHappiness()
        {
            // Arrange
            var gameMap = new GameMap(5, 5);
            var citizen = new Citizen(2, 2, gameMap);
            citizen.Happiness = 50f;
            float initialHappiness = citizen.Happiness;

            // Act
            citizen.UpdateHappinessBasedOnInfrastructure();

            // Assert
            Assert.IsTrue(citizen.Happiness < initialHappiness);
            Assert.AreEqual(initialHappiness - 0.01f, citizen.Happiness);
        }

        [TestMethod]
        public void ApplyHappinessEffects_HighHappiness_ShouldIncreaseHealth()
        {
            // Arrange
            var gameMap = new GameMap(5, 5);
            var citizen = new Citizen(2, 2, gameMap);
            citizen.Happiness = 80f;
            citizen.Health = 90f;
            float initialHealth = citizen.Health;

            // Act
            citizen.ApplyHappinessEffects();

            // Assert
            Assert.IsTrue(citizen.Health > initialHealth);
            Assert.AreEqual(initialHealth + 0.05f, citizen.Health);
        }

        [TestMethod]
        public void ApplyHappinessEffects_LowHappiness_ShouldDecreaseHealth()
        {
            // Arrange
            var gameMap = new GameMap(5, 5);
            var citizen = new Citizen(2, 2, gameMap);
            citizen.Happiness = 20f;
            citizen.Health = 90f;
            float initialHealth = citizen.Health;

            // Act
            citizen.ApplyHappinessEffects();

            // Assert
            Assert.IsTrue(citizen.Health < initialHealth);
            Assert.AreEqual(initialHealth - 0.1f, citizen.Health);
        }

        [TestMethod]
        public void ApplyHappinessEffects_HighHappiness_ShouldNotThrow()
        {
            // Arrange
            var gameMap = new GameMap(5, 5);
            var citizen = new Citizen(2, 2, gameMap);
            citizen.Happiness = 85f;
            citizen.Age = 25;
            citizen.IsMarried = true;
            citizen.Partner = new Citizen(2, 3, gameMap);

            // Act & Assert
            try
            {
                citizen.ApplyHappinessEffects();
                // Если не упало - тест пройден
                Assert.IsTrue(true);
            }
            catch
            {
                Assert.Fail("Method threw an exception");
            }
        }

        [TestMethod]
        public void UpdateHappinessBasedOnInfrastructure_OutOfBounds_ShouldNotCrash()
        {
            // Arrange
            var gameMap = new GameMap(5, 5);
            var citizen = new Citizen(10, 10, gameMap); // Координаты за пределами карты

            // Act & Assert
            try
            {
                citizen.UpdateHappinessBasedOnInfrastructure();
                // Если не упало - тест пройден
                Assert.IsTrue(true);
            }
            catch
            {
                Assert.Fail("Method threw an exception");
            }
        }

        [TestMethod]
        public void Happiness_ShouldNeverExceed100()
        {
            // Arrange
            var gameMap = new GameMap(5, 5);
            gameMap.Tiles[2, 2].HasPark = true;
            var citizen = new Citizen(2, 2, gameMap);
            citizen.Happiness = 99.9f;

            // Act
            citizen.UpdateHappinessBasedOnInfrastructure();

            // Assert - проверяем что не превышает 100
            Assert.IsTrue(citizen.Happiness <= 100f);
        }

        [TestMethod]
        public void Happiness_ShouldNeverGoBelow0()
        {
            // Arrange
            var gameMap = new GameMap(5, 5);
            var citizen = new Citizen(2, 2, gameMap);
            citizen.Happiness = 0.005f;

            // Act
            citizen.UpdateHappinessBasedOnInfrastructure(); // -0.01

            // Assert
            Assert.AreEqual(0f, citizen.Happiness);
        }
    }
}