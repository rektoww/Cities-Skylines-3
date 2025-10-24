using Core.Enums;
using Core.Models.Base;

namespace Core.Models.Map
{
    public class Tile
    {
        public int X { get; set; }
        public int Y { get; set; }
        public TerrainType Terrain { get; set; }
        public float Height { get; set; }
        public List<NaturalResource> Resources { get; set; }

        public Building Building { get; set; }
        public bool HasRoad { get; set; }

        public Tile()
        {
            Resources = new List<NaturalResource>();
        }
    }
}
