using Core.GameEngine;
using Infrastructure.Services;
using Core.Models.Vehicles;

namespace Laboratornaya3.ViewModels
{
    public class VehicleManagementViewModel
    {
        private readonly GameEngine _engine;
        private readonly INotificationService _notifier;

        public VehicleManagementViewModel(GameEngine engine, INotificationService notifier)
        {
            _engine = engine;
            _notifier = notifier;
        }

        public void AddVehicle(Core.Models.Base.Transport vehicle)
        {
            _engine.AddVehicle(vehicle);
        }
    }
}
