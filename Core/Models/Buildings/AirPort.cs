using Core.Models.Base;
using Core.Resourses;
using Core.Models.Map;
using Core.Enums;

namespace Core.Models.Buildings
{
    /// <summary>
    /// Воздушный порт.
    /// </summary>
    public class AirPort : Port
    {
        // Количество юнитов по умолчанию для аэропорта
        protected override int MaxUnits => 5;

        protected override string ResourceType => "AirCargo"; // Базовый тип ресурса
        protected override int UnitCapacity => 100; // Вместимость самолета
        protected override int UnitCooldown => 3; // Время одного цикла продажи (в тиках)
        protected override int UnitRevenue => 200; // Доход за один цикл продажи

        public AirPort(string name, PlayerResources playerResources)
            : base(name, playerResources)
        {
        }

        protected override PortUnit CreateUnit()
        {
            return new PortUnit(UnitCapacity, UnitCooldown, UnitRevenue);
        }

        // Нельзя ставить аэропорт слишком близко к лесу (радиус 2)
        public override bool CanPlace(int x, int y, GameMap map)
        {
            if (!base.CanPlace(x, y, map))
                return false;

            int radius = 2;
            int minX = x - radius;
            int maxX = x + Width - 1 + radius;
            int minY = y - radius;
            int maxY = y + Height - 1 + radius;

            for (int tx = minX; tx <= maxX; tx++)
            {
                for (int ty = minY; ty <= maxY; ty++)
                {
                    if (tx < 0 || tx >= map.Width || ty < 0 || ty >= map.Height)
                        continue;
                    var tile = map.Tiles[tx, ty];
                    if (tile.HasForest || tile.TreeCount > 3)
                        return false;
                }
            }

            return true;
        }
    }
}