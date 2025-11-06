using Core.Models.Map;

using System;
using System.Collections.Generic;
using System.Linq;
using Core.Models.Map;
using Core.Models.Mobs;

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

        public override void Move()
        {
            if (Route == null || Route.Count == 0)
                return;

            // Защита от некорректного индекса
            if (NextWaypointIndex < 0 || NextWaypointIndex >= Route.Count)
                NextWaypointIndex = 0;

            var targetTile = Route[NextWaypointIndex];

            // Если уже на целевом тайле (или очень близко) — считаем что достигли waypoint
            if (X == targetTile.X && Y == targetTile.Y)
            {
                // Если это станция — инициируем dwell / boarding/disembark
                if (_dwellTicksRemaining == 0)
                    _dwellTicksRemaining = DwellTimeTicks;

                HandleStationLogic(targetTile);

                // Если ещё остались такты стоянки — уменьшаем и не двигаемся дальше в этот такт
                if (_dwellTicksRemaining > 0)
                {
                    _dwellTicksRemaining--;
                    // Если стоянка окончена — переходим к следующему waypoint
                    if (_dwellTicksRemaining == 0)
                    {
                        NextWaypointIndex = (NextWaypointIndex + 1) % Route.Count;
                    }

                    // Обновляем позиции пассажиров (остаемся на месте)
                    foreach (var p in Passengers)
                    {
                        p.X = X;
                        p.Y = Y;
                    }

                    return;
                }
            }

            // Движение к targetTile (упрощённая логика)
            float deltaX = targetTile.X - X;
            float deltaY = targetTile.Y - Y;
            double distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            if (distance <= 0.0)
            {
                // Защита от деления на ноль - просто установим координаты
                X = targetTile.X;
                Y = targetTile.Y;
            }
            else
            {
                float moveX = (float)(deltaX / distance * Math.Max(1f, Speed));
                float moveY = (float)(deltaY / distance * Math.Max(1f, Speed));

                // Инкременты приводим к целым координатам карты
                int newX = X + (int)Math.Round(moveX);
                int newY = Y + (int)Math.Round(moveY);

                // Применяем новые координаты
                X = newX;
                Y = newY;
            }

            // Синхронизируем позиции пассажиров
            foreach (var passenger in Passengers)
            {
                passenger.X = X;
                passenger.Y = Y;
            }

            // Если достигли целевого тайла после движения — установим dwell в следующем такте
            if (X == targetTile.X && Y == targetTile.Y)
            {
                // Обрабатываем станцию сразу (в следующем такте будет учтён dwellTicks)
                HandleStationLogic(targetTile);
                _dwellTicksRemaining = Math.Max(0, DwellTimeTicks - 1);
            }
        }

        /// <summary>
        /// Если на тайле есть TransitStation — выполняет высадку/посадку.
        /// Высадка: для пассажиров, для которых IsArrivedAtDestination() == true.
        /// Посадка: берём из WaitingCitizens в порядке очереди до заполнения транспорта.
        /// </summary>
        private void HandleStationLogic(Tile tile)
        {
            if (tile == null || tile.Building == null) return;

            if (tile.Building is not TransitStation station) return;
            if (!station.IsOperational) return;

            // Высадка: используем копию списка, чтобы безопасно модифицировать коллекцию
            foreach (var passenger in Passengers.ToList())
            {
                try
                {
                    // IsArrivedAtDestination учитывает CurrentTransport у пассажира
                    if (passenger.IsArrivedAtDestination())
                    {
                        // Попытка высадки — Transport.TryDisembark обновит состояние пассажира
                        TryDisembark(passenger);
                    }
                }
                catch
                {
                    // Защита от возможных внешних ошибок; в продакшне логировать
                }
            }

            // Посадка: садим ожидающих граждан по очереди, пока есть места
            while (CanAcceptPassenger && station.WaitingCitizens.Count > 0)
            {
                var candidate = station.WaitingCitizens.FirstOrDefault();
                if (candidate == null) break;

                bool boarded = TryBoard(candidate);
                if (boarded)
                {
                    station.RemoveWaitingCitizen(candidate);
                }
                else
                {
                    // Если не удалось посадить — выходим (например, кандидат уже в другом транспорте)
                    break;
                }
            }
        }
    }
}