using Core.Models.Map;
using System.Collections.Generic;

namespace Core.Models.Base;

/// <summary>
/// Абстрактный базовый класс для всех портов (воздушных и морских).
/// Наследуется от Building, предоставляет базовую логику для портов.
/// </summary>
public abstract class Port : Building
{

    /// <summary>
    /// Список коммерческих транспортов, приписанных к порту.
    /// </summary>
    public List<CommercialTransport> AssignedTransports { get; set; } = new List<CommercialTransport>();

    /// <summary>
    /// Вместимость порта (максимальное количество транспортов).
    /// </summary>
    public int Capacity { get; protected set; }

    /// <summary>
    /// Конструктор порта.
    /// </summary>
    public Port() : base(Width: 2, Height: 2)
    {
        Capacity = 5;
    }

    /// <summary>
    /// Добавляет коммерческий транспорт в порт.
    /// </summary>
    public virtual bool AssignTransport(CommercialTransport transport)
    {
        if (AssignedTransports.Count < Capacity)
        {
            AssignedTransports.Add(transport);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Удаляет коммерческий транспорт из порта.
    /// </summary>
    public virtual bool RemoveTransport(CommercialTransport transport)
    {
        return AssignedTransports.Remove(transport);
    }

    /// <summary>
    /// Проверяет, может ли порт принять еще один транспорт.
    /// </summary>
    public virtual bool CanAcceptTransport()
    {
        return AssignedTransports.Count < Capacity;
    }

    public override void OnBuildingPlaced()
    {
        // Базовая логика размещения порта
    }
}