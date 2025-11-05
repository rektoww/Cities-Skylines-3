using Core.Models.Base;

namespace Core.Models.Roads
{
    /// <summary>
    /// Представляет перекрёсток дорог.
    /// Используется для управления движением транспорта на пересечениях дорог.
    /// </summary>
    public class Intersection
    {
        public int X { get; set; }
        public int Y { get; set; }
        public List<RoadSegment> ConnectedRoads { get; set; }
        public bool HasTrafficLight { get; set; }
        public int Capacity { get; set; }
        public List<Transport> CurrentVehicles { get; set; }

        /// <summary>
        /// Конструктор перекрёстка
        /// </summary>
        public Intersection(int x, int y, bool hasTrafficLight = false)
        {
            X = x;
            Y = y;
            HasTrafficLight = hasTrafficLight;
            ConnectedRoads = new List<RoadSegment>();
            CurrentVehicles = new List<Transport>();
            Capacity = 4; // По умолчанию 4 автомобиля могут находиться на перекрёстке
        }

        /// <summary>
        /// Добавляет сегмент дороги к перекрёстку
        /// </summary>
        public void AddRoad(RoadSegment road)
        {
            if (!ConnectedRoads.Contains(road))
            {
                ConnectedRoads.Add(road);
            }
        }

        /// <summary>
        /// Проверяет, может ли транспорт въехать на перекрёсток
        /// </summary>
        public bool CanEnter()
        {
            return CurrentVehicles.Count < Capacity;
        }

        /// <summary>
        /// Транспорт въезжает на перекрёсток
        /// </summary>
        public bool TryEnter(Transport vehicle)
        {
            if (CanEnter())
            {
                CurrentVehicles.Add(vehicle);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Транспорт покидает перекрёсток
        /// </summary>
        public void Leave(Transport vehicle)
        {
            CurrentVehicles.Remove(vehicle);
        }

        /// <summary>
        /// Получает количество подключенных дорог
        /// </summary>
        public int GetRoadCount()
        {
            return ConnectedRoads.Count;
        }
    }
}
