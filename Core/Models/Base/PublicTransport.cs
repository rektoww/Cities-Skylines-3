using Core.Models.Map;

namespace Core.Models.Base;

/// <summary>
/// Абстрактный базовый класс для всех видов общественного транспорта.
/// Наследуется от Mob, чтобы иметь базовые свойства движения (X, Y, GameMap).
/// </summary>
public abstract class PublicTransport(int x, int y, GameMap map, int capacity) 
    : Transport(x, y, map, capacity)
{
    /// <summary>
    /// Маршрут, по которому движется транспорт (последовательность координат или BusStop).
    /// Мы пока будем использовать ListTile для простоты.
    /// </summary>
    public List<Tile> Route { get; set; } = new List<Tile>();

    /// <summary>
    /// Индекс следующей точки маршрута, к которой направляется транспорт.
    /// </summary>
    public int NextWaypointIndex { get; protected set; } = 0;
}