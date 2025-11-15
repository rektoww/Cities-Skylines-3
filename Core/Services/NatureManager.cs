using Core.Enums;
using Core.Interfaces;
using Core.Models.Map;

namespace Core.Services
{
    /// <summary>
    /// Менеджер для управления природными объектами (деревьями)
    /// </summary>
    public class NatureManager : IGameService
    {
        public void Initialize()
        {
        }

        public void Update()
        {
        }

        public void PlantTrees(GameMap map, int x, int y, TreeType treeType, int count = 1)
        {
            if (x >= 0 && x < map.Width && y >= 0 && y < map.Height)
            {
                map.Tiles[x, y].TreeType = treeType;
                map.Tiles[x, y].TreeCount = Math.Clamp(count, 1, 10);
            }
        }

        public void ClearTrees(GameMap map, int x, int y)
        {
            if (x >= 0 && x < map.Width && y >= 0 && y < map.Height)
            {
                map.Tiles[x, y].TreeType = null;
                map.Tiles[x, y].TreeCount = 0;
            }
        }

        public int GetTotalTreeCount(GameMap map)
        {
            int count = 0;
            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    count += map.Tiles[x, y].TreeCount;
                }
            }
            return count;
        }

        public int GetTilesWithTreesCount(GameMap map)
        {
            int count = 0;
            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    if (map.Tiles[x, y].TreeCount > 0)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        public Dictionary<TreeType, int> GetTreeTypeStatistics(GameMap map)
        {
            var statistics = new Dictionary<TreeType, int>();

            foreach (TreeType treeType in Enum.GetValues(typeof(TreeType)))
            {
                statistics[treeType] = 0;
            }

            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    var tile = map.Tiles[x, y];
                    if (tile.TreeType.HasValue && tile.TreeCount > 0)
                    {
                        statistics[tile.TreeType.Value] += tile.TreeCount;
                    }
                }
            }

            return statistics;
        }
    }
}