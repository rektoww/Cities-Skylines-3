using Core.Models.Base;
using Core.Models.Map;

namespace Core.Models.Mobs
{
    public class Car : Mob
    {
        public Car(int x, int y, GameMap map) : base(x, y, map) { }

        public override void Move()
        {
            throw new NotImplementedException();
        }
    }
}
