using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Models.Base;
using Core.Models.Buildings;
using Core.Models.Buildings.CommertialBuildings;
using Core.Models.Buildings.SocialBuildings;
using Core.Models.Map;
using Core.Services;
using Infrastructure.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using Core.Models.Roads;
using Core.Models.Police;
using Core.Models.Vehicles;
using Core.Enums;

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
    }

    public partial class MainViewModel : ObservableObject
    {
        private readonly SaveLoadService _saveLoadService;
        private readonly NatureManager _natureManager;
        private PathfindingService _pathfindingService;
        private PoliceService _policeService;

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

        private readonly Dictionary<string, List<BuildingUI>> _buildingCategories = new();

        // Режим размещения дорог
        private bool _isRoadPlacementMode;
        public bool IsRoadPlacementMode
        {
            get => _isRoadPlacementMode;
            set => SetProperty(ref _isRoadPlacementMode, value);
        }

        private bool _isVehiclePlacementMode;
        public bool IsVehiclePlacementMode
        {
            get => _isVehiclePlacementMode;
            set => SetProperty(ref _isVehiclePlacementMode, value);
        }

        private RoadType _selectedRoadType = RoadType.Street;
        public RoadType SelectedRoadType
        {
            get => _selectedRoadType;
            set => SetProperty(ref _selectedRoadType, value);
        }

        private VehicleType _selectedVehicleType;
        public VehicleType SelectedVehicleType
        {
            get => _selectedVehicleType;
            set => SetProperty(ref _selectedVehicleType, value);
        }

        // Для рисования дорог
        private Point _roadStartPoint;
        private bool _isDrawingRoad;
        private Tile tile;

        public IEnumerable<Tile> TilesFlat
        {
            get
            {
                if (CurrentMap == null) yield break;

                for (int y = 0; y < CurrentMap.Height; y++)
                    for (int x = 0; x < CurrentMap.Width; x++)
                        yield return CurrentMap.Tiles[x, y];
            }
        }

        public MainViewModel()
        {
            _saveLoadService = new SaveLoadService();
            _natureManager = new NatureManager();

            InitializeCategories();

            SelectedCategoryName = "Коммерция";
            UpdateBuildingsDisplay("Коммерция");

            LoadStatic();

            _pathfindingService = new PathfindingService(CurrentMap);
            _policeService = new PoliceService(CurrentMap);

        }

        private void InitializeCategories()
        {
            _buildingCategories.Add("Производство", new List<BuildingUI>
            {
                new BuildingUI { Name = "Завод", Icon = "🏭", Category = "Производство" },
                new BuildingUI { Name = "Ферма", Icon = "🌾", Category = "Производство" },
                new BuildingUI { Name = "Шахта", Icon = "⛏️", Category = "Производство" }
            });

            _buildingCategories.Add("Коммерция", new List<BuildingUI>
            {
                new BuildingUI { Name = "Магазин", Icon = "🛍️", Category = "Коммерция" },
                new BuildingUI { Name = "Супермаркет", Icon = "🛒", Category = "Коммерция" },
                new BuildingUI { Name = "Аптека", Icon = "💊", Category = "Коммерция" },
                new BuildingUI { Name = "Кафе", Icon = "☕", Category = "Коммерция" },
                new BuildingUI { Name = "Ресторан", Icon = "🍴", Category = "Коммерция" },
                new BuildingUI { Name = "Заправка", Icon = "⛽", Category = "Коммерция" }
            });

            _buildingCategories.Add("Социум", new List<BuildingUI>
            {
                new BuildingUI { Name = "Школа", Icon = "🏫", Category = "Социум" },
                new BuildingUI { Name = "Больница", Icon = "🏥", Category = "Социум" },
                new BuildingUI { Name = "Парк", Icon = "🌳", Category = "Социум" },
                new BuildingUI { Name = "Полицейский участок", Icon = "🚓", Category = "Социум" }
            });

            _buildingCategories.Add("Транспорт", new List<BuildingUI>
            {
                new BuildingUI { Name = "Аэропорт", Icon = "✈️", Category = "Транспорт" },
                new BuildingUI { Name = "Ж/Д Вокзал", Icon = "🚉", Category = "Транспорт" },
                new BuildingUI { Name = "Такси", Icon = "🚕", Category = "Транспорт" },
                new BuildingUI { Name = "Грузовик", Icon = "🚚", Category = "Транспорт" },
                new BuildingUI { Name = "Полицейская машина", Icon = "🚔", Category = "Транспорт" }
            });

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

        [RelayCommand]
        private void SelectBuilding(BuildingUI building)
        {
            if (building != null)
            {
                SelectedBuilding = building;
                if (building.Category == "Дороги")
                {
                    // Режим строительства дорог
                    IsRoadPlacementMode = true;
                    IsBuildingMode = false;
                    IsVehiclePlacementMode = false;

                    SelectedRoadType = building.Name switch
                    {
                        "Грунтовая дорога" => RoadType.Dirt,
                        "Городская дорога" => RoadType.Street,
                        "Широкая дорога" => RoadType.Avenue,
                        "Скоростное шоссе" => RoadType.Highway,
                        _ => RoadType.Street
                    };

                    MessageBox.Show($"Режим строительства дорог: {building.Name}. Кликните и протяните для создания дороги.",
                                   "Режим строительства дорог",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Information);
                }
                else if (building.Category == "Транспорт")
                {
                    // Режим размещения транспорта
                    IsVehiclePlacementMode = true;
                    IsBuildingMode = false;
                    IsRoadPlacementMode = false;

                    SelectedVehicleType = building.Name switch
                    {
                        "Такси" => VehicleType.Taxi,
                        "Грузовик" => VehicleType.Truck,
                        "Полицейская машина" => VehicleType.PoliceCar,
                        "Личный автомобиль" => VehicleType.Car,
                        _ => VehicleType.Car
                    };

                    MessageBox.Show($"Режим размещения транспорта: {building.Name}. Кликните на дороге для размещения.",
                                   "Режим размещения транспорта",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Information);
                }
                else
                {
                    // Режим строительства зданий
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

        // Методы для работы с дорогами
        public void StartRoadDrawing(int x, int y)
        {
            if (IsRoadPlacementMode)
            {
                _roadStartPoint = new Point(x, y);
                _isDrawingRoad = true;
            }
        }

        public void EndRoadDrawing(int x, int y)
        {
            if (IsRoadPlacementMode && _isDrawingRoad)
            {
                var endPoint = new Point(x, y);

                // Создаем сегмент дороги (важно использовать целочисленный конструктор)
                var roadSegment = new RoadSegment((int)_roadStartPoint.X, (int)_roadStartPoint.Y, x, y, _selectedRoadType);

                // Пытаемся разместить дорогу
                if (TryPlaceRoad(roadSegment))
                {
                    MessageBox.Show($"Дорога успешно построена от ({_roadStartPoint.X},{_roadStartPoint.Y}) до ({x},{y})",
                                   "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                _isDrawingRoad = false;
            }
        }

        private bool TryPlaceRoad(RoadSegment roadSegment)
        {
            if (CurrentMap == null) return false;

            // Проверяем возможность строительства дороги
            if (CanBuildRoad(roadSegment))
            {
                // Добавляем дорогу на карту
                CurrentMap.RoadSegments.Add(roadSegment);

                // Обновляем тайлы, через которые проходит дорога
                UpdateTilesWithRoad(roadSegment);

                RefreshMap();
                return true;
            }

            MessageBox.Show("Невозможно построить дорогу здесь!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        private bool CanBuildRoad(RoadSegment roadSegment)
        {
            // Проверяем, что координаты в пределах карты
            if (roadSegment.StartX < 0 || roadSegment.StartX >= CurrentMap.Width ||
                roadSegment.StartY < 0 || roadSegment.StartY >= CurrentMap.Height ||
                roadSegment.EndX < 0 || roadSegment.EndX >= CurrentMap.Width ||
                roadSegment.EndY < 0 || roadSegment.EndY >= CurrentMap.Height)
            {
                return false;
            }

            // Проверяем, что не строим через здания
            var points = GetPointsAlongSegment(roadSegment);
            foreach (var point in points)
            {
                var tile = CurrentMap.Tiles[(int)point.X, (int)point.Y];
                if (tile.Building != null && !(tile.Building is Road))
                {
                    return false;
                }
            }

            return true;
        }

        private void UpdateTilesWithRoad(RoadSegment roadSegment)
        {
            var points = GetPointsAlongSegment(roadSegment);
            foreach (var point in points)
            {
                if (point.X >= 0 && point.X < CurrentMap.Width && point.Y >= 0 && point.Y < CurrentMap.Height)
                {
                    CurrentMap.Tiles[(int)point.X, (int)point.Y].HasRoad = true;
                    CurrentMap.Tiles[(int)point.X, (int)point.Y].RoadType = roadSegment.RoadType;
                }
            }
        }

        private List<Point> GetPointsAlongSegment(RoadSegment segment)
        {
            var points = new List<Point>();
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

        private Transport CreateVehicle(VehicleType vehicleType, int x, int y)
        {
            return vehicleType switch
            {
                VehicleType.Taxi => new Taxi(x, y, CurrentMap),
                VehicleType.Truck => new Truck(x, y, CurrentMap),
                VehicleType.PoliceCar => new PoliceCar(x, y, CurrentMap, null),
                VehicleType.Car => new Car(x, y, CurrentMap),
                _ => new Car(x, y, CurrentMap)
            };
        }

        public bool TryPlaceBuilding(int x, int y)
        {
            if (!IsBuildingMode || SelectedBuilding == null || CurrentMap == null)
            {
                return false;
            }

            var realBuilding = CreateRealBuilding(SelectedBuilding);

            bool canPlace = realBuilding.CanPlace(x, y, CurrentMap);

            if (realBuilding != null && canPlace)
            {
                bool placementResult = realBuilding.TryPlace(x, y, CurrentMap);

                if (placementResult)
                {
                    CurrentMap.Buildings.Add(realBuilding);
                    RefreshMap();
                    CancelBuilding();

                    MessageBox.Show($"Здание '{realBuilding.Name}' успешно размещено!",
                                   "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    return true;
                }
            }

            MessageBox.Show("Невозможно разместить здание здесь!\nПроверьте:\n• Достаточно ли места\n• Подходящий ли рельеф\n• Нет ли других зданий",
                           "Ошибка размещения",
                           MessageBoxButton.OK,
                           MessageBoxImage.Warning);
            return false;
        }

        private Core.Models.Base.Building CreateRealBuilding(BuildingUI uiBuilding)
        {
            return uiBuilding.Name switch
            {
                "Магазин" => new Shop(),
                "Супермаркет" => new Supermarket(),
                "Аптека" => new Pharmacy(),
                "Кафе" => new Cafe(),
                "Ресторан" => new Restaurant(),
                "Заправка" => new GasStation(),
                "Парк" => new Park(),
                "Полицейский участок" => new PoliceStation(),
                _ => new Shop()
            };
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
        private void LoadStatic()
        {
            CurrentMap = StaticBigMapProvider.Build50();
        }

        [RelayCommand]
        private void SaveMap()
        {
            if (CurrentMap != null)
                _saveLoadService.SaveMap(CurrentMap, "saved_map.json");
        }

        [RelayCommand]
        private void LoadMap()
        {
            CurrentMap = _saveLoadService.LoadMap("saved_map.json");
        }

        [RelayCommand]
        private void ShowTileInfo(Tile tile)
        {
            if (tile == null)
                return;

            var sb = new StringBuilder();
            sb.AppendLine($"Координаты: ({tile.X}; {tile.Y})");
            sb.AppendLine($"Рельеф: {tile.Terrain}");

            // Smirnov MA - ИНФОРМАЦИЯ О ИНФРАСТРУКТУРЕ
            sb.Append("Инфраструктура: ");
            var infrastructure = new List<string>();
            if (tile.HasPark) infrastructure.Add("Парк");
            if (tile.HasBikeLane) infrastructure.Add("Велодорожка");
            if (tile.HasPedestrianPath) infrastructure.Add("Пешеходная дорожка");

            if (infrastructure.Count > 0)
                sb.AppendLine(string.Join(", ", infrastructure));
            else
                sb.AppendLine("нет");

            // Дороги
            sb.Append("Дорога: ");
            if (tile.HasRoad)
                sb.AppendLine($"{tile.RoadType}");
            else
                sb.AppendLine("нет");

            if (tile.Building != null)
            {
                sb.AppendLine($"Здание: {tile.Building.Name}");

                // SmirnovMA ОСОБАЯ ИНФОРМАЦИЯ ДЛЯ ПАРКА
                if (tile.Building is Park park)
                {
                    sb.AppendLine($"--- Детали парка ---");
                    sb.AppendLine($"Деревья в парке: {park.TreeCount} шт.");
                    sb.AppendLine($"Скамейки: {park.BenchCount} шт.");
                    sb.AppendLine($"Вместимость: {park.MaxOccupancy} человек");
                    sb.AppendLine($"Размер: {park.Width}x{park.Height}");
                }

                // ИНФОРМАЦИЯ ДЛЯ КОММЕРЧЕСКИХ ЗДАНИЙ
                else if (tile.Building is CommercialBuilding commercial)
                {
                    sb.AppendLine($"--- Детали {commercial.Name} ---");
                    sb.AppendLine($"Тип: {commercial.Type}");
                    sb.AppendLine($"Вместимость: {commercial.Capacity} человек");
                    sb.AppendLine($"Сотрудники: {commercial.EmployeeCount} чел.");
                    sb.AppendLine($"Размер: {commercial.Width}x{commercial.Height}");
                    sb.AppendLine($"Этажи: {commercial.Floors}");

                    // ЖКХ информация
                    sb.AppendLine($"Коммуникации: {(commercial.IsOperational ? "✅ Все подключены" : "❌ Не все подключены")}");
                    if (!commercial.IsOperational)
                    {
                        var missingUtils = new List<string>();
                        if (!commercial.HasWater) missingUtils.Add("Вода");
                        if (!commercial.HasGas) missingUtils.Add("Газ");
                        if (!commercial.HasSewage) missingUtils.Add("Канализация");
                        if (!commercial.HasElectricity) missingUtils.Add("Электричество");
                        sb.AppendLine($"Отсутствуют: {string.Join(", ", missingUtils)}");
                    }

                    // Категории товаров
                    if (commercial.ProductCategories?.Count > 0)
                    {
                        sb.AppendLine($"Категории товаров:");
                        foreach (var category in commercial.ProductCategories)
                        {
                            sb.AppendLine($" • {category}");
                        }
                    }
                }

                // ОБЩАЯ ИНФОРМАЦИЯ ДЛЯ ЛЮБОГО ЗДАНИЯ
                else
                {
                    sb.AppendLine($"--- Общая информация ---");
                    sb.AppendLine($"Размер: {tile.Building.Width}x{tile.Building.Height}");
                    sb.AppendLine($"Этажи: {tile.Building.Floors}");
                    sb.AppendLine($"Вместимость: {tile.Building.MaxOccupancy} человек");
                    sb.AppendLine($"Текущая заполненность: {tile.Building.CurrentOccupancy} человек");
                    sb.AppendLine($"Состояние: {tile.Building.Condition}%");
                }
            }

            if (tile.TreeType.HasValue && tile.TreeCount > 0)
            {
                sb.AppendLine($"Деревья: {tile.TreeType.Value} ({tile.TreeCount} шт.)");
            }
            else
            {
                sb.AppendLine("Деревья: нет");
            }

            if (tile.Resources is { Count: > 0 })
            {
                sb.AppendLine("Ресурсы:");
                foreach (var r in tile.Resources)
                    sb.AppendLine($" • {r.Type} — {r.Amount}");
            }
            else
            {
                sb.AppendLine("Ресурсы: нет");
            }

            MessageBox.Show(
                sb.ToString(),
                "Информация о клетке",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        [RelayCommand]
        private void ShowTreeStatistics()
        {
            if (CurrentMap == null)
                return;

            var statistics = _natureManager.GetTreeTypeStatistics(CurrentMap);
            var totalTrees = _natureManager.GetTotalTreeCount(CurrentMap);
            var tilesWithTrees = _natureManager.GetTilesWithTreesCount(CurrentMap);

            var sb = new StringBuilder();
            sb.AppendLine($"Общая статистика деревьев:");
            sb.AppendLine($"Всего деревьев: {totalTrees}");
            sb.AppendLine($"Тайлов с деревьями: {tilesWithTrees}");
            sb.AppendLine();

            sb.AppendLine("По типам деревьев:");
            foreach (var stat in statistics)
            {
                sb.AppendLine($" • {stat.Key}: {stat.Value} шт.");
            }

            MessageBox.Show(
                sb.ToString(),
                "Статистика деревьев",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        /// <summary>
        /// Размещение дороги
        /// </summary>
        private void PlaceRoad(int x, int y)
        {
            if (x < 0 || x >= CurrentMap.Width || y < 0 || y >= CurrentMap.Height)
                return;

            // Создаём сегмент дороги
            var segment = new RoadSegment(x, y, x, y, SelectedRoadType);
            CurrentMap.AddRoadSegment(segment);

            // Обновляем отображение
            OnPropertyChanged(nameof(TilesFlat));
        }

        /// <summary>
        /// Унифицированная попытка размещения выбранного элемента (здание/дорога/перекрёсток)
        /// </summary>
        public void TryPlaceSelected(int x, int y)
        {
            if (SelectedBuilding == null || CurrentMap == null)
                return;

            if (IsRoadPlacementMode || SelectedBuilding.Category == "Дороги")
            {
                if (SelectedBuilding.Name == "Перекрёсток")
                {
                    PlaceIntersection(x, y);
                }
                else
                {
                    PlaceRoad(x, y);
                }
                UpdateStatistics();
                return;
            }

            if (IsVehiclePlacementMode || SelectedBuilding.Category == "Транспорт")
            {
                TryPlaceVehicle(x, y);
                return;
            }

            // Обычное здание
            TryPlaceBuilding(x, y);
        }

        /// <summary>
        /// Размещение транспорта на дороге
        /// </summary>
        public void TryPlaceVehicle(int x, int y)
        {
            if (CurrentMap == null) return;
            if (!IsVehiclePlacementMode && (SelectedBuilding == null || SelectedBuilding.Category != "Транспорт")) return;

            var tile = CurrentMap.Tiles[x, y];
            if (!tile.HasRoad)
            {
                MessageBox.Show("Транспорт можно размещать только на дороге", "Размещение транспорта", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var vehicle = CreateVehicle(SelectedVehicleType, x, y);

            // Пиктограмма транспорта по типу
            string icon = SelectedVehicleType switch
            {
                VehicleType.Taxi => "🚕",
                VehicleType.Truck => "🚚",
                VehicleType.PoliceCar => "🚓",
                VehicleType.Bus => "🚌",
                VehicleType.Car => "🚗",
                _ => "🚗"
            };
            tile.VehicleIcons.Add(icon);

            // Простейший учет: увеличиваем счетчик на тайле
            tile.VehicleCount += 1;
            tile.HasVehicle = tile.VehicleCount > 0;
            RefreshMap();
        }

        /// <summary>
        /// Размещение перекрёстка
        /// </summary>
        private void PlaceIntersection(int x, int y)
        {
            var intersection = new Intersection(x, y, true); // Со светофором
            CurrentMap.AddIntersection(intersection);
            // Помечаем только одну клетку как перекрёсток и дорогу выбранного типа
            var tile = CurrentMap.Tiles[x, y];
            tile.HasRoad = true;
            tile.RoadType = SelectedRoadType;
            tile.HasIntersection = true;

            RefreshMap();
        }

        /// <summary>
        /// Обновление статистики
        /// </summary>
        private void UpdateStatistics()
        {
            if (CurrentMap == null)
                return;

            var sb = new StringBuilder();
            sb.AppendLine($"Дорог: {CurrentMap.RoadSegments.Count}");
            sb.AppendLine($"Перекрёстков: {CurrentMap.Intersections.Count}");

            var policeStations = CurrentMap.Buildings.OfType<PoliceStation>().Count();
            sb.AppendLine($"Полицейских участков: {policeStations}");

            if (_policeService != null)
            {
                sb.AppendLine($"Активных преступлений: {_policeService.GetActiveCrimeCount()}");
                sb.AppendLine($"Раскрыто: {_policeService.TotalCrimesSolved}");
            }

            sb.ToString();
        }

        /// <summary>
        /// Создать тестовое преступление
        /// </summary>
        [RelayCommand]
        private void CreateTestCrime()
        {
            if (_policeService != null && CurrentMap != null)
            {
                var random = new Random();
                int x = random.Next(0, CurrentMap.Width);
                int y = random.Next(0, CurrentMap.Height);

                _policeService.CreateCrime(CrimeType.Theft, x, y);
                UpdateStatistics();

                MessageBox.Show($"Преступление создано на ({x}, {y})", "Тест");
            }
        }

        public void RefreshMap()
        {
            OnPropertyChanged(nameof(TilesFlat));
        }
    }
}