using Core.Models.Base;
using Core.Models.Map;

namespace Core.Models.Buildings;

/// <summary>
/// Конкретная реализация остановки для автобусов.
/// Должна быть размещена на дороге.
/// </summary>
public class BusStop : TransitStation
{
    public BusStop()
    {
        // Переопределяем параметры, если нужно.
        Width = 1;
        Height = 1;
        BuildCost = 500m; // Автобусная остановка дешевле вокзала.
    }

    /// <summary>
    /// Логика размещения: остановка должна быть на тайле с дорогой.
    /// </summary>
    public override bool CanPlace(int x, int y, GameMap map)
    {
        // 1. Сначала выполняем базовую проверку (границы, нет зданий и т.д.)
        if (!base.CanPlace(x, y, map))
            return false;

        // 2. Специфическая проверка: тайл должен иметь дорогу.
        var tile = map.Tiles[x, y];
            
        if (!tile.HasRoad) 
            return false;
            
        return true;
    }

    public override void OnBuildingPlaced()
    {
        // Логика, специфичная для автобусной остановки, например, 
        // автоматическое добавление к ближайшему маршруту.
    }
}