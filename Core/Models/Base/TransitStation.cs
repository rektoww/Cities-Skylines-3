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
        // Базовая стоимость и размеры могут быть переопределены в наследниках.
        Width = 1; 
        Height = 1;
        Floors = 1;
        BuildCost = 1000m; 
    }

    /// <summary>
    /// Попытка добавить гражданина в список ожидания.
    /// </summary>
    public void AddWaitingCitizen(Citizen citizen)
    {
        // Здесь может быть логика, проверяющая, действительно ли гражданин на плитке станции.
        WaitingCitizens.Add(citizen);
    }

    /// <summary>
    /// Попытка удалить гражданина из списка ожидания.
    /// </summary>
    public bool RemoveWaitingCitizen(Citizen citizen)
    {
        return WaitingCitizens.Remove(citizen);
    }

    // Мы не реализуем CanPlace здесь, потому что требования к размещению
    // (на дороге, на трамвайных путях, на ж/д путях) сильно отличаются.
    // Это остается задачей конкретных наследников.
}