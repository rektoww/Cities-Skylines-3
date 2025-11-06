using Core.Enums;
using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Buildings.CommertialBuildings
{
    public class GasStation : CommercialBuilding, IConstructable<GasStation>
    {
        // Статические свойства, уникальные для каждого объекта, для примера (Бардашов)
        public static decimal BuildCost { get; protected set; } = 100000m;

        public static Dictionary<ConstructionMaterial, int> RequiredMaterials { get; protected set; }
            = new Dictionary<ConstructionMaterial, int>
            {
                { ConstructionMaterial.Steel, 1 },
                { ConstructionMaterial.Concrete, 3 },
                { ConstructionMaterial.Glass, 3 }
            };
        public GasStation() : base(CommercialBuildingType.GasStation)
        {
            Name = "Заправка";
        }
    }
}
