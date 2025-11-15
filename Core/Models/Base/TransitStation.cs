using Core.Enums;
using Core.Interfaces;
using Core.Models.Mobs;

namespace Core.Models.Base;

/// <summary>
/// Абстрактный базовый класс для всех остановок, станций и вокзалов.
/// Содержит логику для управления ожидающими гражданами и обслуживаемыми маршрутами.
/// </summary>
public abstract class TransitStation : Building
{
    /// <summary>
    /// Граждане, ожидающие транспорт на этой станции.
    /// </summary>
    public List<Citizen> WaitingCitizens { get; set; } = new List<Citizen>();

    /// <summary>
    /// Список маршрутов общественного транспорта, которые обслуживают эту станцию.
    /// Используем базовый PublicTransport, чтобы можно было добавлять и Bus, и Tram.
    /// </summary>
    public List<PublicTransport> ServicedRoutes { get; set; } = new List<PublicTransport>();

    // Конструктор
    public TransitStation()
    {
        Width = 1;
        Height = 1;
        Floors = 1;
    }

    /// <summary>
    /// Попытка добавить гражданина в список ожидания.
    /// </summary>
    public void AddWaitingCitizen(Citizen citizen)
    {
        if (citizen == null) return;

        if (WaitingCitizens.Contains(citizen)) return;

        WaitingCitizens.Add(citizen);

        citizen.TargetTransitStation = this;

        citizen.X = this.X;
        citizen.Y = this.Y;
    }

    /// <summary>
    /// Попытка удалить гражданина из списка ожидания.
    /// </summary>
    public bool RemoveWaitingCitizen(Citizen citizen)
    {
        return WaitingCitizens.Remove(citizen);
    }
}