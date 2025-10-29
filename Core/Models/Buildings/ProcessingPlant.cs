using System.Collections.Generic;
using Core.Enums;
using Core.Models.Mobs;
using Core.Models.Map;
using Core.Models.Components;

/// <summary>
/// Перерабатывающий завод
/// </summary>
public class ProcessingPlant
{
    /// <summary>Название завода</summary>
    public string Name { get; set; }
    /// <summary>Тип перерабатываемого ресурса</summary>
    public ResourceType InputResourceType { get; set; }
    /// <summary>Тип производимого продукта</summary>
    public string OutputProductType { get; set; }
    /// <summary>Скорость переработки</summary>
    public int ProcessingRate { get; set; }
    /// <summary>Текущее количество рабочих</summary>
    public int WorkersCount { get; set; }
    /// <summary>Максимальное количество рабочих</summary>
    public int MaxWorkers { get; set; }
    /// <summary>Вместимость склада сырья</summary>
    public int InputStorageCapacity { get; set; }
    /// <summary>Вместимость склада продукции</summary>
    public int OutputStorageCapacity { get; set; }
    /// <summary>Текущее количество сырья</summary>
    public int CurrentInputStorage { get; set; }
    /// <summary>Текущее количество продукции</summary>
    public int CurrentOutputStorage { get; set; }
    /// <summary>Активен ли завод</summary>
    public bool IsActive { get; set; }
    /// <summary>Коэффициент преобразования (сколько продукции из единицы сырья)</summary>
    public float ConversionRate { get; set; }

    /// <summary>Производственные цеха завода</summary>
    public List<Workshop> Workshops { get; set; } = new List<Workshop>();

    private List<Citizen> _workers = new List<Citizen>();

    /// <summary>
    /// Создает новый перерабатывающий завод
    /// </summary>
    public ProcessingPlant(string name, ResourceType inputResourceType, string outputProductType,
                         int inputStorageCapacity, int outputStorageCapacity)
    {
        Name = name;
        InputResourceType = inputResourceType;
        OutputProductType = outputProductType;
        InputStorageCapacity = inputStorageCapacity;
        OutputStorageCapacity = outputStorageCapacity;
        ProcessingRate = 15;
        MaxWorkers = 6;
        ConversionRate = 0.7f;
        IsActive = true;

        InitializeWorkshops();
    }

    /// <summary>
    /// Инициализирует цеха завода
    /// </summary>
    private void InitializeWorkshops()
    {
        // Основной цех переработки
        var processingWorkshop = new Workshop
        {
            Name = $"Цех переработки {InputResourceType}",
            ProductionCycleTime = 8
        };
        processingWorkshop.InputRequirements.Add(InputResourceType, 10);
        processingWorkshop.OutputProducts.Add(OutputProductType, (int)(10 * ConversionRate));
        Workshops.Add(processingWorkshop);
    }

    /// <summary>
    /// Добавляет рабочего на завод
    /// </summary>
    public bool AddWorker(Citizen citizen)
    {
        if (_workers.Count >= MaxWorkers || citizen == null) return false;
        if (!citizen.IsEmployed && citizen.Age >= 18)
        {
            _workers.Add(citizen);
            citizen.IsEmployed = true;
            WorkersCount = _workers.Count;
            RecalculateProcessingRate();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Удаляет рабочего с завода
    /// </summary>
    public bool RemoveWorker(Citizen citizen)
    {
        var removed = _workers.Remove(citizen);
        if (removed)
        {
            citizen.IsEmployed = false;
            WorkersCount = _workers.Count;
            RecalculateProcessingRate();
        }
        return removed;
    }

    /// <summary>
    /// Пересчитывает скорость переработки
    /// </summary>
    private void RecalculateProcessingRate()
    {
        ProcessingRate = 15 + (WorkersCount * 3);
    }

    /// <summary>
    /// Добавляет сырье на склад
    /// </summary>
    public bool AddRawMaterials(int amount)
    {
        if (CurrentInputStorage + amount > InputStorageCapacity) return false;
        CurrentInputStorage += amount;
        return true;
    }

    /// <summary>
    /// Выполняет переработку сырья
    /// </summary>
    public int ProcessMaterials()
    {
        if (!IsActive || WorkersCount == 0) return 0;
        if (CurrentInputStorage < ProcessingRate) return 0;

        int materialsToProcess = Math.Min(CurrentInputStorage, ProcessingRate);
        int outputAmount = (int)(materialsToProcess * ConversionRate);
        int availableSpace = OutputStorageCapacity - CurrentOutputStorage;

        if (outputAmount > availableSpace)
        {
            outputAmount = availableSpace;
            materialsToProcess = (int)(outputAmount / ConversionRate);
        }

        CurrentInputStorage -= materialsToProcess;
        CurrentOutputStorage += outputAmount;
        return outputAmount;
    }

    /// <summary>
    /// Запускает производственные циклы во всех цехах
    /// </summary>
    public void ProcessWorkshops()
    {
        if (!IsActive || WorkersCount == 0) return;

        var availableResources = new Dictionary<object, int>
        {
            { InputResourceType, CurrentInputStorage }
        };

        var producedMaterials = new Dictionary<object, int>();

        foreach (var workshop in Workshops)
        {
            workshop.Process(availableResources, producedMaterials);
        }

        // Обновляем хранилища
        CurrentInputStorage = availableResources[InputResourceType];

        // Добавляем произведенную продукцию
        foreach (var product in producedMaterials)
        {
            if (product.Key.ToString() == OutputProductType)
            {
                if (CurrentOutputStorage + product.Value <= OutputStorageCapacity)
                {
                    CurrentOutputStorage += product.Value;
                }
            }
        }
    }

    /// <summary>
    /// Собирает произведенную продукцию
    /// </summary>
    public int CollectProducts()
    {
        int collected = CurrentOutputStorage;
        CurrentOutputStorage = 0;
        return collected;
    }

    /// <summary>
    /// Проверяет возможность переработки
    /// </summary>
    public bool CanProcess()
    {
        return IsActive && WorkersCount > 0 &&
               CurrentInputStorage >= ProcessingRate &&
               CurrentOutputStorage < OutputStorageCapacity;
    }

    /// <summary>
    /// Полный производственный цикл завода
    /// </summary>
    public void FullProductionCycle()
    {
        ProcessWorkshops();
    }

    /// <summary>
    /// Получает список рабочих
    /// </summary>
    public List<Citizen> GetWorkers()
    {
        return new List<Citizen>(_workers);
    }

    /// <summary>
    /// Проверяет, работает ли гражданин на этом заводе
    /// </summary>
    public bool IsWorker(Citizen citizen)
    {
        return _workers.Contains(citizen);
    }
}