using Core.Models.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Models.Buildings.IndustrialBuildings
{
    /// <summary>
    /// Завод по производству картона
    /// </summary>
    public class CardboardFactory
    {
        #region Enums - Типы материалов и продукции

        /// <summary>
        /// Типы сырья для производства картона
        /// </summary>
        public enum CardboardMaterial
        {
            WoodChips,      // Древесная щепа
            RecycledPaper,  // Макулатура
            Chemicals,      // Химические реагенты
            Water,          // Вода
            Energy          // Энергия
        }

        /// <summary>
        /// Типы производимого картона и изделий
        /// </summary>
        public enum CardboardProduct
        {
            CorrugatedCardboard,    // Гофрированный картон
            SolidCardboard,         // Плотный картон
            CardboardSheets,        // Картонные листы
            CardboardBoxes,         // Картонные коробки
            CardboardTubes,         // Картонные гильзы
            EggPackaging,           // Упаковка для яиц
            CardboardPallet,        // Картонный паллет
            DisplayStand,           // Картонный стенд
            ProtectivePackaging,    // Защитная упаковка
            CustomShapeCardboard    // Картон сложной формы
        }

        #endregion

        #region Properties - Основные свойства завода

        /// <summary>
        /// Хранилище сырья
        /// </summary>
        public Dictionary<CardboardMaterial, int> MaterialStorage { get; private set; } =
            new Dictionary<CardboardMaterial, int>();

        /// <summary>
        /// Хранилище готовой продукции
        /// </summary>
        public Dictionary<CardboardProduct, int> ProductStorage { get; private set; } =
            new Dictionary<CardboardProduct, int>();

        /// <summary>
        /// Максимальная вместимость склада сырья
        /// </summary>
        public int MaxMaterialStorage { get; private set; } = 2500;

        /// <summary>
        /// Максимальная вместимость склада готовой продукции
        /// </summary>
        public int MaxProductStorage { get; private set; } = 1800;

        /// <summary>
        /// Производственные цеха завода
        /// </summary>
        public List<Workshop> Workshops { get; private set; } = new List<Workshop>();

        /// <summary>
        /// Текущее количество рабочих
        /// </summary>
        public int WorkersCount { get; private set; }

        /// <summary>
        /// Максимальное количество рабочих
        /// </summary>
        public int MaxWorkers { get; private set; }

        /// <summary>
        /// Эффективность производства
        /// </summary>
        public float ProductionEfficiency => WorkersCount > 0 ?
            0.3f + (WorkersCount / (float)MaxWorkers) * 0.7f : 0f;

        #endregion

        #region Constructor - Конструктор

        /// <summary>
        /// Создание нового завода по производству картона
        /// </summary>
        public CardboardFactory()
        {
            WorkersCount = 0;
            MaxWorkers = 12;
            InitializeWorkshops();
            InitializeStartingMaterials();
        }

        #endregion

        #region Initialization - Инициализация

        /// <summary>
        /// Инициализация производственных цехов
        /// </summary>
        private void InitializeWorkshops()
        {
            // Цех подготовки сырья
            var preparationWorkshop = new Workshop
            {
                Name = "Цех подготовки сырья",
                ProductionCycleTime = 6
            };
            preparationWorkshop.InputRequirements.Add(CardboardMaterial.WoodChips, 15);
            preparationWorkshop.InputRequirements.Add(CardboardMaterial.Water, 10);
            preparationWorkshop.InputRequirements.Add(CardboardMaterial.Energy, 5);
            preparationWorkshop.OutputProducts.Add(CardboardProduct.CardboardSheets, 8);
            Workshops.Add(preparationWorkshop);

            // Цех гофрированного картона
            var corrugatedWorkshop = new Workshop
            {
                Name = "Цех гофрированного картона",
                ProductionCycleTime = 8
            };
            corrugatedWorkshop.InputRequirements.Add(CardboardProduct.CardboardSheets, 6);
            corrugatedWorkshop.InputRequirements.Add(CardboardMaterial.Chemicals, 3);
            corrugatedWorkshop.InputRequirements.Add(CardboardMaterial.Energy, 8);
            corrugatedWorkshop.OutputProducts.Add(CardboardProduct.CorrugatedCardboard, 4);
            corrugatedWorkshop.OutputProducts.Add(CardboardProduct.CardboardBoxes, 3);
            Workshops.Add(corrugatedWorkshop);

            // Цех плотного картона
            var solidCardboardWorkshop = new Workshop
            {
                Name = "Цех плотного картона",
                ProductionCycleTime = 7
            };
            solidCardboardWorkshop.InputRequirements.Add(CardboardMaterial.RecycledPaper, 12);
            solidCardboardWorkshop.InputRequirements.Add(CardboardMaterial.Chemicals, 2);
            solidCardboardWorkshop.InputRequirements.Add(CardboardMaterial.Water, 8);
            solidCardboardWorkshop.InputRequirements.Add(CardboardMaterial.Energy, 6);
            solidCardboardWorkshop.OutputProducts.Add(CardboardProduct.SolidCardboard, 5);
            solidCardboardWorkshop.OutputProducts.Add(CardboardProduct.ProtectivePackaging, 4);
            Workshops.Add(solidCardboardWorkshop);

            // Цех специальных изделий
            var specialProductsWorkshop = new Workshop
            {
                Name = "Цех специальных изделий",
                ProductionCycleTime = 9
            };
            specialProductsWorkshop.InputRequirements.Add(CardboardProduct.SolidCardboard, 4);
            specialProductsWorkshop.InputRequirements.Add(CardboardProduct.CorrugatedCardboard, 3);
            specialProductsWorkshop.InputRequirements.Add(CardboardMaterial.Chemicals, 1);
            specialProductsWorkshop.OutputProducts.Add(CardboardProduct.CardboardTubes, 6);
            specialProductsWorkshop.OutputProducts.Add(CardboardProduct.EggPackaging, 8);
            specialProductsWorkshop.OutputProducts.Add(CardboardProduct.DisplayStand, 2);
            Workshops.Add(specialProductsWorkshop);

            // Цех упаковочных решений
            var packagingWorkshop = new Workshop
            {
                Name = "Цех упаковочных решений",
                ProductionCycleTime = 10
            };
            packagingWorkshop.InputRequirements.Add(CardboardProduct.CorrugatedCardboard, 5);
            packagingWorkshop.InputRequirements.Add(CardboardProduct.SolidCardboard, 3);
            packagingWorkshop.InputRequirements.Add(CardboardMaterial.Energy, 4);
            packagingWorkshop.OutputProducts.Add(CardboardProduct.CardboardPallet, 2);
            packagingWorkshop.OutputProducts.Add(CardboardProduct.CustomShapeCardboard, 3);
            Workshops.Add(packagingWorkshop);
        }

        /// <summary>
        /// Инициализация начальных материалов
        /// </summary>
        private void InitializeStartingMaterials()
        {
            MaterialStorage.Clear();

            AddMaterial(CardboardMaterial.WoodChips, 400);
            AddMaterial(CardboardMaterial.RecycledPaper, 300);
            AddMaterial(CardboardMaterial.Chemicals, 150);
            AddMaterial(CardboardMaterial.Water, 200);
            AddMaterial(CardboardMaterial.Energy, 100);
        }

        #endregion

        #region Public Methods - Публичные методы

        /// <summary>
        /// Установка количества рабочих
        /// </summary>
        public void SetWorkersCount(int count)
        {
            WorkersCount = Math.Clamp(count, 0, MaxWorkers);
        }

        /// <summary>
        /// Добавление сырья на склад
        /// </summary>
        public bool AddMaterial(CardboardMaterial material, int amount)
        {
            if (amount <= 0) return false;

            int currentAmount = MaterialStorage.ContainsKey(material) ?
                MaterialStorage[material] : 0;

            if (GetTotalMaterialStorage() + amount > MaxMaterialStorage)
                return false;

            MaterialStorage[material] = currentAmount + amount;
            return true;
        }

        /// <summary>
        /// Запуск производственного цикла во всех цехах
        /// </summary>
        public void ProcessWorkshops()
        {
            if (WorkersCount == 0 || ProductionEfficiency <= 0) return;

            var availableResources = new Dictionary<object, int>();
            foreach (var material in MaterialStorage)
            {
                availableResources.Add(material.Key, material.Value);
            }

            var producedOutputs = new Dictionary<object, int>();

            foreach (var workshop in Workshops)
            {
                var workshopResources = new Dictionary<object, int>(availableResources);
                var workshopOutputs = new Dictionary<object, int>();

                if (workshop.Process(workshopResources, workshopOutputs))
                {
                    ApplyProductionEfficiency(workshopOutputs);
                    availableResources = workshopResources;

                    foreach (var output in workshopOutputs)
                    {
                        if (producedOutputs.ContainsKey(output.Key))
                            producedOutputs[output.Key] += output.Value;
                        else
                            producedOutputs[output.Key] = output.Value;
                    }
                }
            }

            UpdateMaterialsStorage(availableResources);
            UpdateProductsStorage(producedOutputs);
        }

        /// <summary>
        /// Получение информации о готовой продукции
        /// </summary>
        public Dictionary<CardboardProduct, int> GetProductionOutput()
        {
            return new Dictionary<CardboardProduct, int>(ProductStorage);
        }

        /// <summary>
        /// Получение информации о запасах сырья
        /// </summary>
        public Dictionary<CardboardMaterial, int> GetMaterialStorage()
        {
            return new Dictionary<CardboardMaterial, int>(MaterialStorage);
        }

        /// <summary>
        /// Потребление готовой продукции
        /// </summary>
        public bool ConsumeProduct(CardboardProduct product, int amount)
        {
            if (amount <= 0) return true;

            if (!ProductStorage.ContainsKey(product) || ProductStorage[product] < amount)
                return false;

            ProductStorage[product] -= amount;
            if (ProductStorage[product] == 0)
                ProductStorage.Remove(product);

            return true;
        }

        /// <summary>
        /// Получение общей статистики производства
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
                { "MaterialTypes", MaterialStorage.Count },
                { "ProductTypes", ProductStorage.Count }
            };
        }

        #endregion

        #region Private Methods - Приватные методы

        /// <summary>
        /// Получение общего количества сырья на складе
        /// </summary>
        private int GetTotalMaterialStorage()
        {
            return MaterialStorage.Values.Sum();
        }

        /// <summary>
        /// Получение общего количества продукции на складе
        /// </summary>
        private int GetTotalProductStorage()
        {
            return ProductStorage.Values.Sum();
        }

        /// <summary>
        /// Применение коэффициента эффективности к произведенной продукции
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
        /// Обновление хранилища материалов
        /// </summary>
        private void UpdateMaterialsStorage(Dictionary<object, int> availableResources)
        {
            MaterialStorage.Clear();
            foreach (var resource in availableResources)
            {
                if (resource.Key is CardboardMaterial material)
                {
                    MaterialStorage[material] = resource.Value;
                }
            }
        }

        /// <summary>
        /// Обновление хранилища продукции
        /// </summary>
        private void UpdateProductsStorage(Dictionary<object, int> producedOutputs)
        {
            foreach (var output in producedOutputs)
            {
                if (output.Key is CardboardProduct product)
                {
                    int currentAmount = ProductStorage.ContainsKey(product) ? ProductStorage[product] : 0;
                    int availableSpace = MaxProductStorage - GetTotalProductStorage();
                    int amountToAdd = Math.Min(output.Value, availableSpace);

                    if (amountToAdd > 0)
                    {
                        ProductStorage[product] = currentAmount + amountToAdd;
                    }

                    if (amountToAdd < output.Value)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"Превышена вместимость склада! Потеряно {output.Value - amountToAdd} единиц продукции {product}");
                    }
                }
            }
        }

        #endregion
    }
}