using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.GameEngine;
using Infrastructure.Services;
using System.Windows;
using System.Windows.Threading;

namespace Laboratornaya3.ViewModels
{
    public partial class GameStateViewModel : ObservableObject
    {
        private readonly GameEngine _engine;
        private readonly INotificationService _notifier;
        private readonly FinanceReportService _reportService;
        private DispatcherTimer _gameTimer;

        [ObservableProperty]
        private decimal _cityBudget;

        [ObservableProperty]
        private int _citizenCount;

        [ObservableProperty]
        private int _buildingCount;

        [ObservableProperty]
        private int _roadCount;

        [ObservableProperty]
        private string _financeSummary;

        public event System.Action TickCompleted;

        public GameStateViewModel(GameEngine engine, INotificationService notifier)
        {
            _engine = engine;
            _notifier = notifier;
            _reportService = new FinanceReportService(engine);
            UpdateGameState();
        }

        public void StartGameTimer()
        {
            if (_gameTimer != null) return;

            _gameTimer = new DispatcherTimer();
            _gameTimer.Interval = TimeSpan.FromSeconds(3);
            _gameTimer.Tick += async (s, e) =>
            {
                await System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
                        _engine.Update();
                    }
                    catch
                    {

                    }
                }).ConfigureAwait(false);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    UpdateGameState();
                    TickCompleted?.Invoke();
                });
            };

            _gameTimer.Start();
        }

        public void StopGameTimer()
        {
            _gameTimer?.Stop();
            _gameTimer = null;
        }

        private void UpdateGameState()
        {
            var report = _reportService.GetFinanceReport();

            CityBudget = report.CityBudget;
            CitizenCount = report.CitizenCount;
            BuildingCount = report.BuildingCount;
            RoadCount = report.RoadSegmentsCount;
            FinanceSummary = _reportService.GetFormattedFinanceInfo();
        }

        [RelayCommand]
        private void ShowFinanceInfo()
        {
            try
            {
                var report = _reportService.GetFormattedFinanceInfo();
                _notifier.ShowInfo(report, "Финансовая информация");
            }
            catch (System.Exception ex)
            {
                _notifier.ShowWarning($"Ошибка при получении финансовой информации: {ex.Message}");
            }
        }

        [RelayCommand]
        private void SaveGame()
        {
            try
            {
                var svc = new SaveLoadService();
                svc.SaveGame(_engine.Map, "save.json");
                _notifier.ShowInfo($"Сохранено!\nЗданий: {BuildingCount}\nДорог: {RoadCount}");
            }
            catch (System.Exception ex)
            {
                _notifier.ShowWarning($"Ошибка сохранения: {ex.Message}");
            }
        }

        [RelayCommand]
        private void LoadGame()
        {
            try
            {
                var svc = new SaveLoadService();
                svc.LoadGame(_engine.Map, "save.json");
                UpdateGameState();
                _notifier.ShowInfo($"Загружено!\nЗданий: {BuildingCount}\nДорог: {RoadCount}");
            }
            catch (System.Exception ex)
            {
                _notifier.ShowWarning($"Ошибка загрузки: {ex.Message}");
            }
        }
    }
}