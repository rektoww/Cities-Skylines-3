using CommunityToolkit.Mvvm.ComponentModel;
using Core.Models.Base;
using Core.Models.Roads;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Core.Models.Map
{
    public partial class GameMap : ObservableObject
    {
        [ObservableProperty]
        private int _width;

        [ObservableProperty]
        private int _height;

        [ObservableProperty]
        private Tile[,] _tiles;

        [ObservableProperty]
        private ObservableCollection<Building> _buildings;

        [ObservableProperty]
        private ObservableCollection<RoadSegment> _roadSegments;

        [ObservableProperty]
        private ObservableCollection<Intersection> _intersections;

        [ObservableProperty]
        private ObservableCollection<Transport> _vehicles;

        private Building[,] _buildingsGrid;

        public GameMap(int width, int height)
        {
            Width = width;
            Height = height;
            Tiles = new Tile[Width, Height];
            Buildings = new ObservableCollection<Building>();
            RoadSegments = new ObservableCollection<RoadSegment>();
            Intersections = new ObservableCollection<Intersection>();
            Vehicles = new ObservableCollection<Transport>();
            _buildingsGrid = new Building[Width, Height];

            InitializeMap();
        }

        private void InitializeMap()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Tiles[x, y] = new Tile() { X = x, Y = y };
                }
            }
        }

        public bool TryPlaceBuilding(Building building, int x, int y)
        {
            if (building.TryPlace(x, y, this))
            {
                Buildings.Add(building);
                OnPropertyChanged(nameof(Buildings));
                return true;
            }
            return false;
        }

        public Building GetBuildingAt(int x, int y)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
                return _buildingsGrid[x, y];
            return null;
        }

        public void SetBuildingAt(int x, int y, Building building)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                _buildingsGrid[x, y] = building;
                Tiles[x, y].Building = building;
            }
        }

        public void AddRoadSegment(RoadSegment segment)
        {
            if (!RoadSegments.Contains(segment))
            {
                RoadSegments.Add(segment);
                MarkTilesAsRoad(segment);
                OnPropertyChanged(nameof(RoadSegments));
            }
        }

        private void MarkTilesAsRoad(RoadSegment segment)
        {
            // Упрощенная логика отметки дорог
            var points = GetPointsAlongSegment(segment);
            foreach (var point in points)
            {
                if (point.X >= 0 && point.X < Width && point.Y >= 0 && point.Y < Height)
                {
                    Tiles[point.X, point.Y].HasRoad = true;
                    Tiles[point.X, point.Y].RoadType = segment.RoadType;
                }
            }
        }

        private List<System.Drawing.Point> GetPointsAlongSegment(RoadSegment segment)
        {
            var points = new List<System.Drawing.Point>();
            // Упрощенная логика получения точек сегмента
            points.Add(new System.Drawing.Point(segment.StartX, segment.StartY));
            points.Add(new System.Drawing.Point(segment.EndX, segment.EndY));
            return points;
        }

        // Методы для управления транспортными средствами
        public void AddVehicle(Transport vehicle)
        {
            if (!Vehicles.Contains(vehicle))
            {
                Vehicles.Add(vehicle);
                OnPropertyChanged(nameof(Vehicles));
            }
        }

        public void RemoveVehicle(Transport vehicle)
        {
            if (Vehicles.Contains(vehicle))
            {
                Vehicles.Remove(vehicle);
                OnPropertyChanged(nameof(Vehicles));
            }
        }

        public Transport GetVehicleAt(int x, int y)
        {
            return Vehicles.FirstOrDefault(v => v.X == x && v.Y == y);
        }

        public IEnumerable<Transport> GetVehiclesInArea(int x, int y, int radius)
        {
            return Vehicles.Where(v =>
                Math.Abs(v.X - x) <= radius &&
                Math.Abs(v.Y - y) <= radius);
        }

        public void UpdateAllVehicles()
        {
            foreach (var vehicle in Vehicles.ToList())
            {
                vehicle.Move();
            }
        }
    }
}