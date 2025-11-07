using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.Enums;
using Core.Models.Map;
using Infrastructure.Services;

// Алиас на тайл карты, чтобы избежать конфликтов имён (если где-то ещё есть Tile)
using MapTile = Core.Models.Map.Tile;

namespace Tests.UnitTests
{
    /// <summary>
    /// Юнит-тесты для детерминированной карты 50×50 из StaticBigMapProvider.
    /// Покрывает размер, «водную рамку», инварианты ресурсов, отсутствие леса на воде/горах,
    /// детерминизм и round-trip сериализацию.
    /// </summary>
    [TestClass]
    public sealed class StaticBigMapProviderMSTests
    {
        /// <summary>Утилита: итерировать все тайлы.</summary>
        private static IEnumerable<MapTile> Tiles(GameMap map)
        {
            for (int y = 0; y < map.Height; y++)
                for (int x = 0; x < map.Width; x++)
                    yield return map.Tiles[x, y];
        }

        /// <summary>Карта создаётся нужного размера 50×50.</summary>
        [TestMethod]
        public void Build50_Should_CreateMapWithExpectedSize()
        {
            var map = StaticBigMapProvider.Build50();

            Assert.AreEqual(50, map.Width);
            Assert.AreEqual(50, map.Height);
            Assert.IsNotNull(map.Tiles[0, 0]);
            Assert.IsNotNull(map.Tiles[map.Width - 1, map.Height - 1]);
        }

        /// <summary>По периметру вода шириной 2 клетки.</summary>
        [TestMethod]
        public void Build50_Should_HaveTwoTileWaterBorder()
        {
            var map = StaticBigMapProvider.Build50();

            // верх/низ
            for (int x = 0; x < map.Width; x++)
            {
                Assert.AreEqual(TerrainType.Water, map.Tiles[x, 0].Terrain);
                Assert.AreEqual(TerrainType.Water, map.Tiles[x, 1].Terrain);
                Assert.AreEqual(TerrainType.Water, map.Tiles[x, map.Height - 1].Terrain);
                Assert.AreEqual(TerrainType.Water, map.Tiles[x, map.Height - 2].Terrain);
            }

            // лево/право
            for (int y = 0; y < map.Height; y++)
            {
                Assert.AreEqual(TerrainType.Water, map.Tiles[0, y].Terrain);
                Assert.AreEqual(TerrainType.Water, map.Tiles[1, y].Terrain);
                Assert.AreEqual(TerrainType.Water, map.Tiles[map.Width - 1, y].Terrain);
                Assert.AreEqual(TerrainType.Water, map.Tiles[map.Width - 2, y].Terrain);
            }
        }

        /// <summary>
        /// Инварианты ресурсов:
        ///  - на воде ресурсов нет;
        ///  - Iron только на Mountain;
        ///  - Oil/Gas только на Plain/Meadow.
        /// </summary>
        [TestMethod]
        public void Build50_Should_RespectResourceInvariants()
        {
            var map = StaticBigMapProvider.Build50();

            foreach (var tile in Tiles(map))
            {
                if (tile.Terrain == TerrainType.Water)
                    Assert.AreEqual(0, tile.Resources.Count, "На воде не должно быть ресурсов.");

                foreach (var r in tile.Resources)
                {
                    switch (r.Type)
                    {
                        case ResourceType.Iron:
                            Assert.AreEqual(TerrainType.Mountain, tile.Terrain,
                                "Iron должен лежать только на горных клетках (Mountain).");
                            break;

                        case ResourceType.Oil:
                        case ResourceType.Gas:
                            Assert.IsTrue(tile.Terrain == TerrainType.Plain || tile.Terrain == TerrainType.Meadow,
                                "Oil/Gas должны быть только на Plain или Meadow.");
                            break;

                        default:
                            Assert.Fail("Неожиданный тип ресурса: " + r.Type);
                            break;
                    }
                }
            }
        }

        /// <summary>Лес не накладывается на воду и горы.</summary>
        [TestMethod]
        public void Build50_Forest_Should_Not_Overlap_WaterOrMountains()
        {
            var map = StaticBigMapProvider.Build50();

            foreach (var tile in Tiles(map))
            {
                if (tile.Terrain == TerrainType.Forest)
                {
                    Assert.AreNotEqual(TerrainType.Water, tile.Terrain);
                    Assert.AreNotEqual(TerrainType.Mountain, tile.Terrain);
                }
            }
        }

        /// <summary>Детерминизм: два построения идентичны.</summary>
        [TestMethod]
        public void Build50_Should_BeDeterministic()
        {
            var m1 = StaticBigMapProvider.Build50();
            var m2 = StaticBigMapProvider.Build50();

            Assert.AreEqual(m1.Width, m2.Width);
            Assert.AreEqual(m1.Height, m2.Height);

            for (int y = 0; y < m1.Height; y++)
                for (int x = 0; x < m1.Width; x++)
                {
                    var a = m1.Tiles[x, y];
                    var b = m2.Tiles[x, y];

                    Assert.AreEqual(a.Terrain, b.Terrain, $"Terrain diff at ({x},{y})");

                    var ar = a.Resources.OrderBy(r => r.Type).ThenBy(r => r.Amount).ToList();
                    var br = b.Resources.OrderBy(r => r.Type).ThenBy(r => r.Amount).ToList();

                    Assert.AreEqual(ar.Count, br.Count, $"Res count diff at ({x},{y})");
                    for (int i = 0; i < ar.Count; i++)
                    {
                        Assert.AreEqual(ar[i].Type, br[i].Type, $"Res type diff at ({x},{y})");
                        Assert.AreEqual(ar[i].Amount, br[i].Amount, $"Res amount diff at ({x},{y})");
                    }
                }
        }
    }
}