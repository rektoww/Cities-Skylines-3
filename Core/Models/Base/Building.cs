using Core.Enums;
using Core.Interfaces;
using Core.Models.Map;

namespace Core.Models.Base
{
    public abstract class Building : GameObject, IConstructable
    {

        #region Обязательные поля для строительства (IConstructable)
        public static decimal BuildCost { get; protected set; }
        public static Dictionary<ConstructionMaterial, int> RequiredMaterials { get; protected set; }
        #endregion

        public int Floors { get; set; }
        public float Condition { get; set; } = 100f;
        public int Width { get; set; }
        public int Height { get; set; }
        public int MaxOccupancy { get; set; }
        public int CurrentOccupancy { get; set; }
        /// <summary>
        /// SmirnovMA - СИСТЕМА ЖКХ - ПОДКЛЮЧЕНИЕ К КОММУНАЛЬНЫМ СЕТЯМ
        /// </summary>

        /// Подключено ли здание к электрической сети
        public bool HasElectricity { get; set; }

        /// Подключено ли здание к водоснабжению
        public bool HasWater { get; set; }

        /// Подключено ли здание к газовой сети
        public bool HasGas { get; set; }

        /// Подключено ли здание к канализации
        public bool HasSewage { get; set; }

        /// Работоспособно ли здание (все коммуникации подключены)
        public bool IsOperational => HasElectricity && HasWater && HasGas && HasSewage;

        public GameMap GameMap { get; protected set; }

        /// <summary>
        /// Конструктор здания с значениями по умолчанию
        /// </summary>
        /// <param name="HasWater"> водоснабжение </param>
        /// <param name="HasGas"> газоснабжение </param>
        /// <param name="HasSewage"> канализация </param>
        /// <param name="HasElectricity"> электроснабжение </param>
        /// <param name="Floors"> кол-во этажей </param>
        /// <param name="BuildCoast"> стоимость потройки </param>
        /// <param name="Width"> ширина на карте (x) </param>
        /// <param name="Height"> длина на карте (y) </param>
        protected Building(
            bool HasWater = true,
            bool HasGas = true,
            bool HasSewage = true,
            bool HasElectricity = true,
            int Floors = 1,
            decimal _BuildCost = 0m,
            
            int Width = 1,
            int Height = 1
            )
        {
            this.HasWater = HasWater;
            this.HasGas = HasGas;
            this.HasSewage = HasSewage;
            this.HasElectricity = HasElectricity;
            this.Floors = Floors;
            BuildCost = _BuildCost;

            this.Width = Width;
            this.Height = Height;
        }

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
            if (x < 0 || x + Width > map.Width ||
                y < 0 || y + Height > map.Height)
                return false;

            for (int tileX = x; tileX < x + Width; tileX++)
            {
                for (int tileY = y; tileY < y + Height; tileY++)
                {
                    var tile = map.Tiles[tileX, tileY];
                    if (!IsTileSuitableForBuilding(tile))
                        return false;

                    if (map.GetBuildingAt(tileX, tileY) != null)
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
