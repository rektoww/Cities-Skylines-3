using Core.Models.Base;
using Core.Models.Map;
using Core.GameEngine;
using Core.Models.Roads;

namespace Laboratornaya3.Services.MapPlacement
{
    public interface IMapPlacementService
    {
        bool TryPlaceBuilding(Core.GameEngine.GameEngine engine, Building building, int x, int y);
        bool PlaceRoad(GameMap map, RoadSegment segment);
        bool PlaceVehicle(Core.GameEngine.GameEngine engine, Transport vehicle);
    }
}