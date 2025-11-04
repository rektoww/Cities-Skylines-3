using Core.Enums;
using Core.Interfaces;
using Core.Models.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Buildings.IndustrialBuildings
{
    public class PharmaceuticalFactory : CommercialBuilding, IConstructable<PharmaceuticalFactory>
    {
        #region Static Properties - Construction Cost

        /// <summary>
        /// Стоимость постройки фармацевтического завода
        /// </summary>
        public static decimal BuildCost => 350000m;

        /// <summary>
        /// Материалы, необходимые для строительства фармацевтического завода
        /// </summary>
        public static Dictionary<ConstructionMaterial, int> RequiredMaterials =>
            new Dictionary<ConstructionMaterial, int>
            {
                { ConstructionMaterial.Steel, 8 },
                { ConstructionMaterial.Concrete, 10 },
                { ConstructionMaterial.Glass, 12 }               
            };

        #endregion

        #region Simplified Enums

        /// <summary>
        /// Типы сырья для фармацевтического производства
        /// </summary>
        public enum PharmaceuticalMaterial
        {
            Herbs,          // Лекарственные травы
            Chemicals,      // Химические компоненты
            Water,          // Очищенная вода
            Plants,         // Растительные экстракты
            Minerals        // Минеральные компоненты
        }

        /// <summary>
        /// Типы производимой фармацевтической продукции
        /// </summary>
        public enum PharmaceuticalProduct
        {
            Antibiotics,    // Антибиотики
            Painkillers,    // Обезболивающие
            Vitamins,       // Витамины
            Antiseptics,    // Антисептики
            Bandages,       // Бинты и перевязочные материалы
            Syrups,         // Лечебные сиропы
            Tablets         // Таблетки
        }

        #endregion

        /// <summary>Количество сырья на складе</summary>
        public Dictionary<PharmaceuticalMaterial, int> MaterialsStorage { get; private set; } = new Dictionary<PharmaceuticalMaterial, int>();

        /// <summary>Количество продукции на складе</summary>
        public Dictionary<PharmaceuticalProduct, int> ProductsStorage { get; private set; } = new Dictionary<PharmaceuticalProduct, int>();

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

        /// <summary>Уровень чистоты производства (влияет на качество продукции)</summary>
        public float CleanlinessLevel { get; private set; } = 0.8f;

        // Коэффициент эффективности в зависимости от количества рабочих и чистоты
        public float ProductionEfficiency => WorkersCount > 0 ?
            (0.3f + (WorkersCount / (float)MaxWorkers) * 0.5f) * CleanlinessLevel : 0f;

        /// <summary>
        /// Создает новый фармацевтический завод
        /// </summary>
        public PharmaceuticalFactory() : base(CommercialBuildingType.Factory)
        {
            MaxMaterialStorage = 1500;
            MaxProductStorage = 800;
            MaxWorkers = 15;
            WorkersCount = 0;

            InitializeWorkshops();
            InitializeStartingMaterials();
        }

        /// <summary>
        /// Инициализирует цеха фармацевтического завода
        /// </summary>
        private void InitializeWorkshops()
        {
            // Цех производства таблеток и капсул
            var tabletWorkshop = new Workshop
            {
                Name = "Цех производства таблеток",
                ProductionCycleTime = 6
            };
            tabletWorkshop.InputRequirements.Add("Chemicals", 10);
            tabletWorkshop.InputRequirements.Add("Minerals", 5);
            tabletWorkshop.OutputProducts.Add("Tablets", 15);
            tabletWorkshop.OutputProducts.Add("Vitamins", 8);
            Workshops.Add(tabletWorkshop);

            // Цех производства антибиотиков
            var antibioticsWorkshop = new Workshop
            {
                Name = "Цех производства антибиотиков",
                ProductionCycleTime = 10
            };
            antibioticsWorkshop.InputRequirements.Add("Chemicals", 15);
            antibioticsWorkshop.InputRequirements.Add("Herbs", 8);
            antibioticsWorkshop.InputRequirements.Add("Water", 5);
            antibioticsWorkshop.OutputProducts.Add("Antibiotics", 6);
            Workshops.Add(antibioticsWorkshop);

            // Цех производства обезболивающих
            var painkillersWorkshop = new Workshop
            {
                Name = "Цех производства обезболивающих",
                ProductionCycleTime = 8
            };
            painkillersWorkshop.InputRequirements.Add("Chemicals", 12);
            painkillersWorkshop.InputRequirements.Add("Plants", 6);
            painkillersWorkshop.OutputProducts.Add("Painkillers", 10);
            Workshops.Add(painkillersWorkshop);

            // Цех производства сиропов и жидкостей
            var syrupWorkshop = new Workshop
            {
                Name = "Цех производства сиропов",
                ProductionCycleTime = 7
            };
            syrupWorkshop.InputRequirements.Add("Herbs", 10);
            syrupWorkshop.InputRequirements.Add("Water", 8);
            syrupWorkshop.InputRequirements.Add("Plants", 4);
            syrupWorkshop.OutputProducts.Add("Syrups", 12);
            Workshops.Add(syrupWorkshop);

            // Цех производства перевязочных материалов и антисептиков
            var medicalWorkshop = new Workshop
            {
                Name = "Цех медицинских материалов",
                ProductionCycleTime = 5
            };
            medicalWorkshop.InputRequirements.Add("Chemicals", 8);
            medicalWorkshop.InputRequirements.Add("Plants", 3);
            medicalWorkshop.OutputProducts.Add("Antiseptics", 10);
            medicalWorkshop.OutputProducts.Add("Bandages", 20);
            Workshops.Add(medicalWorkshop);
        }

        /// <summary>
        /// Инициализирует стартовые материалы
        /// </summary>
        private void InitializeStartingMaterials()
        {
            AddMaterial(PharmaceuticalMaterial.Herbs, 300);
            AddMaterial(PharmaceuticalMaterial.Chemicals, 400);
            AddMaterial(PharmaceuticalMaterial.Water, 600);
            AddMaterial(PharmaceuticalMaterial.Plants, 200);
            AddMaterial(PharmaceuticalMaterial.Minerals, 150);
        }

        /// <summary>
        /// Устанавливает количество рабочих на заводе
        /// </summary>
        /// <param name="count">Количество рабочих для установки</param>
        public void SetWorkersCount(int count)
        {
            WorkersCount = Math.Min(count, MaxWorkers);
        }

        /// <summary>
        /// Устанавливает уровень чистоты производства
        /// </summary>
        /// <param name="cleanliness">Уровень чистоты от 0.1 до 1.0</param>
        public void SetCleanlinessLevel(float cleanliness)
        {
            CleanlinessLevel = Math.Clamp(cleanliness, 0.1f, 1.0f);
        }

        /// <summary>
        /// Добавляет сырье на склад завода
        /// </summary>
        /// <param name="material">Тип добавляемого сырья</param>
        /// <param name="amount">Количество сырья для добавления</param>
        /// <returns>True если сырье успешно добавлено, false в противном случае</returns>
        public bool AddMaterial(PharmaceuticalMaterial material, int amount)
        {
            int currentAmount = MaterialsStorage.ContainsKey(material) ? MaterialsStorage[material] : 0;
            if (GetTotalMaterialStorage() + amount > MaxMaterialStorage)
                return false;

            MaterialsStorage[material] = currentAmount + amount;
            return true;
        }

        /// <summary>
        /// Получает общее количество сырья на складе
        /// </summary>
        /// <returns>Общее количество сырья</returns>
        public int GetTotalMaterialStorage()
        {
            return MaterialsStorage.Values.Sum();
        }

        /// <summary>
        /// Запускает производственные циклы во всех цехах завода
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

            // Добавляем существующую продукцию как доступный ресурс
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
                if (Enum.TryParse<PharmaceuticalMaterial>(resource.Key.ToString(), out var material))
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
                if (Enum.TryParse<PharmaceuticalProduct>(output.Key.ToString(), out var product))
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
        /// </summary>
        public void FullProductionCycle()
        {
            ProcessWorkshops();
        }

        /// <summary>
        /// Получает текущие запасы готовой продукции
        /// </summary>
        /// <returns>Словарь с текущими запасами продукции</returns>
        public Dictionary<PharmaceuticalProduct, int> GetProductionOutput()
        {
            return new Dictionary<PharmaceuticalProduct, int>(ProductsStorage);
        }

        /// <summary>
        /// Получает текущее количество сырья на складе
        /// </summary>
        /// <returns>Словарь с текущими запасами сырья</returns>
        public Dictionary<PharmaceuticalMaterial, int> GetMaterialStorage()
        {
            return new Dictionary<PharmaceuticalMaterial, int>(MaterialsStorage);
        }

        /// <summary>
        /// Потребляет продукцию (продажа или использование)
        /// </summary>
        /// <param name="product">Тип потребляемой продукции</param>
        /// <param name="amount">Количество продукции для потребления</param>
        /// <returns>True если продукция успешно потреблена, false в противном случае</returns>
        public bool ConsumeProduct(PharmaceuticalProduct product, int amount)
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
                { "CleanlinessLevel", CleanlinessLevel },
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
