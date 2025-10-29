using System.Collections.Generic;
using Core.Enums;
using Core.Models.Mobs;
using Core.Models.Map;

/// <summary>
/// Добывающее предприятие для природных ресурсов
/// </summary>
public class ExtractionFacility
{
    /// <summary>Название предприятия</summary>
    public string Name { get; set; }
    /// <summary>Тип добываемого ресурса</summary>
    public ResourceType ResourceType { get; set; }
    /// <summary>Скорость добычи ресурсов</summary>
    public int ExtractionRate { get; set; }
    /// <summary>Текущее количество рабочих</summary>
    public int WorkersCount { get; set; }
    /// <summary>Максимальное количество рабочих</summary>
    public int MaxWorkers { get; set; }
    /// <summary>Вместимость хранилища</summary>
    public int StorageCapacity { get; set; }
    /// <summary>Текущее количество ресурсов</summary>
    public int CurrentStorage { get; set; }
    /// <summary>Активно ли предприятие</summary>
    public bool IsActive { get; set; }
    /// <summary>Природный ресурс, с которым работает предприятие</summary>
    public NaturalResource ConnectedResource { get; set; }

    private List<Citizen> _workers = new List<Citizen>();

    /// <summary>
    /// Создает новое добывающее предприятие
    /// </summary>
    public ExtractionFacility(string name, ResourceType resourceType, int storageCapacity, NaturalResource connectedResource = null)
    {
        Name = name;
        ResourceType = resourceType;
        StorageCapacity = storageCapacity;
        ConnectedResource = connectedResource;
        ExtractionRate = 10;
        MaxWorkers = 5;
        IsActive = true;
    }

    /// <summary>
    /// Добавляет рабочего на предприятие
    /// </summary>
    public bool AddWorker(Citizen citizen)
    {
        if (_workers.Count >= MaxWorkers || citizen == null) return false;
        if (!citizen.IsEmployed && citizen.Age >= 18)
        {
            _workers.Add(citizen);
            citizen.IsEmployed = true;
            WorkersCount = _workers.Count;
            RecalculateExtractionRate();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Удаляет рабочего с предприятия
    /// </summary>
    public bool RemoveWorker(Citizen citizen)
    {
        var removed = _workers.Remove(citizen);
        if (removed)
        {
            citizen.IsEmployed = false;
            WorkersCount = _workers.Count;
            RecalculateExtractionRate();
        }
        return removed;
    }

    /// <summary>
    /// Пересчитывает скорость добычи
    /// </summary>
    private void RecalculateExtractionRate()
    {
        ExtractionRate = 10 + (WorkersCount * 2);
    }

    /// <summary>
    /// Выполняет добычу ресурсов
    /// </summary>
    public int ExtractResources()
    {
        if (!IsActive || WorkersCount == 0) return 0;
        if (ConnectedResource != null && ConnectedResource.Amount <= 0) return 0;

        int extractedAmount = ExtractionRate;
        int availableSpace = StorageCapacity - CurrentStorage;

        if (extractedAmount > availableSpace)
            extractedAmount = availableSpace;

        if (ConnectedResource != null && extractedAmount > ConnectedResource.Amount)
            extractedAmount = ConnectedResource.Amount;

        if (ConnectedResource != null)
            ConnectedResource.Amount -= extractedAmount;

        CurrentStorage += extractedAmount;
        return extractedAmount;
    }

    /// <summary>
    /// Собирает добытые ресурсы
    /// </summary>
    public int CollectResources()
    {
        int collected = CurrentStorage;
        CurrentStorage = 0;
        return collected;
    }

    /// <summary>
    /// Проверяет возможность добычи
    /// </summary>
    public bool CanExtract()
    {
        return IsActive && WorkersCount > 0 && CurrentStorage < StorageCapacity &&
               (ConnectedResource == null || ConnectedResource.Amount > 0);
    }

    /// <summary>
    /// Получает список рабочих
    /// </summary>
    public List<Citizen> GetWorkers()
    {
        return new List<Citizen>(_workers);
    }

    /// <summary>
    /// Проверяет, работает ли гражданин на этом предприятии
    /// </summary>
    public bool IsWorker(Citizen citizen)
    {
        return _workers.Contains(citizen);
    }
}