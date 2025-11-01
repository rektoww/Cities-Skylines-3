using Core.Enums;
using Core.Models.Base;
using Core.Models.Buildings;
using Core.Models.Buildings.CommertialBuildings;
using Core.Models.Map;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

        // Тестирование коммерческих зданий
        [TestMethod]
        public void CommercialBuilding_ShopCreation_ShouldSetDefaultValues()
        {
            // Arrange & Act
            var shop = new Shop();

            // Assert
            Assert.AreEqual(CommercialBuildingType.Shop, shop.Type);
            Assert.AreEqual(10, shop.Capacity);
            Assert.AreEqual(3, shop.EmployeeCount);
            Assert.AreEqual(50000m, shop.BuildCost);
            Assert.AreEqual(2, shop.Width);
            Assert.AreEqual(1, shop.Height);
            Assert.AreEqual(1, shop.Floors);
            Assert.IsTrue(shop.ProductCategories.Contains("Продовольствие"));
            Assert.IsTrue(shop.ProductCategories.Contains("Напитки"));
        }

        [TestMethod]
        public void CommercialBuilding_SupermarketCreation_ShouldSetDefaultValues()
        {
            // Arrange & Act
            var supermarket = new Supermarket();

            // Assert
            Assert.AreEqual(CommercialBuildingType.Supermarket, supermarket.Type);
            Assert.AreEqual(50, supermarket.Capacity);
            Assert.AreEqual(15, supermarket.EmployeeCount);
            Assert.AreEqual(200000m, supermarket.BuildCost);
            Assert.AreEqual(3, supermarket.Width);
            Assert.AreEqual(2, supermarket.Height);
            Assert.IsTrue(supermarket.ProductCategories.Contains("Продовольствие"));
            Assert.IsTrue(supermarket.ProductCategories.Contains("Напитки"));
            Assert.IsTrue(supermarket.ProductCategories.Contains("Хозтовары"));
        }

        [TestMethod]
        public void CommercialBuilding_PharmacyCreation_ShouldSetMedicalProducts()
        {
            // Arrange & Act
            var pharmacy = new Pharmacy();

            // Assert
            Assert.AreEqual(CommercialBuildingType.Pharmacy, pharmacy.Type);
            Assert.AreEqual(12, pharmacy.Capacity);
            Assert.AreEqual(4, pharmacy.EmployeeCount);
            Assert.IsTrue(pharmacy.ProductCategories.Contains("Лекарства"));
            Assert.IsTrue(pharmacy.ProductCategories.Contains("Медицинские товары"));
            Assert.IsTrue(pharmacy.ProductCategories.Contains("Витамины"));
        }

        [TestMethod]
        public void CommercialBuilding_GasStationCreation_ShouldSetFuelProducts()
        {
            // Arrange & Act
            var gasStation = new GasStation();

            // Assert
            Assert.AreEqual(CommercialBuildingType.GasStation, gasStation.Type);
            Assert.AreEqual(15, gasStation.Capacity);
            Assert.AreEqual(4, gasStation.EmployeeCount);
            Assert.IsTrue(gasStation.ProductCategories.Contains("Бензин"));
            Assert.IsTrue(gasStation.ProductCategories.Contains("Дизель"));
            Assert.IsTrue(gasStation.ProductCategories.Contains("Сопутствующие товары"));
        }

        [TestMethod]
        public void CommercialBuilding_WithAllUtilities_ShouldBeOperational()
        {
            // Arrange
            var shop = new Shop
            {
                HasElectricity = true,
                HasWater = true,
                HasGas = true,
                HasSewage = true
            };

            // Act
            bool isOperational = shop.IsOperational;

            // Assert
            Assert.IsTrue(isOperational);
        }

        [TestMethod]
        public void CommercialBuilding_WithoutElectricity_ShouldNotBeOperational()
        {
            // Arrange
            var cafe = new Cafe
            {
                HasElectricity = false,
                HasWater = true,
                HasGas = true,
                HasSewage = true
            };

            // Act
            bool isOperational = cafe.IsOperational;

            // Assert
            Assert.IsFalse(isOperational);
        }

        [TestMethod]
        public void CommercialBuilding_PlaceOnMap_ShouldSucceed()
        {
            // Arrange
            var shop = new Shop();
            var gameMap = new GameMap(10, 10);

            // Act
            bool placed = shop.TryPlace(2, 2, gameMap);

            // Assert
            Assert.IsTrue(placed);
            Assert.AreEqual(2, shop.X);
            Assert.AreEqual(2, shop.Y);
            Assert.AreEqual(gameMap, shop.GameMap);
        }

        [TestMethod]
        public void CommercialBuilding_PlaceOnWater_ShouldFail()
        {
            // Arrange
            var restaurant = new Restaurant();
            var gameMap = new GameMap(10, 10);

            // Устанавливаем воду на тайлы
            for (int x = 3; x < 6; x++)
            {
                for (int y = 3; y < 6; y++)
                {
                    gameMap.Tiles[x, y].Terrain = TerrainType.Water;
                }
            }

            // Act
            bool placed = restaurant.TryPlace(3, 3, gameMap);

            // Assert
            Assert.IsFalse(placed);
        }

        [TestMethod]
        public void CommercialBuilding_DifferentTypes_ShouldHaveCorrectProductCategories()
        {
            // Arrange & Act
            var shop = new Shop();
            var cafe = new Cafe();
            var restaurant = new Restaurant();
            var pharmacy = new Pharmacy();
            var gasStation = new GasStation();

            // Assert
            Assert.AreEqual(2, shop.ProductCategories.Count);
            Assert.AreEqual(3, cafe.ProductCategories.Count);
            Assert.AreEqual(4, restaurant.ProductCategories.Count);
            Assert.AreEqual(3, pharmacy.ProductCategories.Count);
            Assert.AreEqual(3, gasStation.ProductCategories.Count);
        }

        [TestMethod]
        public void CommercialBuilding_AllTypes_InheritFromCommercialBuilding()
        {
            // Arrange & Act
            var shop = new Shop();
            var cafe = new Cafe();
            var restaurant = new Restaurant();
            var pharmacy = new Pharmacy();
            var gasStation = new GasStation();

            // Assert
            Assert.IsInstanceOfType(shop, typeof(CommercialBuilding));
            Assert.IsInstanceOfType(cafe, typeof(CommercialBuilding));
            Assert.IsInstanceOfType(restaurant, typeof(CommercialBuilding));
            Assert.IsInstanceOfType(pharmacy, typeof(CommercialBuilding));
            Assert.IsInstanceOfType(gasStation, typeof(CommercialBuilding));
        }

        [TestMethod]
        public void CommercialBuilding_AllTypes_InheritFromBuilding()
        {
            // Arrange & Act
            var shop = new Shop();
            var cafe = new Cafe();
            var restaurant = new Restaurant();

            // Assert
            Assert.IsInstanceOfType(shop, typeof(Building));
            Assert.IsInstanceOfType(cafe, typeof(Building));
            Assert.IsInstanceOfType(restaurant, typeof(Building));
        }

    }
}