using System;
using System.Collections.Generic;

/// <summary>
/// Деревообрабатывающая фабрика
/// </summary>
public class WoodProcessingFactory
{
    /// <summary>Название фабрики</summary>
    public string Name { get; set; }
    /// <summary>Количество сырой древесины</summary>
    public int WoodStorage { get; set; }
    /// <summary>Количество обработанной древесины</summary>
    public int ProcessedWoodStorage { get; set; }
    /// <summary>Количество мебели</summary>
    public int FurnitureStorage { get; set; }
    /// <summary>Количество бумаги</summary>
    public int PaperStorage { get; set; }
    /// <summary>Скорость обработки древесины</summary>
    public int WoodProcessingRate { get; set; }
    /// <summary>Скорость производства мебели</summary>
    public int FurnitureProductionRate { get; set; }
    /// <summary>Скорость производства бумаги</summary>
    public int PaperProductionRate { get; set; }
    /// <summary>Максимум сырой древесины</summary>
    public int MaxWoodStorage { get; set; }
    /// <summary>Максимум обработанной древесины</summary>
    public int MaxProcessedWoodStorage { get; set; }
    /// <summary>Максимум мебели</summary>
    public int MaxFurnitureStorage { get; set; }
    /// <summary>Максимум бумаги</summary>
    public int MaxPaperStorage { get; set; }
    /// <summary>Активна ли фабрика</summary>
    public bool IsActive { get; set; }
    /// <summary>Операционные расходы</summary>
    public decimal OperationalCost { get; set; }

    /// <summary>
    /// Создает новую деревообрабатывающую фабрику
    /// </summary>
    public WoodProcessingFactory(string name)
    {
        Name = name;
        MaxWoodStorage = 1000;
        MaxProcessedWoodStorage = 500;
        MaxFurnitureStorage = 100;
        MaxPaperStorage = 200;
        WoodProcessingRate = 20;
        FurnitureProductionRate = 5;
        PaperProductionRate = 10;
        OperationalCost = 150m;
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
    /// Обрабатывает древесину
    /// </summary>
    public void ProcessWood()
    {
        if (!IsActive || WoodStorage < WoodProcessingRate) return;

        int woodToProcess = Math.Min(WoodStorage, WoodProcessingRate);
        WoodStorage -= woodToProcess;

        int processedWoodOutput = (int)(woodToProcess * 0.8);
        if (ProcessedWoodStorage + processedWoodOutput <= MaxProcessedWoodStorage)
        {
            ProcessedWoodStorage += processedWoodOutput;
        }
    }

    /// <summary>
    /// Производит мебель
    /// </summary>
    public void ProduceFurniture()
    {
        if (!IsActive || ProcessedWoodStorage < FurnitureProductionRate * 2) return;

        ProcessedWoodStorage -= FurnitureProductionRate * 2;
        if (FurnitureStorage < MaxFurnitureStorage)
        {
            FurnitureStorage += FurnitureProductionRate;
        }
    }

    /// <summary>
    /// Производит бумагу
    /// </summary>
    public void ProducePaper()
    {
        if (!IsActive || WoodStorage < PaperProductionRate) return;

        WoodStorage -= PaperProductionRate;
        if (PaperStorage < MaxPaperStorage)
        {
            PaperStorage += PaperProductionRate;
        }
    }

    /// <summary>
    /// Собирает произведенную мебель
    /// </summary>
    public int CollectFurniture()
    {
        int collected = FurnitureStorage;
        FurnitureStorage = 0;
        return collected;
    }

    /// <summary>
    /// Собирает произведенную бумагу
    /// </summary>
    public int CollectPaper()
    {
        int collected = PaperStorage;
        PaperStorage = 0;
        return collected;
    }

    /// <summary>
    /// Собирает обработанную древесину
    /// </summary>
    public int CollectProcessedWood()
    {
        int collected = ProcessedWoodStorage;
        ProcessedWoodStorage = 0;
        return collected;
    }

    /// <summary>
    /// Получает статус производства
    /// </summary>
    public ProductionStatus GetStatus()
    {
        return new ProductionStatus
        {
            WoodAmount = WoodStorage,
            ProcessedWoodAmount = ProcessedWoodStorage,
            FurnitureAmount = FurnitureStorage,
            PaperAmount = PaperStorage,
            CanProcessWood = WoodStorage >= WoodProcessingRate,
            CanProduceFurniture = ProcessedWoodStorage >= FurnitureProductionRate * 2,
            CanProducePaper = WoodStorage >= PaperProductionRate
        };
    }
}

/// <summary>
/// Статус производства на фабрике
/// </summary>
public class ProductionStatus
{
    /// <summary>Количество сырой древесины</summary>
    public int WoodAmount { get; set; }
    /// <summary>Количество обработанной древесины</summary>
    public int ProcessedWoodAmount { get; set; }
    /// <summary>Количество мебели</summary>
    public int FurnitureAmount { get; set; }
    /// <summary>Количество бумаги</summary>
    public int PaperAmount { get; set; }
    /// <summary>Можно ли обрабатывать древесину</summary>
    public bool CanProcessWood { get; set; }
    /// <summary>Можно ли производить мебель</summary>
    public bool CanProduceFurniture { get; set; }
    /// <summary>Можно ли производить бумагу</summary>
    public bool CanProducePaper { get; set; }
}