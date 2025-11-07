using Core.Models.Base;
using Core.Models.Buildings;
using Core.Models.Buildings.CommertialBuildings;
using Core.Models.Map;
using Laboratornaya3.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Laboratornaya3
{
    public partial class MainWindow : Window
    {
        private Point? lastMousePosition;
        private bool isDragging = false;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        private void MapScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control)
                return;

            var scale = e.Delta > 0 ? 1.1 : 0.9;
            ScaleTransform.ScaleX *= scale;
            ScaleTransform.ScaleY *= scale;
            e.Handled = true;
        }

        private void MapScrollViewer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var viewModel = DataContext as MainViewModel;

            if (viewModel?.IsBuildingMode == true && viewModel.SelectedBuilding != null)
            {
                TryPlaceBuilding(e.GetPosition(MapGrid));
                e.Handled = true;
            }
            else if (e.ChangedButton == MouseButton.Left)
            {
                lastMousePosition = e.GetPosition(this);
                isDragging = true;
                MapScrollViewer.CaptureMouse();
                MapScrollViewer.Cursor = Cursors.SizeAll;
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                viewModel?.CancelBuildingCommand.Execute(null);
            }
        }

        private void MapScrollViewer_MouseMove(object sender, MouseEventArgs e)
        {
            var viewModel = DataContext as MainViewModel;

            if (viewModel?.IsBuildingMode == true)
            {
                MapScrollViewer.Cursor = Cursors.Cross;
                ShowBuildingPreview(e.GetPosition(MapGrid));
            }
            else if (isDragging && lastMousePosition.HasValue)
            {
                Point currentPosition = e.GetPosition(this);
                Vector delta = currentPosition - lastMousePosition.Value;

                TranslateTransform.X += delta.X;
                TranslateTransform.Y += delta.Y;

                lastMousePosition = currentPosition;
                MapScrollViewer.Cursor = Cursors.SizeAll;
            }
            else
            {
                MapScrollViewer.Cursor = Cursors.Arrow;
            }
        }

        private void MapScrollViewer_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && isDragging)
            {
                isDragging = false;
                lastMousePosition = null;
                MapScrollViewer.ReleaseMouseCapture();

                var viewModel = DataContext as MainViewModel;
                MapScrollViewer.Cursor = viewModel?.IsBuildingMode == true ? Cursors.Cross : Cursors.Arrow;
            }
        }

        private void ShowBuildingPreview(Point mousePosition)
        {
            var tile = GetTileAtPosition(mousePosition);
        }

        private void TryPlaceBuilding(Point mousePosition)
        {
            var viewModel = DataContext as MainViewModel;
            if (viewModel == null) return;

            if (!viewModel.IsBuildingMode || viewModel.SelectedBuilding == null) return;

            var tile = GetTileAtPosition(mousePosition);
            if (tile != null)
            {
                viewModel.TryPlaceBuilding(tile.X, tile.Y);
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

                var viewModel = DataContext as MainViewModel;
                if (viewModel?.CurrentMap != null &&
                    x >= 0 && x < viewModel.CurrentMap.Width &&
                    y >= 0 && y < viewModel.CurrentMap.Height)
                {
                    return viewModel.CurrentMap.Tiles[x, y];
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private void LegendButton_Click(object sender, RoutedEventArgs e)
        {
            LegendPanel.Visibility = LegendPanel.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as MainViewModel;
            if (viewModel?.CurrentMap != null)
            {
                var testBuilding = new Shop();

                bool placed = false;
                for (int offset = 0; offset < 5 && !placed; offset++)
                {
                    int centerX = viewModel.CurrentMap.Width / 2 + offset;
                    int centerY = viewModel.CurrentMap.Height / 2;

                    if (testBuilding.TryPlace(centerX, centerY, viewModel.CurrentMap))
                    {
                        viewModel.CurrentMap.Buildings.Add(testBuilding);
                        viewModel.RefreshMap();
                        MessageBox.Show($"Тестовое здание размещено в ({centerX}, {centerY})",
                                       "Тест", MessageBoxButton.OK, MessageBoxImage.Information);
                        placed = true;
                    }
                }

                if (!placed)
                {
                    MessageBox.Show("Не удалось разместить тестовое здание ни в одной из позиций!",
                                   "Тест", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
        private void Tile_LeftClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is Tile tile)
            {
                if (DataContext is MainViewModel vm)
                {
                    // просто вызвать команду показа информации
                    if (vm.ShowTileInfoCommand.CanExecute(tile))
                        vm.ShowTileInfoCommand.Execute(tile);
                }
            }
        }

        // ПКМ — поставить здание
        private void Tile_RightClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is Tile tile)
            {
                if (DataContext is MainViewModel vm)
                {
                    vm.TryPlaceBuilding(tile.X, tile.Y);
                }
            }
        }

        private void ClearBuildings_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as MainViewModel;
            if (viewModel?.CurrentMap != null)
            {
                int buildingCount = viewModel.CurrentMap.Buildings.Count;

                viewModel.CurrentMap.Buildings.Clear();

                for (int x = 0; x < viewModel.CurrentMap.Width; x++)
                {
                    for (int y = 0; y < viewModel.CurrentMap.Height; y++)
                    {
                        viewModel.CurrentMap.SetBuildingAt(x, y, null);
                    }
                }

                viewModel.RefreshMap();
                MessageBox.Show($"Удалено {buildingCount} зданий", "Очистка", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MapGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(MapGrid);
            var tile = GetTileAtPosition(position);
        }
    }
}