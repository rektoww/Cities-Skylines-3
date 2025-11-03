using Core.Models.Base;

namespace Core.Models.Map
{
    public class GameMap
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Tile[,] Tiles { get; set; }
        public List<Building> Buildings { get; private set; }
        public Building[,] _buildingsGrid;

        public GameMap(int width, int height)
        {
            Width = width;
            Height = height;
            Tiles = new Tile[Width, Height];
            Buildings = new List<Building>();
            _buildingsGrid = new Building[Width, Height]; // добавил начальную инициализацию, чтобы NullReferenceException на тестах не выкидывало

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
    }
}