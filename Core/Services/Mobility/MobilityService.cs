using Core.Models.Mobs;
using Core.Models.Map;
using Core.Models.Buildings;
using Core.Models.Base;

namespace Core.Services.Mobility
{
    public class MobilityService : IMobilityService
    {
        private readonly GameMap _map;

        public MobilityService(GameMap map)
        {
            _map = map;
        }

        public Core.Models.Base.TransitStation FindNearestTransitStation(Citizen citizen)
        {
            if (citizen == null || _map?.Buildings == null) return null;

            Core.Models.Base.TransitStation nearest = null;
            double minDist = double.MaxValue;

            foreach (var b in _map.Buildings)
            {
                if (b is Core.Models.Base.TransitStation station && station.IsOperational)
                {
                    double d = Math.Pow(citizen.X - station.X, 2) + Math.Pow(citizen.Y - station.Y, 2);
                    if (d < minDist)
                    {
                        minDist = d;
                        nearest = station;
                    }
                }
            }

            return nearest;
        }

        public void MoveCitizen(Citizen citizen)
        {
            if (citizen == null) return;

            if (citizen.IsOnTransport)
            {
                if (citizen.CurrentTransport != null && IsArrivedAtDestination(citizen))
                {
                    TryDisembarkTransport(citizen);
                }
                return;
            }

            if (citizen.IsWaitingForTransport)
            {
                return; // wait for transport
            }

            if (citizen.DestinationBuilding == null)
            {
                return;
            }

            var dest = citizen.DestinationBuilding;
            var dist = Math.Sqrt(Math.Pow(citizen.X - dest.X, 2) + Math.Pow(citizen.Y - dest.Y, 2));
            if (dist <= 10 || FindNearestTransitStation(citizen) == null)
            {
                MoveTowards(citizen, dest.X, dest.Y);
            }
            else
            {
                var station = citizen.TargetTransitStation ?? FindNearestTransitStation(citizen);
                citizen.TargetTransitStation = station;
                if (station != null && (citizen.X != station.X || citizen.Y != station.Y))
                {
                    MoveTowards(citizen, station.X, station.Y);
                    if (citizen.X == station.X && citizen.Y == station.Y)
                    {
                        station.AddWaitingCitizen(citizen);
                    }
                }
            }
        }

        private bool IsArrivedAtDestination(Citizen citizen)
        {
            if (citizen.DestinationBuilding == null) return false;

            int tx = citizen.DestinationBuilding.X;
            int ty = citizen.DestinationBuilding.Y;

            if (citizen.IsOnTransport && citizen.CurrentTransport != null)
            {
                tx = citizen.CurrentTransport.X;
                ty = citizen.CurrentTransport.Y;
            }

            return citizen.X >= tx && citizen.X < tx + citizen.DestinationBuilding.Width &&
                   citizen.Y >= ty && citizen.Y < ty + citizen.DestinationBuilding.Height;
        }

        private void MoveTowards(Citizen citizen, int targetX, int targetY)
        {
            int dx = targetX - citizen.X;
            int dy = targetY - citizen.Y;

            if (Math.Abs(dx) > Math.Abs(dy))
            {
                citizen.TryMoveTo(citizen.X + Math.Sign(dx), citizen.Y);
            }
            else
            {
                citizen.TryMoveTo(citizen.X, citizen.Y + Math.Sign(dy));
            }
        }

        public bool TryBoardTransport(Citizen citizen, Core.Models.Base.Transport transport)
        {
            if (citizen == null || transport == null) return false;

            if (transport.TryBoard(citizen))
            {
                citizen.CurrentTransport = transport;
                return true;
            }

            return false;
        }

        public bool TryDisembarkTransport(Citizen citizen)
        {
            if (citizen == null || citizen.CurrentTransport == null) return false;

            var transport = citizen.CurrentTransport;
            if (transport.TryDisembark(citizen))
            {
                citizen.X = transport.X;
                citizen.Y = transport.Y;
                citizen.CurrentTransport = null;
                citizen.TargetTransitStation = null;
                return true;
            }

            return false;
        }
    }
}
