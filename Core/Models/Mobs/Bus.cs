using Core.Models.Base;
using Core.Models.Map;

namespace Core.Models.Mobs;

public class Bus : PublicTransport
{
    private const int DefaultBusCapacity = 80;
    private const float DefaultBusSpeed = 10f; // Условная скорость

    public Bus(int x, int y, GameMap map, List<Tile> route) 
        : base(x, y, map, DefaultBusCapacity)
    {
        Route = route;
        Speed = DefaultBusSpeed;
        
        // Если маршрут пуст, назначаем случайное движение
        if (Route.Count == 0)
        {
            // Задаем простейший начальный маршрут:
            // Например, текущая позиция и какая-то произвольная точка.
            Route.Add(map.Tiles[X, Y]);
            // TODO: Реализовать логику генерации маршрута.
        }
    }

    /// <summary>
    /// Реализация движения Bus по заданному маршруту (Route).
    /// </summary>
    public override void Move()
    {
        if (Route == null || Route.Count <= 1)
        {
            // Нет маршрута - стоим
            return;
        }

        // Цель - следующий пункт маршрута
        var targetTile = Route[NextWaypointIndex];

        // TODO: движение потом изменить, когда карта готова (все что ниже)
        
        // Расчет направления (очень упрощенно)
        float deltaX = targetTile.X - X;
        float deltaY = targetTile.Y - Y;
        
        // Длина до цели
        double distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        
        // Если мы почти достигли цели, переходим к следующему пункту
        if (distance < 0.5) 
        {
            NextWaypointIndex = (NextWaypointIndex + 1) % Route.Count; // Цикличный маршрут
            // TODO: Тут должна быть логика остановки, высадки/посадки.
            
            // Обновляем цель на новый NextWaypointIndex
            targetTile = Route[NextWaypointIndex]; 
            deltaX = targetTile.X - X;
            deltaY = targetTile.Y - Y;
            distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }

        // Нормализация вектора и движение
        float moveX = (float)(deltaX / distance * Speed);
        float moveY = (float)(deltaY / distance * Speed);

        X += (int)moveX;
        Y += (int)moveY;

        // Также нужно обновить положение всех пассажиров!
        foreach (var passenger in Passengers)
        {
            passenger.X = X;
            passenger.Y = Y;
        }
    }
}