using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Infrastructure.Services;
using Core.Models.Map;

namespace Laboratornaya3.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly SaveLoadService _saveLoadService;
        private GameMap _currentMap;

        public GameMap CurrentMap
        {
            get => _currentMap;
            set => SetProperty(ref _currentMap, value);
        }

        public MainViewModel()
        {
            _saveLoadService = new SaveLoadService();
            _currentMap = new GameMap(10, 10);
        }

        [RelayCommand]
        private void SaveMap()
        {
            _saveLoadService.SaveMap(_currentMap, "saved_map.json");
        }

        [RelayCommand]
        private void LoadMap()
        {
            CurrentMap = _saveLoadService.LoadMap("saved_map.json");
        }
    }
}