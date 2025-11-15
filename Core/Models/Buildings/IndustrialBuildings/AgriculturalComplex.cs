using Core.Enums;
using Core.Enums.Core.Enums;
using Core.Interfaces;
using Core.Models.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Models.Buildings.IndustrialBuildings
{
    public class AgriculturalComplex : CommercialBuilding
    {
        #region Static Properties - Construction Cost

        /// <summary>
        /// Стоимость постройки сельскохозяйственного комбината
        /// </summary>
        public static decimal BuildCost { get; protected set; } = 320000m;

        /// <summary>
        /// Материалы, необходимые для строительства сельскохозяйственного комбината
        /// </summary>
        public static Dictionary<ConstructionMaterial, int> RequiredMaterials { get; protected set; }
            = new Dictionary<ConstructionMaterial, int>
            {
                { ConstructionMaterial.Steel, 8 },
                { ConstructionMaterial.Concrete, 10 },
                { ConstructionMaterial.Glass, 6 },
                { ConstructionMaterial.Plastic, 4 }
            };

        #endregion

        #region Simplified Enums

        /// <summary>
        /// Типы сельскохозяйственного сырья
        /// </summary>
        public enum AgriMaterial
        {
            Seeds,
            Fertilizer,
            Water,
            AnimalFeed
        }

        /// <summary>
        /// Типы сельскохозяйственной продукции
        /// </summary>
        public enum AgriProduct
        {
            Wheat,
            Vegetables,
            Fruits,
            Milk,
            Eggs,
            Meat,
            ProcessedFood
        }

        #endregion

        /// <summary>Количество сырья на складе</summary>
        public Dictionary<AgriMaterial, int> MaterialsStorage { get; private set; } = new Dictionary<AgriMaterial, int>();

        /// <summary>Количество продукции на складе</summary>
        public Dictionary<AgriProduct, int> ProductsStorage { get; private set; } = new Dictionary<AgriProduct, int>();

        /// <summary>Максимальная вместимость склада сырья</summary>
        public int MaxMaterialStorage { get; private set; }

        /// <summary>Максимальная вместимость склада продукции</summary>
        public int MaxProductStorage { get; private set; }

        /// <summary>Производственные цеха комбината</summary>
        public List<Workshop> Workshops { get; private set; } = new List<Workshop>();

        /// <summary>Текущее количество рабочих</summary>
        public int WorkersCount { get; private set; }

        /// <summary>Максимальное количество рабочих</summary>
        public int MaxWorkers { get; private set; }

        // Коэффициент эффективности в зависимости от количества рабочих
        public float ProductionEfficiency => WorkersCount > 0 ? 0.3f + (WorkersCount / (float)MaxWorkers) * 0.7f : 0f;

        /// <summary>
        /// Создает новый сельскохозяйственный комбинат
        /// Выходные данные: инициализированный комбинат с цехами и стартовыми материалами
        /// </summary>
        public AgriculturalComplex() : base(CommercialBuildingType.Factory)
        {
            MaxMaterialStorage = 2000;
            MaxProductStorage = 1000;
            MaxWorkers = 15;
            WorkersCount = 0;

            InitializeWorkshops();
            InitializeStartingMaterials();
        }

        /// <summary>
        /// Инициализирует цеха комбината для растениеводства и животноводства
        /// Входные данные: отсутствуют
        /// Выходные данные: созданные и настроенные производственные цеха
        /// </summary>
        private void InitializeWorkshops()
        {
            // Цех растениеводства (зерновые культуры)
            var cropWorkshop = new Workshop
            {
                Name = "Цех растениеводства",
                ProductionCycleTime = 6
            };
            cropWorkshop.InputRequirements.Add("Seeds", 10);
            cropWorkshop.InputRequirements.Add("Fertilizer", 5);
            cropWorkshop.InputRequirements.Add("Water", 8);
            cropWorkshop.OutputProducts.Add("Wheat", 15);
            Workshops.Add(cropWorkshop);

            // Цех овощеводства
            var vegetableWorkshop = new Workshop
            {
                Name = "Цех овощеводства",
                ProductionCycleTime = 5
            };
            vegetableWorkshop.InputRequirements.Add("Seeds", 8);
            vegetableWorkshop.InputRequirements.Add("Fertilizer", 4);
            vegetableWorkshop.InputRequirements.Add("Water", 6);
            vegetableWorkshop.OutputProducts.Add("Vegetables", 12);
            Workshops.Add(vegetableWorkshop);

            // Цех садоводства
            var orchardWorkshop = new Workshop
            {
                Name = "Цех садоводства",
                ProductionCycleTime = 8
            };
            orchardWorkshop.InputRequirements.Add("Seeds", 6);
            orchardWorkshop.InputRequirements.Add("Fertilizer", 3);
            orchardWorkshop.InputRequirements.Add("Water", 5);
            orchardWorkshop.OutputProducts.Add("Fruits", 10);
            Workshops.Add(orchardWorkshop);

            // Цех животноводства (молочная продукция)
            var dairyWorkshop = new Workshop
            {
                Name = "Молочный цех",
                ProductionCycleTime = 7
            };
            dairyWorkshop.InputRequirements.Add("AnimalFeed", 12);
            dairyWorkshop.InputRequirements.Add("Water", 4);
            dairyWorkshop.OutputProducts.Add("Milk", 8);
            dairyWorkshop.OutputProducts.Add("Eggs", 6);
            Workshops.Add(dairyWorkshop);

            // Цех переработки мяса
            var meatWorkshop = new Workshop
            {
                Name = "Цех переработки мяса",
                ProductionCycleTime = 10
            };
            meatWorkshop.InputRequirements.Add("AnimalFeed", 15);
            meatWorkshop.InputRequirements.Add("Water", 3);
            meatWorkshop.OutputProducts.Add("Meat", 5);
            Workshops.Add(meatWorkshop);

            // Цех пищевой переработки
            var processingWorkshop = new Workshop
            {
                Name = "Цех пищевой переработки",
                ProductionCycleTime = 9
            };
            processingWorkshop.InputRequirements.Add("Wheat", 8);
            processingWorkshop.InputRequirements.Add("Milk", 4);
            processingWorkshop.InputRequirements.Add("Vegetables", 6);
            processingWorkshop.OutputProducts.Add("ProcessedFood", 10);
            Workshops.Add(processingWorkshop);
        }

        /// <summary>
        /// Инициализирует стартовые материалы (семена, удобрения, вода, корм)
        /// Входные данные: отсутствуют
        /// Выходные данные: заполненное хранилище начальным количеством сырья
        /// </summary>
        private void InitializeStartingMaterials()
        {
            AddMaterial(AgriMaterial.Seeds, 600);
            AddMaterial(AgriMaterial.Fertilizer, 400);
            AddMaterial(AgriMaterial.Water, 800);
            AddMaterial(AgriMaterial.AnimalFeed, 300);
        }

        /// <summary>
        /// Устанавливает количество рабочих на комбинате
        /// Входные данные: count - количество рабочих для установки
        /// Выходные данные: отсутствуют (устанавливает значение WorkersCount)
        /// </summary>
        /// <param name="count">Количество рабочих для установки</param>
        public void SetWorkersCount(int count)
        {
            WorkersCount = Math.Min(count, MaxWorkers);
        }

        /// <summary>
        /// Добавляет сырье на склад комбината
        /// Входные данные: material - тип сырья, amount - количество для добавления
        /// Выходные данные: true если сырье успешно добавлено, false если превышена вместимость склада
        /// </summary>
        /// <param name="material">Тип добавляемого сырья</param>
        /// <param name="amount">Количество сырья для добавления</param>
        /// <returns>True если сырье успешно добавлено, false в противном случае</returns>
        public bool AddMaterial(AgriMaterial material, int amount)
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
        /// Запускает производственные циклы во всех цехах комбината
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

            // Добавляем существующую продукцию как доступный ресурс для цехов переработки
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
        /// </summary>
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
        /// </summary>
        private void UpdateMaterialsStorage(Dictionary<object, int> availableResources)
        {
            MaterialsStorage.Clear();
            foreach (var resource in availableResources)
            {
                if (Enum.TryParse<AgriMaterial>(resource.Key.ToString(), out var material))
                {
                    MaterialsStorage[material] = resource.Value;
                }
            }
        }

        /// <summary>
        /// Обновляет хранилище продукции с проверкой вместимости
        /// </summary>
        private void UpdateProductsStorage(Dictionary<object, int> producedOutputs)
        {
            foreach (var output in producedOutputs)
            {
                if (Enum.TryParse<AgriProduct>(output.Key.ToString(), out var product))
                {
                    int currentAmount = ProductsStorage.ContainsKey(product) ? ProductsStorage[product] : 0;
                    int availableSpace = MaxProductStorage - GetTotalProductStorage();
                    int amountToAdd = Math.Min(output.Value, availableSpace);

                    if (amountToAdd > 0)
                    {
                        ProductsStorage[product] = currentAmount + amountToAdd;
                    }

                    // Логируем потерю продукции при переполнении
                    if (amountToAdd < output.Value)
                    {
                        System.Diagnostics.Debug.WriteLine($"Превышена вместимость склада! Потеряно {output.Value - amountToAdd} единиц продукции {product}");
                    }
                }
            }
        }

        /// <summary>
        /// Выполняет полный рабочий цикл комбината
        /// Входные данные: текущее состояние комбината
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
        public Dictionary<AgriProduct, int> GetProductionOutput()
        {
            return new Dictionary<AgriProduct, int>(ProductsStorage);
        }

        /// <summary>
        /// Получает текущее количество сырья на складе
        /// Входные данные: отсутствуют
        /// Выходные данные: словарь с типами сырья и их количеством на складе
        /// </summary>
        /// <returns>Словарь с текущими запасами сырья</returns>
        public Dictionary<AgriMaterial, int> GetMaterialStorage()
        {
            return new Dictionary<AgriMaterial, int>(MaterialsStorage);
        }

        /// <summary>
        /// Потребляет продукцию (продажа или использование)
        /// Входные данные: product - тип продукции, amount - количество для потребления
        /// Выходные данные: true если продукция успешно потреблена, false если недостаточно продукции
        /// </summary>
        /// <param name="product">Тип потребляемой продукции</param>
        /// <param name="amount">Количество продукции для потребления</param>
        /// <returns>True если продукция успешно потреблена, false в противном случае</returns>
        public bool ConsumeProduct(AgriProduct product, int amount)
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
        /// Получает информацию о производственной эффективности
        /// </summary>
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
                { "ActiveWorkshops", Workshops.Count },
                { "SeasonalBonus", GetSeasonalBonus() }
            };
        }

        /// <summary>
        /// Получает сезонный бонус к производству (имитация сезонности)
        /// </summary>
        private float GetSeasonalBonus()
        {
            // Простая имитация сезонности - можно интегрировать с реальной системой времени
            var month = DateTime.Now.Month;
            return month >= 3 && month <= 9 ? 1.2f : 0.8f; // +20% летом, -20% зимой
        }
    }
}
