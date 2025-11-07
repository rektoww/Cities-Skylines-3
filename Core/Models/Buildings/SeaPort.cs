using Core.Models.Base;
using Core.Resourses;
using Core.Models.Map;
using Core.Enums;

namespace Core.Models.Buildings
{
    /// <summary>
    /// Морской порт.
    /// </summary>
    public class SeaPort : Port
    {
        // Количество юнитов по умолчанию для морского порта
        protected override int MaxUnits => 10;

        // Параметры юнитов
        protected override string ResourceType => "SeaCargo"; // Базовый тип ресурса
        protected override int UnitCapacity => 200; // Вместимость корабля
        protected override int UnitCooldown => 5; // Время одного цикла продажи (в тиках)
        protected override int UnitRevenue => 300; // Доход за один цикл продажи

        public SeaPort(string name, PlayerResources playerResources)
            : base(name, playerResources)
        {
        }

        protected override PortUnit CreateUnit()
        {
            return new PortUnit(UnitCapacity, UnitCooldown, UnitRevenue);
        }

        // Порт должен стоять рядом с водой (по крайней мере один соседний тайл водный)
        public override bool CanPlace(int x, int y, GameMap map)
        {
            if (!base.CanPlace(x, y, map))
                return false;

            for (int tx = x; tx < x + Width; tx++)
            {
                for (int ty = y; ty < y + Height; ty++)
                {
                    var neighbors = new (int dx, int dy)[] { (1, 0), (-1, 0), (0, 1), (0, -1) };
                    foreach (var n in neighbors)
                    {
                        int nx = tx + n.dx;
                        int ny = ty + n.dy;
                        if (nx < 0 || nx >= map.Width || ny < 0 || ny >= map.Height) continue;
                        if (map.Tiles[nx, ny].Terrain == TerrainType.Water)
                            return true;
                    }
                }
            }

            return false;
        }
    }
}