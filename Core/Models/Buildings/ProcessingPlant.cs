using System;
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
    /// <summary>Тип перерабатываемого ресурса</summary>
    public ResourceType InputResourceType { get; private set; }
    /// <summary>Тип производимого продукта</summary>
    public string OutputProductType { get; private set; }
    /// <summary>Скорость переработки</summary>
    public int ProcessingRate { get; private set; }
    /// <summary>Текущее количество рабочих</summary>
    public int WorkersCount { get; private set; }
    /// <summary>Максимальное количество рабочих</summary>
    public int MaxWorkers { get; private set; }
    /// <summary>Вместимость склада сырья</summary>
    public int InputStorageCapacity { get; private set; }
    /// <summary>Вместимость склада продукции</summary>
    public int OutputStorageCapacity { get; private set; }
    /// <summary>Текущее количество сырья</summary>
    public int CurrentInputStorage { get; private set; }
    /// <summary>Текущее количество продукции</summary>
    public int CurrentOutputStorage { get; private set; }
    /// <summary>Коэффициент преобразования (сколько продукции из единицы сырья)</summary>
    public float ConversionRate { get; private set; }

    /// <summary>Производственные цеха завода</summary>
    public List<Workshop> Workshops { get; private set; } = new List<Workshop>();

    /// <summary>
    /// Создает новый перерабатывающий завод
    /// </summary>
    public ProcessingPlant(ResourceType inputResourceType, string outputProductType,
                         int inputStorageCapacity, int outputStorageCapacity)
    {
        InputResourceType = inputResourceType;
        OutputProductType = outputProductType;
        InputStorageCapacity = inputStorageCapacity;
        OutputStorageCapacity = outputStorageCapacity;
        ProcessingRate = 15;
        MaxWorkers = 6;
        ConversionRate = 0.7f;
        WorkersCount = 0;

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
        processingWorkshop.InputRequirements.Add(InputResourceType.ToString(), 10);
        processingWorkshop.OutputProducts.Add(OutputProductType, (int)(10 * ConversionRate));
        Workshops.Add(processingWorkshop);
    }

    /// <summary>
    /// Устанавливает количество рабочих
    /// </summary>
    public void SetWorkersCount(int count)
    {
        WorkersCount = Math.Min(count, MaxWorkers);
        RecalculateProcessingRate();
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
        if (WorkersCount == 0) return 0;
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
        if (WorkersCount == 0) return;

        var availableResources = new Dictionary<object, int>
        {
            { InputResourceType.ToString(), CurrentInputStorage }
        };

        var producedMaterials = new Dictionary<object, int>();

        foreach (var workshop in Workshops)
        {
            workshop.Process(availableResources, producedMaterials);
        }

        // Обновляем хранилища
        CurrentInputStorage = availableResources[InputResourceType.ToString()];

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
    /// Полный производственный цикл завода
    /// </summary>
    public void FullProductionCycle()
    {
        ProcessWorkshops();
    }

    /// <summary>
    /// Получает текущие запасы сырья и продукции
    /// </summary>
    public (int inputMaterials, int outputProducts) GetStorageStatus()
    {
        return (CurrentInputStorage, CurrentOutputStorage);
    }

    /// <summary>
    /// Получает информацию о производственных мощностях
    /// </summary>
    public Dictionary<string, object> GetProductionInfo()
    {
        return new Dictionary<string, object>
        {
            { "InputResourceType", InputResourceType },
            { "OutputProductType", OutputProductType },
            { "ProcessingRate", ProcessingRate },
            { "WorkersCount", WorkersCount },
            { "MaxWorkers", MaxWorkers },
            { "ConversionRate", ConversionRate }
        };
    }
}