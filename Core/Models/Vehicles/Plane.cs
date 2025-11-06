using Core.Models.Map;
using Core.Models.Base;
using System.Collections.Generic;

namespace Core.Models.Vehicles;

public class Airplane : CommercialTransport
{
    private const float DefaultAirplaneSpeed = 50f;

    /// <summary>
    /// Порт отправления
    /// </summary>
    public Port DeparturePort { get; private set; }

    /// <summary>
    /// Порт назначения
    /// </summary>
    public Port DestinationPort { get; private set; }

    /// <summary>
    /// Флаг достижения цели
    /// </summary>
    public bool HasReachedDestination { get; private set; }

    public Airplane(int x, int y, GameMap map, Port departurePort, Port destinationPort, int capacity = 100)
        : base(x, y, map, capacity)
    {
        DeparturePort = departurePort;
        DestinationPort = destinationPort;
        Speed = DefaultAirplaneSpeed;
        HasReachedDestination = false;

        // Создаем маршрут по прямой между портами
        CreateStraightRoute();
    }

    /// <summary>
    /// Создает маршрут по прямой между портами отправления и назначения
    /// </summary>
    private void CreateStraightRoute()
    {
        Route.Clear();

        // Добавляем точку отправления (центр порта)
        int startX = DeparturePort.X + DeparturePort.Width / 2;
        int startY = DeparturePort.Y + DeparturePort.Height / 2;
        Route.Add(GameMap.Tiles[startX, startY]);

        // Добавляем точку назначения (центр порта)
        int endX = DestinationPort.X + DestinationPort.Width / 2;
        int endY = DestinationPort.Y + DestinationPort.Height / 2;
        Route.Add(GameMap.Tiles[endX, endY]);
    }

    /// <summary>
    /// Реализация движения самолета по прямой между портами
    /// </summary>
    public override void Move()
    {
        if (HasReachedDestination || Route.Count < 2)
            return;

        var targetTile = Route[1]; // Вторая точка - порт назначения

        // Расчет направления
        float deltaX = targetTile.X - X;
        float deltaY = targetTile.Y - Y;

        // Длина до цели
        double distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

        // Если достигли цели
        if (distance < 1.0f)
        {
            X = targetTile.X;
            Y = targetTile.Y;
            HasReachedDestination = true;
            return;
        }

        // Нормализация вектора и движение
        float moveX = (float)(deltaX / distance * Speed);
        float moveY = (float)(deltaY / distance * Speed);

        X += (int)moveX;
        Y += (int)moveY;
    }

    /// <summary>
    /// Проверяет, достиг ли самолет пункта назначения
    /// </summary>
    public bool CheckDestinationReached()
    {
        if (!HasReachedDestination && Route.Count >= 2)
        {
            var targetTile = Route[1];
            double distance = Math.Sqrt(Math.Pow(targetTile.X - X, 2) + Math.Pow(targetTile.Y - Y, 2));

            if (distance < 1.0f)
            {
                HasReachedDestination = true;
            }
        }

        return HasReachedDestination;
    }
}