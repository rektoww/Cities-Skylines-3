using Core.Models.Map;
using Core.Models.Mobs;
using System.Collections.Generic;

namespace Core.Models.Base;

/// <summary>
/// Абстрактный базовый класс для всего колесного транспорта (личного и общественного).
/// Наследуется от Mob, чтобы иметь базовые свойства движения (X, Y, Speed).
/// </summary>
public abstract class Transport(int x, int y, GameMap map, int capacity)
    : Mob(x, y, map)
{
    /// <summary>
    /// Максимальное количество пассажиров (включая водителя, если применимо).
    /// </summary>
    public int MaxCapacity { get; protected set; } = capacity;

    /// <summary>
    /// Список текущих пассажиров. Ссылка только для чтения.
    /// </summary>
    public List<Citizen> Passengers { get; } = new List<Citizen>();

    public float Speed { get; protected set; } = 0;

    /// <summary>
    /// Флаг, указывающий, может ли транспорт принять еще пассажиров.
    /// </summary>
    public bool CanAcceptPassenger => Passengers.Count < MaxCapacity;


    /// <summary>
    /// Базовая логика посадки пассажира. 
    /// Может быть переопределена в наследниках (например, для проверки билетов).
    /// </summary>
    public virtual bool TryBoard(Citizen citizen)
    {
        if (CanAcceptPassenger)
        {
            Passengers.Add(citizen);
            // Наследники должны убедиться, что X и Y пассажира синхронизированы с транспортом.
            return true;
        }
        return false;
    }

    /// <summary>
    /// Базовая логика высадки пассажира.
    /// </summary>
    public virtual bool TryDisembark(Citizen citizen)
    {
        return Passengers.Remove(citizen);
    }
}