using System;
using System.Collections.Generic;
using System.Linq;
using Core.Enums;
using Core.Models.Components;

namespace Core.Models.Buildings.CommertialBuildings
{
    /// <summary>
    /// Фабрика по производству косметической продукции
    /// Управляет производственным процессом, включая цеха, сырье и готовую продукцию
    /// </summary>
    public class CosmeticsFactory
    {
        #region Simplified Enums

        /// <summary>
        /// Типы сырья для производства косметики
        /// </summary>
        public enum CosmeticMaterial
        {
            Alcohol,
            Pigments,
            Oils,
            Chemicals,
            Waxes,
            PlantExtracts
        }

        /// <summary>
        /// Типы производимой косметической продукции
        /// </summary>
        public enum CosmeticProduct
        {
            FaceCream,
            BodyLotion,
            Lipstick,
            EyeShadow,
            Perfume,
            EauDeToilette,
            Shampoo,
            Conditioner
        }

        #endregion

        /// <summary>Количество материалов на складе</summary>
        public Dictionary<CosmeticMaterial, int> MaterialsStorage { get; private set; } = new Dictionary<CosmeticMaterial, int>();

        /// <summary>Количество продукции на складе</summary>
        public Dictionary<CosmeticProduct, int> ProductsStorage { get; private set; } = new Dictionary<CosmeticProduct, int>();

        /// <summary>Максимальная вместимость склада материалов</summary>
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
        /// Создает новую косметическую фабрику
        /// Выходные данные: инициализированная фабрика с цехами и стартовыми материалами
        /// </summary>
        public CosmeticsFactory()
        {
            MaxMaterialStorage = 1000;
            MaxProductStorage = 800;
            MaxWorkers = 12;
            WorkersCount = 0;

            InitializeWorkshops();
            InitializeStartingMaterials();
        }

        /// <summary>
        /// Инициализирует цеха фабрики
        /// Входные данные: отсутствуют
        /// Выходные данные: созданные и настроенные производственные цеха
        /// </summary>
        private void InitializeWorkshops()
        {
            // Цех производства кремов и лосьонов
            var creamWorkshop = new Workshop
            {
                Name = "Цех кремов и лосьонов",
                ProductionCycleTime = 6
            };
            creamWorkshop.InputRequirements.Add(CosmeticMaterial.Alcohol, 2);
            creamWorkshop.InputRequirements.Add(CosmeticMaterial.Oils, 3);
            creamWorkshop.InputRequirements.Add(CosmeticMaterial.Chemicals, 2);
            creamWorkshop.OutputProducts.Add(CosmeticProduct.FaceCream, 8);
            creamWorkshop.OutputProducts.Add(CosmeticProduct.BodyLotion, 6);
            Workshops.Add(creamWorkshop);

            // Цех декоративной косметики
            var makeupWorkshop = new Workshop
            {
                Name = "Цех декоративной косметики",
                ProductionCycleTime = 8
            };
            makeupWorkshop.InputRequirements.Add(CosmeticMaterial.Pigments, 4);
            makeupWorkshop.InputRequirements.Add(CosmeticMaterial.Waxes, 3);
            makeupWorkshop.InputRequirements.Add(CosmeticMaterial.Oils, 2);
            makeupWorkshop.OutputProducts.Add(CosmeticProduct.Lipstick, 10);
            makeupWorkshop.OutputProducts.Add(CosmeticProduct.EyeShadow, 12);
            Workshops.Add(makeupWorkshop);

            // Цех парфюмерии
            var perfumeWorkshop = new Workshop
            {
                Name = "Цех парфюмерии",
                ProductionCycleTime = 10
            };
            perfumeWorkshop.InputRequirements.Add(CosmeticMaterial.Alcohol, 5);
            perfumeWorkshop.InputRequirements.Add(CosmeticMaterial.Oils, 3);
            perfumeWorkshop.InputRequirements.Add(CosmeticMaterial.PlantExtracts, 2);
            perfumeWorkshop.OutputProducts.Add(CosmeticProduct.Perfume, 6);
            perfumeWorkshop.OutputProducts.Add(CosmeticProduct.EauDeToilette, 8);
            Workshops.Add(perfumeWorkshop);

            // Цех ухода за волосами
            var hairCareWorkshop = new Workshop
            {
                Name = "Цех ухода за волосами",
                ProductionCycleTime = 7
            };
            hairCareWorkshop.InputRequirements.Add(CosmeticMaterial.PlantExtracts, 3);
            hairCareWorkshop.InputRequirements.Add(CosmeticMaterial.Chemicals, 3);
            hairCareWorkshop.InputRequirements.Add(CosmeticMaterial.Oils, 2);
            hairCareWorkshop.OutputProducts.Add(CosmeticProduct.Shampoo, 15);
            hairCareWorkshop.OutputProducts.Add(CosmeticProduct.Conditioner, 12);
            Workshops.Add(hairCareWorkshop);
        }

