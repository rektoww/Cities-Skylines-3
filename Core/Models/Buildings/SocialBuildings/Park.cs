using System.Xml.Linq;
using System.Collections.Generic;
using Core.Enums;
using Core.Interfaces;
using Core.Models.Base;
using Core.Models.Map;

namespace Core.Models.Buildings.SocialBuildings
{
    /// <summary>
    /// SmirnovMA
    /// Класс парка - зеленая зона для отдыха
    /// </summary>
    public class Park : Building, IConstructable<Park>
    {
        // Стоимость и требуемые материалы для строительства парка
        public static decimal BuildCost { get; protected set; } = 1000m;
        public static Dictionary<ConstructionMaterial, int> RequiredMaterials { get; protected set; } = new();

        public int TreeCount { get; set; }
        public int BenchCount { get; set; }

        public Park() : base(
            HasWater: false,     // Без коммуникаций
            HasGas: false,
            HasSewage: false,
            HasElectricity: false,
            Floors: 0,
            _BuildCost: 1000m,   // Стоимость
            Width: 2,            // Размер 2x2
            Height: 2
        )
        {
            Name = "Парк";
            TreeCount = 8;
            BenchCount = 4;
            MaxOccupancy = 30;
        }
        public override void OnBuildingPlaced()
        {
            // При размещении парка создаем дорожки вокруг
            CreatePathsAroundPark();
        }

        /// <summary>
        /// Создает дорожки вокруг парка в радиусе 1 тайл
        /// </summary>
        private void CreatePathsAroundPark()
        {
            if (GameMap == null) return;

            // Создаем дорожки по периметру парка (радиус 1)
            for (int x = X - 1; x <= X + Width; x++)
            {
                for (int y = Y - 1; y <= Y + Height; y++)
                {
                    // Пропускаем сам парк
                    if (x >= X && x < X + Width && y >= Y && y < Y + Height)
                        continue;

                    if (x >= 0 && x < GameMap.Width && y >= 0 && y < GameMap.Height)
                    {
                        var tile = GameMap.Tiles[x, y];

                        // Создаем велодорожки по краям парка
                        if (x == X - 1 || x == X + Width || y == Y - 1 || y == Y + Height)
                        {
                            tile.HasBikeLane = true;
                        }

                        // Все дорожки вокруг парка - пешеходные
                        tile.HasPedestrianPath = true;
                    }
                }
            }
        }             

        /// <summary>
        /// Парк можно размещать на равнинах, лугах и в лесах
        /// </summary>
        protected override bool IsTileSuitableForBuilding(Tile tile)
        {
            return tile.Terrain == TerrainType.Plain ||
                   tile.Terrain == TerrainType.Meadow ||
                   tile.Terrain == TerrainType.Forest;
        }
    }
}
