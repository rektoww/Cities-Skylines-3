using System.Collections.Generic;
using Core.Enums;
using Core.Models.Mobs;
using Core.Models.Map;

/// <summary>
/// Добывающее предприятие для природных ресурсов
/// </summary>
public class ExtractionFacility
{
    /// <summary>Тип добываемого ресурса</summary>
    public ResourceType ResourceType { get; private set; }
    /// <summary>Скорость добычи ресурсов</summary>
    public int ExtractionRate { get; private set; }
    /// <summary>Текущее количество рабочих</summary>
    public int WorkersCount { get; private set; }
    /// <summary>Максимальное количество рабочих</summary>
    public int MaxWorkers { get; private set; }
    /// <summary>Вместимость хранилища</summary>
    public int StorageCapacity { get; private set; }
    /// <summary>Текущее количество ресурсов</summary>
    public int CurrentStorage { get; private set; }
    /// <summary>Природный ресурс, с которым работает предприятие</summary>
    public NaturalResource ConnectedResource { get; private set; }

    /// <summary>
    /// Создает новое добывающее предприятие
    /// </summary>
    public ExtractionFacility(ResourceType resourceType, int storageCapacity, NaturalResource connectedResource = null)
    {
        ResourceType = resourceType;
        StorageCapacity = storageCapacity;
        ConnectedResource = connectedResource;
        ExtractionRate = 10;
        MaxWorkers = 5;
        WorkersCount = 0;
    }

    /// <summary>
    /// Устанавливает количество рабочих
    /// </summary>
    public void SetWorkersCount(int count)
    {
        WorkersCount = Math.Min(count, MaxWorkers);
        RecalculateExtractionRate();
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
        if (WorkersCount == 0) return 0;
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
    /// Получает текущие запасы ресурсов
    /// </summary>
    public int GetStoredResources()
    {
        return CurrentStorage;
    }

    /// <summary>
    /// Устанавливает подключенный природный ресурс
    /// </summary>
    public void SetConnectedResource(NaturalResource resource)
    {
        ConnectedResource = resource;
    }
}