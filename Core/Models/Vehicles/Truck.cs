using Core.Models.Base;
using Core.Models.Map;
using Core.Services;

namespace Core.Models.Vehicles
{
    /// <summary>
    /// Класс грузовика для перевозки товаров между зданиями.
    /// Используется для логистики и торговли в городе.
    /// </summary>
    public class Truck : Transport
    {
        private const int TruckCapacity = 2; // Водитель + 1 помощник
        public float CargoCapacity { get; set; }
        public float CurrentCargoWeight { get; set; }
        public string CargoType { get; set; }
        public Building OriginBuilding { get; set; }
        public Building DestinationBuilding { get; set; }
        public List<PathNode> CurrentRoute { get; set; }
        public bool IsBusy { get; set; }

        /// <summary>
        /// Конструктор грузовика
        /// </summary>
        public Truck(int x, int y, GameMap map) : base(x, y, map, TruckCapacity)
        {
            Speed = 7f;
            CargoCapacity = 10f; // 10 тонн
            CurrentCargoWeight = 0f;
            CurrentRoute = new List<PathNode>();
            IsBusy = false;
        }

        /// <summary>
        /// Движение грузовика по маршруту
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

                // Если прибыли в пункт назначения
                if (CurrentRoute.Count == 0)
                {
                    DeliverCargo();
                }
            }
        }

        /// <summary>
        /// Загружает груз в грузовик
        /// </summary>
        public bool LoadCargo(float weight, string cargoType, Building origin)
        {
            if (weight > CargoCapacity)
                return false;

            CurrentCargoWeight = weight;
            CargoType = cargoType;
            OriginBuilding = origin;
            return true;
        }

        /// <summary>
        /// Доставляет груз в пункт назначения
        /// </summary>
        private void DeliverCargo()
        {
            if (DestinationBuilding != null)
            {
                // Груз доставлен
                CurrentCargoWeight = 0f;
                CargoType = null;
                OriginBuilding = null;
                DestinationBuilding = null;
                IsBusy = false;
            }
        }

        /// <summary>
        /// Устанавливает маршрут доставки
        /// </summary>
        public bool SetDeliveryRoute(Building destination)
        {
            if (CurrentCargoWeight == 0)
                return false;

            DestinationBuilding = destination;
            IsBusy = true;

            // Создаём маршрут к зданию-получателю
            var pathfinder = new PathfindingService(GameMap);
            CurrentRoute = pathfinder.FindPath(X, Y, destination.X, destination.Y);

            return CurrentRoute != null;
        }

        /// <summary>
        /// Проверяет, может ли грузовик взять груз указанного веса
        /// </summary>
        public bool CanCarry(float weight)
        {
            return weight <= CargoCapacity && !IsBusy;
        }
    }
}
