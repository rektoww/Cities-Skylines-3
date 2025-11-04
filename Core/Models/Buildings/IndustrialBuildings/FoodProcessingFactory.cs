using Core.Enums;
using Core.Interfaces;
using Core.Models.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Models.Buildings.IndustrialBuildings
{
    /// <summary>
    /// Фабрика пищевой промышленности - производит различные продукты питания
    /// из сельскохозяйственного сырья для снабжения магазинов и населения
    /// </summary>
    public class FoodProcessingFactory : CommercialBuilding, IConstructable<FoodProcessingFactory>
    {
        #region Static Properties - Construction Cost

        /// <summary>
        /// Стоимость постройки пищевой фабрики
        /// </summary>
        public static decimal BuildCost { get; protected set; } = 320000m;

        /// <summary>
        /// Материалы, необходимые для строительства пищевой фабрики
        /// </summary>
        public static Dictionary<ConstructionMaterial, int> RequiredMaterials { get; protected set; }
            = new Dictionary<ConstructionMaterial, int>
            {
                { ConstructionMaterial.Steel, 8 },
                { ConstructionMaterial.Concrete, 10 },
                { ConstructionMaterial.Glass, 3 },
                { ConstructionMaterial.Plastic, 4 }
            };

        #endregion

        #region Simplified Enums

        /// <summary>
        /// Типы сырья для пищевой промышленности
        /// </summary>
        public enum FoodMaterial
        {
            Wheat,      // Пшеница
            Milk,       // Молоко
            Meat,       // Мясо
            Vegetables, // Овощи
            Fruits,     // Фрукты
            Sugar,      // Сахар
            Eggs        // Яйца
        }

        /// <summary>
        /// Типы производимой пищевой продукции
        /// </summary>
        public enum FoodProduct
        {
            Bread,          // Хлеб
            Pasta,          // Макароны
            Cheese,         // Сыр
            Yogurt,         // Йогурт
            Sausages,       // Колбасы
            CannedVegetables, // Консервированные овощи
            Juice,          // Сок
            Jam,            // Варенье
            Butter,         // Масло
            Eggs            // Яйца (упаковка)
        }

        #endregion

        /// <summary>Количество сырья на складе</summary>
        public Dictionary<FoodMaterial, int> MaterialsStorage { get; private set; } = new Dictionary<FoodMaterial, int>();

        /// <summary>Количество продукции на складе</summary>
        public Dictionary<FoodProduct, int> ProductsStorage { get; private set; } = new Dictionary<FoodProduct, int>();

        /// <summary>Максимальная вместимость склада сырья</summary>
        public int MaxMaterialStorage { get; private set; }

        /// <summary>Максимальная вместимость склада продукции</summary>
        public int MaxProductStorage { get; private set; }

        /// <summary>Производственные цеха фабрики</summary>
        public List<Workshop> Workshops { get; private set; } = new List<Workshop>();

        /// <summary>Текущее количество рабочих</summary>
        public int WorkersCount { get; private set; }

        /// <summary>Максимальное количество рабочих</summary>
        public int MaxWorkers { get; private set; }

        /// <summary>
        /// Коэффициент эффективности производства в зависимости от количества рабочих
        /// Базовая эффективность 40% + пропорционально количеству рабочих до 100%
        /// </summary>
        public float ProductionEfficiency => WorkersCount > 0 ? 0.4f + WorkersCount / (float)MaxWorkers * 0.6f : 0f;

        /// <summary>
        /// Создает новую фабрику пищевой промышленности
        /// Выходные данные: инициализированная фабрика с цехами и стартовыми материалами
        /// </summary>
        public FoodProcessingFactory() : base(CommercialBuildingType.Factory)
        {
            MaxMaterialStorage = 1500;  // Большой склад сырья для сельхозпродукции
            MaxProductStorage = 1000;   // Вместительный склад готовой продукции
            MaxWorkers = 15;           // Крупное производство требует больше рабочих
            WorkersCount = 0;

            InitializeWorkshops();
            InitializeStartingMaterials();
        }

        /// <summary>
        /// Инициализирует производственные цеха пищевой фабрики
        /// Входные данные: отсутствуют
        /// Выходные данные: созданные и настроенные производственные цеха
        /// </summary>
        private void InitializeWorkshops()
        {
            // Хлебопекарный цех - производит хлеб и макароны из пшеницы
            var bakeryWorkshop = new Workshop
            {
                Name = "Хлебопекарный цех",
                ProductionCycleTime = 6  // Быстрое производство
            };
            bakeryWorkshop.InputRequirements.Add("Wheat", 10);
            bakeryWorkshop.InputRequirements.Add("Sugar", 2);
            bakeryWorkshop.OutputProducts.Add("Bread", 12);
            bakeryWorkshop.OutputProducts.Add("Pasta", 8);
            Workshops.Add(bakeryWorkshop);

            // Молочный цех - производит молочные продукты
            var dairyWorkshop = new Workshop
            {
                Name = "Молочный цех",
                ProductionCycleTime = 8  // Среднее время для ферментации
            };
            dairyWorkshop.InputRequirements.Add("Milk", 15);
            dairyWorkshop.InputRequirements.Add("Sugar", 3);
            dairyWorkshop.OutputProducts.Add("Cheese", 5);
            dairyWorkshop.OutputProducts.Add("Yogurt", 10);
            dairyWorkshop.OutputProducts.Add("Butter", 4);
            Workshops.Add(dairyWorkshop);

            // Мясной цех - производит мясные продукты
            var meatWorkshop = new Workshop
            {
                Name = "Мясной цех",
                ProductionCycleTime = 10  // Дольше из-за обработки мяса
            };
            meatWorkshop.InputRequirements.Add("Meat", 8);
            meatWorkshop.InputRequirements.Add("Eggs", 4);
            meatWorkshop.OutputProducts.Add("Sausages", 6);
            Workshops.Add(meatWorkshop);

            // Консервный цех - производит консервы и соки
            var canningWorkshop = new Workshop
            {
                Name = "Консервный цех",
                ProductionCycleTime = 12  // Дольше из-за процесса консервации
            };
            canningWorkshop.InputRequirements.Add("Vegetables", 10);
            canningWorkshop.InputRequirements.Add("Fruits", 8);
            canningWorkshop.InputRequirements.Add("Sugar", 5);
            canningWorkshop.OutputProducts.Add("CannedVegetables", 8);
            canningWorkshop.OutputProducts.Add("Juice", 12);
            canningWorkshop.OutputProducts.Add("Jam", 6);
            Workshops.Add(canningWorkshop);

            // Цех упаковки яиц
            var eggsWorkshop = new Workshop
            {
                Name = "Цех упаковки яиц",
                ProductionCycleTime = 4  // Быстрая упаковка
            };
            eggsWorkshop.InputRequirements.Add("Eggs", 20);
            eggsWorkshop.OutputProducts.Add("Eggs", 15);  // Упакованные яйца
            Workshops.Add(eggsWorkshop);
        }

        /// <summary>
        /// Инициализирует стартовые запасы сырья на фабрике
        /// Входные данные: отсутствуют
        /// Выходные данные: заполненное хранилище начальным количеством сырья
        /// </summary>
        private void InitializeStartingMaterials()
        {
            // Стартовые запасы сырья для начала производства
            AddMaterial(FoodMaterial.Wheat, 300);
            AddMaterial(FoodMaterial.Milk, 200);
            AddMaterial(FoodMaterial.Meat, 150);
            AddMaterial(FoodMaterial.Vegetables, 250);
            AddMaterial(FoodMaterial.Fruits, 180);
            AddMaterial(FoodMaterial.Sugar, 100);
            AddMaterial(FoodMaterial.Eggs, 120);
        }

        /// <summary>
        /// Устанавливает количество рабочих на фабрике
        /// Входные данные: count - количество рабочих для установки
        /// Выходные данные: отсутствуют (устанавливает значение WorkersCount)
        /// </summary>
        /// <param name="count">Количество рабочих для установки</param>
        public void SetWorkersCount(int count)
        {
            WorkersCount = Math.Min(count, MaxWorkers);
        }

        /// <summary>
        /// Добавляет сырье на склад фабрики
        /// Входные данные: material - тип сырья, amount - количество для добавления
        /// Выходные данные: true если сырье успешно добавлено, false если превышена вместимость склада
        /// </summary>
        /// <param name="material">Тип добавляемого сырья</param>
        /// <param name="amount">Количество сырья для добавления</param>
        /// <returns>True если сырье успешно добавлено, false в противном случае</returns>
        public bool AddMaterial(FoodMaterial material, int amount)
        {
            int currentAmount = MaterialsStorage.ContainsKey(material) ? MaterialsStorage[material] : 0;
            if (GetTotalMaterialStorage() + amount > MaxMaterialStorage)
                return false;

            MaterialsStorage[material] = currentAmount + amount;
            return true;
        }

        /// <summary>
        /// Получает общее количество сырья на складе
        /// Входные данные: отсутствуют
        /// Выходные данные: общее количество всего сырья на складе
        /// </summary>
        /// <returns>Общее количество сырья</returns>
        public int GetTotalMaterialStorage()
        {
            return MaterialsStorage.Values.Sum();
        }

        /// <summary>
        /// Запускает производственные циклы во всех цехах фабрики
        /// Входные данные: текущее количество сырья и состояние хранилища продукции
        /// Выходные данные: отсутствуют (обновляет количество сырья и хранилище продукции)
        /// </summary>
        public void ProcessWorkshops()
        {
            if (WorkersCount == 0 || ProductionEfficiency <= 0) return;

            var availableResources = new Dictionary<object, int>();

            // Конвертируем enum в строки для совместимости с Workshop
            foreach (var material in MaterialsStorage)
            {
                availableResources.Add(material.Key.ToString(), material.Value);
            }

            // Добавляем существующую продукцию как доступный ресурс для цехов
            foreach (var product in ProductsStorage)
            {
                availableResources.Add(product.Key.ToString(), product.Value);
            }

            var producedOutputs = new Dictionary<object, int>();

            foreach (var workshop in Workshops)
            {
                // Создаем копию ресурсов для каждого цеха
                var workshopResources = new Dictionary<object, int>(availableResources);
                var workshopOutputs = new Dictionary<object, int>();

                if (workshop.Process(workshopResources, workshopOutputs))
                {
                    // Учитываем эффективность производства
                    ApplyProductionEfficiency(workshopOutputs);

                    // Обновляем ресурсы только если производство успешно
                    availableResources = workshopResources;

                    // Добавляем выходы цеха в общие выходы
                    foreach (var output in workshopOutputs)
                    {
                        if (producedOutputs.ContainsKey(output.Key))
                            producedOutputs[output.Key] += output.Value;
                        else
                            producedOutputs[output.Key] = output.Value;
                    }
                }
            }

            // Обновляем хранилище материалов
            UpdateMaterialsStorage(availableResources);

            // Обновляем хранилище продукции с проверкой вместимости
            UpdateProductsStorage(producedOutputs);
        }

        /// <summary>
        /// Применяет коэффициент эффективности к произведенной продукции
        /// Входные данные: outputs - словарь с произведенной продукцией
        /// Выходные данные: отсутствуют (изменяет переданный словарь)
        /// </summary>
        /// <param name="outputs">Словарь с произведенной продукцией для корректировки</param>
        private void ApplyProductionEfficiency(Dictionary<object, int> outputs)
        {
            if (ProductionEfficiency >= 1f) return;

            var keys = outputs.Keys.ToList();
            foreach (var key in keys)
            {
                outputs[key] = (int)(outputs[key] * ProductionEfficiency);
                if (outputs[key] <= 0)
                    outputs.Remove(key);
            }
        }

        /// <summary>
        /// Обновляет хранилище материалов из результатов обработки
        /// Входные данные: availableResources - доступные ресурсы после производства
        /// Выходные данные: отсутствуют (обновляет MaterialsStorage)
        /// </summary>
        /// <param name="availableResources">Доступные ресурсы после производственного цикла</param>
        private void UpdateMaterialsStorage(Dictionary<object, int> availableResources)
        {
            MaterialsStorage.Clear();
            foreach (var resource in availableResources)
            {
                if (Enum.TryParse<FoodMaterial>(resource.Key.ToString(), out var material))
                {
                    MaterialsStorage[material] = resource.Value;
                }
            }
        }

        /// <summary>
        /// Обновляет хранилище продукции с проверкой вместимости
        /// Входные данные: producedOutputs - произведенная продукция
        /// Выходные данные: отсутствуют (обновляет ProductsStorage)
        /// </summary>
        /// <param name="producedOutputs">Произведенная продукция для добавления на склад</param>
        private void UpdateProductsStorage(Dictionary<object, int> producedOutputs)
        {
            foreach (var output in producedOutputs)
            {
                if (Enum.TryParse<FoodProduct>(output.Key.ToString(), out var product))
                {
                    int currentAmount = ProductsStorage.ContainsKey(product) ? ProductsStorage[product] : 0;
                    int availableSpace = MaxProductStorage - GetTotalProductStorage();
                    int amountToAdd = Math.Min(output.Value, availableSpace);

                    if (amountToAdd > 0)
                    {
                        ProductsStorage[product] = currentAmount + amountToAdd;
                    }

                    // Логируем потерю продукции при переполнении склада
                    if (amountToAdd < output.Value)
                    {
                        System.Diagnostics.Debug.WriteLine($"Превышена вместимость склада! Потеряно {output.Value - amountToAdd} единиц продукции {product}");
                    }
                }
            }
        }

        /// <summary>
        /// Выполняет полный рабочий цикл фабрики
        /// Входные данные: текущее состояние фабрики
        /// Выходные данные: отсутствуют (обновляет все производственные процессы)
        /// </summary>
        public void FullProductionCycle()
        {
            ProcessWorkshops();
        }

        /// <summary>
        /// Получает текущие запасы готовой продукции
        /// Входные данные: отсутствуют
        /// Выходные данные: словарь с типами продукции и их количеством на складе
        /// </summary>
        /// <returns>Словарь с текущими запасами продукции</returns>
        public Dictionary<FoodProduct, int> GetProductionOutput()
        {
            return new Dictionary<FoodProduct, int>(ProductsStorage);
        }

        /// <summary>
        /// Получает текущее количество сырья на складе
        /// Входные данные: отсутствуют
        /// Выходные данные: словарь с типами сырья и их количеством на складе
        /// </summary>
        /// <returns>Словарь с текущими запасами сырья</returns>
        public Dictionary<FoodMaterial, int> GetMaterialStorage()
        {
            return new Dictionary<FoodMaterial, int>(MaterialsStorage);
        }

        /// <summary>
        /// Потребляет продукцию (продажа или использование)
        /// Входные данные: product - тип продукции, amount - количество для потребления
        /// Выходные данные: true если продукция успешно потреблена, false если недостаточно продукции
        /// </summary>
        /// <param name="product">Тип потребляемой продукции</param>
        /// <param name="amount">Количество продукции для потребления</param>
        /// <returns>True если продукция успешно потреблена, false в противном случае</returns>
        public bool ConsumeProduct(FoodProduct product, int amount)
        {
            if (!ProductsStorage.ContainsKey(product) || ProductsStorage[product] < amount)
                return false;

            ProductsStorage[product] -= amount;
            if (ProductsStorage[product] == 0)
                ProductsStorage.Remove(product);

            return true;
        }

        /// <summary>
        /// Получает общее количество продукции на складе
        /// Входные данные: отсутствуют
        /// Выходные данные: общее количество всей продукции на складе
        /// </summary>
        /// <returns>Общее количество продукции</returns>
        public int GetTotalProductStorage()
        {
            return ProductsStorage.Values.Sum();
        }

        /// <summary>
        /// Получает информацию о производственной эффективности фабрики
        /// Входные данные: отсутствуют
        /// Выходные данные: словарь с ключевыми показателями эффективности
        /// </summary>
        /// <returns>Словарь с информацией о производстве</returns>
        public Dictionary<string, object> GetProductionInfo()
        {
            return new Dictionary<string, object>
            {
                { "WorkersCount", WorkersCount },
                { "MaxWorkers", MaxWorkers },
                { "ProductionEfficiency", ProductionEfficiency },
                { "TotalMaterialStorage", GetTotalMaterialStorage() },
                { "MaxMaterialStorage", MaxMaterialStorage },
                { "TotalProductStorage", GetTotalProductStorage() },
                { "MaxProductStorage", MaxProductStorage },
                { "ActiveWorkshops", Workshops.Count }
            };
        }

        /// <summary>
        /// Метод, вызываемый при размещении здания на карте
        /// Входные данные: отсутствуют
        /// Выходные данные: отсутствуют (запускает начальный производственный цикл)
        /// </summary>
        public override void OnBuildingPlaced()
        {
            // Запускаем начальный производственный цикл при размещении фабрики
            FullProductionCycle();
        }
    }
}