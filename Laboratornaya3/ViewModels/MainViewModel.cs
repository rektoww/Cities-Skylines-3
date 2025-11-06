using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Models.Base;
using Core.Models.Buildings.CommertialBuildings;
using Core.Models.Map;
using Core.Services;
using Infrastructure.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;

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
                new BuildingUI { Name = "Кафе", Icon = "☕", Category = "Коммерция" },
                new BuildingUI { Name = "Ресторан", Icon = "🍴", Category = "Коммерция" },
                new BuildingUI { Name = "Заправка", Icon = "⛽", Category = "Коммерция" }
            });

            _buildingCategories.Add("Социум", new List<BuildingUI>
            {
                new BuildingUI { Name = "Школа", Icon = "🏫", Category = "Социум" },
                new BuildingUI { Name = "Больница", Icon = "🏥", Category = "Социум" },
                new BuildingUI { Name = "Парк", Icon = "🌳", Category = "Социум" }
            });

            _buildingCategories.Add("Транспорт", new List<BuildingUI>
            {
                new BuildingUI { Name = "Аэропорт", Icon = "✈️", Category = "Транспорт" },
                new BuildingUI { Name = "Ж/Д Вокзал", Icon = "🚉", Category = "Транспорт" }
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
                IsBuildingMode = true;

                MessageBox.Show($"Выбрано: {building.Name}. Кликните на карте для размещения.",
                               "Режим строительства",
                               MessageBoxButton.OK,
                               MessageBoxImage.Information);
            }
        }

        [RelayCommand]
        private void CancelBuilding()
        {
            IsBuildingMode = false;
            SelectedBuilding = null;
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
                "Кафе" => new Cafe(),
                "Ресторан" => new Restaurant(),
                "Заправка" => new GasStation(),
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

            if (tile.Building != null)
                sb.AppendLine($"Здание: {tile.Building.Name}");

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

        public void RefreshMap()
        {
            OnPropertyChanged(nameof(TilesFlat));
        }
    }
}