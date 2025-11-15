using Core.Interfaces;
using Core.Models.Buildings;
using Core.Models.Map;

namespace Core.Services
{
    public class ProductionService : IGameService
    {
        private readonly GameMap _map;

        public ProductionService(GameMap map)
        {
            _map = map;
        }

        public void Initialize()
        {
        }

        public void Update()
        {
            if (_map == null) return;

            var industrials = _map.Buildings?.OfType<IndustrialBuilding>();
            if (industrials == null) return;

            foreach (var b in industrials.ToList())
            {
                try
                {
                    b.ProduceResources();
                }
                catch
                {
                }
            }
        }
    }
}