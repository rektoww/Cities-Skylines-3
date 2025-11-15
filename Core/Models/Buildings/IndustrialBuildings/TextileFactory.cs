using Core.Enums;
using Core.Enums.Core.Enums;
using Core.Interfaces;
using Core.Models.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Models.Buildings.IndustrialBuildings
{
    /// <summary>
    /// Текстильная фабрика - производит различные ткани и текстильные изделия
    /// из натуральных и синтетических волокон для снабжения магазинов одежды
    /// </summary>
    public class TextileFactory : CommercialBuilding
    {
        #region Static Properties - Construction Cost

        /// <summary>
        /// Стоимость постройки текстильной фабрики
        /// </summary>
        public static decimal BuildCost { get; protected set; } = 350000m;

        /// <summary>
        /// Материалы, необходимые для строительства текстильной фабрики
        /// </summary>
        public static Dictionary<ConstructionMaterial, int> RequiredMaterials { get; protected set; }
            = new Dictionary<ConstructionMaterial, int>
            {
                { ConstructionMaterial.Steel, 12 },
                { ConstructionMaterial.Concrete, 8 },
                { ConstructionMaterial.Glass, 2 },
                { ConstructionMaterial.Plastic, 6 }
            };

        #endregion

        #region Simplified Enums

        /// <summary>
        /// Типы сырья для текстильной промышленности
        /// </summary>
        public enum TextileMaterial
        {
            Cotton,         // Хлопок
            Wool,           // Шерсть
            Silk,           // Шелк
            Linen,          // Лен
            SyntheticFiber, // Синтетическое волокно
            Dye,            // Краситель
            Thread          // Нить
        }

        /// <summary>
        /// Типы производимой текстильной продукции
        /// </summary>
        public enum TextileProduct
        {
            CottonFabric,       // Хлопковая ткань
            WoolFabric,         // Шерстяная ткань
            SilkFabric,         // Шелковая ткань
            LinenFabric,        // Льняная ткань
            SyntheticFabric,    // Синтетическая ткань
            Denim,              // Джинсовая ткань
            Knitwear,           // Трикотаж
            Yarn,               // Пряжа
            Clothing,           // Готовая одежда
            HomeTextile         // Домашний текстиль
        }

        #endregion

        /// <summary>Количество сырья на складе</summary>
        public Dictionary<TextileMaterial, int> MaterialsStorage { get; private set; } = new Dictionary<TextileMaterial, int>();

        /// <summary>Количество продукции на складе</summary>
        public Dictionary<TextileProduct, int> ProductsStorage { get; private set; } = new Dictionary<TextileProduct, int>();

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
        /// Базовая эффективность 35% + пропорционально количеству рабочих до 100%
        /// </summary>
        public float ProductionEfficiency => WorkersCount > 0 ? 0.35f + WorkersCount / (float)MaxWorkers * 0.65f : 0f;

        /// <summary>
        /// Создает новую текстильную фабрику
        /// Выходные данные: инициализированная фабрика с цехами и стартовыми материалами
        /// </summary>
        public TextileFactory() : base(CommercialBuildingType.Factory)
        {       
            MaxMaterialStorage = 1200;  // Средний склад сырья (волокна занимают место)
            MaxProductStorage = 800;    // Вместительный склад готовой продукции
            MaxWorkers = 18;           // Текстильное производство требует много рабочих
            WorkersCount = 0;

            InitializeWorkshops();
            InitializeStartingMaterials();
        }

        /// <summary>
        /// Инициализирует производственные цеха текстильной фабрики
        /// Входные данные: отсутствуют
        /// Выходные данные: созданные и настроенные производственные цеха
        /// </summary>
        private void InitializeWorkshops()
        {
            // Прядильный цех - производит пряжу из различных волокон
            var spinningWorkshop = new Workshop
            {
                Name = "Прядильный цех",
                ProductionCycleTime = 7  // Среднее время для прядения
            };
            spinningWorkshop.InputRequirements.Add("Cotton", 12);
            spinningWorkshop.InputRequirements.Add("Wool", 10);
            spinningWorkshop.InputRequirements.Add("Silk", 8);
            spinningWorkshop.OutputProducts.Add("Yarn", 15);
            spinningWorkshop.OutputProducts.Add("Thread", 20);
            Workshops.Add(spinningWorkshop);

            // Ткацкий цех - производит различные ткани из пряжи
            var weavingWorkshop = new Workshop
            {
                Name = "Ткацкий цех",
                ProductionCycleTime = 9  // Дольше из-за процесса ткачества
            };
            weavingWorkshop.InputRequirements.Add("Yarn", 8);
            weavingWorkshop.InputRequirements.Add("Thread", 12);
            weavingWorkshop.InputRequirements.Add("Dye", 4);
            weavingWorkshop.OutputProducts.Add("CottonFabric", 10);
            weavingWorkshop.OutputProducts.Add("WoolFabric", 8);
            weavingWorkshop.OutputProducts.Add("SilkFabric", 6);
            weavingWorkshop.OutputProducts.Add("LinenFabric", 7);
            Workshops.Add(weavingWorkshop);

            // Цех синтетических тканей - производит ткани из синтетических волокон
            var syntheticWorkshop = new Workshop
            {
                Name = "Цех синтетических тканей",
                ProductionCycleTime = 6  // Быстрее чем натуральные ткани
            };
            syntheticWorkshop.InputRequirements.Add("SyntheticFiber", 15);
            syntheticWorkshop.InputRequirements.Add("Dye", 5);
            syntheticWorkshop.OutputProducts.Add("SyntheticFabric", 12);
            syntheticWorkshop.OutputProducts.Add("Knitwear", 8);
            Workshops.Add(syntheticWorkshop);

            // Швейный цех - производит готовую одежду из тканей
            var sewingWorkshop = new Workshop
            {
                Name = "Швейный цех",
                ProductionCycleTime = 8  // Среднее время для пошива
            };
            sewingWorkshop.InputRequirements.Add("CottonFabric", 6);
            sewingWorkshop.InputRequirements.Add("WoolFabric", 5);
            sewingWorkshop.InputRequirements.Add("SilkFabric", 4);
            sewingWorkshop.InputRequirements.Add("Thread", 8);
            sewingWorkshop.OutputProducts.Add("Clothing", 8);
            sewingWorkshop.OutputProducts.Add("HomeTextile", 6);
            Workshops.Add(sewingWorkshop);

            // Джинсовый цех - специализированное производство джинсовой ткани
            var denimWorkshop = new Workshop
            {
                Name = "Джинсовый цех",
                ProductionCycleTime = 11  // Дольше из-за специальной обработки
            };
            denimWorkshop.InputRequirements.Add("Cotton", 15);
            denimWorkshop.InputRequirements.Add("Dye", 6);
            denimWorkshop.InputRequirements.Add("Thread", 10);
            denimWorkshop.OutputProducts.Add("Denim", 7);
            denimWorkshop.OutputProducts.Add("Clothing", 5);  // Джинсовая одежда
            Workshops.Add(denimWorkshop);
        }

        /// <summary>
        /// Инициализирует стартовые запасы сырья на фабрике
        /// Входные данные: отсутствуют
        /// Выходные данные: заполненное хранилище начальным количеством сырья
        /// </summary>
        private void InitializeStartingMaterials()
        {
            // Стартовые запасы сырья для начала производства
            AddMaterial(TextileMaterial.Cotton, 400);
            AddMaterial(TextileMaterial.Wool, 250);
            AddMaterial(TextileMaterial.Silk, 150);
            AddMaterial(TextileMaterial.Linen, 200);
            AddMaterial(TextileMaterial.SyntheticFiber, 300);
            AddMaterial(TextileMaterial.Dye, 100);
            AddMaterial(TextileMaterial.Thread, 180);
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
        public bool AddMaterial(TextileMaterial material, int amount)
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
                if (Enum.TryParse<TextileMaterial>(resource.Key.ToString(), out var material))
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
                if (Enum.TryParse<TextileProduct>(output.Key.ToString(), out var product))
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
        public Dictionary<TextileProduct, int> GetProductionOutput()
        {
            return new Dictionary<TextileProduct, int>(ProductsStorage);
        }

        /// <summary>
        /// Получает текущее количество сырья на складе
        /// Входные данные: отсутствуют
        /// Выходные данные: словарь с типами сырья и их количеством на складе
        /// </summary>
        /// <returns>Словарь с текущими запасами сырья</returns>
        public Dictionary<TextileMaterial, int> GetMaterialStorage()
        {
            return new Dictionary<TextileMaterial, int>(MaterialsStorage);
        }

        /// <summary>
        /// Потребляет продукцию (продажа или использование)
        /// Входные данные: product - тип продукции, amount - количество для потребления
        /// Выходные данные: true если продукция успешно потреблена, false если недостаточно продукции
        /// </summary>
        /// <param name="product">Тип потребляемой продукции</param>
        /// <param name="amount">Количество продукции для потребления</param>
        /// <returns>True если продукция успешно потреблена, false в противном случае</returns>
        public bool ConsumeProduct(TextileProduct product, int amount)
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