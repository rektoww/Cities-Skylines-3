using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Infrastructure.Services;
using Core.Models.Map;

namespace Laboratornaya3.ViewModels
{
    /// <summary>
    /// Основная ViewModel приложения
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        private readonly SaveLoadService _saveLoadService;
        private GameMap _currentMap;

        /// <summary>Текущая игровая карта</summary>
        public GameMap CurrentMap
        {
            get => _currentMap;
            set => SetProperty(ref _currentMap, value);
        }

        /// <summary>
        /// Создает экземпляр MainViewModel
        /// </summary>
        public MainViewModel()
        {
            _saveLoadService = new SaveLoadService();
            _currentMap = new GameMap(10, 10);
        }

        /// <summary>
        /// Команда для сохранения карты
        /// </summary>
        [RelayCommand]
        private void SaveMap()
        {
            _saveLoadService.SaveMap(_currentMap, "saved_map.json");
        }

        /// <summary>
        /// Команда для загрузки карты
        /// </summary>
        [RelayCommand]
        private void LoadMap()
        {
            CurrentMap = _saveLoadService.LoadMap("saved_map.json");
        }
    }
}