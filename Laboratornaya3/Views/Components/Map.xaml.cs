using Core.Models.Map;
using Laboratornaya3.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Laboratornaya3.Views.Components
{
    /// <summary>
    /// Логика взаимодействия для Map.xaml
    /// </summary>
    public partial class Map : UserControl
    {
        private MainViewModel ViewModel => DataContext as MainViewModel;

        private Point? _lastMousePosition;
        private bool _isDragging = false;

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
            if (tile == null) return;

            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    HandleLeftClick(tile, e);
                    break;
                case MouseButton.Right:
                    HandleRightClick(tile, e);
                    break;
            }
        }

        private void HandleLeftClick(Tile tile, MouseButtonEventArgs e)
        {
            if (ViewModel.IsBuildingMode && ViewModel.SelectedBuilding != null)
            {
                ViewModel.TryPlaceBuilding(tile.X, tile.Y);
                e.Handled = true;
            }
            else
            {
                // Начало перетаскивания карты
                _lastMousePosition = e.GetPosition(this);
                _isDragging = true;
                MapScrollViewer.CaptureMouse();
                MapScrollViewer.Cursor = Cursors.SizeAll;
            }
        }

        private void HandleRightClick(Tile tile, MouseButtonEventArgs e)
        {
            if (ViewModel.IsRoadPlacementMode)
            {
                if (ViewModel.SelectedBuilding?.Name == "Перекрёсток")
                    ViewModel.TryPlaceSelected(tile.X, tile.Y);
                else
                    ViewModel.StartRoadDrawing(tile.X, tile.Y);
                e.Handled = true;
            }
            else if (ViewModel.IsVehiclePlacementMode)
            {
                ViewModel.TryPlaceVehicle(tile.X, tile.Y);
                e.Handled = true;
            }
            else
            {
                ViewModel.CancelBuildingCommand.Execute(null);
            }
        }

        private void MapScrollViewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (ViewModel == null) return;

            if (ViewModel.IsBuildingMode || ViewModel.IsRoadPlacementMode || ViewModel.IsVehiclePlacementMode)
            {
                MapScrollViewer.Cursor = Cursors.Cross;
            }
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
            if (ViewModel?.IsRoadPlacementMode == true && e.ChangedButton == MouseButton.Right)
            {
                var tile = GetTileAtPosition(e.GetPosition(MapGrid));
                if (tile != null && ViewModel.SelectedBuilding?.Name != "Перекрёсток")
                {
                    ViewModel.EndRoadDrawing(tile.X, tile.Y);
                }
                e.Handled = true;
            }

            if (e.ChangedButton == MouseButton.Left && _isDragging)
            {
                _isDragging = false;
                _lastMousePosition = null;
                MapScrollViewer.ReleaseMouseCapture();
                MapScrollViewer.Cursor = ViewModel?.IsBuildingMode == true ? Cursors.Cross : Cursors.Arrow;
            }
        }

        // исправить привязку
        private void Tile_LeftClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is Tile tile)
            {
                ViewModel?.ShowTileInfoCommand.Execute(tile);
            }
        }

        // исправить привязку
        private void Tile_RightClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is Tile tile)
            {
                ViewModel?.TryPlaceBuilding(tile.X, tile.Y);
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
