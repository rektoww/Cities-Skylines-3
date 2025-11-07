using Core.Enums;
using Core.Interfaces;
using Core.Models.Base;
using Core.Models.Map;
using Core.Resourses;
using System.Xml.Linq;
using Core.Models.Base;
using Core.Resourses;
using Core.Models.Map;
using Core.Enums;
using Core.Interfaces;
using System.Collections.Generic;

namespace Core.Models.Buildings
{
    /// <summary>
    /// Воздушный порт.
    /// </summary>
    public class AirPort : Port, IConstructable<AirPort>
    {
        // Количество юнитов по умолчанию для аэропорта
        protected override int MaxUnits => 5;

        protected override string ResourceType => ""; // Базовый тип ресурса 
        protected override int UnitCapacity => 100; // Вместимость самолета
        protected override int UnitCooldown => 3; // Время одного цикла продажи в тиках
        protected override int UnitRevenue => 200; // Доход за один цикл продажи

        // Сколько людей помещается в одном самолёте (требование)
        protected override int PassengersPerUnit => 10;

        // Статические параметры для строительства (IConstructable)
        public static decimal BuildCost { get; } = 50000m;
        public static Dictionary<ConstructionMaterial, int> RequiredMaterials { get; } =
            new Dictionary<ConstructionMaterial, int>
            {
                { ConstructionMaterial.Steel, 6 },
                { ConstructionMaterial.Glass, 4 },
                { ConstructionMaterial.Concrete, 3 }
            };

        public AirPort(PlayerResources playerResources)
            : base(playerResources)
        {
            // Width/Height можно настроить при создании (наследуется от Building/TransitStation)
            Width = 2;
            Height = 2;
            Name = "Аэропорт";
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