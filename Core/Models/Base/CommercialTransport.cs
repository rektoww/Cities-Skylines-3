using Core.Models.Map;
using Core.Enums;
using System.Collections.Generic;

namespace Core.Models.Base;

// TODO: ПОКА КРИВАЯ РЕАЛИЗАЦИЯ Т.К. ПИШЕТСЯ ПОД КОРАБЛИ И САМОЛЕТЫ
//      ЧТО ПЕРЕВОЗЯТ И ПАССАЖИРОВ, И ТОВАРЫ
//      ДАЛЬНЕЙШЯЯ РЕДАКТУРА ЧЕРЕЗ ДАНЮ И ОБСУЖДЕНИЕ
//      КАК ТАКОВЫХ ПРОТИВОРЕЧИЙ С РОДИТЕЛЕМ НЕТ
//
// НУЖНО ПЕРЕДЕЛАТЬ ВСЕ КЛАССЫ ДЛЯ СФЕРЫ ТРАНСПОРТА:
//      РАЗДЕЛЕНИЕ НА ОБЩЕСТВЕННЫЙ И КОММЕРЧЕСКИЙ ТРАНСПОРТ?
//      ОПРЕДЕЛЕНИЕ ЦЕЛИ ТРАНСПОРТА В НАСЛЕДНИКЕ?
//
// НУЖНО ОПРЕДЕЛИТЬСЯ, БУДУТ ЛИ ВОДИТЕЛИ/ПИЛОТЫ/КАПИТАНЫ. ЕСЛИ ДА - 
//      СДЕЛАТЬ ВЕСЬ КОММЕРЧЕСКИЙ ТРАНСПОРТ ТРАНСПОРТ ПОСТОЯННЫМ МОБОМ -> ДОПИЛИТЬ КЛАССЫ
//      ЧТОБЫ БЫЛО ЛИБО РАСПИСАНИЕ, ЛИБО МАРШРУТ (PublicTransport)
//      ТОГДА НУЖНО БУДЕТ АВТОМАТИЧЕСКИ СОЗДАВАТЬ ТРАНСПОРТ ПРИ РАСПОЛОЖЕНИИ ПОРТА
//
// ДОПОЛНИТЬ PlayerResources: МАТЕРИАЛЫ ДЛЯ ТРАНСПОРТИРОВКИ БЕРУТСЯ ОТТУДА, А НЕ С ЗАВОДОВ (?) 
//
// ВНЕШНИЙ ВИД (ПИКЧА, ЧТО ПО КАРТЕ ДВИГАЕТСЯ) ОПРЕДЕЛЯЕТСЯ ПРИ СОЗДАНИИ МОБА В ИГРЕ? ЕСЛИ НЕТ - ПРИДЕТСЯ СИЛЬНО МЕНЯТЬ ЛОГИКУ ТРАНСПОРТА.

/// <summary>
/// Абстрактный базовый класс для коммерческого транспорта: самолеты, корабли и (ВОЗМОЖНО) грузовые машины (ПЕРЕМЕЩЕНИЕ РЕСУРСОВ ПО ГОРОДУ).
/// Наследуется от Transport, но работает с товарами и ресурсами вместо пассажиров.
/// Тип транспорта (корабль, самолет) определяется в наследниках через параметры и путь движения.
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
    /// Маршрут, по которому движется коммерческий транспорт.
    /// </summary>
    public List<Tile> Route { get; set; } = new List<Tile>();



    /// <summary>
    /// Попытка загрузить товар на транспорт.
    /// </summary>
    /// <param name="material">Тип материала</param>
    /// <param name="amount">Количество</param>
    /// <returns>True если загрузка успешна</returns>
    public virtual bool LoadCargo(TransportedGoods material, int amount)
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