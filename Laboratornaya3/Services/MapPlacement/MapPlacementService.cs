using Core.Models.Base;
using Core.Models.Map;
using Infrastructure.Services;
using Core.GameEngine;
using Core.Models.Roads;

namespace Laboratornaya3.Services.MapPlacement
{
    public class MapPlacementService : IMapPlacementService
    {
        private readonly INotificationService _notifier;

        public MapPlacementService(INotificationService notifier)
        {
            _notifier = notifier;
        }

        public bool TryPlaceBuilding(GameEngine engine, Building building, int x, int y)
        {
            if (engine == null || building == null) return false;

            bool res = engine.TryPlaceBuilding(building, x, y);
            if (!res)
            {
                _notifier.ShowWarning("Нельзя построить здесь. Возможные причины:\n• Недостаточно бюджета или материалов\n• Недостаточно места\n• Неподходящий рельеф\n• Место занято");
            }
            else
            {
                _notifier.ShowInfo($"Здание '{building.Name}' успешно построено!\nБюджет: {engine.FinancialSystem.CityBudget:N0}");
            }
            return res;
        }

        public bool PlaceRoad(GameMap map, RoadSegment segment)
        {
            if (map == null || segment == null) return false;
            map.AddRoadSegment(segment);
            _notifier.ShowInfo($"Дорога успешно построена от ({segment.StartX},{segment.StartY}) до ({segment.EndX},{segment.EndY})");
            return true;
        }

        public bool PlaceVehicle(GameEngine engine, Transport vehicle)
        {
            if (engine == null || vehicle == null) return false;
            engine.AddVehicle(vehicle);
            _notifier.ShowInfo("Транспорт размещен");
            return true;
        }
    }
}