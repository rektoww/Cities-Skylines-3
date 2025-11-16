using Core.Models.Mobs;
using Core.Models.Map;

namespace Core.Services.Happiness
{
    public class HappinessService : IHappinessService
    {
        private readonly GameMap _map;

        public HappinessService(GameMap map)
        {
            _map = map;
        }

        public void UpdateHappiness(Citizen citizen)
        {
            if (citizen == null || _map?.Tiles == null) return;
            if (citizen.X < 0 || citizen.X >= _map.Width || citizen.Y < 0 || citizen.Y >= _map.Height) return;

            var tile = _map.Tiles[citizen.X, citizen.Y];
            if (tile == null) return;

            if (tile.HasPark)
                citizen.Happiness = Math.Min(100, citizen.Happiness + 0.1f);

            if (tile.HasBikeLane)
                citizen.Happiness = Math.Min(100, citizen.Happiness + 0.05f);

            if (tile.HasPedestrianPath)
                citizen.Happiness = Math.Min(100, citizen.Happiness + 0.03f);

            if (citizen.Home?.IsOperational == true)
                citizen.Happiness = Math.Min(100, citizen.Happiness + 0.02f);

            if (citizen.Workplace?.IsOperational == true)
                citizen.Happiness = Math.Min(100, citizen.Happiness + 0.01f);

            citizen.Happiness = Math.Max(0, citizen.Happiness - 0.01f);
        }

        public void ApplyHappinessEffects(Citizen citizen)
        {
            if (citizen == null) return;

            if (citizen.Happiness > 70f)
                citizen.Health = Math.Min(100, citizen.Health + 0.05f);
            else if (citizen.Happiness < 30f)
                citizen.Health = Math.Max(0, citizen.Health - 0.1f);
        }
    }
}
