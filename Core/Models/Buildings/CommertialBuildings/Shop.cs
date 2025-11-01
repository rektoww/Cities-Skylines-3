using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Enums;

namespace Core.Models.Buildings.CommertialBuildings
{
    public class Shop : CommercialBuilding
    {
        public Shop() : base(CommercialBuildingType.Shop)
        {
        }
    }
}
