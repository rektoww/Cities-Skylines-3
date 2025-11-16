using Core.Models.Map;

namespace Core.Models.Base
{
    /// <summary>
    /// Абстрактный класс для общественного транспорта, движущегося по маршруту.
    /// Обрабатывает прибытие на waypoint, dwell time, посадку/высадку на TransitStation.
    /// </summary>
    public abstract class PublicTransport : Transport
    {
        public List<Tile> Route { get; set; } = new List<Tile>();
        public int NextWaypointIndex { get; set; } = 0;

        /// <summary>
        /// Длительность остановки в тактах при заходе на станцию.
        /// </summary>
        public int DwellTimeTicks { get; set; } = 1;

        private int _dwellTicksRemaining = 0;

        public PublicTransport(int x, int y, GameMap map, int capacity) : base(x, y, map, capacity)
        {
        }

        //public override void Move()
        //{
        //    if (Route == null || Route.Count == 0)
        //        return;

        //    if (NextWaypointIndex < 0 || NextWaypointIndex >= Route.Count)
        //        NextWaypointIndex = 0;

        //    var targetTile = Route[NextWaypointIndex];

        //    if (X == targetTile.X && Y == targetTile.Y)
        //    {
        //        if (_dwellTicksRemaining == 0)
        //            _dwellTicksRemaining = DwellTimeTicks;

        //        HandleStationLogic(targetTile);

        //        if (_dwellTicksRemaining > 0)
        //        {
        //            _dwellTicksRemaining--;
        //            if (_dwellTicksRemaining == 0)
        //            {
        //                NextWaypointIndex = (NextWaypointIndex + 1) % Route.Count;
        //            }

        //            foreach (var p in Passengers)
        //            {
        //                p.X = X;
        //                p.Y = Y;
        //            }

        //            return;
        //        }
        //    }

        //    float deltaX = targetTile.X - X;
        //    float deltaY = targetTile.Y - Y;
        //    double distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

        //    if (distance <= 0.0)
        //    {
        //        X = targetTile.X;
        //        Y = targetTile.Y;
        //    }
        //    else
        //    {
        //        float moveX = (float)(deltaX / distance * Math.Max(1f, Speed));
        //        float moveY = (float)(deltaY / distance * Math.Max(1f, Speed));

        //        int newX = X + (int)Math.Round(moveX);
        //        int newY = Y + (int)Math.Round(moveY);

        //        X = newX;
        //        Y = newY;
        //    }

        //    foreach (var passenger in Passengers)
        //    {
        //        passenger.X = X;
        //        passenger.Y = Y;
        //    }

        //    if (X == targetTile.X && Y == targetTile.Y)
        //    {
        //        HandleStationLogic(targetTile);
        //        _dwellTicksRemaining = Math.Max(0, DwellTimeTicks - 1);
        //    }
        //}

        /// <summary>
        /// Если на тайле есть TransitStation — выполняет высадку/посадку.
        /// Высадка: для пассажиров, для которых IsArrivedAtDestination() == true.
        /// Посадка: берём из WaitingCitizens в порядке очереди до заполнения транспорта.
        /// </summary>
        //private void HandleStationLogic(Tile tile)
        //{
        //    if (tile == null || tile.Building == null) return;

        //    if (tile.Building is not TransitStation station) return;
        //    if (!station.IsOperational) return;

        //    foreach (var passenger in Passengers.ToList())
        //    {
        //        try
        //        {
        //            if (passenger.IsArrivedAtDestination())
        //            {
        //                TryDisembark(passenger);
        //            }
        //        }
        //        catch
        //        {
        //        }
        //    }

        //    while (CanAcceptPassenger && station.WaitingCitizens.Count > 0)
        //    {
        //        var candidate = station.WaitingCitizens.FirstOrDefault();
        //        if (candidate == null) break;

        //        bool boarded = TryBoard(candidate);
        //        if (boarded)
        //        {
        //            station.RemoveWaitingCitizen(candidate);
        //        }
        //        else
        //        {
        //            break;
        //        }
        //    }
        //}
    }
}