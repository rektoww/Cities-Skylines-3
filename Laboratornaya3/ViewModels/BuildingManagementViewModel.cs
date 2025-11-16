using Core.GameEngine;
using Infrastructure.Services;
using Core.Models.Buildings;
using Core.Models.Map;
using Core.Models.Base;

namespace Laboratornaya3.ViewModels
{
    public class BuildingManagementViewModel
    {
        private readonly GameEngine _engine;
        private readonly INotificationService _notifier;

        public BuildingManagementViewModel(GameEngine engine, INotificationService notifier)
        {
            _engine = engine;
            _notifier = notifier;
        }

        public bool TryPlaceBuilding(Building building, int x, int y)
        {
            return _engine.TryPlaceBuilding(building, x, y);
        }
    }
}
