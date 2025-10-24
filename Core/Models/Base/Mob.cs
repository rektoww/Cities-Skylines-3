using Core.Enums;
using Core.Models.Map;

namespace Core.Models.Base
{
    public abstract class Mob : GameObject
    {
        public GameMap GameMap { get; protected set; }
        public abstract void Move();
        protected Mob(int x, int y, GameMap map)
        {
            X = x;
            Y = y;
            GameMap = map;
        }

        protected bool MoveTo(int toX, int toY)
        {
            if (CanMoveTo(toX, toY))
            {
                X = toX;
                Y = toY;
                return true;
            }
            return false;
        }

        protected bool CanMoveTo(int toX, int toY)
        {
            if (toX < 0 || toY < 0 || 
                toX >= GameMap.Width || toY >= GameMap.Height)
                return false;

            var targetTile = GameMap.Tiles[toX, toY];
            return IsTilePassable(targetTile);

        }
        protected virtual bool IsTilePassable(Tile tile)
        {
            return tile.Terrain != TerrainType.Mountain &&
                   tile.Terrain != TerrainType.Water;
        }
    }
}
