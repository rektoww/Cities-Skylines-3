using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Базовый класс коммерческого здания
/// </summary>
public abstract class CommercialBuilding
{
    /// <summary>Название заведения</summary>
    public string Name { get; set; }
    /// <summary>Тип коммерческого заведения</summary>
    public CommercialType Type { get; set; }
    /// <summary>Вместимость клиентов</summary>
    public int CustomerCapacity { get; set; }
    /// <summary>Текущее количество клиентов</summary>
    public int CurrentCustomers { get; set; }
    /// <summary>Общая выручка</summary>
    public decimal Revenue { get; set; }
    /// <summary>Ежедневные расходы</summary>
    public decimal DailyCosts { get; set; }
    /// <summary>Открыто ли заведение</summary>
    public bool IsOpen { get; set; }
    /// <summary>Список товаров</summary>
    public List<Product> Products { get; set; } = new List<Product>();
    /// <summary>Список сотрудников</summary>
    public List<Employee> Employees { get; set; } = new List<Employee>();

    protected Random _random = new Random();

    /// <summary>
    /// Создает коммерческое здание
    /// </summary>
    public CommercialBuilding(string name, CommercialType type, int capacity)
    {
        Name = name;
        Type = type;
        CustomerCapacity = capacity;
        DailyCosts = 50m;
    }

    /// <summary>
    /// Добавляет клиента
    /// </summary>
    public virtual bool AddCustomer()
    {
        if (CurrentCustomers >= CustomerCapacity || !IsOpen) return false;
        CurrentCustomers++;
        return true;
    }

    /// <summary>
    /// Удаляет клиента
    /// </summary>
    public virtual void RemoveCustomer()
    {
        if (CurrentCustomers > 0) CurrentCustomers--;
    }

    /// <summary>
    /// Обрабатывает бизнес-процессы
    /// </summary>
    public abstract void ProcessBusiness();

    /// <summary>
    /// Открывает заведение
    /// </summary>
    public virtual void Open()
    {
        IsOpen = true;
        CurrentCustomers = 0;
    }

    /// <summary>
    /// Закрывает заведение
    /// </summary>
    public virtual void Close()
    {
        IsOpen = false;
        CurrentCustomers = 0;
    }

    /// <summary>
    /// Добавляет товар
    /// </summary>
    public bool AddProduct(Product product)
    {
        if (Products.Count >= 10) return false;
        Products.Add(product);
        return true;
    }

    /// <summary>
    /// Удаляет товар
    /// </summary>
    public bool RemoveProduct(string productName)
    {
        var product = Products.FirstOrDefault(p => p.Name == productName);
        return product != null && Products.Remove(product);
    }

    /// <summary>
    /// Нанять сотрудника
    /// </summary>
    public bool HireEmployee(Employee employee)
    {
        if (Employees.Count >= 3) return false;
        Employees.Add(employee);
        return true;
    }

    /// <summary>
    /// Рассчитывает ежедневную прибыль
    /// </summary>
    public decimal CalculateDailyProfit()
    {
        return Revenue - DailyCosts - Employees.Sum(e => e.Salary);
    }
}

/// <summary>
/// Магазин
/// </summary>
public class Store : CommercialBuilding
{
    /// <summary>Вместимость склада</summary>
    public int StockCapacity { get; set; }
    /// <summary>Текущий запас товаров</summary>
    public int CurrentStock { get; set; }

    /// <summary>
    /// Создает магазин
    /// </summary>
    public Store(string name) : base(name, CommercialType.Store, 20)
    {
        StockCapacity = 100;
        DailyCosts = 80m;
    }

    /// <summary>
    /// Обрабатывает бизнес магазина
    /// </summary>
    public override void ProcessBusiness()
    {
        if (!IsOpen) return;

        int potentialSales = _random.Next(0, CurrentCustomers + 1);
        decimal dailyRevenue = 0;

        for (int i = 0; i < potentialSales && CurrentStock > 0; i++)
        {
            var product = Products[_random.Next(Products.Count)];
            dailyRevenue += product.Price;
            CurrentStock--;
        }

        Revenue += dailyRevenue;

        int leavingCustomers = _random.Next(0, CurrentCustomers / 2);
        for (int i = 0; i < leavingCustomers; i++)
        {
            RemoveCustomer();
        }

        int newCustomers = _random.Next(0, CustomerCapacity - CurrentCustomers);
        for (int i = 0; i < newCustomers; i++)
        {
            AddCustomer();
        }
    }

    /// <summary>
    /// Пополняет запасы
    /// </summary>
    public bool Restock(int amount)
    {
        if (CurrentStock + amount > StockCapacity) return false;
        CurrentStock += amount;
        return true;
    }
}

