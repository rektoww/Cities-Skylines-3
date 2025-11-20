using CommunityToolkit.Mvvm.ComponentModel;
using Core.Enums.Core.Enums;
using Core.Models.Base;

namespace Laboratornaya3.ViewModels
{
    public partial class BuildingUI : ObservableObject
    {
        private Building _building;
        public Building Building { get => _building; set => SetProperty(ref _building, value); }

        private string _name;
        public string Name { get => _name; set => SetProperty(ref _name, value); }

        private string _icon;
        public string Icon { get => _icon; set => SetProperty(ref _icon, value); }

        private string _category;
        public string Category { get => _category; set => SetProperty(ref _category, value); }

        private BuildingType _buildingType;
        public BuildingType BuildingType { get => _buildingType; set => SetProperty(ref _buildingType, value); }

        private CommercialBuildingType? _commercialType;
        public CommercialBuildingType? CommercialType { get => _commercialType; set => SetProperty(ref _commercialType, value); }

        private ServiceBuildingType? _serviceType;
        public ServiceBuildingType? ServiceType { get => _serviceType; set => SetProperty(ref _serviceType, value); }

        private IndustrialBuildingType? _industrialType;
        public IndustrialBuildingType? IndustrialType { get => _industrialType; set => SetProperty(ref _industrialType, value); }

        private ResidentialType? _residentialType;
        public ResidentialType? ResidentialType { get => _residentialType; set => SetProperty(ref _residentialType, value); }
    }
}
