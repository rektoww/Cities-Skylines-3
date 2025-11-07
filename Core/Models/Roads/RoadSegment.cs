using Core.Enums;

namespace Core.Models.Roads
{
    /// <summary>
    /// Представляет сегмент дороги между двумя точками.
    /// Используется для построения дорожной сети города.
    /// </summary>
    public class RoadSegment
    {
        private double x1;
        private double y1;
        private int x2;
        private int y2;
        private RoadType selectedRoadType;

        public int StartX { get; set; }
        public int StartY { get; set; }
        public int EndX { get; set; }
        public int EndY { get; set; }
        public RoadType RoadType { get; set; }
        public float SpeedLimit { get; set; }
        public decimal BuildCost { get; set; }

        /// <summary>
        /// Конструктор сегмента дороги
        /// </summary>
        public RoadSegment(int startX, int startY, int endX, int endY, RoadType roadType)
        {
            StartX = startX;
            StartY = startY;
            EndX = endX;
            EndY = endY;
            RoadType = roadType;

            // Устанавливаем характеристики в зависимости от типа дороги
            SetRoadCharacteristics();
        }

        public RoadSegment(double x1, double y1, int x2, int y2, RoadType selectedRoadType)
        {
            this.x1 = x1;
            this.y1 = y1;
            this.x2 = x2;
            this.y2 = y2;
            this.selectedRoadType = selectedRoadType;
        }

        /// <summary>
        /// Устанавливает характеристики дороги (скорость, стоимость) в зависимости от типа
        /// </summary>
        private void SetRoadCharacteristics()
        {
            switch (RoadType)
            {
                case RoadType.Dirt:
                    SpeedLimit = 20f;
                    BuildCost = 100m;
                    break;

                case RoadType.Street:
                    SpeedLimit = 40f;
                    BuildCost = 500m;
                    break;

                case RoadType.Avenue:
                    SpeedLimit = 60f;
                    BuildCost = 1500m;
                    break;

                case RoadType.Highway:
                    SpeedLimit = 100f;
                    BuildCost = 3000m;
                    break;

                default:
                    SpeedLimit = 30f;
                    BuildCost = 300m;
                    break;
            }
        }

        /// <summary>
        /// Вычисляет длину сегмента дороги
        /// </summary>
        /// <returns>Длина сегмента</returns>
        public double GetLength()
        {
            int deltaX = EndX - StartX;
            int deltaY = EndY - StartY;
            return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }

        /// <summary>
        /// Проверяет, проходит ли сегмент через указанные координаты
        /// </summary>
        public bool PassesThrough(int x, int y)
        {
            // Проверяем, находится ли точка на линии сегмента
            double distance = DistanceFromPoint(x, y);
            return distance < 1.0; // Допуск для попадания на линию
        }

        /// <summary>
        /// Вычисляет расстояние от точки до линии сегмента
        /// </summary>
        private double DistanceFromPoint(int x, int y)
        {
            double A = x - StartX;
            double B = y - StartY;
            double C = EndX - StartX;
            double D = EndY - StartY;

            double dot = A * C + B * D;
            double lenSq = C * C + D * D;

            if (lenSq == 0) return Math.Sqrt(A * A + B * B);

            double param = dot / lenSq;

            double xx, yy;

            if (param < 0)
            {
                xx = StartX;
                yy = StartY;
            }
            else if (param > 1)
            {
                xx = EndX;
                yy = EndY;
            }
            else
            {
                xx = StartX + param * C;
                yy = StartY + param * D;
            }

            double dx = x - xx;
            double dy = y - yy;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
