using Core.Enums;
using Core.Models.Base;
using Core.Enums;
using System.Collections.Generic;

namespace Core.Models.Map
{
    public class Tile
    {
        public int X { get; set; }
        public int Y { get; set; }
        public TerrainType Terrain { get; set; }
        public float Height { get; set; }
        public List<NaturalResource> Resources { get; set; }

        public Building Building { get; set; }
        public bool HasRoad { get; set; }

        /// <summary>
        /// SmirnovMA - ПЕШЕХОДНАЯ ИНФРАСТРУКТУРА
        /// </summary>

        /// <summary>
        /// Есть ли велодорожка на этом тайле
        /// </summary>
        public bool HasBikeLane { get; set; }

        /// <summary>
        /// Есть ли парк/зеленая зона на этом тайле
        /// </summary>
        public bool HasPark { get; set; }

        /// <summary>
        /// Есть ли пешеходная дорожка на этом тайле
        /// </summary>
        public bool HasPedestrianPath { get; set; }

        // НОВЫЕ СВОЙСТВА ДЛЯ ПРИРОДЫ
        /// <summary>
        /// Тип дерева на тайле (null если деревьев нет)
        /// </summary>
        public TreeType? TreeType { get; set; }

        /// <summary>
        /// Количество деревьев на тайле (от 1 до 10)
        /// </summary>
        public int TreeCount { get; set; }

        /// <summary>
        /// Есть ли лес на этом тайле
        /// </summary>
        public bool HasForest => Terrain == TerrainType.Forest;

        public Tile()
        {
            Resources = new List<NaturalResource>();
            TreeCount = 0; // По умолчанию нет деревьев
        }
    }
}