using Core.Models.Map;
using Laboratornaya3.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Laboratornaya3.Views.Components
{
    /// <summary>
    /// Логика взаимодействия для Map.xaml
    /// </summary>
    public partial class Map : UserControl
    {
        private MainViewModel ViewModel => DataContext as MainViewModel;

        private Point? _lastMousePosition;
        private bool _isDragging = false; // Флаг для отслеживания перетаскивания карты

        public Map()
        {
            InitializeComponent();
        }

        private void MapScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control) return;

            var scale = e.Delta > 0 ? 1.1 : 0.9;
            ScaleTransform.ScaleX *= scale;
            ScaleTransform.ScaleY *= scale;
            e.Handled = true;
        }

        private void MapScrollViewer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel == null) return;

            // Перетаскивание карты только по ПКМ
            if (e.ChangedButton == MouseButton.Right)
            {
                // Если событие уже обработано дочерним элементом (например, Tile_RightClick для размещения транспорта),
                // мы не начинаем перетаскивание.
                if (e.Handled) return;

                // Отмена режима постройки/размещения перед началом перетаскивания
                ViewModel.CancelBuildingCommand.Execute(null);

                // Запуск перетаскивания
                _lastMousePosition = e.GetPosition(this); // Получаем начальную позицию мыши
                _isDragging = true;
                MapScrollViewer.CaptureMouse(); // Захватываем мышь для корректного отслеживания
                MapScrollViewer.Cursor = Cursors.SizeAll;
                e.Handled = true;
            }
        }

        private void Tile_LeftClick(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel == null) return;

            // Самый надежный способ получить объект Tile
            var border = sender as Border;
            var tile = border?.DataContext as Tile;

            if (tile == null) return;

            // 1. Размещение дорог (Требование 2)
            if (ViewModel.IsRoadPlacementMode)
            {
                if (ViewModel.SelectedBuilding?.Name == "Перекрёсток")
                    ViewModel.TryPlaceSelected(tile.X, tile.Y);
                else
                    ViewModel.StartRoadDrawing(tile.X, tile.Y);
                e.Handled = true; // Останавливаем всплытие, чтобы не сработал MapScrollViewer_MouseDown
            }
            // 2. Размещение здания (Требование 1 и 3)
            else if (ViewModel.IsBuildingMode && ViewModel.SelectedBuilding != null)
            {
                ViewModel.TryPlaceBuilding(tile.X, tile.Y);
                e.Handled = true; // Останавливаем всплытие
            }
            // 3. Показ информации (если ни один режим не активен)
            else
            {
                ViewModel.ShowTileInfoCommand.Execute(tile);
                e.Handled = true; // Останавливаем всплытие
            }
        }

        private void Tile_RightClick(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel == null) return;

            // Получаем объект Tile
            var border = sender as Border;
            var tile = border?.DataContext as Tile;

            if (tile == null) return;

            // 1. Обработка режима размещения транспорта
            if (ViewModel.IsVehiclePlacementMode)
            {
                ViewModel.TryPlaceVehicle(tile.X, tile.Y);
                e.Handled = true; // Останавливаем всплытие. Это предотвратит начало перетаскивания.
            }
            // Если режим транспорта НЕ активен, e.Handled = false, и событие поднимается 
            // к MapScrollViewer_MouseDown, где запустится перетаскивание (Pan/Drag).
        }

        private void MapScrollViewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (ViewModel == null) return;

            // Управление курсором
            if (ViewModel.IsBuildingMode || ViewModel.IsRoadPlacementMode || ViewModel.IsVehiclePlacementMode)
            {
                MapScrollViewer.Cursor = Cursors.Cross;
            }
            // Логика перетаскивания
            else if (_isDragging && _lastMousePosition.HasValue)
            {
                HandleMapDragging(e);
            }
            else
            {
                MapScrollViewer.Cursor = Cursors.Arrow;
            }
        }

        private void HandleMapDragging(MouseEventArgs e)
        {
            Point currentPosition = e.GetPosition(this);
            Vector delta = currentPosition - _lastMousePosition.Value;

            TranslateTransform.X += delta.X;
            TranslateTransform.Y += delta.Y;

            _lastMousePosition = currentPosition;
            MapScrollViewer.Cursor = Cursors.SizeAll;
        }

        private void MapScrollViewer_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel?.IsRoadPlacementMode == true && e.ChangedButton == MouseButton.Left)
            {
                var tile = GetTileAtPosition(e.GetPosition(MapGrid));
                if (tile != null && ViewModel.SelectedBuilding?.Name != "Перекрёсток")
                {
                    ViewModel.EndRoadDrawing(tile.X, tile.Y);
                }
                e.Handled = true;
            }

            // Завершение перетаскивания по ПКМ
            if (_isDragging && e.ChangedButton == MouseButton.Right)
            {
                _isDragging = false;
                _lastMousePosition = null;
                MapScrollViewer.ReleaseMouseCapture();

                // Возвращаем правильный курсор
                MapScrollViewer.Cursor = ViewModel?.IsBuildingMode == true || ViewModel?.IsRoadPlacementMode == true || ViewModel?.IsVehiclePlacementMode == true
                    ? Cursors.Cross
                    : Cursors.Arrow;

                e.Handled = true;
            }
        }

        private Tile GetTileAtPosition(Point position)
        {
            try
            {
                var inverseTransform = new Matrix();
                inverseTransform.Scale(1 / ScaleTransform.ScaleX, 1 / ScaleTransform.ScaleY);
                inverseTransform.Translate(-TranslateTransform.X, -TranslateTransform.Y);

                var transformedPoint = inverseTransform.Transform(position);

                double tileSize = 50;
                int x = (int)(transformedPoint.X / tileSize);
                int y = (int)(transformedPoint.Y / tileSize);

                if (ViewModel?.CurrentMap != null &&
                    x >= 0 && x < ViewModel.CurrentMap.Width &&
                    y >= 0 && y < ViewModel.CurrentMap.Height)
                {
                    return ViewModel.CurrentMap.Tiles[x, y];
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}