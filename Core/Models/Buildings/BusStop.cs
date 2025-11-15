using Core.Enums;
using Core.Enums.Core.Enums;
using Core.Interfaces;
using Core.Models.Base;
using Core.Models.Map;
using System.Collections.Generic;

namespace Core.Models.Buildings
{
    public class BusStop : TransitStation, IConstructable
    {
        public override decimal BuildCost => 500m;
        public override Dictionary<ConstructionMaterial, int> RequiredMaterials =>
            new Dictionary<ConstructionMaterial, int>
            {
                { ConstructionMaterial.Concrete, 2 },
                { ConstructionMaterial.Steel, 1 }
            };
        public override BuildingType BuildingType => BuildingType.BusStop;

        public BusStop()
        {
            Width = 1;
            Height = 1;
            Name = "Автобусная остановка";
        }

        public override bool CanPlace(int x, int y, GameMap map)
        {
            if (!base.CanPlace(x, y, map))
                return false;

            var tile = map.Tiles[x, y];
            return tile.HasRoad;
        }

        public override void OnBuildingPlaced()
        {
            // Логика инициализации
        }
    }
}