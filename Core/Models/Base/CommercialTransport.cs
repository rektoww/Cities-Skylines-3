using Core.Models.Map;
using Core.Enums;
using System.Collections.Generic;

namespace Core.Models.Base;

/// <summary>
/// Абстрактный базовый класс для коммерческого транспорта (грузовые перевозки).
/// Наследуется от Transport, но работает с товарами и ресурсами вместо пассажиров.
/// </summary>
public abstract class CommercialTransport(int x, int y, GameMap map, int capacity)
    : Transport(x, y, map, capacity)
{
    /// <summary>
    /// Словарь перевозимых товаров и ресурсов.
    /// Key - тип материала, Value - количество
    /// </summary>
    public Dictionary<TransportedGoods, int> Cargo { get; private set; } = new Dictionary<TransportedGoods, int>();

    /// <summary>
    /// Общее количество товаров на борту.
    /// </summary>
    public int TotalCargoAmount => Cargo.Values.Sum();

    /// <summary>
    /// Флаг, указывающий, может ли транспорт принять еще товаров.
    /// </summary>ф
    public bool CanAcceptCargo => TotalCargoAmount < MaxCapacity;

    /// <summary>
    /// Флаг, указывающий что товары были успешно доставлены и транспорт должен быть удален.
    /// </summary>
    public bool ShouldBeRemoved { get; protected set; } = false;

    /// <summary>
    /// Маршрут, по которому движется коммерческий транспорт.
    /// </summary>
    public List<Tile> Route { get; set; } = new List<Tile>();


    /// <summary>
    /// Попытка загрузить товар на транспорт.
    /// </summary>
    /// <param name="material">Тип материала</param>
    /// <param name="amount">Количество</param>
    /// <returns>True если загрузка успешна</returns>
    public virtual bool TryLoadCargo(TransportedGoods material, int amount)
    {
        if (!CanAcceptCargo || amount <= 0)
            return false;

        int availableSpace = MaxCapacity - TotalCargoAmount;
        int amountToLoad = Math.Min(amount, availableSpace);

        if (amountToLoad > 0)
        {
            if (Cargo.ContainsKey(material))
            {
                Cargo[material] += amountToLoad;
            }
            else
            {
                Cargo[material] = amountToLoad;
            }
            return true;
        }

        return false;
    }

    /// <summary>
    /// Помечает транспорт для удаления после успешной доставки.
    /// </summary>
    public virtual void MarkForRemoval()
    {
        ShouldBeRemoved = true;
    }

    /// <summary>
    /// Получить количество конкретного товара на борту.
    /// </summary>
    public int GetCargoAmount(TransportedGoods material)
    {
        return Cargo.GetValueOrDefault(material, 0);
    }

    /// <summary>
    /// Проверить, есть ли место для загрузки указанного количества товара.
    /// </summary>
    public bool CanLoadAmount(int amount)
    {
        return TotalCargoAmount + amount <= MaxCapacity;
    }

    /// <summary>
    /// Получить список всех типов товаров на борту.
    /// </summary>
    public List<TransportedGoods> GetCargoTypes()
    {
        return Cargo.Keys.ToList();
    }
}