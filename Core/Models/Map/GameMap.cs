using Core.Models.Base;
using Core.Models.Roads;

namespace Core.Models.Map
{
    public class GameMap
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Tile[,] Tiles { get; set; }
        public List<Building> Buildings { get; private set; }
        public Building[,] _buildingsGrid;

        public List<Road> Roads { get; private set; }
        public List<RoadSegment> RoadSegments { get; private set; }
        public List<Intersection> Intersections { get; private set; }

        public GameMap(int width, int height)
        {
            Width = width;
            Height = height;
            Tiles = new Tile[Width, Height];
            Buildings = new List<Building>();
            _buildingsGrid = new Building[Width, Height]; // добавил начальную инициализацию, чтобы NullReferenceException на тестах не выкидывало
            Roads = new List<Road>();
            RoadSegments = new List<RoadSegment>();
            Intersections = new List<Intersection>();

            InitializeMap();
        }

        private void InitializeMap()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Tiles[x, y] = new Tile() { X = x, Y = y };
                }
            }
        }

        public bool TryPlaceBuilding(Building building, int x, int y)
        {
            return building.TryPlace(x, y, this);
        }

        public Building GetBuildingAt(int x, int y)
        {
            if (x >= 0 && x < Width &&
                y >= 0 && y < Height)
                return _buildingsGrid[x, y];
            return null;
        }

        public void SetBuildingAt(int x, int y, Building building)
        {
            if (x >= 0 && x < Width &&
                y >= 0 && y < Height)
            {
                _buildingsGrid[x, y] = building;
                Tiles[x, y].Building = building;
            }
        }

        /// <summary>
        /// Добавляет дорогу на карту
        /// </summary>
        public void AddRoad(Road road)
        {
            if (!Roads.Contains(road))
            {
                Roads.Add(road);
                
                // Добавляем все сегменты дороги
                foreach (var segment in road.Segments)
                {
                    AddRoadSegment(segment);
                }
            }
        }

        /// <summary>
        /// Добавляет сегмент дороги на карту
        /// </summary>
        public void AddRoadSegment(RoadSegment segment)
        {
            if (!RoadSegments.Contains(segment))
            {
                RoadSegments.Add(segment);
                
                // Отмечаем тайлы как имеющие дорогу
                MarkTilesAsRoad(segment);
            }
        }

        /// <summary>
        /// Отмечает тайлы как содержащие дорогу
        /// </summary>
        private void MarkTilesAsRoad(RoadSegment segment)
        {
            // Простой способ: отмечаем начальную и конечную точки
            if (segment.StartX >= 0 && segment.StartX < Width && 
                segment.StartY >= 0 && segment.StartY < Height)
            {
                Tiles[segment.StartX, segment.StartY].HasRoad = true;
            }

            if (segment.EndX >= 0 && segment.EndX < Width && 
                segment.EndY >= 0 && segment.EndY < Height)
            {
                Tiles[segment.EndX, segment.EndY].HasRoad = true;
            }

            // Отмечаем промежуточные тайлы
            int steps = (int)segment.GetLength();
            for (int i = 0; i <= steps; i++)
            {
                float t = steps > 0 ? (float)i / steps : 0;
                int x = (int)(segment.StartX + t * (segment.EndX - segment.StartX));
                int y = (int)(segment.StartY + t * (segment.EndY - segment.StartY));

                if (x >= 0 && x < Width && y >= 0 && y < Height)
                {
                    Tiles[x, y].HasRoad = true;
                }
            }
        }

        /// <summary>
        /// Добавляет перекрёсток на карту
        /// </summary>
        public void AddIntersection(Intersection intersection)
        {
            if (!Intersections.Contains(intersection))
            {
                Intersections.Add(intersection);
            }
        }
    }
}
