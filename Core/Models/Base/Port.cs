using Core.Models.Map;
using System.Collections.Generic;

namespace Core.Models.Base;

/// <summary>
/// Абстрактный базовый класс для всех портов (воздушных и морских).
/// Наследуется от TransitStation, предоставляет базовую логику для портов.
/// Сочетает функции остановки общественного транспорта и пункта прибытия коммерческих грузов.
/// </summary>
public abstract class Port : TransitStation
{
    /// <summary>
    /// Вместимость порта (максимальное количество транспортов, которые можно отправить в порт).
    /// </summary>
    public int Capacity { get; protected set; }

    /// <summary>
    /// Текущее количество транспортов, направленных в порт.
    /// </summary>
    public int CurrentTransports { get; protected set; }

    /// <summary>
    /// Конструктор порта.
    /// </summary>
    public Port()
    {
        // Базовые размеры порта
        Width = 2;
        Height = 2;
        Floors = 1;
        Capacity = 5;
        CurrentTransports = 0;
    }

    /// <summary>
    /// Проверяет, может ли порт принять еще один транспорт.
    /// </summary>
    public virtual bool CanAcceptTransport()
    {
        return CurrentTransports < Capacity;
    }

    /// <summary>
    /// Увеличивает счетчик транспортов, направленных в порт.
    /// </summary>
    public virtual void AddTransport()
    {
        if (CanAcceptTransport())
        {
            CurrentTransports++;
        }
    }

    /// <summary>
    /// Уменьшает счетчик транспортов (при удалении транспорта).
    /// </summary>
    public virtual void RemoveTransport()
    {
        if (CurrentTransports > 0)
        {
            CurrentTransports--;
        }
    }

    /// <summary>
    /// Удаляет транспорт с карты
    /// </summary>
    public virtual void DeleteTransport()
    {
        // TODO: НУЖЕН МЕТОД ДЛЯ УДАЛЕНИЯ МОБОВ С КАРТЫ, ЕСЛИ ТРАНСПОРТ НЕ БУДЕТ ПОСТОЯННЫМ.


    }

    /// <summary>
    /// Получает количество доступных слотов для транспорта.
    /// </summary>
    public virtual int GetAvailableSlots()
    {
        return Capacity - CurrentTransports;
    }

}