using Core.Models.Map;

namespace Core.Services
{
    /// <summary>
    /// Менеджер для управления парками и пешеходной инфраструктурой
    /// </summary>
    public class ParkManager
    {
        /// <summary>
        /// Создать парк на указанных тайлах
        /// </summary>
        public void CreatePark(GameMap map, int startX, int startY, int width, int height)
        {
            for (int x = startX; x < startX + width; x++)
            {
                for (int y = startY; y < startY + height; y++)
                {
                    if (x >= 0 && x < map.Width && y >= 0 && y < map.Height)
                    {
                        map.Tiles[x, y].HasPark = true;
                        map.Tiles[x, y].HasPedestrianPath = true;
                    }
                }
            }
        }

        /// <summary>
        /// Создать велодорожку на указанных тайлах
        /// </summary>
        public void CreateBikeLane(GameMap map, int startX, int startY, int length, bool isHorizontal)
        {
            for (int i = 0; i < length; i++)
            {
                int x = isHorizontal ? startX + i : startX;
                int y = isHorizontal ? startY : startY + i;

                if (x >= 0 && x < map.Width && y >= 0 && y < map.Height)
                {
                    map.Tiles[x, y].HasBikeLane = true;
                    map.Tiles[x, y].HasPedestrianPath = true;
                }
            }
        }

        /// <summary>
        /// Получить количество "счастливых" тайлов (с парками/велодорожками)
        /// </summary>
        public int GetHappyTilesCount(GameMap map)
        {
            int count = 0;
            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    if (map.Tiles[x, y].HasPark || map.Tiles[x, y].HasBikeLane)
                        count++;
                }
            }
            return count;
        }
    }
}