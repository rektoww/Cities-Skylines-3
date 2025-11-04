using Core.Enums;
using Core.Interfaces;
using Core.Models.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Models.Buildings.IndustrialBuildings
{
    public class AlcoholFactory : CommercialBuilding, IConstructable<AlcoholFactory>
    {
        #region Static Properties - Construction Cost

        /// <summary>
        /// Стоимость постройки завода алкоголя
        /// </summary>
        public static decimal BuildCost { get; protected set; } = 280000m;

        /// <summary>
        /// Материалы, необходимые для строительства завода алкоголя
        /// </summary>
        public static Dictionary<ConstructionMaterial, int> RequiredMaterials { get; protected set; }
            = new Dictionary<ConstructionMaterial, int>
            {
                { ConstructionMaterial.Steel, 6 },
                { ConstructionMaterial.Concrete, 7 },
                { ConstructionMaterial.Glass, 5 }
            };

        #endregion

        #region Simplified Enums

        /// <summary>
        /// Типы сырья для производства алкоголя
        /// </summary>
        public enum AlcoholMaterial
        {
            Wheat,
            Water,
            Grapes
        }

        /// <summary>
        /// Типы производимой алкогольной продукции
        /// </summary>
        public enum AlcoholProduct
        {
            Beer,
            Vodka,
            Whiskey,
            Wine,
            Brandy,
            Alcohol
        }

        #endregion

        /// <summary>Количество сырья на складе</summary>
        public Dictionary<AlcoholMaterial, int> MaterialsStorage { get; private set; } = new Dictionary<AlcoholMaterial, int>();

        /// <summary>Количество продукции на складе</summary>
        public Dictionary<AlcoholProduct, int> ProductsStorage { get; private set; } = new Dictionary<AlcoholProduct, int>();

        /// <summary>Максимальная вместимость склада сырья</summary>
        public int MaxMaterialStorage { get; private set; }

        /// <summary>Максимальная вместимость склада продукции</summary>
        public int MaxProductStorage { get; private set; }

        /// <summary>Производственные цеха завода</summary>
        public List<Workshop> Workshops { get; private set; } = new List<Workshop>();

        /// <summary>Текущее количество рабочих</summary>
        public int WorkersCount { get; private set; }

        /// <summary>Максимальное количество рабочих</summary>
        public int MaxWorkers { get; private set; }

        // Коэффициент эффективности в зависимости от количества рабочих
        public float ProductionEfficiency => WorkersCount > 0 ? 0.4f + (WorkersCount / (float)MaxWorkers) * 0.6f : 0f;

        /// <summary>
        /// Создает новый завод по производству алкоголя
        /// Выходные данные: инициализированный завод с цехами и стартовыми материалами
        /// </summary>
        public AlcoholFactory() : base(CommercialBuildingType.Factory)
        {
            MaxMaterialStorage = 1200;
            MaxProductStorage = 600;
            MaxWorkers = 10;
            WorkersCount = 0;

            InitializeWorkshops();
            InitializeStartingMaterials();
        }

        /// <summary>
        /// Инициализирует цеха завода, использующие пшеницу, воду и виноград для производства алкоголя
        /// Входные данные: отсутствуют
        /// Выходные данные: созданные и настроенные производственные цеха
        /// </summary>
        private void InitializeWorkshops()
        {
            // Цех брожения (производство пива и вина)
            var fermentationWorkshop = new Workshop
            {
                Name = "Цех брожения",
                ProductionCycleTime = 8
            };
            fermentationWorkshop.InputRequirements.Add("Wheat", 8);
            fermentationWorkshop.InputRequirements.Add("Water", 6);
            fermentationWorkshop.InputRequirements.Add("Grapes", 4);
            fermentationWorkshop.OutputProducts.Add("Beer", 10);
            fermentationWorkshop.OutputProducts.Add("Wine", 8);
            Workshops.Add(fermentationWorkshop);

            // Цех дистилляции (производство водки и виски)
            var distillationWorkshop = new Workshop
            {
                Name = "Цех дистилляции",
                ProductionCycleTime = 12
            };
            distillationWorkshop.InputRequirements.Add("Wheat", 12);
            distillationWorkshop.InputRequirements.Add("Water", 5);
            distillationWorkshop.OutputProducts.Add("Vodka", 6);
            distillationWorkshop.OutputProducts.Add("Whiskey", 4);
            Workshops.Add(distillationWorkshop);

            // Цех производства чистого спирта
            var alcoholWorkshop = new Workshop
            {
                Name = "Цех производства спирта",
                ProductionCycleTime = 10
            };
            alcoholWorkshop.InputRequirements.Add("Wheat", 15);
            alcoholWorkshop.InputRequirements.Add("Water", 3);
            alcoholWorkshop.OutputProducts.Add("Alcohol", 8);
            Workshops.Add(alcoholWorkshop);

            // Цех выдержки (производство бренди из вина)
            var agingWorkshop = new Workshop
            {
                Name = "Цех выдержки",
                ProductionCycleTime = 15
            };
            agingWorkshop.InputRequirements.Add("Wine", 4);
            agingWorkshop.OutputProducts.Add("Brandy", 3);
            Workshops.Add(agingWorkshop);
        }

        /// <summary>
        /// Инициализирует стартовые материалы (пшеница, вода, виноград)
        /// Входные данные: отсутствуют
        /// Выходные данные: заполненное хранилище начальным количеством сырья
        /// </summary>
        private void InitializeStartingMaterials()
        {
            AddMaterial(AlcoholMaterial.Wheat, 400);
            AddMaterial(AlcoholMaterial.Water, 500);
            AddMaterial(AlcoholMaterial.Grapes, 200);
        }

        /// <summary>
        /// Устанавливает количество рабочих на заводе
        /// Входные данные: count - количество рабочих для установки
        /// Выходные данные: отсутствуют (устанавливает значение WorkersCount)
        /// </summary>
        /// <param name="count">Количество рабочих для установки</param>
        public void SetWorkersCount(int count)
        {
            WorkersCount = Math.Min(count, MaxWorkers);
        }

        /// <summary>
        /// Добавляет сырье на склад завода
        /// Входные данные: material - тип сырья, amount - количество для добавления
        /// Выходные данные: true если сырье успешно добавлено, false если превышена вместимость склада
        /// </summary>
        /// <param name="material">Тип добавляемого сырья</param>
        /// <param name="amount">Количество сырья для добавления</param>
        /// <returns>True если сырье успешно добавлено, false в противном случае</returns>
        public bool AddMaterial(AlcoholMaterial material, int amount)
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
        /// Запускает производственные циклы во всех цехах завода
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

            // Добавляем существующую продукцию как доступный ресурс для цеха выдержки
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
                if (Enum.TryParse<AlcoholMaterial>(resource.Key.ToString(), out var material))
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
                if (Enum.TryParse<AlcoholProduct>(output.Key.ToString(), out var product))
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
        /// Выполняет полный рабочий цикл завода
        /// Входные данные: текущее состояние завода
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
        public Dictionary<AlcoholProduct, int> GetProductionOutput()
        {
            return new Dictionary<AlcoholProduct, int>(ProductsStorage);
        }

        /// <summary>
        /// Получает текущее количество сырья на складе
        /// Входные данные: отсутствуют
        /// Выходные данные: словарь с типами сырья и их количеством на складе
        /// </summary>
        /// <returns>Словарь с текущими запасами сырья</returns>
        public Dictionary<AlcoholMaterial, int> GetMaterialStorage()
        {
            return new Dictionary<AlcoholMaterial, int>(MaterialsStorage);
        }

        /// <summary>
        /// Потребляет продукцию (продажа или использование)
        /// Входные данные: product - тип продукции, amount - количество для потребления
        /// Выходные данные: true если продукция успешно потреблена, false если недостаточно продукции
        /// </summary>
        /// <param name="product">Тип потребляемой продукции</param>
        /// <param name="amount">Количество продукции для потребления</param>
        /// <returns>True если продукция успешно потреблена, false в противном случае</returns>
        public bool ConsumeProduct(AlcoholProduct product, int amount)
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
                { "ActiveWorkshops", Workshops.Count }
            };
        }

        public override void OnBuildingPlaced()
        {
            // Базовая логика при размещении
            FullProductionCycle();
        }
    }
}