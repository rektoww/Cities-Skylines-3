using Core.Models.Base;
using Core.Models.Buildings;
using Core.Models.Buildings.IndustrialBuildings;
using Core.Models.Police;
using Core.Enums;
using Core.Enums.Core.Enums;
using Laboratornaya3.ViewModels;

namespace Laboratornaya3.Services.Factories
{
    public class BuildingFactory
    {
        public Building CreateFromUI(BuildingUI ui)
        {
            if (ui == null) return null;

            return ui.BuildingType switch
            {
                BuildingType.Residential when ui.ResidentialType.HasValue =>
                    new ResidentialBuilding(ui.ResidentialType.Value),

                BuildingType.Commercial when ui.CommercialType.HasValue =>
                    new CommercialBuilding(ui.CommercialType.Value),

                BuildingType.Service when ui.ServiceType.HasValue =>
                    new ServiceBuilding(ui.ServiceType.Value),

                BuildingType.Industrial when ui.IndustrialType.HasValue =>
                    new IndustrialBuilding(ui.IndustrialType.Value),

                // Some UI items may represent special types like Park or PoliceStation
                _ => ui.BuildingType switch
                {
                    BuildingType.Park => null, // placeholder for Park
                    BuildingType.PoliceStation => new PoliceStation(),
                    _ => null
                }
            };
        }
    }
}