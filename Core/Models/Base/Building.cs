using Core.Enums;
using Core.Enums.Core.Enums;
using Core.Interfaces;
using Core.Models.Map;
using System.Collections.Generic;

namespace Core.Models.Base
{
    public abstract class Building : GameObject, IConstructable
    {
        // Базовые свойства
        public int Floors { get; set; } = 1;
        public float Condition { get; set; } = 100f;
        public int Width { get; set; } = 1;
        public int Height { get; set; } = 1;
        public int MaxOccupancy { get; set; }
        public int CurrentOccupancy { get; set; }

        // Коммуникации
        public bool HasElectricity { get; set; } = true;
        public bool HasWater { get; set; } = true;
        public bool HasGas { get; set; } = true;
        public bool HasSewage { get; set; } = true;
        public bool IsOperational => HasElectricity && HasWater && HasGas && HasSewage;

        public abstract decimal BuildCost { get; }
        public abstract Dictionary<ConstructionMaterial, int> RequiredMaterials { get; }
        public abstract BuildingType BuildingType { get; }

        public GameMap GameMap { get; protected set; }

        protected Building() { }

        public bool TryPlace(int x, int y, GameMap map)
        {
            if (!CanPlace(x, y, map))
                return false;

            X = x;
            Y = y;
            GameMap = map;
            OccupyTiles();
            OnBuildingPlaced();
            return true;
        }

        public abstract void OnBuildingPlaced();

        public virtual bool CanPlace(int x, int y, GameMap map)
        {
            if (x < 0 || x + Width > map.Width || y < 0 || y + Height > map.Height)
                return false;

            for (int tileX = x; tileX < x + Width; tileX++)
            {
                for (int tileY = y; tileY < y + Height; tileY++)
                {
                    var tile = map.Tiles[tileX, tileY];
                    if (!IsTileSuitableForBuilding(tile) || map.GetBuildingAt(tileX, tileY) != null)
                        return false;
                }
            }
            return true;
        }

        protected virtual bool IsTileSuitableForBuilding(Tile tile)
        {
            return tile.Terrain != TerrainType.Mountain &&
                   tile.Terrain != TerrainType.Water &&
                   tile.Height <= 0.3f;
        }

        protected virtual void OccupyTiles()
        {
            for (int tileX = X; tileX < X + Width; tileX++)
            {
                for (int tileY = Y; tileY < Y + Height; tileY++)
                {
                    GameMap.SetBuildingAt(tileX, tileY, this);
                }
            }
        }
    }
}