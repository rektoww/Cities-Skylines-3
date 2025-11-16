using Core.Models.Map;
using Infrastructure.Services;

namespace Laboratornaya3.ViewModels
{
    public class MapViewModel
    {
        private readonly GameMap _map;
        private readonly INotificationService _notifier;

        public MapViewModel(GameMap map, INotificationService notifier)
        {
            _map = map;
            _notifier = notifier;
        }

        public GameMap Map => _map;
    }
}