        /// <summary>
        /// Инициализирует стартовые материалы
        /// Входные данные: отсутствуют
        /// Выходные данные: заполненное хранилище начальными материалами
        /// </summary>
        private void InitializeStartingMaterials()
        {
            AddMaterial(CosmeticMaterial.Alcohol, 100);
            AddMaterial(CosmeticMaterial.Pigments, 80);
            AddMaterial(CosmeticMaterial.Oils, 120);
            AddMaterial(CosmeticMaterial.Chemicals, 90);
            AddMaterial(CosmeticMaterial.Waxes, 60);
            AddMaterial(CosmeticMaterial.PlantExtracts, 70);
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
        /// Добавляет материал на склад фабрики
        /// Входные данные: material - тип материала, amount - количество для добавления
        /// Выходные данные: true если материал успешно добавлен, false если превышена вместимость склада
        /// </summary>
        /// <param name="material">Тип добавляемого материала</param>
        /// <param name="amount">Количество материала для добавления</param>
        /// <returns>True если материал успешно добавлен, false в противном случае</returns>
        public bool AddMaterial(CosmeticMaterial material, int amount)
        {
            int currentAmount = MaterialsStorage.ContainsKey(material) ? MaterialsStorage[material] : 0;
            if (currentAmount + amount > MaxMaterialStorage)
                return false;

            MaterialsStorage[material] = currentAmount + amount;
            return true;
        }

        /// <summary>
        /// Запускает производственные циклы во всех цехах фабрики
        /// Входные данные: текущее состояние хранилищ материалов и продукции
        /// Выходные данные: отсутствуют (обновляет хранилища материалов и продукции)
        /// </summary>
        public void ProcessWorkshops()
        {
            if (WorkersCount == 0) return;

            // Создаем копию текущих материалов для обработки
            var availableResources = new Dictionary<object, int>();
            foreach (var material in MaterialsStorage)
            {
                availableResources.Add(material.Key, material.Value);
            }

            // Добавляем существующую продукцию как доступный ресурс для возможных дальнейших переработок
            foreach (var product in ProductsStorage)
            {
                availableResources.Add(product.Key, product.Value);
            }

            var producedOutputs = new Dictionary<object, int>();

            // Обрабатываем каждый цех
            foreach (var workshop in Workshops)
            {
                workshop.Process(availableResources, producedOutputs);
            }

            // Обновляем хранилище материалов
            MaterialsStorage.Clear();
            foreach (var resource in availableResources)
            {
                if (resource.Key is CosmeticMaterial material)
                {
                    MaterialsStorage[material] = resource.Value;
                }
            }

            // Обновляем хранилище продукции
            foreach (var output in producedOutputs)
            {
                if (output.Key is CosmeticProduct product)
                {
                    int currentAmount = ProductsStorage.ContainsKey(product) ? ProductsStorage[product] : 0;
                    if (currentAmount + output.Value <= MaxProductStorage)
                    {
                        ProductsStorage[product] = currentAmount + output.Value;
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
        public Dictionary<CosmeticProduct, int> GetProductionOutput()
        {
            return new Dictionary<CosmeticProduct, int>(ProductsStorage);
        }

        /// <summary>
        /// Получает текущие запасы сырья на складе
        /// Входные данные: отсутствуют
        /// Выходные данные: словарь с типами материалов и их количеством на складе
        /// </summary>
        /// <returns>Словарь с текущими запасами сырья</returns>
        public Dictionary<CosmeticMaterial, int> GetRawMaterials()
        {
            return new Dictionary<CosmeticMaterial, int>(MaterialsStorage);
        }

        /// <summary>
        /// Потребляет продукцию (продажа или использование)
        /// Входные данные: product - тип продукции, amount - количество для потребления
        /// Выходные данные: true если продукция успешно потреблена, false если недостаточно продукции
        /// </summary>
        /// <param name="product">Тип потребляемой продукции</param>
        /// <param name="amount">Количество продукции для потребления</param>
        /// <returns>True если продукция успешно потреблена, false в противном случае</returns>
        public bool ConsumeProduct(CosmeticProduct product, int amount)
        {
            if (!ProductsStorage.ContainsKey(product) || ProductsStorage[product] < amount)
                return false;

            ProductsStorage[product] -= amount;
            if (ProductsStorage[product] == 0)
                ProductsStorage.Remove(product);

            return true;
        }

        /// <summary>
        /// Получает общее количество материалов на складе
        /// Входные данные: отсутствуют
        /// Выходные данные: общее количество всех материалов на складе
        /// </summary>
        /// <returns>Общее количество материалов</returns>
        public int GetTotalMaterialStorage()
        {
            return MaterialsStorage.Values.Sum();
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
    }
}