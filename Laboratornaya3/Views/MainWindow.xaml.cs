using Laboratornaya3.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Laboratornaya3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Button _lastSelectedButton;

        // Переменные для масштабирования
        private double _scale = 1.0;
        private const double MinScale = 0.5;
        private const double MaxScale = 4.0;
        private const double ScaleStep = 0.2;

        // Переменные для панорамирования (UI-логика)
        private Point _lastMousePosition;
        private bool _isPanning = false;

        private double _offsetX = 0;
        private double _offsetY = 0;

        private MainViewModel ViewModel => DataContext as MainViewModel;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel();
        }

        /// <summary>
        /// Обработчик нажатия на кнопку "Легенда". Переключает видимость панели LegendPanel.
        /// </summary>
        private void LegendButton_Click(object sender, RoutedEventArgs e)
        {
            if (LegendPanel.Visibility == Visibility.Collapsed)
            {
                // Если скрыт, показываем
                LegendPanel.Visibility = Visibility.Visible;
            }
            else
            {
                // Если показан, скрываем
                LegendPanel.Visibility = Visibility.Collapsed;
            }
        }

        // =======================
        // КАРТА
        // =======================

        /// <summary>
        /// Обработчик колеса мыши для масштабирования (приближение/отдаление).
        /// </summary>
        private void MapScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true; // Останавливаем ScrollViewer от обработки колеса мыши

            double newScale = _scale;

            if (e.Delta > 0)
            {
                // Приближение (Увеличение масштаба)
                newScale += ScaleStep;
            }
            else
            {
                // Отдаление (Уменьшение масштаба)
                newScale -= ScaleStep;
            }

            // Ограничиваем масштаб
            newScale = Math.Clamp(newScale, MinScale, MaxScale);

            // Обновляем ScaleTransform в XAML
            ScaleTransform.ScaleX = newScale;
            ScaleTransform.ScaleY = newScale;
            _scale = newScale;
        }

        /// <summary>
        /// Нажатие кнопки мыши (начало панорамирования).
        /// </summary>
        private void MapScrollViewer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Проверяем, что зажата именно ЛЕВАЯ кнопка мыши.
            // Используем LeftButton == Pressed, так как это состояние, которое мы отслеживаем в MouseUp
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _isPanning = true;

                // Получаем начальную позицию курсора относительно MapScrollViewer.
                _lastMousePosition = e.GetPosition(MapScrollViewer);

                // Считываем текущее смещение из Transform, чтобы продолжить с него
                _offsetX = TranslateTransform.X;
                _offsetY = TranslateTransform.Y;

                MapScrollViewer.Cursor = Cursors.Hand;
                MapScrollViewer.CaptureMouse();

                e.Handled = true;
            }
        }

        /// <summary>
        /// Перемещение курсора (логика панорамирования)
        /// </summary>
        private void MapScrollViewer_MouseMove(object sender, MouseEventArgs e)
        {
            // Проверяем, что панорамирование активно И что мышь захвачена
            if (_isPanning && MapScrollViewer.IsMouseCaptured)
            {
                // Получаем текущую позицию курсора относительно MapScrollViewer
                Point currentPosition = e.GetPosition(MapScrollViewer);

                // 1. Вычисляем разницу в положении мыши (дельту).
                double deltaX = currentPosition.X - _lastMousePosition.X;
                double deltaY = currentPosition.Y - _lastMousePosition.Y;

                // 2. Обновляем смещение
                _offsetX += deltaX;
                _offsetY += deltaY;

                // 3. Применяем новое смещение к TranslateTransform
                TranslateTransform.X = _offsetX;
                TranslateTransform.Y = _offsetY;

                // 4. Обновляем последнюю позицию мыши.
                _lastMousePosition = currentPosition;

                // Поглощаем событие MouseMove
                e.Handled = true;
            }
        }

        /// <summary>
        /// Отпускание кнопки мыши (окончание панорамирования).
        /// </summary>
        private void MapScrollViewer_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // Проверяем, что событие вызвано отпусканием ЛЕВОЙ кнопки
            if (e.ChangedButton == MouseButton.Left && _isPanning)
            {
                _isPanning = false;
                MapScrollViewer.Cursor = Cursors.Arrow;
                MapScrollViewer.ReleaseMouseCapture();

                e.Handled = true;
            }
        }
    }
}