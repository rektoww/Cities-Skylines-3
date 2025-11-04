using Core.Enums;
using Core.Interfaces;
using Core.Models.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Models.Buildings.IndustrialBuildings
{
    public class FisheryComplex : CommercialBuilding, IConstructable<FisheryComplex>
    {
        #region Static Properties - Construction Cost

        /// <summary>
        /// Стоимость постройки рыбодобывающего комбината
        /// </summary>
        public static decimal BuildCost { get; protected set; } = 350000m;

        /// <summary>
        /// Материалы, необходимые для строительства рыбодобывающего комбината
        /// </summary>
        public static Dictionary<ConstructionMaterial, int> RequiredMaterials { get; protected set; }
            = new Dictionary<ConstructionMaterial, int>
            {
                { ConstructionMaterial.Steel, 12 },
                { ConstructionMaterial.Concrete, 8 },
                { ConstructionMaterial.Glass, 3 },
                { ConstructionMaterial.Plastic, 8 }
            };

        #endregion

        #region Simplified Enums

        /// <summary>
        /// Типы сырья для рыбодобывающей отрасли
        /// </summary>
        public enum FisheryMaterial
        {
            Fuel,
            FishingGear,
            Ice,
            Salt
        }

        /// <summary>
        /// Типы рыбной продукции
        /// </summary>
        public enum FisheryProduct
        {
            FreshFish,
            FrozenFish,
            CannedFish,
            FishFillets,
            FishOil,
            FishMeal,
            Seafood
        }

        #endregion

        /// <summary>Количество сырья на складе</summary>
        public Dictionary<FisheryMaterial, int> MaterialsStorage { get; private set; } = new Dictionary<FisheryMaterial, int>();

        /// <summary>Количество продукции на складе</summary>
        public Dictionary<FisheryProduct, int> ProductsStorage { get; private set; } = new Dictionary<FisheryProduct, int>();

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
        public float ProductionEfficiency => WorkersCount > 0 ? 0.4f + (WorkersCount / (float)MaxWorkers) * 0.6f : 0f;

        /// <summary>
        /// Создает новый рыбодобывающий комбинат
        /// Выходные данные: инициализированный комбинат с цехами и стартовыми материалами
        /// </summary>
        public FisheryComplex() : base(CommercialBuildingType.Factory)
        {
            MaxMaterialStorage = 1500;
            MaxProductStorage = 800;
            MaxWorkers = 12;
            WorkersCount = 0;

            InitializeWorkshops();
            InitializeStartingMaterials();
        }

        /// <summary>
        /// Инициализирует цеха комбината для рыбодобычи и переработки
        /// Входные данные: отсутствуют
        /// Выходные данные: созданные и настроенные производственные цеха
        /// </summary>
        private void InitializeWorkshops()
        {
            // Цех добычи рыбы
            var fishingWorkshop = new Workshop
            {
                Name = "Цех добычи рыбы",
                ProductionCycleTime = 8
            };
            fishingWorkshop.InputRequirements.Add("Fuel", 15);
            fishingWorkshop.InputRequirements.Add("FishingGear", 8);
            fishingWorkshop.OutputProducts.Add("FreshFish", 20);
            Workshops.Add(fishingWorkshop);

            // Цех заморозки рыбы
            var freezingWorkshop = new Workshop
            {
                Name = "Цех заморозки рыбы",
                ProductionCycleTime = 6
            };
            freezingWorkshop.InputRequirements.Add("FreshFish", 12);
            freezingWorkshop.InputRequirements.Add("Ice", 10);
            freezingWorkshop.OutputProducts.Add("FrozenFish", 8);
            Workshops.Add(freezingWorkshop);

            // Цех производства консервов
            var canningWorkshop = new Workshop
            {
                Name = "Консервный цех",
                ProductionCycleTime = 10
            };
            canningWorkshop.InputRequirements.Add("FreshFish", 15);
            canningWorkshop.InputRequirements.Add("Salt", 5);
            canningWorkshop.OutputProducts.Add("CannedFish", 12);
            Workshops.Add(canningWorkshop);

            // Цех разделки рыбы
            var processingWorkshop = new Workshop
            {
                Name = "Цех разделки рыбы",
                ProductionCycleTime = 7
            };
            processingWorkshop.InputRequirements.Add("FreshFish", 10);
            processingWorkshop.InputRequirements.Add("Ice", 3);
            processingWorkshop.OutputProducts.Add("FishFillets", 6);
            processingWorkshop.OutputProducts.Add("Seafood", 4);
            Workshops.Add(processingWorkshop);

            // Цех переработки отходов
            var byproductWorkshop = new Workshop
            {
                Name = "Цех переработки отходов",
                ProductionCycleTime = 9
            };
            byproductWorkshop.InputRequirements.Add("FreshFish", 8); // отходы от разделки
            byproductWorkshop.OutputProducts.Add("FishOil", 3);
            byproductWorkshop.OutputProducts.Add("FishMeal", 5);
            Workshops.Add(byproductWorkshop);
        }

        /// <summary>
        /// Инициализирует стартовые материалы (топливо, снасти, лед, соль)
        /// Входные данные: отсутствуют
        /// Выходные данные: заполненное хранилище начальным количеством сырья
        /// </summary>
        private void InitializeStartingMaterials()
        {
            AddMaterial(FisheryMaterial.Fuel, 500);
            AddMaterial(FisheryMaterial.FishingGear, 300);
            AddMaterial(FisheryMaterial.Ice, 400);
            AddMaterial(FisheryMaterial.Salt, 200);
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
        public bool AddMaterial(FisheryMaterial material, int amount)
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
                if (Enum.TryParse<FisheryMaterial>(resource.Key.ToString(), out var material))
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
                if (Enum.TryParse<FisheryProduct>(output.Key.ToString(), out var product))
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
        public Dictionary<FisheryProduct, int> GetProductionOutput()
        {
            return new Dictionary<FisheryProduct, int>(ProductsStorage);
        }

        /// <summary>
        /// Получает текущее количество сырья на складе
        /// Входные данные: отсутствуют
        /// Выходные данные: словарь с типами сырья и их количеством на складе
        /// </summary>
        /// <returns>Словарь с текущими запасами сырья</returns>
        public Dictionary<FisheryMaterial, int> GetMaterialStorage()
        {
            return new Dictionary<FisheryMaterial, int>(MaterialsStorage);
        }

        /// <summary>
        /// Потребляет продукцию (продажа или использование)
        /// Входные данные: product - тип продукции, amount - количество для потребления
        /// Выходные данные: true если продукция успешно потреблена, false если недостаточно продукции
        /// </summary>
        /// <param name="product">Тип потребляемой продукции</param>
        /// <param name="amount">Количество продукции для потребления</param>
        /// <returns>True если продукция успешно потреблена, false в противном случае</returns>
        public bool ConsumeProduct(FisheryProduct product, int amount)
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
                { "FleetEfficiency", GetFleetEfficiency() }
            };
        }

        /// <summary>
        /// Получает эффективность флота (имитация погодных условий)
        /// </summary>
        private float GetFleetEfficiency()
        {
            // Простая имитация погодных условий для рыболовного флота
            var random = new Random();
            return 0.7f + (float)random.NextDouble() * 0.3f; // 70-100% эффективности
        }

        public override void OnBuildingPlaced()
        {
            // Базовая логика при размещении
            FullProductionCycle();
        }
    }
}
