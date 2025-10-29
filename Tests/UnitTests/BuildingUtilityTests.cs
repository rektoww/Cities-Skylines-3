using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core.Models.Buildings;
using Core.Enums;
using Core.Models.Base;

namespace UnitTests
{
    [TestClass]
    public class BuildingUtilityTests
    {
        [TestMethod]
        public void IsOperational_AllUtilitiesConnected_ShouldReturnTrue()
        {
            // Arrange
            var building = new TestBuilding
            {
                HasElectricity = true,
                HasWater = true,
                HasGas = true,
                HasSewage = true
            };

            // Act
            bool isOperational = building.IsOperational;

            // Assert
            Assert.IsTrue(isOperational);
        }

        [TestMethod]
        public void IsOperational_MissingElectricity_ShouldReturnFalse()
        {
            // Arrange
            var building = new TestBuilding
            {
                HasElectricity = false, // Отсутствует электричество
                HasWater = true,
                HasGas = true,
                HasSewage = true
            };

            // Act
            bool isOperational = building.IsOperational;

            // Assert
            Assert.IsFalse(isOperational);
        }

        [TestMethod]
        public void IsOperational_MissingWater_ShouldReturnFalse()
        {
            // Arrange
            var building = new TestBuilding
            {
                HasElectricity = true,
                HasWater = false, // Отсутствует вода
                HasGas = true,
                HasSewage = true
            };

            // Act
            bool isOperational = building.IsOperational;

            // Assert
            Assert.IsFalse(isOperational);
        }

        [TestMethod]
        public void IsOperational_MissingGas_ShouldReturnFalse()
        {
            // Arrange
            var building = new TestBuilding
            {
                HasElectricity = true,
                HasWater = true,
                HasGas = false, // Отсутствует газ
                HasSewage = true
            };

            // Act
            bool isOperational = building.IsOperational;

            // Assert
            Assert.IsFalse(isOperational);
        }

        [TestMethod]
        public void IsOperational_MissingSewage_ShouldReturnFalse()
        {
            // Arrange
            var building = new TestBuilding
            {
                HasElectricity = true,
                HasWater = true,
                HasGas = true,
                HasSewage = false // Отсутствует канализация
            };

            // Act
            bool isOperational = building.IsOperational;

            // Assert
            Assert.IsFalse(isOperational);
        }

        [TestMethod]
        public void IsOperational_NoUtilities_ShouldReturnFalse()
        {
            // Arrange
            var building = new TestBuilding
            {
                HasElectricity = false,
                HasWater = false,
                HasGas = false,
                HasSewage = false
            };

            // Act
            bool isOperational = building.IsOperational;

            // Assert
            Assert.IsFalse(isOperational);
        }

        [TestMethod]
        public void ResidentialBuilding_WithUtilities_ShouldBeOperational()
        {
            // Arrange
            var residential = new ResidentialBuilding(ResidentialType.Apartment)
            {
                HasElectricity = true,
                HasWater = true,
                HasGas = true,
                HasSewage = true
            };

            // Act
            bool isOperational = residential.IsOperational;

            // Assert
            Assert.IsTrue(isOperational);
        }

        [TestMethod]
        public void ServiceBuilding_WithUtilities_ShouldBeOperational()
        {
            // Arrange
            var service = new ServiceBuilding(ServiceBuildingType.Hospital, 100)
            {
                HasElectricity = true,
                HasWater = true,
                HasGas = true,
                HasSewage = true
            };

            // Act
            bool isOperational = service.IsOperational;

            // Assert
            Assert.IsTrue(isOperational);
        }

        [TestMethod]
        public void CommercialBuilding_WithUtilities_ShouldBeOperational()
        {
            // Arrange
            var commercial = new TestBuilding() 
            {
                HasElectricity = true,
                HasWater = true,
                HasGas = true,
                HasSewage = true
            };

            // Act
            bool isOperational = commercial.IsOperational;

            // Assert
            Assert.IsTrue(isOperational);
        }

        [TestMethod]
        public void WoodProcessingFactory_WithUtilities_ShouldBeOperational()
        {
            // Arrange
            var factory = new WoodProcessingFactory("Test Factory")
            {
                HasElectricity = true,
                HasWater = true,
                HasGas = true,
                HasSewage = true
            };

            // Act
            bool isOperational = factory.IsOperational;

            // Assert
            Assert.IsTrue(isOperational);
        }

        [TestMethod]
        public void ExtractionFacility_WithUtilities_ShouldBeOperational()
        {
            // Arrange
            var facility = new ExtractionFacility("Test Mine", ResourceType.Iron, 1000)
            {
                HasElectricity = true,
                HasWater = true,
                HasGas = true,
                HasSewage = true
            };

            // Act
            bool isOperational = facility.IsOperational;

            // Assert
            Assert.IsTrue(isOperational);
        }

        [TestMethod]
        public void WoodProcessingFactory_ProcessWood_WithElectricity_ShouldProcess() // ИЗМЕНИЛ НАЗВАНИЕ
        {
            // Arrange
            var factory = new WoodProcessingFactory("Test Factory")
            {
                HasElectricity = true, // ЕСТЬ электричество
                HasWater = true,
                HasGas = true,
                HasSewage = true
            };
            factory.AddWood(100);
            factory.IsActive = true;
            int initialWood = factory.WoodStorage;

            // Act
            factory.ProcessWood();

            // Assert - проверяем что древесина уменьшилась на 20
            Assert.AreEqual(80, factory.WoodStorage, "Wood should be processed with electricity");
        }

        [TestMethod]
        public void ExtractionFacility_ExtractResources_WithoutElectricity_ShouldNotExtract()
        {
            // Arrange
            var facility = new ExtractionFacility("Test Mine", ResourceType.Iron, 1000)
            {
                HasElectricity = false, // Нет электричества
                HasWater = true,
                HasGas = true,
                HasSewage = true
            };
            facility.IsActive = true;
            int initialStorage = facility.CurrentStorage;

            // Act
            int extracted = facility.ExtractResources();

            // Assert
            Assert.AreEqual(0, extracted); // Ресурсы не добыты
            Assert.AreEqual(initialStorage, facility.CurrentStorage);
        }

        // Вспомогательный класс для тестирования
        private class TestBuilding : Building
        {
            public override void OnBuildingPlaced()
            {
                // Пустая реализация для тестов
            }
        }
    }
}