using CommunityToolkit.Mvvm.ComponentModel;
using Core.Enums;
using Core.Models.Base;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Core.Models.Map
{
    public partial class Tile : ObservableObject
    {
        [ObservableProperty]
        private int _x;

        [ObservableProperty]
        private int _y;

        [ObservableProperty]
        private TerrainType _terrain;

        [ObservableProperty]
        private float _height;

        [ObservableProperty]
        private Building _building;

        [ObservableProperty]
        private bool _hasRoad;

        [ObservableProperty]
        private RoadType _roadType = Core.Enums.RoadType.Regular;

        [ObservableProperty]
        private bool _hasIntersection;

        [ObservableProperty]
        private int _vehicleCount;

        [ObservableProperty]
        private bool _hasVehicle;

        [ObservableProperty]
        private ObservableCollection<string> _vehicleIcons;

        [ObservableProperty]
        private bool _hasBikeLane;

        [ObservableProperty]
        private bool _hasPark;

        [ObservableProperty]
        private bool _hasPedestrianPath;

        [ObservableProperty]
        private TreeType? _treeType;

        [ObservableProperty]
        private int _treeCount;

        public List<NaturalResource> Resources { get; set; }

        /// <summary>
        /// Есть ли лес на этом тайле
        /// </summary>
        public bool HasForest => Terrain == TerrainType.Forest;

        public Tile()
        {
            Resources = new List<NaturalResource>();
            TreeCount = 0;
            VehicleIcons = new ObservableCollection<string>();
            VehicleCount = 0;
            HasVehicle = false;
        }

        // Метод для добавления иконки транспорта с уведомлением
        public void AddVehicleIcon(string icon)
        {
            VehicleIcons.Add(icon);
            VehicleCount = VehicleIcons.Count;
            HasVehicle = VehicleCount > 0;
            OnPropertyChanged(nameof(VehicleIcons));
            OnPropertyChanged(nameof(VehicleCount));
            OnPropertyChanged(nameof(HasVehicle));
        }

        // Метод для очистки транспорта
        public void ClearVehicleIcons()
        {
            VehicleIcons.Clear();
            VehicleCount = 0;
            HasVehicle = false;
            OnPropertyChanged(nameof(VehicleIcons));
            OnPropertyChanged(nameof(VehicleCount));
            OnPropertyChanged(nameof(HasVehicle));
        }
    }
}