using Core.Enums;
using Core.Interfaces;
using Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Buildings.CommertialBuildings
{
    public class Supermarket : CommercialBuilding, IConstructable<Supermarket>
    {
        // Статические свойства, уникальные для каждого объекта, для примера (Бардашов)
        public static decimal BuildCost { get; protected set; } = 200000m;

        public static Dictionary<ConstructionMaterial, int> RequiredMaterials { get; protected set; }
            = new Dictionary<ConstructionMaterial, int>
            {
                { ConstructionMaterial.Steel, 5 },
                { ConstructionMaterial.Concrete, 5 }
            };
        public Supermarket() : base(CommercialBuildingType.Supermarket)
        {
        }
    }
}
