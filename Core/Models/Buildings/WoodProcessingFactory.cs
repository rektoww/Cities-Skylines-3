using System;
using System.Collections.Generic;
using Core.Enums;
using Core.Models.Mobs;
using Core.Models.Map;
using Core.Models.Components;

/// <summary>
/// Деревообрабатывающая фабрика
/// </summary>
public class WoodProcessingFactory
{
    /// <summary>Название фабрики</summary>
    public string Name { get; set; }
    /// <summary>Количество сырой древесины</summary>
    public int WoodStorage { get; set; }
    /// <summary>Максимум сырой древесины</summary>
    public int MaxWoodStorage { get; set; }
    /// <summary>Активна ли фабрика</summary>
    public bool IsActive { get; set; }
    /// <summary>Лесной ресурс, с которым работает фабрика</summary>
    public NaturalResource ConnectedForest { get; set; }
    /// <summary>Производственные цеха фабрики</summary>
    public List<Workshop> Workshops { get; set; } = new List<Workshop>();

    private List<Citizen> _workers = new List<Citizen>();
    /// <summary>Текущее количество рабочих</summary>
    public int WorkersCount { get; set; }
    /// <summary>Максимальное количество рабочих</summary>
    public int MaxWorkers { get; set; }

    /// <summary>
    /// Создает новую деревообрабатывающую фабрику
    /// </summary>
    public WoodProcessingFactory(string name, NaturalResource connectedForest = null)
    {
        Name = name;
        ConnectedForest = connectedForest;
        MaxWoodStorage = 1000;
        MaxWorkers = 8;
        IsActive = true;

        InitializeWorkshops();
    }

    /// <summary>
    /// Инициализирует цеха фабрики
    /// </summary>
    private void InitializeWorkshops()
    {
        // Цех обработки древесины
        var woodProcessing = new Workshop
        {
            Name = "Цех обработки древесины",
            ProductionCycleTime = 5
        };
        woodProcessing.InputRequirements.Add("Древесина", 10);
        woodProcessing.OutputProducts.Add("Обработанная древесина", 8);
        Workshops.Add(woodProcessing);

        // Цех производства мебели
        var furnitureWorkshop = new Workshop
        {
            Name = "Мебельный цех",
            ProductionCycleTime = 8
        };
        furnitureWorkshop.InputRequirements.Add("Обработанная древесина", 5);
        furnitureWorkshop.OutputProducts.Add("Мебель", 1);
        Workshops.Add(furnitureWorkshop);

        // Цех производства бумаги
        var paperWorkshop = new Workshop
        {
            Name = "Цех производства бумаги",
            ProductionCycleTime = 6
        };
        paperWorkshop.InputRequirements.Add("Древесина", 3);
        paperWorkshop.OutputProducts.Add("Бумага", 5);
        Workshops.Add(paperWorkshop);
    }

    /// <summary>
    /// Добавляет рабочего на фабрику
    /// </summary>
    public bool AddWorker(Citizen citizen)
    {
        if (_workers.Count >= MaxWorkers || citizen == null) return false;
        if (!citizen.IsEmployed && citizen.Age >= 18)
        {
            _workers.Add(citizen);
            citizen.IsEmployed = true;
            WorkersCount = _workers.Count;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Удаляет рабочего с фабрики
    /// </summary>
    public bool RemoveWorker(Citizen citizen)
    {
        var removed = _workers.Remove(citizen);
        if (removed)
        {
            citizen.IsEmployed = false;
            WorkersCount = _workers.Count;
        }
        return removed;
    }

    /// <summary>
    /// Добывает древесину из леса
    /// </summary>
    public int ExtractWood()
    {
        if (!IsActive || WorkersCount == 0) return 0;
        if (ConnectedForest != null && ConnectedForest.Amount <= 0) return 0;

        int extractionRate = 20 + (WorkersCount * 3);
        int woodToExtract = extractionRate;
        int availableSpace = MaxWoodStorage - WoodStorage;

        if (woodToExtract > availableSpace)
            woodToExtract = availableSpace;

        if (ConnectedForest != null && woodToExtract > ConnectedForest.Amount)
            woodToExtract = ConnectedForest.Amount;

        if (ConnectedForest != null)
            ConnectedForest.Amount -= woodToExtract;

        WoodStorage += woodToExtract;
        return woodToExtract;
    }

    /// <summary>
    /// Добавляет сырую древесину на фабрику
    /// </summary>
    public bool AddWood(int amount)
    {
        if (WoodStorage + amount > MaxWoodStorage) return false;
        WoodStorage += amount;
        return true;
    }

    /// <summary>
    /// Запускает производственные циклы во всех цехах
    /// </summary>
    public void ProcessWorkshops()
    {
        if (!IsActive || WorkersCount == 0) return;

        var availableResources = new Dictionary<object, int>
        {
            { "Древесина", WoodStorage }
        };

        foreach (var workshop in Workshops)
        {
            var workshopOutputs = new Dictionary<object, int>();
            workshop.Process(availableResources, workshopOutputs);

            // Обновляем доступные ресурсы после каждого цеха
            foreach (var output in workshopOutputs)
            {
                if (availableResources.ContainsKey(output.Key))
                    availableResources[output.Key] += output.Value;
                else
                    availableResources[output.Key] = output.Value;
            }
        }

        // Обновляем хранилище древесины
        WoodStorage = availableResources.ContainsKey("Древесина") ? availableResources["Древесина"] : 0;
    }

    /// <summary>
    /// Полный рабочий цикл фабрики
    /// </summary>
    public void FullProductionCycle()
    {
        ExtractWood();
        ProcessWorkshops();
    }

    /// <summary>
    /// Проверяет возможность добычи древесины
    /// </summary>
    public bool CanExtractWood()
    {
        return IsActive && WorkersCount > 0 && WoodStorage < MaxWoodStorage &&
               (ConnectedForest == null || ConnectedForest.Amount > 0);
    }

    /// <summary>
    /// Получает список рабочих
    /// </summary>
    public List<Citizen> GetWorkers()
    {
        return new List<Citizen>(_workers);
    }

    /// <summary>
    /// Проверяет, работает ли гражданин на этой фабрике
    /// </summary>
    public bool IsWorker(Citizen citizen)
    {
        return _workers.Contains(citizen);
    }
}