/// <summary>
/// Кафе
/// </summary>
public class Cafe : CommercialBuilding
{
    /// <summary>Качество еды</summary>
    public int FoodQuality { get; set; }
    /// <summary>Количество столов</summary>
    public int TablesCount { get; set; }

    /// <summary>
    /// Создает кафе
    /// </summary>
    public Cafe(string name) : base(name, CommercialType.Cafe, 30)
    {
        FoodQuality = 5;
        TablesCount = 10;
        DailyCosts = 120m;
    }

    /// <summary>
    /// Обрабатывает бизнес кафе
    /// </summary>
    public override void ProcessBusiness()
    {
        if (!IsOpen) return;

        decimal baseRevenuePerCustomer = 5.0m;
        decimal qualityMultiplier = FoodQuality * 0.2m;
        decimal dailyRevenue = CurrentCustomers * baseRevenuePerCustomer * qualityMultiplier;

        Revenue += dailyRevenue;

        int satisfactionRate = _random.Next(FoodQuality * 5, 101);
        int stayingCustomers = (int)(CurrentCustomers * (satisfactionRate / 100.0));

        CurrentCustomers = stayingCustomers;

        int attractionRate = FoodQuality * 3;
        int newCustomers = _random.Next(0, Math.Min(attractionRate, CustomerCapacity - CurrentCustomers));
        for (int i = 0; i < newCustomers; i++)
        {
            AddCustomer();
        }
    }

    /// <summary>
    /// Улучшает качество еды
    /// </summary>
    public void ImproveFoodQuality()
    {
        if (FoodQuality < 10) FoodQuality++;
        DailyCosts += 10m;
    }
}

/// <summary>
/// Заправочная станция
/// </summary>
public class GasStation : CommercialBuilding
{
    /// <summary>Вместимость топлива</summary>
    public int FuelCapacity { get; set; }
    /// <summary>Текущее количество топлива</summary>
    public int CurrentFuel { get; set; }
    /// <summary>Цена топлива</summary>
    public decimal FuelPrice { get; set; }

    /// <summary>
    /// Создает заправочную станцию
    /// </summary>
    public GasStation(string name) : base(name, CommercialType.GasStation, 8)
    {
        FuelCapacity = 10000;
        CurrentFuel = FuelCapacity;
        FuelPrice = 2.0m;
        DailyCosts = 200m;
    }

    /// <summary>
    /// Обрабатывает бизнес заправки
    /// </summary>
    public override void ProcessBusiness()
    {
        if (!IsOpen || CurrentFuel == 0) return;

        decimal dailyRevenue = 0;
        foreach (var _ in Enumerable.Range(0, CurrentCustomers))
        {
            int fuelAmount = _random.Next(10, 51);
            if (fuelAmount > CurrentFuel)
            {
                fuelAmount = CurrentFuel;
            }

            decimal sale = fuelAmount * FuelPrice;
            dailyRevenue += sale;
            CurrentFuel -= fuelAmount;

            if (CurrentFuel == 0) break;
        }

        Revenue += dailyRevenue;
        CurrentCustomers = 0;

        int newCustomers = _random.Next(0, CustomerCapacity);
        for (int i = 0; i < newCustomers; i++)
        {
            AddCustomer();
        }
    }

    /// <summary>
    /// Пополняет запасы топлива
    /// </summary>
    public bool Refuel(int amount)
    {
        if (CurrentFuel + amount > FuelCapacity) return false;
        CurrentFuel += amount;
        return true;
    }

    /// <summary>
    /// Устанавливает цену на топливо
    /// </summary>
    public void SetFuelPrice(decimal newPrice)
    {
        FuelPrice = newPrice;
    }
}

/// <summary>
/// Товар
/// </summary>
public class Product
{
    /// <summary>Название товара</summary>
    public string Name { get; set; }
    /// <summary>Цена товара</summary>
    public decimal Price { get; set; }
    /// <summary>Категория товара</summary>
    public ProductCategory Category { get; set; }
}

/// <summary>
/// Сотрудник
/// </summary>
public class Employee
{
    /// <summary>Имя сотрудника</summary>
    public string Name { get; set; }
    /// <summary>Должность</summary>
    public string Position { get; set; }
    /// <summary>Зарплата</summary>
    public decimal Salary { get; set; }
    /// <summary>Уровень навыка</summary>
    public int SkillLevel { get; set; }
}

/// <summary>
/// Типы коммерческих заведений
/// </summary>
public enum CommercialType
{
    Store,
    Cafe,
    GasStation,
    Restaurant,
    Mall
}

/// <summary>
/// Категории товаров
/// </summary>
public enum ProductCategory
{
    Food,
    Electronics,
    Clothing,
    Furniture,
    Automotive
}