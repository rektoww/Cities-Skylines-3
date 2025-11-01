using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Enums;

namespace Core.Models.Buildings.CommertialBuildings
{
    public class Supermarket : CommercialBuilding
    {
        public Supermarket() : base(CommercialBuildingType.Supermarket)
        {
        }
    }
}
