using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core.Services;
using Core.Enums;
using Core.Models.Buildings;
using Core.Models.Base;

namespace UnitTests
{
    [TestClass]
    public class UtilityServiceTests
    {
        [TestMethod]
        public void ConnectBuilding_ShouldSetElectricityFlag()
        {
            // Arrange
            var utilityService = new UtilityService { ServiceType = UtilityType.Electricity };
            var building = new TestBuilding();

            // Act
            utilityService.ConnectBuilding(building);

            // Assert
            Assert.IsTrue(building.HasElectricity);
        }

        [TestMethod]
        public void ConnectBuilding_ShouldSetWaterFlag()
        {
            // Arrange
            var utilityService = new UtilityService { ServiceType = UtilityType.Water };
            var building = new TestBuilding();

            // Act
            utilityService.ConnectBuilding(building);

            // Assert
            Assert.IsTrue(building.HasWater);
        }

        [TestMethod]
        public void ConnectBuilding_ShouldSetGasFlag()
        {
            // Arrange
            var utilityService = new UtilityService { ServiceType = UtilityType.Gas };
            var building = new TestBuilding();

            // Act
            utilityService.ConnectBuilding(building);

            // Assert
            Assert.IsTrue(building.HasGas);
        }

        [TestMethod]
        public void ConnectBuilding_ShouldSetSewageFlag()
        {
            // Arrange
            var utilityService = new UtilityService { ServiceType = UtilityType.Sewage };
            var building = new TestBuilding();

            // Act
            utilityService.ConnectBuilding(building);

            // Assert
            Assert.IsTrue(building.HasSewage);
        }

        [TestMethod]
        public void DisconnectBuilding_ShouldClearElectricityFlag()
        {
            // Arrange
            var utilityService = new UtilityService { ServiceType = UtilityType.Electricity };
            var building = new TestBuilding { HasElectricity = true };
            utilityService.ConnectBuilding(building);

            // Act
            utilityService.DisconnectBuilding(building);

            // Assert
            Assert.IsFalse(building.HasElectricity);
        }

        [TestMethod]
        public void DisconnectBuilding_ShouldClearWaterFlag()
        {
            // Arrange
            var utilityService = new UtilityService { ServiceType = UtilityType.Water };
            var building = new TestBuilding { HasWater = true };
            utilityService.ConnectBuilding(building);

            // Act
            utilityService.DisconnectBuilding(building);

            // Assert
            Assert.IsFalse(building.HasWater);
        }

        [TestMethod]
        public void SetNetworkState_False_ShouldDisableAllConnectedBuildings()
        {
            // Arrange
            var utilityService = new UtilityService { ServiceType = UtilityType.Electricity };
            var building1 = new TestBuilding();
            var building2 = new TestBuilding();

            utilityService.ConnectBuilding(building1);
            utilityService.ConnectBuilding(building2);

            // Act
            utilityService.SetNetworkState(false);

            // Assert
            Assert.IsFalse(building1.HasElectricity);
            Assert.IsFalse(building2.HasElectricity);
            Assert.IsFalse(utilityService.IsNetworkOperational);
        }

        [TestMethod]
        public void SetNetworkState_True_ShouldEnableAllConnectedBuildings()
        {
            // Arrange
            var utilityService = new UtilityService { ServiceType = UtilityType.Electricity };
            var building1 = new TestBuilding();
            var building2 = new TestBuilding();

            utilityService.ConnectBuilding(building1);
            utilityService.ConnectBuilding(building2);
            utilityService.SetNetworkState(false); // Сначала отключаем

            // Act
            utilityService.SetNetworkState(true);

            // Assert
            Assert.IsTrue(building1.HasElectricity);
            Assert.IsTrue(building2.HasElectricity);
            Assert.IsTrue(utilityService.IsNetworkOperational);
        }

        [TestMethod]
        public void GetConnectedBuildingsCount_ShouldReturnCorrectCount()
        {
            // Arrange
            var utilityService = new UtilityService { ServiceType = UtilityType.Electricity };
            var building1 = new TestBuilding();
            var building2 = new TestBuilding();
            var building3 = new TestBuilding();

            utilityService.ConnectBuilding(building1);
            utilityService.ConnectBuilding(building2);

            // Act
            int count = utilityService.GetConnectedBuildingsCount();

            // Assert
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public void ConnectBuilding_AlreadyConnected_ShouldNotDuplicate()
        {
            // Arrange
            var utilityService = new UtilityService { ServiceType = UtilityType.Electricity };
            var building = new TestBuilding();

            // Act
            utilityService.ConnectBuilding(building);
            utilityService.ConnectBuilding(building); // Двойное подключение

            // Assert
            Assert.AreEqual(1, utilityService.GetConnectedBuildingsCount());
        }

        // Вспомогательный класс для тестирования
        private class TestBuilding : Building
        {
            public override void OnBuildingPlaced()
            {

            }
        }
    }
}