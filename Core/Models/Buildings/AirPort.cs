using Core.Enums;
using Core.Enums.Core.Enums;
using Core.Interfaces;
using Core.Models.Base;
using Core.Models.Map;
using Core.Resourses;
using System.Collections.Generic;

namespace Core.Models.Buildings
{
    public class AirPort : Port, IConstructable
    {
        public override decimal BuildCost => 50000m;
        public override Dictionary<ConstructionMaterial, int> RequiredMaterials =>
            new Dictionary<ConstructionMaterial, int>
            {
                { ConstructionMaterial.Steel, 6 },
                { ConstructionMaterial.Glass, 4 },
                { ConstructionMaterial.Concrete, 3 }
            };
        public override BuildingType BuildingType => BuildingType.Airport;

        protected override int MaxUnits => 5;
        protected override string ResourceType => "AirTransport";
        protected override int UnitCapacity => 100;
        protected override int UnitCooldown => 3;
        protected override int UnitRevenue => 200;
        protected override int PassengersPerUnit => 10;

        public AirPort(PlayerResources playerResources) : base(playerResources)
        {
            Width = 2;
            Height = 2;
            Name = "Аэропорт";
        }

        protected override PortUnit CreateUnit() => new PortUnit(UnitCapacity, UnitCooldown, UnitRevenue);

        public override bool CanPlace(int x, int y, GameMap map)
        {
            if (!base.CanPlace(x, y, map))
                return false;

            // Проверка на отсутствие леса в радиусе 2 тайлов
            for (int tx = x - 2; tx <= x + Width + 1; tx++)
            {
                for (int ty = y - 2; ty <= y + Height + 1; ty++)
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

        public override void OnBuildingPlaced()
        {

        }
    }
}