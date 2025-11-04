using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Models.Map;
using Core.Services; // Добавляем для NatureManager
using Infrastructure.Services; // Содержит StaticMapProvider и SaveLoadService
using Laboratornaya3.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq; // Добавляем для работы с деревьями
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Laboratornaya3.ViewModels
{
    // Штука для хранения название-иконка-категория для UI 
    public class Building // TODO: Подумть над связкой с беком
    {
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Category { get; set; }
    }

    public class TileViewModel : ViewModelBase // Наследуем от ViewModelBase!
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Coordinates => $"({X}, {Y})";

        private string _backgroundColor;
        public string BackgroundColor
        {
            get => _backgroundColor;
            set => SetProperty(ref _backgroundColor, value);
        }
    }

    /// <summary>
    /// Главная ViewModel приложения.
    /// Отвечает за загрузку/сохранение статичной карты, предоставление данных для отрисовки
    /// и показ информации о клетке по клику.
    /// </summary>
    public partial class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Сервис сохранения/загрузки карты в/из JSON.
        /// Хранится один инстанс на всю VM.
        /// </summary>
        private readonly SaveLoadService _saveLoadService;

        /// <summary>
        /// Менеджер для работы с природными объектами
        /// </summary>
        private readonly NatureManager _natureManager;

        /// <summary>
        /// Текущая карта (модель уровня Core). Не должна быть null при нормальной работе,
        /// однако поле объявлено с «null!» для совместимости с генератором кода CommunityToolkit
        /// и дальнейшей инициализации в конструкторе через вызов <see cref="LoadStatic"/>.
        /// </summary>
        private GameMap _currentMap = null!;

        /// <summary>
        /// Текущая игровая карта, которую видит UI.
        /// При присвоении поднимает уведомление также для <see cref="TilesFlat"/>,
        /// чтобы перерисовать сетку.
        /// </summary>
        public GameMap CurrentMap
        {
            get => _currentMap;
            set
            {
                // SetProperty поднимет PropertyChanged только если значение изменилось
                SetProperty(ref _currentMap, value);

                // Очень важно: ItemsControl привязан к TilesFlat,
                // поэтому после смены CurrentMap сообщаем, что TilesFlat тоже «изменился».
                OnPropertyChanged(nameof(TilesFlat));
            }
        }

        // Для активной категории
        private string _selectedCategoryName;
        public string SelectedCategoryName
        {
            get => _selectedCategoryName;
            set => SetProperty(ref _selectedCategoryName, value);
        }

        // Для списка зданий в нижней панели
        private ObservableCollection<Building> _visibleBuildings;
        public ObservableCollection<Building> VisibleBuildings
        {
            get => _visibleBuildings;
            set => SetProperty(ref _visibleBuildings, value);
        }

        // Словарь для хранения всех данных
        private readonly Dictionary<string, List<Building>> _buildingCategories = new Dictionary<string, List<Building>>();

        // 2. Команды для обработки действий
        public ICommand SelectCategoryCommand { get; }

        /// <summary>
        /// Плоское перечисление всех тайлов карты (для WPF ItemsControl).
        /// Порядок: построчно — сначала Y от 0 до Height-1, внутри каждой строки X от 0 до Width-1.
        /// Это соответствует тому, как <c>UniformGrid</c> раскладывает элементы слева направо и сверху вниз.
        /// </summary>
        public IEnumerable<Tile> TilesFlat
        {
            get
            {
                if (CurrentMap == null) yield break;

                // Внимание: карта хранится как двумерный массив Tile[,],
                // поэтому обращаемся через индексы [x, y].
                for (int y = 0; y < CurrentMap.Height; y++)
                    for (int x = 0; x < CurrentMap.Width; x++)
                        yield return CurrentMap.Tiles[x, y];
            }
        }

        /// <summary>
        /// Конструктор VM.
        /// Инициализирует сервисы и сразу поднимает статичную карту,
        /// чтобы окно при старте уже отображало содержимое.
        /// </summary>
        public MainViewModel()
        {
            _saveLoadService = new SaveLoadService();
            _natureManager = new NatureManager(); // Инициализируем менеджер природы

            InitializeCategories();

            // Инициализация команды
            SelectCategoryCommand = new RelayCommand(SelectCategory);

            // Установка категории по умолчанию
            SelectedCategoryName = "Коммерция";
            UpdateBuildingsDisplay("Коммерция");

            LoadStatic();
        }

        // !!! НОВЫЙ МЕТОД: Заполнение данных категорий
        private void InitializeCategories()
        {
            _buildingCategories.Add("Производство", new List<Building>
            {
                new Building { Name = "Завод", Icon = "🏭" },
                new Building { Name = "Ферма", Icon = "🌾" },
                new Building { Name = "Шахта", Icon = "⛏️" }
            });

            _buildingCategories.Add("Коммерция", new List<Building>
            {
                new Building { Name = "Магазин", Icon = "🛍️" },
                new Building { Name = "Кафе", Icon = "☕" },
                new Building { Name = "Ресторан", Icon = "🍴" },
                new Building { Name = "Заправка", Icon = "⛽" }
            });

            _buildingCategories.Add("Социум", new List<Building>
            {
                new Building { Name = "Школа", Icon = "🏫" },
                new Building { Name = "Больница", Icon = "🏥" },
                new Building { Name = "Парк", Icon = "🌳" }
            });

            _buildingCategories.Add("Транспорт", new List<Building>
            {
                new Building { Name = "Аэропорт", Icon = "✈️" },
                new Building { Name = "Ж/Д Вокзал", Icon = "🚉" }
            });
        }

        // Метод, который будет вызываться командой
        private void SelectCategory(object parameter)
        {
            if (parameter is string categoryName)
            {
                SelectedCategoryName = categoryName;
                UpdateBuildingsDisplay(categoryName);
            }
        }

        // Логика обновления списка зданий
        private void UpdateBuildingsDisplay(string categoryName)
        {
            if (_buildingCategories.TryGetValue(categoryName, out var buildings))
            {
                VisibleBuildings = new ObservableCollection<Building>(buildings);
            }
            else
            {
                VisibleBuildings = new ObservableCollection<Building>();
            }
        }

        /// <summary>
        /// Команда: загрузить статичную карту из <see cref="StaticMapProvider"/>.
        /// Используется для первоначальной инициализации и по кнопке «Загрузить статичную карту».
        /// </summary>
        [RelayCommand]
        private void LoadStatic()
        {
            // Строим новый объект карты и публикуем его в CurrentMap.
            // SetProperty + OnPropertyChanged(nameof(TilesFlat)) в сеттере CurrentMap
            // обеспечат обновление UI.
            CurrentMap = StaticMapProvider.Build();
        }

        /// <summary>
        /// Команда: сохранить карту в JSON-файл.
        /// Файл сохраняется по относительному пути "saved_map.json" рядом с исполняемым файлом.
        /// </summary>
        [RelayCommand]
        private void SaveMap()
        {
            // На всякий случай проверяем, что карта есть.
            if (CurrentMap != null)
                _saveLoadService.SaveMap(CurrentMap, "saved_map.json");
        }

        /// <summary>
        /// Команда: загрузить карту из JSON-файла "saved_map.json".
        /// Перезаписывает <see cref="CurrentMap"/>, тем самым инициируя перерисовку сетки.
        /// </summary>
        [RelayCommand]
        private void LoadMap()
        {
            CurrentMap = _saveLoadService.LoadMap("saved_map.json");
        }

        /// <summary>
        /// Команда: показать информацию по выбранной клетке (тайлу) во всплывающем окне.
        /// Используется по клику на клетку — сам <see cref="Tile"/> передаётся в <c>CommandParameter</c>.
        /// </summary>
        /// <param name="tile">Тайл карты, по которому кликнули. Может быть null при ошибочном вызове.</param>
        [RelayCommand]
        private void ShowTileInfo(Tile tile)
        {
            if (tile == null)
                return;

            // Формируем понятный для пользователя текст:
            // координаты, рельеф, перечень ресурсов (или сообщение «нет»),
            // а также (опционально) сведения о здании, если оно присутствует.
            var sb = new StringBuilder();
            sb.AppendLine($"Координаты: ({tile.X}; {tile.Y})");
            sb.AppendLine($"Рельеф: {tile.Terrain}");

            // НОВЫЙ КОД ДЛЯ ОТОБРАЖЕНИЯ ДЕРЕВЬЕВ
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

            // Если в модели у клетки есть привязанное здание — тоже покажем.
            if (tile.Building != null)
                sb.AppendLine($"Здание: {tile.Building.Name}");

            // Простое системное диалоговое окно — быстро и наглядно.
            MessageBox.Show(
                sb.ToString(),
                "Информация о клетке",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        /// <summary>
        /// Команда: показать статистику по деревьям на карте
        /// </summary>
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
    }
}