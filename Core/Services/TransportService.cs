using Core.Interfaces;
using Core.Models.Map;


namespace Core.Services
{
    public class TransportService : IGameService
    {
        private readonly GameMap _map;
        private readonly PathfindingService _pathfinder;

        public TransportService(GameMap map, PathfindingService pathfindingService)
        {
            _map = map;
            _pathfinder = pathfindingService;
        }

        public void Initialize()
        {
            // Подготовка маршрутов, регистрация остановок и т.п.
        }

        public void Update()
        {
            // Простая логика: обновляем движение транспорта (вызывает Move у каждого транспортного средства)
            if (_map?.Vehicles == null) return;

            // Копия списка, чтобы не прервать перечисление при модификации
            foreach (var vehicle in _map.Vehicles.ToList())
            {
                try
                {
                    vehicle.Move();
                }
                catch
                {
                    // Защита от исключений в логике движения конкретного транспорта
                }
            }
        }

        public System.Collections.Generic.List<PathNode> FindPath(int sx, int sy, int ex, int ey)
        {
            return _pathfinder?.FindPath(sx, sy, ex, ey);
        }
    }
}