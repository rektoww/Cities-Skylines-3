using Core.Enums;
using Core.Enums.Core.Enums;

namespace Core.Models.Buildings.CommertialBuildings
{
    public class GasStation : CommercialBuilding
    {
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
