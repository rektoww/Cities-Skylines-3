using Core.Enums;

namespace Core.Models.Roads
{
    /// <summary>
    /// Представляет дорогу в городе.
    /// Дорога состоит из нескольких сегментов и может соединять разные части города.
    /// </summary>
    public class Road
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<RoadSegment> Segments { get; set; }
        public RoadType Type { get; set; }
        public double TotalLength { get; private set; }
        public decimal TotalCost { get; private set; }

        /// <summary>
        /// Конструктор дороги
        /// </summary>
        public Road(int id, string name, RoadType type)
        {
            Id = id;
            Name = name;
            Type = type;
            Segments = new List<RoadSegment>();
            TotalLength = 0;
            TotalCost = 0;
        }

        /// <summary>
        /// Добавляет сегмент к дороге
        /// </summary>
        public void AddSegment(RoadSegment segment)
        {
            Segments.Add(segment);
            RecalculateStats();
        }

        /// <summary>
        /// Удаляет сегмент из дороги
        /// </summary>
        public void RemoveSegment(RoadSegment segment)
        {
            Segments.Remove(segment);
            RecalculateStats();
        }

        /// <summary>
        /// Пересчитывает общую длину и стоимость дороги
        /// </summary>
        private void RecalculateStats()
        {
            TotalLength = 0;
            TotalCost = 0;

            foreach (var segment in Segments)
            {
                TotalLength += segment.GetLength();
                TotalCost += segment.BuildCost;
            }
        }

        /// <summary>
        /// Получает среднюю скорость движения по дороге
        /// </summary>
        public float GetAverageSpeed()
        {
            if (Segments.Count == 0)
                return 0f;

            float totalSpeed = 0f;
            foreach (var segment in Segments)
            {
                totalSpeed += segment.SpeedLimit;
            }

            return totalSpeed / Segments.Count;
        }

        /// <summary>
        /// Проверяет, проходит ли дорога через указанные координаты
        /// </summary>
        public bool PassesThrough(int x, int y)
        {
            foreach (var segment in Segments)
            {
                if (segment.PassesThrough(x, y))
                    return true;
            }
            return false;
        }
    }
}
