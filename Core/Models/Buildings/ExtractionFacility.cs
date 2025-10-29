using System;
using System.Collections.Generic;
using System.Linq;

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
    /// <summary>Стоимость обслуживания</summary>
    public decimal MaintenanceCost { get; set; }

    ///ЖКХ, SmirnovMA - Подключено ли здание к электрической сети
    public bool HasElectricity { get; set; }

    /// Подключено ли здание к водоснабжению
    public bool HasWater { get; set; }

    /// Подключено ли здание к газовой сети
    public bool HasGas { get; set; }

    /// Подключено ли здание к канализации
    public bool HasSewage { get; set; }

    /// Работоспособно ли здание (все коммуникации подключены)
    public bool IsOperational => HasElectricity && HasWater && HasGas && HasSewage;

    private List<Worker> _workers = new List<Worker>();

    /// <summary>
    /// Создает новое добывающее предприятие
    /// </summary>
    public ExtractionFacility(string name, ResourceType resourceType, int storageCapacity)
    {
        Name = name;
        ResourceType = resourceType;
        StorageCapacity = storageCapacity;
        ExtractionRate = 10;
        MaxWorkers = 5;
        MaintenanceCost = 100m;
    }

    /// <summary>
    /// Добавляет рабочего на предприятие
    /// </summary>
    public bool AddWorker(Worker worker)
    {
        if (_workers.Count >= MaxWorkers) return false;
        _workers.Add(worker);
        WorkersCount = _workers.Count;
        RecalculateExtractionRate();
        return true;
    }

    /// <summary>
    /// Удаляет рабочего с предприятия
    /// </summary>
    public bool RemoveWorker(Worker worker)
    {
        var removed = _workers.Remove(worker);
        if (removed)
        {
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

        int extractedAmount = ExtractionRate;
        int availableSpace = StorageCapacity - CurrentStorage;

        if (extractedAmount > availableSpace) extractedAmount = availableSpace;

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
        return IsActive && WorkersCount > 0 && CurrentStorage < StorageCapacity;
    }
}

/// <summary>
/// Рабочий на добывающем предприятии
/// </summary>
public class Worker
{
    /// <summary>Имя рабочего</summary>
    public string Name { get; set; }
    /// <summary>Уровень навыка</summary>
    public int SkillLevel { get; set; }
    /// <summary>Зарплата</summary>
    public decimal Salary { get; set; }
    /// <summary>Доступен ли для работы</summary>
    public bool IsAvailable { get; set; } = true;
}

/// <summary>
/// Типы природных ресурсов
/// </summary>
public enum ResourceType
{
    Wood,
    Stone,
    Iron,
    Copper,
    Oil,
    Gas,
    Coal
}