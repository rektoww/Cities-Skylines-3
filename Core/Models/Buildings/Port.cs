using Core.Enums;
using Core.Interfaces;
using Core.Models.Base;
using Core.Models.Map;
using System.Collections.Generic;

namespace Core.Models.Buildings;

/// <summary>
/// Конкретная реализация порта для размещения на карте.
/// Принимает как коммерческие транспорты (самолеты, корабли), так и общественный транспорт.
/// Наследуется от абстрактного класса Port.
/// </summary>
public class Port : Base.Port, IConstructable<Port>
{
    // Статические свойства для интерфейса IConstructable
    public static decimal BuildCost { get; protected set; } = 150000m;

    public static Dictionary<ConstructionMaterial, int> RequiredMaterials { get; protected set; }
        = new Dictionary<ConstructionMaterial, int>
        {
            { ConstructionMaterial.Steel, 25 },
            { ConstructionMaterial.Concrete, 40 },
            { ConstructionMaterial.Glass, 10 }
        };

    /// <summary>
    /// Конструктор порта
    /// </summary>
    public Port()
    {
        // Настройки для универсального порта
        Width = 3;
        Height = 3;
        Floors = 2;
        Capacity = 10; // Универсальный порт может принимать разный транспорт
    }

    /// <summary>
    /// Логика размещения порта
    /// </summary>
    public override bool CanPlace(int x, int y, GameMap map)
    {
        // Базовая проверка размещения
        if (!base.CanPlace(x, y, map))
            return false;

        // Порт может быть размещен на любой подходящей местности
        // TODO: ПРОПИСАТЬ УСЛОВИЯ ДЛЯ АЭРОПОРТА И МОРСКОГОПОРТА
        return true;
    }

    public override void HideTransport(CommercialTransport transport)
    {
        // TODO: реализовать сокрытие транспорта по достижению цели, когда отработаем абстрактные классы.
    }


    public override void DeleteTransport(CommercialTransport transport)
    {
        // TODO: сделать удаление, если транспорт движется в один конец

    }

    public override void OnBuildingPlaced()
    {
        // TODO: та же залупа, что и у общественного транспорта.
    }
}