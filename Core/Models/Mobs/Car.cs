using Core.Models.Base;
using Core.Models.Map;

namespace Core.Models.Mobs
{
    /// <summary>
    /// Класс для личного транспорта граждан.
    /// </summary>
    public class Car : Transport
    {
        private const int DefaultCarCapacity = 4; // 1 водитель + 3 пассажира

        public Citizen Owner { get; set; }
        public Citizen Driver { get; set; }

        public Car(int x, int y, GameMap map) 
            : base(x, y, map, DefaultCarCapacity) // Вызываем конструктор Vehicle
        {
            Speed = 8f;
        }
        
        public override void Move()
        {
            // TODO: Реализовать логику движения личного транспорта к пункту назначения
            
            // Для начала, просто случайное блуждание, пока не будет реализована логика пути
            X += new Random().Next(-1, 2);
            Y += new Random().Next(-1, 2);
            
            // Ограничение границ карты, если необходимо
            X = Math.Clamp(X, 0, GameMap.Width - 1);
            Y = Math.Clamp(Y, 0, GameMap.Height - 1);
            
            // Пассажиры и водитель двигаются вместе с машиной
            foreach (var passenger in Passengers)
            {
                passenger.X = X;
                passenger.Y = Y;
            }
            if (Driver != null)
            {
                Driver.X = X;
                Driver.Y = Y;
            }
        }
    }
}
