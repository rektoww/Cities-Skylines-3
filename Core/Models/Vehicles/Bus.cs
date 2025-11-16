using Core.Models.Base;
using Core.Models.Map;

namespace Core.Models.Vehicles;

//public class Bus : PublicTransport
//{
//    private const int DefaultBusCapacity = 80;
//    private const float DefaultBusSpeed = 10f; // Условная скорость

//    public Bus(int x, int y, GameMap map, List<Tile> route)
//        : base(x, y, map, DefaultBusCapacity)
//    {
//        Route = route ?? new List<Tile>();
//        Speed = DefaultBusSpeed;

//        // Если маршрут пуст, назначаем текущую позицию как один из тайлов
//        if (Route.Count == 0 && map != null && X >= 0 && X < map.Width && Y >= 0 && Y < map.Height)
//        {
//            Route.Add(map.Tiles[X, Y]);
//        }

//        // Можно настроить время стоянки на остановках
//        DwellTimeTicks = 1;
//    }

//    /// <summary>
//    /// Делегируем движение в базовый PublicTransport.
//    /// </summary>
//    public override void Move()
//    {
//        base.Move();
//    }
//}