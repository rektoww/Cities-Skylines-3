using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Enums;
using Core.Enums.Core.Enums;
using Core.GameEngine;
using Core.Models.Base;
using Core.Models.Map;
using Core.Models.Police;
using Core.Models.Roads;
using Core.Models.Vehicles;
using Core.Resourses;
using Infrastructure.Services;
using Laboratornaya3.Services.Factories;
using Laboratornaya3.Services.MapPlacement;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;

namespace Laboratornaya3.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly GameEngine _gameEngine;
        private readonly INotificationService _notifier;
        private readonly BuildingFactory _buildingFactory;
        private readonly IMapPlacementService _placementService;

        public GameStateViewModel GameState { get; }
        public BuildingManagementViewModel BuildingManagement { get; }
        public RoadManagementViewModel RoadManagement { get; }
        public VehicleManagementViewModel VehicleManagement { get; }
        public MapViewModel Map { get; }

        [ObservableProperty]
        private GameMap _currentMap;

        [ObservableProperty]
        private string _selectedCategoryName = "Коммерция";

        [ObservableProperty]
        private ObservableCollection<BuildingUI> _visibleBuildings = new();

        [ObservableProperty]
        private BuildingUI _selectedBuilding;

        [ObservableProperty]
        private bool _isBuildingMode;

        [ObservableProperty]
        private bool _isRoadPlacementMode;

        [ObservableProperty]
        private bool _isVehiclePlacementMode;

        [ObservableProperty]
        private RoadType _selectedRoadType = RoadType.Street;

        [ObservableProperty]
        private VehicleType _selectedVehicleType;

        public decimal CityBudget => _gameEngine?.FinancialSystem?.CityBudget ?? 0;
        public int CitizenCount => _gameEngine?.PopulationService?.CitizenCount ?? 0;
        public PlayerResources PlayerResources => _gameEngine?.PlayerResources;

        private readonly Dictionary<string, List<BuildingUI>> _buildingCategories = new();
        private Point _roadStartPoint;
        private bool _isDrawingRoad;

        private ObservableCollection<Tile> _tilesObservable;
        private Tile[,] _previousTilesState;
        private bool _needsFullRefresh = true;

        public ObservableCollection<Tile> TilesObservable
        {
            get
            {
                if (_tilesObservable == null || _needsFullRefresh)
                {
                    RefreshTilesCollection();
                }
                return _tilesObservable;
            }
        }

        public MainViewModel()
        {
            var map = StaticBigMapProvider.Build50();
            _currentMap = map;
            _gameEngine = new GameEngine(map);
            _notifier = new Services.NotificationService();
            _buildingFactory = new BuildingFactory();
            _placementService = new MapPlacementService(_notifier);

            GameState = new GameStateViewModel(_gameEngine, _notifier);
            BuildingManagement = new BuildingManagementViewModel(_gameEngine, _notifier);
            RoadManagement = new RoadManagementViewModel(_gameEngine, _notifier);
            VehicleManagement = new VehicleManagementViewModel(_gameEngine, _notifier);
            Map = new MapViewModel(map, _notifier);

            GameState.TickCompleted += OnGameTickCompleted;

            InitializeCategories();
            UpdateBuildingsDisplay("Коммерция");
            GameState.StartGameTimer();
        }

        private void OnGameTickCompleted()
        {
            UpdateChangedTiles();
            OnPropertyChanged(nameof(CityBudget));
            OnPropertyChanged(nameof(CitizenCount));
        }

        private void InitializeCategories()
        {
            // ЖИЛЫЕ ЗДАНИЯ
            _buildingCategories.Add("Жилье", new List<BuildingUI>
            {
                new BuildingUI { Name = "Апартаменты", Icon = "🏢", Category = "Жилье", BuildingType = BuildingType.Residential, ResidentialType = Core.Enums.Core.Enums.ResidentialType.Apartment },
                new BuildingUI { Name = "Общежитие", Icon = "🏘️", Category = "Жилье", BuildingType = BuildingType.Residential, ResidentialType = Core.Enums.Core.Enums.ResidentialType.Dormitory },
                new BuildingUI { Name = "Отель", Icon = "🏨", Category = "Жилье", BuildingType = BuildingType.Residential, ResidentialType = Core.Enums.Core.Enums.ResidentialType.Hotel }
            });

            // ПРОМЫШЛЕННЫЕ ЗДАНИЯ
            _buildingCategories.Add("Производство", new List<BuildingUI>
            {
                new BuildingUI { Name = "Завод", Icon = "🏭", Category = "Производство", BuildingType = BuildingType.Industrial, IndustrialType = IndustrialBuildingType.Factory },
                new BuildingUI { Name = "Ферма", Icon = "🌾", Category = "Производство", BuildingType = BuildingType.Industrial, IndustrialType = IndustrialBuildingType.Farm },
                new BuildingUI { Name = "Шахта", Icon = "⛏️", Category = "Производство", BuildingType = BuildingType.Industrial, IndustrialType = IndustrialBuildingType.Mine },
                new BuildingUI { Name = "Электростанция", Icon = "⚡", Category = "Производство", BuildingType = BuildingType.Industrial, IndustrialType = IndustrialBuildingType.PowerPlant }
            });

            // КОММЕРЧЕСКИЕ ЗДАНИЯ
            _buildingCategories.Add("Коммерция", new List<BuildingUI>
            {
                new BuildingUI { Name = "Магазин", Icon = "🛍️", Category = "Коммерция", BuildingType = BuildingType.Commercial, CommercialType = Core.Enums.Core.Enums.CommercialBuildingType.Shop },
                new BuildingUI { Name = "Супермаркет", Icon = "🛒", Category = "Коммерция", BuildingType = BuildingType.Commercial, CommercialType = Core.Enums.Core.Enums.CommercialBuildingType.Supermarket },
                new BuildingUI { Name = "Аптека", Icon = "💊", Category = "Коммерция", BuildingType = BuildingType.Commercial, CommercialType = Core.Enums.Core.Enums.CommercialBuildingType.Pharmacy },
                new BuildingUI { Name = "Кафе", Icon = "☕", Category = "Коммерция", BuildingType = BuildingType.Commercial, CommercialType = Core.Enums.Core.Enums.CommercialBuildingType.Cafe },
                new BuildingUI { Name = "Ресторан", Icon = "🍴", Category = "Коммерция", BuildingType = BuildingType.Commercial, CommercialType = Core.Enums.Core.Enums.CommercialBuildingType.Restaurant },
                new BuildingUI { Name = "Заправка", Icon = "⛽", Category = "Коммерция", BuildingType = BuildingType.Commercial, CommercialType = Core.Enums.Core.Enums.CommercialBuildingType.GasStation }
            });

            // СОЦИАЛЬНЫЕ ЗДАНИЯ
            _buildingCategories.Add("Социум", new List<BuildingUI>
            {
                new BuildingUI { Name = "Школа", Icon = "🏫", Category = "Социум", BuildingType = BuildingType.Service, ServiceType = Core.Enums.Core.Enums.ServiceBuildingType.School },
                new BuildingUI { Name = "Больница", Icon = "🏥", Category = "Социум", BuildingType = BuildingType.Service, ServiceType = Core.Enums.Core.Enums.ServiceBuildingType.Hospital },
                new BuildingUI { Name = "Университет", Icon = "🎓", Category = "Социум", BuildingType = BuildingType.Service, ServiceType = Core.Enums.Core.Enums.ServiceBuildingType.University },
                new BuildingUI { Name = "Парк", Icon = "🌳", Category = "Социум", BuildingType = BuildingType.Park },
                new BuildingUI { Name = "Полицейский участок", Icon = "🚓", Category = "Социум", BuildingType = BuildingType.PoliceStation }
            });

            // ТРАНСПОРТНЫЕ ЗДАНИЯ
            _buildingCategories.Add("Транспорт", new List<BuildingUI>
            {
                new BuildingUI { Name = "Аэропорт", Icon = "✈️", Category = "Транспорт", BuildingType = BuildingType.Airport },
                new BuildingUI { Name = "Морской порт", Icon = "⚓", Category = "Транспорт", BuildingType = BuildingType.Seaport },
                new BuildingUI { Name = "Ж/Д Вокзал", Icon = "🚉", Category = "Транспорт", BuildingType = BuildingType.TrainStation },
                new BuildingUI { Name = "Автобусная остановка", Icon = "🚏", Category = "Транспорт", BuildingType = BuildingType.BusStop },
                new BuildingUI { Name = "Такси", Icon = "🚕", Category = "Транспорт" },
                new BuildingUI { Name = "Грузовик", Icon = "🚚", Category = "Транспорт" },
                new BuildingUI { Name = "Полицейская машина", Icon = "🚔", Category = "Транспорт" }
            });

            // ДОРОГИ
            _buildingCategories.Add("Дороги", new List<BuildingUI>
            {
                new BuildingUI { Name = "Грунтовая дорога", Icon = "🛤️", Category = "Дороги"},
                new BuildingUI { Name = "Городская дорога", Icon = "🛣️", Category = "Дороги" },
                new BuildingUI { Name = "Широкая дорога", Icon = "🛣️", Category = "Дороги" },
                new BuildingUI { Name = "Скоростное шоссе", Icon = "🏁", Category = "Дороги" },
                new BuildingUI { Name = "Перекрёсток", Icon = "🚦", Category = "Дороги" }
            });
        }

        [RelayCommand]
        private void SelectCategory(string categoryName)
        {
            if (!string.IsNullOrEmpty(categoryName))
            {
                SelectedCategoryName = categoryName;
                UpdateBuildingsDisplay(categoryName);
            }
        }

        private void UpdateBuildingsDisplay(string categoryName)
        {
            if (_buildingCategories.TryGetValue(categoryName, out var buildings))
            {
                VisibleBuildings = new ObservableCollection<BuildingUI>(buildings);
            }
            else
            {
                VisibleBuildings = new ObservableCollection<BuildingUI>();
            }
        }

        [RelayCommand]
        private void SelectBuilding(BuildingUI building)
        {
            if (building != null)
            {
                SelectedBuilding = building;

                if (building.Category == "Дороги")
                {
                    IsRoadPlacementMode = true;
                    IsBuildingMode = false;
                    IsVehiclePlacementMode = false;
                    SelectedRoadType = GetRoadTypeFromName(building.Name);

                    _notifier.ShowInfo($"Режим строительства дорог: {building.Name}. Кликните и протяните для создания дороги.");
                }
                else if (building.Category == "Транспорт" && IsVehicleBuilding(building.Name))
                {
                    IsVehiclePlacementMode = true;
                    IsBuildingMode = false;
                    IsRoadPlacementMode = false;
                    SelectedVehicleType = GetVehicleTypeFromName(building.Name);

                    _notifier.ShowInfo($"Режим размещения транспорта: {building.Name}. Кликните на дороге для размещения.");
                }
                else
                {
                    IsBuildingMode = true;
                    IsRoadPlacementMode = false;
                    IsVehiclePlacementMode = false;

                    _notifier.ShowInfo($"Выбрано: {building.Name}. Кликните на карте для размещения.");
                }
            }
        }

        [RelayCommand]
        private void CancelBuilding()
        {
            IsBuildingMode = false;
            IsRoadPlacementMode = false;
            IsVehiclePlacementMode = false;
            SelectedBuilding = null;
            _isDrawingRoad = false;
        }

        [RelayCommand]
        public void ShowTileInfo(Tile tile)
        {
            if (tile == null) return;

            var sb = new StringBuilder();
            sb.AppendLine($"Координаты: {tile.X}, {tile.Y}");
            sb.AppendLine($"Рельеф: {tile.Terrain}");
            if (tile.Resources != null && tile.Resources.Count > 0)
            {
                sb.AppendLine("Ресурсы:");
                foreach (var r in tile.Resources)
                {
                    sb.AppendLine($" - {r.Type}: {r.Amount}");
                }
            }

            if (tile.Building != null)
            {
                sb.AppendLine($"Здание: {tile.Building.Name} ({tile.Building.BuildingType})");
            }

            if (tile.HasRoad)
            {
                sb.AppendLine($"Дорога: {tile.RoadType}");
                if (tile.HasIntersection) sb.AppendLine("Перекрёсток: Да");
            }

            if (tile.HasVehicle)
            {
                sb.AppendLine($"Транспорт: {tile.VehicleCount}");
            }

            _notifier.ShowInfo(sb.ToString(), "Информация о клетке");
        }

        private void RefreshTilesCollection()
        {
            if (CurrentMap == null) return;

            _tilesObservable = new ObservableCollection<Tile>();
            _previousTilesState = new Tile[CurrentMap.Width, CurrentMap.Height];

            for (int y = 0; y < CurrentMap.Height; y++)
            {
                for (int x = 0; x < CurrentMap.Width; x++)
                {
                    var tile = CurrentMap.Tiles[x, y];
                    _tilesObservable.Add(tile);
                    _previousTilesState[x, y] = CloneTileState(tile);
                }
            }
            _needsFullRefresh = false;
        }

        private Tile CloneTileState(Tile tile)
        {
            return new Tile
            {
                X = tile.X,
                Y = tile.Y,
                Terrain = tile.Terrain,
                Building = tile.Building,
                HasRoad = tile.HasRoad,
                RoadType = tile.RoadType,
                HasIntersection = tile.HasIntersection,
                VehicleIcons = new ObservableCollection<string>(tile.VehicleIcons),
                VehicleCount = tile.VehicleCount,
                HasVehicle = tile.HasVehicle
            };
        }

        private void UpdateChangedTiles()
        {
            if (CurrentMap == null || _tilesObservable == null) return;

            bool anyChanges = false;

            for (int y = 0; y < CurrentMap.Height; y++)
            {
                for (int x = 0; x < CurrentMap.Width; x++)
                {
                    var currentTile = CurrentMap.Tiles[x, y];
                    var previousTile = _previousTilesState[x, y];

                    if (HasTileChanged(currentTile, previousTile))
                    {
                        int index = y * CurrentMap.Width + x;
                        if (index >= 0 && index < _tilesObservable.Count)
                        {
                            UpdateTileProperties(_tilesObservable[index], currentTile);
                            _previousTilesState[x, y] = CloneTileState(currentTile);
                            anyChanges = true;
                        }
                    }
                }
            }

            if (anyChanges)
            {
                OnPropertyChanged(nameof(TilesObservable));
            }
        }

        private bool HasTileChanged(Tile current, Tile previous)
        {
            return current.Building != previous.Building ||
                   current.HasRoad != previous.HasRoad ||
                   current.RoadType != previous.RoadType ||
                   current.HasIntersection != previous.HasIntersection ||
                   current.VehicleCount != previous.VehicleCount ||
                   current.HasVehicle != previous.HasVehicle ||
                   !current.VehicleIcons.SequenceEqual(previous.VehicleIcons);
        }

        private void UpdateTileProperties(Tile target, Tile source)
        {
            target.Building = source.Building;
            target.HasRoad = source.HasRoad;
            target.RoadType = source.RoadType;
            target.HasIntersection = source.HasIntersection;
            target.VehicleCount = source.VehicleCount;
            target.HasVehicle = source.HasVehicle;

            if (!target.VehicleIcons.SequenceEqual(source.VehicleIcons))
            {
                target.VehicleIcons.Clear();
                foreach (var icon in source.VehicleIcons)
                {
                    target.VehicleIcons.Add(icon);
                }
            }
        }

        public void RefreshSpecificTile(int x, int y)
        {
            if (_tilesObservable == null || CurrentMap == null) return;

            int index = y * CurrentMap.Width + x;
            if (index >= 0 && index < _tilesObservable.Count)
            {
                var currentTile = CurrentMap.Tiles[x, y];
                UpdateTileProperties(_tilesObservable[index], currentTile);
                _previousTilesState[x, y] = CloneTileState(currentTile);

                OnPropertyChanged(nameof(TilesObservable));
            }
        }

        public void RefreshMap(bool forceFull = false)
        {
            if (forceFull)
            {
                _needsFullRefresh = true;
                OnPropertyChanged(nameof(TilesObservable));
            }
        }

        public void TryPlaceSelected(int x, int y)
        {
            if (SelectedBuilding == null || CurrentMap == null) return;

            if (IsRoadPlacementMode)
            {
                if (SelectedBuilding.Name == "Перекрёсток")
                {
                    PlaceIntersection(x, y);
                }
                else
                {
                    PlaceRoad(x, y);
                }
            }
            else if (IsVehiclePlacementMode)
            {
                TryPlaceVehicle(x, y);
            }
            else
            {
                TryPlaceBuilding(x, y);
            }
        }

        public bool TryPlaceBuilding(int x, int y)
        {
            if (!IsBuildingMode || SelectedBuilding == null) return false;

            var building = CreateBuildingFromUI(SelectedBuilding);
            if (building != null && _placementService.TryPlaceBuilding(_gameEngine, building, x, y))
            {
                for (int tileX = x; tileX < x + building.Width; tileX++)
                {
                    for (int tileY = y; tileY < y + building.Height; tileY++)
                    {
                        RefreshSpecificTile(tileX, tileY);
                    }
                }

                CancelBuilding();
                OnPropertyChanged(nameof(CityBudget));

                return true;
            }

            return false;
        }

        private Building CreateBuildingFromUI(BuildingUI buildingUI)
        {
            return _buildingFactory.CreateFromUI(buildingUI);
        }

        private void PlaceRoad(int x, int y)
        {
            if (!_isDrawingRoad)
            {
                _roadStartPoint = new Point(x, y);
                _isDrawingRoad = true;
            }
            else
            {
                var roadSegment = new RoadSegment((int)_roadStartPoint.X, (int)_roadStartPoint.Y, x, y, SelectedRoadType);
                _isDrawingRoad = false;

                var points = GetPointsAlongSegment(roadSegment);
                foreach (var point in points)
                {
                    RefreshSpecificTile((int)point.X, (int)point.Y);
                }

                _placementService.PlaceRoad(CurrentMap, roadSegment);
            }
        }

        private System.Collections.Generic.List<Point> GetPointsAlongSegment(RoadSegment segment)
        {
            var points = new System.Collections.Generic.List<Point>();
            int dx = Math.Abs(segment.EndX - segment.StartX);
            int dy = Math.Abs(segment.EndY - segment.StartY);
            int steps = Math.Max(dx, dy);

            for (int i = 0; i <= steps; i++)
            {
                float t = (float)i / steps;
                int x = (int)Math.Round(segment.StartX + t * (segment.EndX - segment.StartX));
                int y = (int)Math.Round(segment.StartY + t * (segment.EndY - segment.StartY));
                points.Add(new Point(x, y));
            }

            return points;
        }

        public void StartRoadDrawing(int x, int y)
        {
            _roadStartPoint = new Point(x, y);
            _isDrawingRoad = true;
        }

        public void EndRoadDrawing(int x, int y)
        {
            if (_isDrawingRoad)
            {
                PlaceRoad(x, y);
            }
            else
            {
                _roadStartPoint = new Point(x, y);
                _isDrawingRoad = true;
                PlaceRoad(x, y);
            }
        }

        private void PlaceIntersection(int x, int y)
        {
            var intersection = new Intersection(x, y, true);
            var tile = CurrentMap.Tiles[x, y];
            tile.HasRoad = true;
            tile.RoadType = SelectedRoadType;
            tile.HasIntersection = true;
            RefreshSpecificTile(x, y);
        }

        public void TryPlaceVehicle(int x, int y)
        {
            var tile = CurrentMap.Tiles[x, y];
            if (!tile.HasRoad)
            {
                _notifier.ShowWarning("Транспорт можно размещать только на дороге");
                return;
            }

            var vehicle = CreateVehicle(SelectedVehicleType, x, y);
            _placementService.PlaceVehicle(_gameEngine, vehicle);

            tile.VehicleIcons.Add(GetVehicleIcon(SelectedVehicleType));
            tile.VehicleCount++;
            tile.HasVehicle = true;

            RefreshSpecificTile(x, y);
        }

        private Transport CreateVehicle(VehicleType vehicleType, int x, int y)
        {
            return vehicleType switch
            {
                VehicleType.Taxi => new Taxi(x, y, CurrentMap),
                VehicleType.Truck => new Truck(x, y, CurrentMap),
                VehicleType.PoliceCar => new PoliceCar(x, y, CurrentMap, null),
                _ => new Car(x, y, CurrentMap)
            };
        }

        private string GetVehicleIcon(VehicleType vehicleType) => vehicleType switch
        {
            VehicleType.Taxi => "🚕",
            VehicleType.Truck => "🚚",
            VehicleType.PoliceCar => "🚓",
            _ => "🚗"
        };

        private RoadType GetRoadTypeFromName(string name) => name switch
        {
            "Грунтовая дорога" => RoadType.Dirt,
            "Городская дорога" => RoadType.Street,
            "Широкая дорога" => RoadType.Avenue,
            "Скоростное шоссе" => RoadType.Highway,
            _ => RoadType.Street
        };

        private VehicleType GetVehicleTypeFromName(string name) => name switch
        {
            "Такси" => VehicleType.Taxi,
            "Грузовик" => VehicleType.Truck,
            "Полицейская машина" => VehicleType.PoliceCar,
            _ => VehicleType.Car
        };

        private bool IsVehicleBuilding(string name) =>
            name == "Такси" || name == "Грузовик" || name == "Полицейская машина";
    }
}