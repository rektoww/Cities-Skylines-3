using Core.Enums;
using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Buildings.CommertialBuildings
{
    public class Shop : CommercialBuilding, IConstructable<Shop>
    {
        // Статические свойства, уникальные для каждого объекта, для примера (Бардашов)
        public static decimal BuildCost { get; protected set; } = 50000m;

        public static Dictionary<ConstructionMaterial, int> RequiredMaterials { get; protected set; }
            = new Dictionary<ConstructionMaterial, int>
            {
                { ConstructionMaterial.Steel, 2 },
                { ConstructionMaterial.Concrete, 3 },
                { ConstructionMaterial.Glass, 2 }
            };

        public Shop() : base(CommercialBuildingType.Shop)
        {
            Name = "Магазин";
        }
    }
}
