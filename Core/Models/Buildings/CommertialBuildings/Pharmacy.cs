using Core.Enums;
using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Buildings.CommertialBuildings
{
    public class Pharmacy : CommercialBuilding, IConstructable<Pharmacy>
    {
        // Статические свойства, уникальные для каждого объекта, для примера (Бардашов)
        public static decimal BuildCost { get; protected set; } = 70000m;

        public static Dictionary<ConstructionMaterial, int> RequiredMaterials { get; protected set; }
            = new Dictionary<ConstructionMaterial, int>
            {
                { ConstructionMaterial.Steel, 5 },
                { ConstructionMaterial.Concrete, 5 }
            };
        public Pharmacy() : base(CommercialBuildingType.Pharmacy)
        {
        }
    }
}