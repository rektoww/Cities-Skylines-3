using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Enums;
using Core.Enums.Core.Enums;
using Core.GameEngine;
using Core.Models.Base;
using Core.Models.Buildings;
using Core.Models.Buildings.IndustrialBuildings;
using Core.Models.Map;
using Core.Models.Police;
using Core.Models.Roads;
using Core.Models.Vehicles;
using Core.Resourses;
using Infrastructure.Services;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace Laboratornaya3.ViewModels
{
    public partial class BuildingUI : ObservableObject
    {
        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private string icon;

        [ObservableProperty]
        private string category;

        [ObservableProperty]
        private BuildingType buildingType;

        [ObservableProperty]
        private CommercialBuildingType? commercialType;

        [ObservableProperty]
        private ServiceBuildingType? serviceType;

        [ObservableProperty]
        private IndustrialBuildingType? industrialType;

        [ObservableProperty]
        private ResidentialType? residentialType;
    }

    public partial class MainViewModel : ObservableObject
    {
        private readonly SaveLoadService _saveLoadService;
        private readonly GameEngine _gameEngine;
        private DispatcherTimer _gameTimer;

        [ObservableProperty]
        private GameMap _currentMap;

        [ObservableProperty]
        private string _selectedCategoryName;

        [ObservableProperty]
        private ObservableCollection<BuildingUI> _visibleBuildings;

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

        private Point _roadStartPoint;
        private bool _isDrawingRoad;

        public decimal CityBudget => _gameEngine?.FinancialSystem?.CityBudget ?? 0;
        public int CitizenCount => _gameEngine?.PopulationService?.CitizenCount ?? 0;
        public PlayerResources PlayerResources => _gameEngine?.PlayerResources;

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

        private readonly Dictionary<string, List<BuildingUI>> _buildingCategories = new();

        public MainViewModel()
        {
            _saveLoadService = new SaveLoadService();

            LoadStatic();

            _gameEngine = new GameEngine(CurrentMap);

            InitializeCategories();
            SelectedCategoryName = "Коммерция";
            UpdateBuildingsDisplay("Коммерция");

            StartGameTimer();
        }

        private void StartGameTimer()
        {
            _gameTimer = new DispatcherTimer();
            _gameTimer.Interval = TimeSpan.FromSeconds(3);

            _gameTimer.Tick += async (s, e) =>
            {
                await System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
                        _gameEngine.Update();
                    }
                    catch (Exception ex)
                    {
                    }
                }).ConfigureAwait(false);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    UpdateChangedTiles();
                    OnPropertyChanged(nameof(CityBudget));
                    OnPropertyChanged(nameof(CitizenCount));
                });
            };

            _gameTimer.Start();
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

        private void InitializeCategories()
        {
            // ЖИЛЫЕ ЗДАНИЯ
            _buildingCategories.Add("Жилье", new List<BuildingUI>
            {
                new BuildingUI { Name = "Апартаменты", Icon = "🏢", Category = "Жилье", BuildingType = BuildingType.Residential, ResidentialType = ResidentialType.Apartment },
                new BuildingUI { Name = "Общежитие", Icon = "🏘️", Category = "Жилье", BuildingType = BuildingType.Residential, ResidentialType = ResidentialType.Dormitory },
                new BuildingUI { Name = "Отель", Icon = "🏨", Category = "Жилье", BuildingType = BuildingType.Residential, ResidentialType = ResidentialType.Hotel }
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
                new BuildingUI { Name = "Магазин", Icon = "🛍️", Category = "Коммерция", BuildingType = BuildingType.Commercial, CommercialType = CommercialBuildingType.Shop },
                new BuildingUI { Name = "Супермаркет", Icon = "🛒", Category = "Коммерция", BuildingType = BuildingType.Commercial, CommercialType = CommercialBuildingType.Supermarket },
                new BuildingUI { Name = "Аптека", Icon = "💊", Category = "Коммерция", BuildingType = BuildingType.Commercial, CommercialType = CommercialBuildingType.Pharmacy },
                new BuildingUI { Name = "Кафе", Icon = "☕", Category = "Коммерция", BuildingType = BuildingType.Commercial, CommercialType = CommercialBuildingType.Cafe },
                new BuildingUI { Name = "Ресторан", Icon = "🍴", Category = "Коммерция", BuildingType = BuildingType.Commercial, CommercialType = CommercialBuildingType.Restaurant },
                new BuildingUI { Name = "Заправка", Icon = "⛽", Category = "Коммерция", BuildingType = BuildingType.Commercial, CommercialType = CommercialBuildingType.GasStation }
            });

            // СОЦИАЛЬНЫЕ ЗДАНИЯ
            _buildingCategories.Add("Социум", new List<BuildingUI>
            {
                new BuildingUI { Name = "Школа", Icon = "🏫", Category = "Социум", BuildingType = BuildingType.Service, ServiceType = ServiceBuildingType.School },
                new BuildingUI { Name = "Больница", Icon = "🏥", Category = "Социум", BuildingType = BuildingType.Service, ServiceType = ServiceBuildingType.Hospital },
                new BuildingUI { Name = "Университет", Icon = "🎓", Category = "Социум", BuildingType = BuildingType.Service, ServiceType = ServiceBuildingType.University },
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

                    MessageBox.Show($"Режим строительства дорог: {building.Name}. Кликните и протяните для создания дороги.",
                                   "Режим строительства дорог",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Information);
                }
                else if (building.Category == "Транспорт" && IsVehicleBuilding(building.Name))
                {
                    IsVehiclePlacementMode = true;
                    IsBuildingMode = false;
                    IsRoadPlacementMode = false;
                    SelectedVehicleType = GetVehicleTypeFromName(building.Name);

                    MessageBox.Show($"Режим размещения транспорта: {building.Name}. Кликните на дороге для размещения.",
                                   "Режим размещения транспорта",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Information);
                }
                else
                {
                    IsBuildingMode = true;
                    IsRoadPlacementMode = false;
                    IsVehiclePlacementMode = false;

                    MessageBox.Show($"Выбрано: {building.Name}. Кликните на карте для размещения.",
                                   "Режим строительства",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Information);
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
        private void LoadStatic()
        {
            CurrentMap = StaticBigMapProvider.Build50();
            RefreshMap(forceFull: true);
        }

        [RelayCommand]
        private void SaveGame()
        {
            try
            {
                _saveLoadService.SaveGame(CurrentMap, "save.json");
                MessageBox.Show($"Сохранено!\nЗданий: {CurrentMap.Buildings.Count}\nДорог: {CurrentMap.RoadSegments.Count}",
                              "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void LoadGame()
        {
            try
            {
                _saveLoadService.LoadGame(CurrentMap, "save.json");
                RefreshMap(forceFull: true);
                MessageBox.Show($"Загружено!\nЗданий: {CurrentMap.Buildings.Count}\nДорог: {CurrentMap.RoadSegments.Count}",
                              "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void ShowResourcesInfo()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Инвентарь строительных материалов:");
            sb.AppendLine();
            foreach (var mat in _gameEngine.PlayerResources.StoredMaterials)
            {
                sb.AppendLine($" • {mat.Key}: {mat.Value} шт.");
            }
            sb.AppendLine();
            sb.AppendLine($"Баланс игрока: {_gameEngine.PlayerResources.Balance:N0} валюты");

            MessageBox.Show(sb.ToString(), "Ресурсы", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private void ShowFinanceInfo()
        {
            var report = _gameEngine.FinancialSystem.GetFinancialReport();
            var sb = new StringBuilder();
            sb.AppendLine("Финансовый отчет города:");
            sb.AppendLine();
            sb.AppendLine($"Бюджет: {report.CurrentBudget:N0} валюты");
            sb.AppendLine($"Доходы: {report.TotalIncome:N0} валюты");
            sb.AppendLine($"Расходы: {report.TotalExpenses:N0} валюты");
            sb.AppendLine($"Чистый баланс за период: {report.PeriodBalance:N0} валюты");

            MessageBox.Show(sb.ToString(), "Финансы", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private void BuyMaterials()
        {
            MessageBox.Show("Функция покупки материалов в разработке", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private void SellMaterials()
        {
            MessageBox.Show("Функция продажи материалов в разработке", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
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
            if (building != null && _gameEngine.TryPlaceBuilding(building, x, y))
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

                MessageBox.Show($"Здание '{building.Name}' успешно построено!\n" +
                                $"Бюджет: {_gameEngine.FinancialSystem.CityBudget:N0}",
                                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                return true;
            }

            MessageBox.Show("Нельзя построить здесь. Возможные причины:\n• Недостаточно бюджета или материалов\n• Недостаточно места\n• Неподходящий рельеф\n• Место занято",
                           "Ошибка строительства",
                           MessageBoxButton.OK,
                           MessageBoxImage.Warning);
            return false;
        }

        private Building CreateBuildingFromUI(BuildingUI buildingUI)
        {
            return buildingUI.BuildingType switch
            {
                BuildingType.Residential when buildingUI.ResidentialType.HasValue =>
                    new ResidentialBuilding(buildingUI.ResidentialType.Value),

                BuildingType.Commercial when buildingUI.CommercialType.HasValue =>
                    new CommercialBuilding(buildingUI.CommercialType.Value),

                BuildingType.Service when buildingUI.ServiceType.HasValue =>
                    new ServiceBuilding(buildingUI.ServiceType.Value),

                BuildingType.Industrial when buildingUI.IndustrialType.HasValue =>
                    new IndustrialBuilding(buildingUI.IndustrialType.Value),

                _ => null
            };
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
                CurrentMap.AddRoadSegment(roadSegment);
                _isDrawingRoad = false;

                var points = GetPointsAlongSegment(roadSegment);
                foreach (var point in points)
                {
                    RefreshSpecificTile((int)point.X, (int)point.Y);
                }

                MessageBox.Show($"Дорога успешно построена от ({_roadStartPoint.X},{_roadStartPoint.Y}) до ({x},{y})",
                               "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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
                MessageBox.Show("Транспорт можно размещать только на дороге", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var vehicle = CreateVehicle(SelectedVehicleType, x, y);
            _gameEngine.AddVehicle(vehicle);

            tile.VehicleIcons.Add(GetVehicleIcon(SelectedVehicleType));
            tile.VehicleCount++;
            tile.HasVehicle = true;

            RefreshSpecificTile(x, y);

            MessageBox.Show($"Транспорт '{SelectedVehicleType}' размещен на ({x}, {y})",
                           "Транспорт размещен", MessageBoxButton.OK, MessageBoxImage.Information);
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

        [RelayCommand]
        private void ShowTileInfo(Tile tile)
        {
            if (tile == null) return;

            var sb = new StringBuilder();
            sb.AppendLine($"Координаты: ({tile.X}; {tile.Y})");
            sb.AppendLine($"Рельеф: {tile.Terrain}");

            if (tile.Building != null)
            {
                sb.AppendLine($"Здание: {tile.Building.Name}");
                sb.AppendLine($"Тип: {tile.Building.GetType().Name}");

                if (tile.Building is Mine mine)
                {
                    sb.AppendLine($"--- Детали шахты ---");
                    sb.AppendLine($"Добывает: {mine.ProducedMaterial}");
                    sb.AppendLine($"Накоплено: {mine.StoredResources}/{mine.MaxStorage}");

                    if (mine.StoredResources > 0)
                    {
                        var result = MessageBox.Show(
                            $"Собрать {mine.StoredResources} ед. {mine.ProducedMaterial}?",
                            "Сбор ресурсов",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            int collected = mine.CollectResources();
                            if (_gameEngine.PlayerResources.StoredMaterials.ContainsKey(mine.ProducedMaterial))
                                _gameEngine.PlayerResources.StoredMaterials[mine.ProducedMaterial] += collected;
                            else
                                _gameEngine.PlayerResources.StoredMaterials[mine.ProducedMaterial] = collected;

                            RefreshSpecificTile(tile.X, tile.Y);
                            MessageBox.Show($"Собрано: {collected} ед. {mine.ProducedMaterial}",
                                            "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
            }

            MessageBox.Show(sb.ToString(), "Информация о клетке", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private void CreateTestCrime()
        {
            var random = new Random();
            int x = random.Next(0, CurrentMap.Width);
            int y = random.Next(0, CurrentMap.Height);

            _gameEngine.PoliceService.CreateCrime(CrimeType.Theft, x, y);

            MessageBox.Show($"Преступление создано на ({x}, {y})", "Тест");
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