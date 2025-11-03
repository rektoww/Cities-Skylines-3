using Core.Enums;
using Core.Models.Buildings.CommertialBuildings;
using Core.Models.Map;
using Core.Resourses;
using Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Tests.UnitTests
{
    [TestClass]
    public sealed class ConstructionTests
    {
        private GameMap map;
        private Dictionary<ConstructionMaterial, int> storedMaterials;
        private decimal balance;
        private ConstructionCompany company;
        private PlayerResources resources;

        [TestInitialize]
        public void Setup()
        {
            // создаём карту
            map = new GameMap(10, 10);
            for (int x = 0; x < map.Width; x++)
                for (int y = 0; y < map.Height; y++)
                    map.Tiles[x, y].Terrain = TerrainType.Plain;

            // создаём материалы и баланс
            storedMaterials = new Dictionary<ConstructionMaterial, int>
            {
                { ConstructionMaterial.Steel, 10 },
                { ConstructionMaterial.Concrete, 10 }
            };
            balance = 100000m;

            resources = new PlayerResources(balance, storedMaterials);

            company = new ConstructionCompany(resources);
        }

        // Успешное строительство при достаточных ресурсах и свободном месте
        [TestMethod]
        public void TryBuild_ShouldSucceed_WhenEnoughResourcesAndSpace()
        {
            bool result = company.TryBuild<Shop>(2, 2, map, new object[] { }, out var building);

            Assert.IsTrue(result, "Здание должно успешно построиться.");
            Assert.IsNotNull(building, "Объект здания не должен быть null.");
            Assert.AreEqual(resources.Balance, 100000m - Shop.BuildCost, "Баланс должен уменьшиться на BuildCost.");
        }

        // Проверка списания материалов
        [TestMethod]
        public void TryBuild_ShouldConsumeMaterials_WhenSuccessful()
        {
            company.TryBuild<Shop>(2, 2, map, new object[] { }, out var building);

            foreach (var material in Shop.RequiredMaterials)
            {
                Assert.AreEqual(storedMaterials[material.Key], 10 - material.Value, $"Материал {material.Key} должен быть списан.");
            }
        }

        // Недостаточно ресурсов
        [TestMethod]
        public void TryBuild_ShouldFail_WhenNotEnoughResources()
        {
            storedMaterials[ConstructionMaterial.Steel] = 1; // недостаточно стали

            bool result = company.TryBuild<Shop>(2, 2, map, new object[] { }, out var building);

            Assert.IsFalse(result, "Строительство должно провалиться из-за нехватки материалов.");
            Assert.IsNull(building, "Объект здания должен быть null.");
            Assert.AreEqual(1, storedMaterials[ConstructionMaterial.Steel], "Материалы не должны списываться.");
            Assert.AreEqual(balance, 100000m, "Баланс не должен измениться.");
        }

        // Попытка построить на занятой клетке
        [TestMethod]
        public void TryBuild_ShouldFail_WhenTileOccupied()
        {
            company.TryBuild<Shop>(2, 2, map, new object[] { }, out var firstBuilding);

            bool result = company.TryBuild<Shop>(2, 2, map, new object[] { }, out var secondBuilding);

            Assert.IsFalse(result, "Строительство должно провалиться, так как тайл занят.");
            Assert.IsNull(secondBuilding, "Второе здание должно быть null.");
        }

        // Попытка построить за пределами карты
        [TestMethod]
        public void TryBuild_ShouldFail_WhenOutOfBounds()
        {
            bool result = company.TryBuild<Shop>(9, 9, map, new object[] { }, out var building);

            Assert.IsFalse(result, "Здание не должно размещаться за границами карты.");
            Assert.IsNull(building, "Объект здания должен быть null.");
        }
    }
}
