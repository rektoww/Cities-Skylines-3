using Core.Models.Map;
using System.Collections.Generic;
using Core.Models.Vehicles;

namespace Core.Models.Base;

//TODO: РЕАЛИЗОВАТЬ ПОСЛЕ ИЗМЕНЕНИЙ ТРАНСПОРТА ТИП (КОРАБЛЬ, САМОЛЕТ) ТРАНСПОРТА, КОТОРЫЙ МОЖЕТ ПРИНИМАТЬ ПОРТ
//      ПОКА ЧТО КОРАБЛЬ = САМОЛЕТ ДЛЯ ПОРТА

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
    /// Список прибывших транспортов (для учёта и возможной последующей разгрузки).
    /// </summary>
    public List<CommercialTransport> ArrivedTransports { get; private set; } = new List<CommercialTransport>();

    /// <summary>
    /// Конструктор порта.
    /// </summary>
    public Port()
    {
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
            CurrentTransports++;
    }

    /// <summary>
    /// Уменьшает счетчик транспортов (при удалении транспорта).
    /// </summary>
    public virtual void RemoveTransport()
    {
        if (CurrentTransports > 0)
            CurrentTransports--;
    }

    /// <summary>
    /// Обрабатывает прибытие транспорта (самолет или корабль) в порт.
    /// Проверяет достижение цели и при успешном прибытии добавляет транспорт в список прибывших.
    /// </summary>
    /// <param name="transport">Коммерческий транспорт</param>
    public virtual void ReceiveTransport(CommercialTransport transport)
    {
        // Проверка типа и статуса прибытия
        if (transport is Airplane airplane && airplane.HasReachedDestination)
        {
            if (CanAcceptTransport())
            {
                ArrivedTransports.Add(airplane);
                AddTransport();
                HideTransport(airplane);
            }
        }
        else if (transport is Ship ship && ship.HasReachedDestination)
        {
            if (CanAcceptTransport())
            {
                ArrivedTransports.Add(ship);
                AddTransport();
                HideTransport(ship);
            }
        }
    }


    // TODO: ПО-ХОРОШЕМУ РЕАЛИЗУЕТ МЕТОДЫ КЛАССА Mob
    /// <summary>
    /// Абстрактный метод-заглушка для скрытия транспорта, достигшего конечной цели.
    /// Реализуется в потомках (например, визуальное скрытие, снятие с карты и т.п.).
    /// </summary>
    /// <param name="transport">Транспорт, который нужно скрыть</param>
    public abstract void HideTransport(CommercialTransport transport);


    /// <summary>
    /// Удаляет транспорт с карты либо сам, либо через взаимодействие с классом CommercialTransport.
    /// </summary>
    public abstract void DeleteTransport(CommercialTransport transport);
    
        // TODO: метод для удаления мобов с карты, если транспорт не будет постоянным.
    

    /// <summary>
    /// Получает количество доступных слотов для транспорта.
    /// Использовать для логики отправки грузового транспорта в порт.
    /// </summary>
    public virtual int GetAvailableSlots()
    {
        return Capacity - CurrentTransports;
    }
}
