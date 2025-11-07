using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Models.Base;
using Core.Models.Buildings;
using Core.Models.Buildings.CommertialBuildings;
using Core.Models.Buildings.SocialBuildings;
using Core.Models.Buildings.IndustrialBuildings;
using Core.Models.Map;
using Core.Services;
using Core.Resourses;
using Core.Enums;
using Core.Config;
using Infrastructure.Services;
using Core.Models.Mobs;
using Core.Models.Roads;
using Core.Models.Police;
using Core.Models.Vehicles;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    }

    public partial class MainViewModel : ObservableObject
    {
        private readonly SaveLoadService _saveLoadService;
        private readonly NatureManager _natureManager;
        private PathfindingService _pathfindingService;
        private PoliceService _policeService;

        // Экономика/ресурсы
        private readonly FinancialSystem _financialSystem;
        private readonly PlayerResources _playerResources;
        private readonly ExternalConnectionsManager _externalConnections;
        private readonly ConstructionCompany _constructionCompany;
        private readonly MarketService _marketService;
        private readonly ResourceProductionService _productionService;

        [ObservableProperty]
        private GameMap _currentMap;

        /// <summary>
        /// Бюджет города для отображения в UI
        /// </summary>
        public decimal CityBudget => _financialSystem?.CityBudget ?? 0m;

        /// <summary>
        /// Количество граждан для отображения в UI
        /// </summary>
        [ObservableProperty]
        private int _citizenCount;

        /// <summary>
        /// Список граждан города
        /// </summary>
        private List<Citizen> _citizens = new();

        /// <summary>
        /// Список жилых зданий
        /// </summary>
        private List<ResidentialBuilding> _residentialBuildings = new();

        /// <summary>
        /// Таймер для автоматической миграции
        /// </summary>
        private DispatcherTimer? _migrationTimer;
        private readonly Random _random = new Random();

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

            // Инициализация экономики и ресурсов 
            _financialSystem = new FinancialSystem(initialBudget: EconomyConfig.DefaultCityBudget);
            _playerResources = new PlayerResources(
                balance: EconomyConfig.DefaultCityBudget,
                materials: new Dictionary<ConstructionMaterial, int>(EconomyConfig.DefaultStartMaterials)
            );
            
            // Система внешних связей (импорт/экспорт)
            _externalConnections = new ExternalConnectionsManager(_playerResources, _financialSystem);
            
            _constructionCompany = new ConstructionCompany(_playerResources, _financialSystem);
            _marketService = new MarketService(_externalConnections);
            _productionService = new ResourceProductionService(_playerResources, _externalConnections);

            InitializeCategories();

            SelectedCategoryName = "Коммерция";
            UpdateBuildingsDisplay("Коммерция");

            LoadStatic();

            // Инициализация таймера миграции
            InitializeMigrationTimer();

            // Инициализация дорог и полиции
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
                new BuildingUI { Name = "Больница", Icon = "🏭", Category = "Социум" },
                new BuildingUI { Name = "Парк", Icon = "🌳", Category = "Социум" },
                new BuildingUI { Name = "Полицейский участок", Icon = "🚓", Category = "Социум" }
            });

            _buildingCategories.Add("Транспорт", new List<BuildingUI>
            {
                new BuildingUI { Name = "Аэропорт", Icon = "✈️", Category = "Транспорт" },
                new BuildingUI { Name = "Морской порт", Icon = "⚓", Category = "Транспорт" },
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
                else if (building.Category == "Транспорт" && (building.Name == "Такси" || building.Name == "Грузовик" || building.Name == "Полицейская машина"))
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

        public bool TryPlaceBuilding(int x, int y)
        {
            if (!IsBuildingMode || SelectedBuilding == null || CurrentMap == null)
            {
                return false;
            }

            // Строим через ConstructionCompany, чтобы списались деньги и материалы
            bool built = false;
            Core.Models.Base.Building builtBuilding = null;

            switch (SelectedBuilding.Name)
            {
                case "Парк":
                    built = _constructionCompany.TryBuild<Park>(x, y, CurrentMap, new object[] { }, out var park);
                    builtBuilding = park as Core.Models.Base.Building;
                    break;
                case "Магазин":
                    built = _constructionCompany.TryBuild<Shop>(x, y, CurrentMap, new object[] { }, out var shop);
                    builtBuilding = shop as Core.Models.Base.Building;
                    break;
                case "Супермаркет":
                    built = _constructionCompany.TryBuild<Supermarket>(x, y, CurrentMap, new object[] { }, out var supermarket);
                    builtBuilding = supermarket as Core.Models.Base.Building;
                    break;
                case "Аптека":
                    built = _constructionCompany.TryBuild<Pharmacy>(x, y, CurrentMap, new object[] { }, out var pharmacy);
                    builtBuilding = pharmacy as Core.Models.Base.Building;
                    break;
                case "Кафе":
                    built = _constructionCompany.TryBuild<Cafe>(x, y, CurrentMap, new object[] { }, out var cafe);
                    builtBuilding = cafe as Core.Models.Base.Building;
                    break;
                case "Ресторан":
                    built = _constructionCompany.TryBuild<Restaurant>(x, y, CurrentMap, new object[] { }, out var restaurant);
                    builtBuilding = restaurant as Core.Models.Base.Building;
                    break;
                case "Заправка":
                    built = _constructionCompany.TryBuild<GasStation>(x, y, CurrentMap, new object[] { }, out var gasStation);
                    builtBuilding = gasStation as Core.Models.Base.Building;
                    break;
                case "Шахта":
                    built = _constructionCompany.TryBuild<Mine>(x, y, CurrentMap, new object[] { }, out var mine);
                    builtBuilding = mine as Core.Models.Base.Building;
                    break;
                default:
                    // Фолбэк: старое поведение для несопровождаемых типов
                    var realBuilding = CreateRealBuilding(SelectedBuilding);
                    if (realBuilding != null && realBuilding.CanPlace(x, y, CurrentMap) && realBuilding.TryPlace(x, y, CurrentMap))
                    {
                        built = true;
                        builtBuilding = realBuilding;
                    }
                    break;
            }

            if (built && builtBuilding != null)
            {
                CurrentMap.Buildings.Add(builtBuilding);
                RefreshMap();
                CancelBuilding();

                // Обновляем бюджет в UI
                OnPropertyChanged(nameof(CityBudget));

                MessageBox.Show($"Здание '{builtBuilding.Name}' успешно построено!\n" +
                                $"Бюджет: {_financialSystem.CityBudget:N0} | Баланс игрока: {_playerResources.Balance:N0}",
                                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                return true;
            }

            MessageBox.Show("Нельзя построить здесь. Возможные причины:\n• Недостаточно бюджета или материалов\n• Недостаточно места\n• Неподходящий рельеф\n• Место занято",
                           "Ошибка строительства",
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
                "Шахта" => new Mine(),
                "Полицейский участок" => new PoliceStation(),
                "Аэропорт" => new AirPort(new Core.Resourses.PlayerResources(0m, new Dictionary<Core.Enums.ConstructionMaterial, int>())),
                "Морской порт" => new SeaPort(new Core.Resourses.PlayerResources(0m, new Dictionary<Core.Enums.ConstructionMaterial, int>())),
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

                // ИНФОРМАЦИЯ ДЛЯ ШАХТЫ
                else if (tile.Building is Mine mine)
                {
                    sb.AppendLine($"--- Детали шахты ---");
                    sb.AppendLine($"Добывает: {mine.ProducedMaterial}");
                    sb.AppendLine($"Накоплено: {mine.StoredResources}/{mine.MaxStorage}");
                    sb.AppendLine($"Скорость: {mine.ProductionRate} ед./тик");
                    sb.AppendLine($"Размер: {mine.Width}x{mine.Height}");

                    MessageBox.Show(
                        sb.ToString(),
                        "Информация о клетке",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    // Предложить собрать ресурсы
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
                            if (_playerResources.StoredMaterials.ContainsKey(mine.ProducedMaterial))
                                _playerResources.StoredMaterials[mine.ProducedMaterial] += collected;
                            else
                                _playerResources.StoredMaterials[mine.ProducedMaterial] = collected;

                            RefreshMap();
                            MessageBox.Show(
                                $"Собрано: {collected} ед. {mine.ProducedMaterial}",
                                "Успех",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                        }
                    }
                    return; // Early return after handling mine
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

        [RelayCommand]
        private void ShowResourcesInfo()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Инвентарь строительных материалов:");
            sb.AppendLine();
            foreach (var mat in _playerResources.StoredMaterials)
            {
                sb.AppendLine($" • {mat.Key}: {mat.Value} шт.");
            }
            sb.AppendLine();
            sb.AppendLine($"Баланс игрока: {_playerResources.Balance:N0} валюты");

            MessageBox.Show(
                sb.ToString(),
                "Ресурсы",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        [RelayCommand]
        private void ShowFinanceInfo()
        {
            var report = _financialSystem.GetFinancialReport();
            var sb = new StringBuilder();
            sb.AppendLine("Финансовый отчет города:");
            sb.AppendLine();
            sb.AppendLine($"Бюджет: {report.CurrentBudget:N0} валюты");
            sb.AppendLine($"Доходы: {report.TotalIncome:N0} валюты");
            sb.AppendLine($"Расходы: {report.TotalExpenses:N0} валюты");
            sb.AppendLine($"Чистый баланс за период: {report.PeriodBalance:N0} валюты");
            sb.AppendLine();

            if (report.ExpensesByCategory.Count > 0)
            {
                sb.AppendLine("Расходы по категориям:");
                foreach (var exp in report.ExpensesByCategory)
                {
                    if (exp.Value > 0)
                        sb.AppendLine($" • {exp.Key}: {exp.Value:N0}");
                }
            }

            if (report.IncomesBySource.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Доходы по источникам:");
                foreach (var inc in report.IncomesBySource)
                {
                    if (inc.Value > 0)
                        sb.AppendLine($" • {inc.Key}: {inc.Value:N0}");
                }
            }

            MessageBox.Show(
                sb.ToString(),
                "Финансы",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        [RelayCommand]
        private void ProduceMines()
        {
            // Производство ресурсов на всех шахтах
            if (CurrentMap?.Buildings == null) return;

            int totalProduced = 0;
            foreach (var building in CurrentMap.Buildings.OfType<Mine>())
            {
                building.ProduceResources();
                totalProduced += building.ProductionRate;
            }

            RefreshMap();

            if (totalProduced > 0)
            {
                MessageBox.Show(
                    $"Шахты произвели ресурсы!\nКликните по шахте чтобы собрать.",
                    "Производство",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        [RelayCommand]
        private void SellMaterials()
        {
            try
            {
                var dialog = new Views.SellMaterialsDialog(_playerResources, _productionService);
                
                // Пытаемся найти главное окно
                var mainWindow = Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive) 
                               ?? Application.Current?.MainWindow;
                
                if (mainWindow != null)
                {
                    dialog.Owner = mainWindow;
                }
                
                if (dialog.ShowDialog() == true && dialog.SoldSuccessfully)
                {
                    OnPropertyChanged(nameof(CityBudget));
                    MessageBox.Show(
                        $"Продано материалов на {dialog.TotalRevenue:N0} валюты!\nНовый бюджет: {_financialSystem.CityBudget:N0}",
                        "Успех",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при открытии диалога продажи:\n{ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void BuyMaterials()
        {
            try
            {
                var dialog = new Views.BuyMaterialsDialog(_marketService, _financialSystem, _playerResources);
                
                // Пытаемся найти главное окно
                var mainWindow = Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive) 
                               ?? Application.Current?.MainWindow;
                
                if (mainWindow != null)
                {
                    dialog.Owner = mainWindow;
                }
                
                if (dialog.ShowDialog() == true)
                {
                    OnPropertyChanged(nameof(CityBudget));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при открытии диалога покупки:\n{ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Инициализация таймера для автоматической миграции
        /// </summary>
        private void InitializeMigrationTimer()
        {
            _migrationTimer = new DispatcherTimer();
            _migrationTimer.Tick += MigrationTimer_Tick;
            SetRandomMigrationInterval();
            _migrationTimer.Start();
        }

        /// <summary>
        /// Установка случайного интервала от 10 до 60 секунд
        /// </summary>
        private void SetRandomMigrationInterval()
        {
            if (_migrationTimer == null) return;
            
            int seconds = _random.Next(10, 61); // от 10 до 60 секунд
            _migrationTimer.Interval = TimeSpan.FromSeconds(seconds);
        }

        /// <summary>
        /// Обработчик таймера миграции
        /// </summary>
        private void MigrationTimer_Tick(object? sender, EventArgs e)
        {
            // Проверяем наличие транспортного здания
            if (HasTransportBuilding())
            {
                // Добавляем 5-10 иммигрантов
                int count = _random.Next(5, 11);
                int added = _externalConnections.AddImmigrants(_citizens, _residentialBuildings, count);
                CitizenCount = _citizens.Count;
            }

            // Устанавливаем новый случайный интервал
            SetRandomMigrationInterval();
        }

        /// <summary>
        /// Проверка наличия транспортного здания на карте (Аэропорт или Морской порт)
        /// </summary>
        private bool HasTransportBuilding()
        {
            if (CurrentMap?.Buildings == null)
                return false;

            return CurrentMap.Buildings.Any(b => 
                b.Name == "Аэропорт" || b.Name == "Морской порт");
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
