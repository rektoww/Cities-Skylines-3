// Map.xaml.cs

using Core.Models.Map;
using Laboratornaya3.ViewModels;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Laboratornaya3.Views.Components
{
    public partial class Map : UserControl
    {
        private MainViewModel ViewModel => DataContext as MainViewModel;

        private Point? _lastMousePosition;
        private bool _isLMBDragging = false;
        private bool _isRMBDragging = false; // Флаг для перетаскивания ПКМ

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

            var position = e.GetPosition(MapGrid);
            var tile = GetTileAtPosition(position);

            // Обработка нажатия ПКМ (Панорамирование или Размещение Транспорта)
            if (e.ChangedButton == MouseButton.Right)
            {
                if (ViewModel.IsVehiclePlacementMode && tile != null)
                {
                    // Размещение транспорта по ПКМ (мгновенная установка)
                    HandleRightClick(tile, e);
                }
                else
                {
                    // Начинаем перетаскивание правой кнопкой (Панорамирование)
                    StartRMBDragging(e);
                }
            }
            // Обработка нажатия ЛКМ (Размещение Здания/Дороги или Отмена режима)
            else if (e.ChangedButton == MouseButton.Left)
            {
                if (tile != null && ViewModel.IsRoadPlacementMode)
                {
                    // КЛЮЧЕВОЕ ИЗМЕНЕНИЕ ДЛЯ ДОРОГ (ЛКМ): Начинаем рисование дороги
                    if (ViewModel.SelectedBuilding?.Name == "Перекрёсток")
                        ViewModel.TryPlaceSelected(tile.X, tile.Y);
                    else
                        ViewModel.StartRoadDrawing(tile.X, tile.Y);

                    _lastMousePosition = e.GetPosition(this);
                    _isLMBDragging = true; // Начинаем перетаскивание ЛКМ для рисования
                    MapScrollViewer.CaptureMouse();
                    e.Handled = true;
                }
                else
                {
                    // Если не дорога, то просто отменяем режим (если клик вне тайла)
                    HandleLeftClick(tile, e);
                }
            }
        }

        private void StartRMBDragging(MouseButtonEventArgs e)
        {
            _lastMousePosition = e.GetPosition(this);
            _isRMBDragging = true;
            MapScrollViewer.CaptureMouse();
            MapScrollViewer.Cursor = Cursors.SizeAll;
            e.Handled = true;
        }

        // УПРОЩЕНО: Теперь этот метод только отменяет активные режимы (при клике вне тайла)
        private void HandleLeftClick(Tile tile, MouseButtonEventArgs e)
        {
            if (ViewModel == null) return;

            if (ViewModel.IsBuildingMode || ViewModel.IsRoadPlacementMode || ViewModel.IsVehiclePlacementMode)
            {
                ViewModel.CancelBuildingCommand.Execute(null);
                e.Handled = true;
            }
        }

        private void HandleRightClick(Tile tile, MouseButtonEventArgs e)
        {
            // Этот метод теперь используется для мгновенной установки транспорта (ПКМ)
            if (ViewModel == null || tile == null) return;

            if (ViewModel.IsVehiclePlacementMode)
            {
                ViewModel.TryPlaceVehicle(tile.X, tile.Y);
                e.Handled = true;
            }
        }

        private void MapScrollViewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (ViewModel == null) return;

            // Курсор в режимах размещения
            if (ViewModel.IsBuildingMode || ViewModel.IsRoadPlacementMode || ViewModel.IsVehiclePlacementMode)
            {
                // ПЕРЕТАСКИВАНИЕ КАРТЫ ПКМ
                if (_isRMBDragging && _lastMousePosition.HasValue)
                {
                    HandleMapDragging(e);
                    MapScrollViewer.Cursor = Cursors.SizeAll;
                    return;
                }

                // РИСОВАНИЕ ДОРОГИ ЛКМ
                if (_isLMBDragging && ViewModel.IsRoadPlacementMode)
                {
                    var tile = GetTileAtPosition(e.GetPosition(MapGrid));
                    if (tile != null && ViewModel.SelectedBuilding?.Name != "Перекрёсток")
                    {
                        // ИСПРАВЛЕНИЕ: Используем StartRoadDrawing для рисования сегментов
                        ViewModel.StartRoadDrawing(tile.X, tile.Y);
                    }
                    e.Handled = true;
                    return;
                }

                // Если не тащим, устанавливаем курсор для размещения
                MapScrollViewer.Cursor = Cursors.Cross;
            }
            // Перетаскивание правой кнопкой (когда нет активных режимов)
            else if (_isRMBDragging && _lastMousePosition.HasValue)
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
            if (ViewModel == null) return;

            // Завершение рисования дороги по отпусканию ЛКМ
            if (ViewModel.IsRoadPlacementMode && e.ChangedButton == MouseButton.Left && _isLMBDragging)
            {
                var tile = GetTileAtPosition(e.GetPosition(MapGrid));
                if (tile != null && ViewModel.SelectedBuilding?.Name != "Перекрёсток")
                {
                    // ИСПРАВЛЕНИЕ: Вызываем EndRoadDrawing без дополнительных аргументов
                    ViewModel.EndRoadDrawing(tile.X, tile.Y);
                }

                _isLMBDragging = false;
                _lastMousePosition = null;
                MapScrollViewer.ReleaseMouseCapture();
                MapScrollViewer.Cursor = Cursors.Cross; // Остаемся в режиме дороги (до отмены)
                e.Handled = true;
            }

            // Завершение перетаскивания правой кнопкой мыши
            if (e.ChangedButton == MouseButton.Right && _isRMBDragging)
            {
                _isRMBDragging = false;
                _lastMousePosition = null;
                MapScrollViewer.ReleaseMouseCapture();
                MapScrollViewer.Cursor = (ViewModel.IsBuildingMode || ViewModel.IsRoadPlacementMode || ViewModel.IsVehiclePlacementMode) ? Cursors.Cross : Cursors.Arrow;
            }
        }

        // Обрабатываем размещение зданий или показ инфо (ЛКМ)
        private void Tile_LeftClick(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel == null || !(sender is Border border) || !(border.DataContext is Tile tile)) return;

            // 1. ПРИОРИТЕТ: Проверяем режим строительства (только одиночный клик ЛКМ)
            if (ViewModel.IsBuildingMode && ViewModel.SelectedBuilding != null)
            {
                bool placementSuccessful = ViewModel.TryPlaceBuilding(tile.X, tile.Y);

                // Если размещение успешно, отменяем режим строительства
                if (placementSuccessful)
                {
                    ViewModel.CancelBuildingCommand.Execute(null);
                }

                e.Handled = true; // Останавливаем событие
            }
            // 2. Если не в режиме строительства и не в режиме дорог, показываем информацию о тайле
            else if (!ViewModel.IsRoadPlacementMode)
            {
                ViewModel?.ShowTileInfoCommand.Execute(tile);
                e.Handled = true; // Останавливаем событие
            }
            // Если в режиме дорог, то не делаем ничего (обработка клика уже произошла в MouseDown)
        }

        private void Tile_RightClick(object sender, MouseButtonEventArgs e)
        {
            // Используется для панорамирования (MapScrollViewer_MouseDown)
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