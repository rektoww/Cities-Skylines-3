using Core.GameEngine;
using Infrastructure.Services;
using Core.Models.Map;
using Core.Models.Roads;

namespace Laboratornaya3.ViewModels
{
    public class RoadManagementViewModel
    {
        private readonly GameEngine _engine;
        private readonly INotificationService _notifier;

        public RoadManagementViewModel(GameEngine engine, INotificationService notifier)
        {
            _engine = engine;
            _notifier = notifier;
        }

        public bool PlaceRoad(GameMap map, RoadSegment segment)
        {
            map.AddRoadSegment(segment);
            return true;
        }
    }
}
