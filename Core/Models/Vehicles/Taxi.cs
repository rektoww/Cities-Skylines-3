using Core.Models.Base;
using Core.Models.Map;
using Core.Models.Mobs;
using Core.Services;

namespace Core.Models.Vehicles
{
    /// <summary>
    /// Класс такси для перевозки пассажиров по городу.
    /// Ездит по дорогам и подбирает граждан.
    /// </summary>
    public class Taxi : Transport
    {
        private const int TaxiCapacity = 4; // Водитель + 3 пассажира
        public decimal FarePerTile { get; set; }
        public bool IsAvailable { get; set; }
        public List<PathNode> CurrentRoute { get; set; }
        public Citizen TargetPassenger { get; set; }

        /// <summary>
        /// Конструктор такси
        /// </summary>
        public Taxi(int x, int y, GameMap map) : base(x, y, map, TaxiCapacity)
        {
            Speed = 10f;
            FarePerTile = 2m;
            IsAvailable = true;
            CurrentRoute = new List<PathNode>();
        }

        /// <summary>
        /// Движение такси по маршруту
        /// </summary>
        public override void Move()
        {
            // Если есть маршрут, следуем по нему
            if (CurrentRoute != null && CurrentRoute.Count > 0)
            {
                var nextNode = CurrentRoute[0];

                // Перемещаемся к следующей точке маршрута
                X = nextNode.X;
                Y = nextNode.Y;

                // Убираем пройденную точку из маршрута
                CurrentRoute.RemoveAt(0);

                // Синхронизируем положение пассажиров
                foreach (var passenger in Passengers)
                {
                    passenger.X = X;
                    passenger.Y = Y;
                }

                // Если маршрут закончился, такси становится доступным
                if (CurrentRoute.Count == 0)
                {
                    IsAvailable = true;
                    TargetPassenger = null;
                }
            }
            else
            {
                // Если нет маршрута, стоим на месте
                IsAvailable = true;
            }
        }

        /// <summary>
        /// Устанавливает маршрут для такси
        /// </summary>
        public void SetRoute(List<PathNode> route)
        {
            CurrentRoute = route;
            IsAvailable = false;
        }

        /// <summary>
        /// Вызывает такси для пассажира
        /// </summary>
        public bool CallTaxi(Citizen passenger, int destinationX, int destinationY)
        {
            if (!IsAvailable || !CanAcceptPassenger)
                return false;

            TargetPassenger = passenger;
            IsAvailable = false;

            // Создаём маршрут к пассажиру
            var pathfinder = new PathfindingService(GameMap);
            CurrentRoute = pathfinder.FindPath(X, Y, passenger.X, passenger.Y);

            return CurrentRoute != null;
        }

        /// <summary>
        /// Вычисляет стоимость поездки
        /// </summary>
        public decimal CalculateFare(int distance)
        {
            return FarePerTile * distance;
        }
    }
}
