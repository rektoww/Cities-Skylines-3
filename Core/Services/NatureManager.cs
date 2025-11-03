using Core.Enums;
using Core.Models.Map;

namespace Core.Services
{
    /// <summary>
    /// Менеджер для управления природными объектами (деревьями)
    /// </summary>
    public class NatureManager
    {
        /// <summary>
        /// Посадить деревья на указанном тайле
        /// </summary>
        /// <param name="map">Игровая карта</param>
        /// <param name="x">Координата X</param>
        /// <param name="y">Координата Y</param>
        /// <param name="treeType">Тип дерева</param>
        /// <param name="count">Количество деревьев (от 1 до 10)</param>
        public void PlantTrees(GameMap map, int x, int y, TreeType treeType, int count = 1)
        {
            if (x >= 0 && x < map.Width && y >= 0 && y < map.Height)
            {
                map.Tiles[x, y].TreeType = treeType;
                map.Tiles[x, y].TreeCount = Math.Clamp(count, 1, 10);
            }
        }

        /// <summary>
        /// Очистить деревья с указанного тайла
        /// </summary>
        public void ClearTrees(GameMap map, int x, int y)
        {
            if (x >= 0 && x < map.Width && y >= 0 && y < map.Height)
            {
                map.Tiles[x, y].TreeType = null;
                map.Tiles[x, y].TreeCount = 0;
            }
        }

        /// <summary>
        /// Получить общее количество деревьев на карте
        /// </summary>
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

        /// <summary>
        /// Получить количество тайлов с деревьями
        /// </summary>
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

        /// <summary>
        /// Получить статистику по типам деревьев
        /// </summary>
        public Dictionary<TreeType, int> GetTreeTypeStatistics(GameMap map)
        {
            var statistics = new Dictionary<TreeType, int>();

            // Инициализируем все типы деревьев с нулевым счетчиком
            foreach (TreeType treeType in Enum.GetValues(typeof(TreeType)))
            {
                statistics[treeType] = 0;
            }

            // Считаем деревья по типам
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