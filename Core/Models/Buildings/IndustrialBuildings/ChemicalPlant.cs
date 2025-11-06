using Core.Enums;
using Core.Interfaces;
using Core.Models.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Models.Buildings.IndustrialBuildings
{
    /// <summary>
    /// Химический завод - производит широкий спектр химической продукции
    /// </summary>
    public class ChemicalPlant : CommercialBuilding, IConstructable<ChemicalPlant>
    {
        #region Static Properties - Construction Cost

        public static decimal BuildCost { get; protected set; } = 450000m;

        public static Dictionary<ConstructionMaterial, int> RequiredMaterials { get; protected set; }
            = new Dictionary<ConstructionMaterial, int>
            {
                { ConstructionMaterial.Steel, 15 },
                { ConstructionMaterial.Concrete, 12 },
                { ConstructionMaterial.Glass, 8 },
                { ConstructionMaterial.Plastic, 5 }
            };

        #endregion

        #region Instance Properties

        /// <summary>
        /// Специализация химического завода
        /// </summary>
        public ChemicalIndustryType PlantType { get; private set; }

        /// <summary>
        /// Хранилище химического сырья
        /// </summary>
        public Dictionary<ChemicalMaterial, int> RawMaterialsStorage { get; private set; } = new Dictionary<ChemicalMaterial, int>();

        /// <summary>
        /// Хранилище химической продукции
        /// </summary>
        public Dictionary<ChemicalProduct, int> ProductsStorage { get; private set; } = new Dictionary<ChemicalProduct, int>();

        /// <summary>
        /// Производственные установки (реакторы, колонны и т.д.)
        /// </summary>
        public List<ChemicalReactor> ProductionUnits { get; private set; } = new List<ChemicalReactor>();

        /// <summary>
        /// Максимальная вместимость склада сырья
        /// </summary>
        public int MaxRawMaterialStorage { get; private set; } = 2000;

        /// <summary>
        /// Максимальная вместимость склада продукции
        /// </summary>
        public int MaxProductStorage { get; private set; } = 1500;

        /// <summary>
        /// Текущее количество химиков-технологов
        /// </summary>
        public int ChemistsCount { get; private set; }

        /// <summary>
        /// Максимальное количество химиков
        /// </summary>
        public int MaxChemists { get; private set; } = 20;

        /// <summary>
        /// Уровень безопасности завода
        /// </summary>
        public float SafetyLevel { get; private set; } = 0.8f;

        /// <summary>
        /// Уровень экологической безопасности
        /// </summary>
        public float EnvironmentalSafety { get; private set; } = 0.7f;

        /// <summary>
        /// Эффективность производства
        /// </summary>
        public float ProductionEfficiency => ChemistsCount > 0 ?
            0.4f + (ChemistsCount / (float)MaxChemists) * 0.4f + SafetyLevel * 0.2f : 0f;

        #endregion

        #region Constructor

        public ChemicalPlant(ChemicalIndustryType plantType) : base(CommercialBuildingType.Factory)
        {
            PlantType = plantType;
            InitializeChemicalPlant();
        }

        #endregion

        #region Initialization

        private void InitializeChemicalPlant()
        {
            InitializeProductionUnits();
            InitializeStartingMaterials();
            InitializeSafetySystems();
        }

        private void InitializeProductionUnits()
        {
            // Создаем производственные установки в зависимости от типа завода
            switch (PlantType)
            {
                case ChemicalIndustryType.PetrochemicalPlant:
                    ProductionUnits.Add(new ChemicalReactor
                    {
                        Name = "Установка крекинга",
                        ProcessType = ChemicalProcess.Cracking,
                        Capacity = 100,
                        InputMaterials = new List<ChemicalMaterial> { ChemicalMaterial.CrudeOil },
                        OutputProducts = new List<ChemicalProduct> { ChemicalProduct.Ethylene, ChemicalProduct.Propylene }
                    });
                    break;

                case ChemicalIndustryType.FertilizerPlant:
                    ProductionUnits.Add(new ChemicalReactor
                    {
                        Name = "Аммиачный реактор",
                        ProcessType = ChemicalProcess.Hydrogenation,
                        Capacity = 80,
                        InputMaterials = new List<ChemicalMaterial> { ChemicalMaterial.NaturalGas, ChemicalMaterial.Nitrogen },
                        OutputProducts = new List<ChemicalProduct> { ChemicalProduct.Ammonia, ChemicalProduct.Urea }
                    });
                    break;

                case ChemicalIndustryType.PolymerPlant:
                    ProductionUnits.Add(new ChemicalReactor
                    {
                        Name = "Полимеризационный реактор",
                        ProcessType = ChemicalProcess.Polymerization,
                        Capacity = 60,
                        InputMaterials = new List<ChemicalMaterial> { ChemicalMaterial.Ethylene, ChemicalMaterial.Propylene },
                        OutputProducts = new List<ChemicalProduct> { ChemicalProduct.Polyethylene, ChemicalProduct.Polypropylene }
                    });
                    break;

                case ChemicalIndustryType.PharmaceuticalPlant:
                    ProductionUnits.Add(new ChemicalReactor
                    {
                        Name = "Биохимический реактор",
                        ProcessType = ChemicalProcess.Fermentation,
                        Capacity = 40,
                        InputMaterials = new List<ChemicalMaterial> { ChemicalMaterial.Ethanol, ChemicalMaterial.Nitrogen },
                        OutputProducts = new List<ChemicalProduct> { ChemicalProduct.Pharmaceuticals }
                    });
                    break;
            }
        }

        private void InitializeStartingMaterials()
        {
            // Загружаем стартовое сырье в зависимости от типа завода
            switch (PlantType)
            {
                case ChemicalIndustryType.PetrochemicalPlant:
                    AddRawMaterial(ChemicalMaterial.CrudeOil, 500);
                    AddRawMaterial(ChemicalMaterial.NaturalGas, 300);
                    break;

                case ChemicalIndustryType.FertilizerPlant:
                    AddRawMaterial(ChemicalMaterial.NaturalGas, 400);
                    AddRawMaterial(ChemicalMaterial.Nitrogen, 200);
                    AddRawMaterial(ChemicalMaterial.Phosphates, 300);
                    break;

                case ChemicalIndustryType.PolymerPlant:
                    AddRawMaterial(ChemicalMaterial.Ethylene, 350);
                    AddRawMaterial(ChemicalMaterial.Propylene, 250);
                    break;

                case ChemicalIndustryType.PharmaceuticalPlant:
                    AddRawMaterial(ChemicalMaterial.Ethanol, 200);
                    AddRawMaterial(ChemicalMaterial.Nitrogen, 150);
                    break;
            }
        }

        private void InitializeSafetySystems()
        {
            // Инициализация систем безопасности
            SafetyLevel = 0.8f;
            EnvironmentalSafety = 0.7f;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Установить количество химиков
        /// </summary>
        public void SetChemistsCount(int count)
        {
            ChemistsCount = Math.Min(count, MaxChemists);
        }

        /// <summary>
        /// Добавить сырье на склад
        /// </summary>
        public bool AddRawMaterial(ChemicalMaterial material, int amount)
        {
            int currentAmount = RawMaterialsStorage.ContainsKey(material) ? RawMaterialsStorage[material] : 0;
            if (GetTotalRawMaterialStorage() + amount > MaxRawMaterialStorage)
                return false;

            RawMaterialsStorage[material] = currentAmount + amount;
            return true;
        }

        /// <summary>
        /// Запустить химические процессы
        /// </summary>
        public void RunChemicalProcesses()
        {
            if (ChemistsCount == 0 || ProductionEfficiency <= 0) return;

            foreach (var reactor in ProductionUnits)
            {
                if (reactor.CanProcess(RawMaterialsStorage))
                {
                    var products = reactor.Process(RawMaterialsStorage, ProductionEfficiency);
                    UpdateProductsStorage(products);
                }
            }
        }

        /// <summary>
        /// Улучшить систему безопасности
        /// </summary>
        public void UpgradeSafetySystem()
        {
            SafetyLevel = Math.Min(1.0f, SafetyLevel + 0.05f);
            EnvironmentalSafety = Math.Min(1.0f, EnvironmentalSafety + 0.03f);
        }

        /// <summary>
        /// Добавить новую производственную установку
        /// </summary>
        public void AddProductionUnit(ChemicalReactor reactor)
        {
            ProductionUnits.Add(reactor);
        }

        /// <summary>
        /// Получить информацию о производстве
        /// </summary>
        public Dictionary<string, object> GetProductionInfo()
        {
            return new Dictionary<string, object>
            {
                { "PlantType", PlantType },
                { "ChemistsCount", ChemistsCount },
                { "MaxChemists", MaxChemists },
                { "ProductionEfficiency", ProductionEfficiency },
                { "SafetyLevel", SafetyLevel },
                { "EnvironmentalSafety", EnvironmentalSafety },
                { "TotalRawMaterialStorage", GetTotalRawMaterialStorage() },
                { "MaxRawMaterialStorage", MaxRawMaterialStorage },
                { "TotalProductStorage", GetTotalProductStorage() },
                { "MaxProductStorage", MaxProductStorage },
                { "ActiveReactors", ProductionUnits.Count },
                { "ProductionUnits", ProductionUnits.Select(r => r.Name).ToList() }
            };
        }

        /// <summary>
        /// Получить текущие запасы сырья
        /// </summary>
        public Dictionary<ChemicalMaterial, int> GetRawMaterials()
        {
            return new Dictionary<ChemicalMaterial, int>(RawMaterialsStorage);
        }

        /// <summary>
        /// Получить текущие запасы продукции
        /// </summary>
        public Dictionary<ChemicalProduct, int> GetChemicalProducts()
        {
            return new Dictionary<ChemicalProduct, int>(ProductsStorage);
        }

        /// <summary>
        /// Продать химическую продукцию
        /// </summary>
        public bool SellProduct(ChemicalProduct product, int quantity)
        {
            if (!ProductsStorage.ContainsKey(product) || ProductsStorage[product] < quantity)
                return false;

            ProductsStorage[product] -= quantity;
            if (ProductsStorage[product] == 0)
                ProductsStorage.Remove(product);

            return true;
        }

        #endregion

        #region Private Methods

        public int GetTotalRawMaterialStorage()
        {
            return RawMaterialsStorage.Values.Sum();
        }

        public int GetTotalProductStorage()
        {
            return ProductsStorage.Values.Sum();
        }

        private void UpdateProductsStorage(Dictionary<ChemicalProduct, int> newProducts)
        {
            foreach (var product in newProducts)
            {
                int currentAmount = ProductsStorage.ContainsKey(product.Key) ? ProductsStorage[product.Key] : 0;
                int availableSpace = MaxProductStorage - GetTotalProductStorage();
                int amountToAdd = Math.Min(product.Value, availableSpace);

                if (amountToAdd > 0)
                {
                    ProductsStorage[product.Key] = currentAmount + amountToAdd;
                }

                if (amountToAdd < product.Value)
                {
                    System.Diagnostics.Debug.WriteLine($"Превышена вместимость склада! Потеряно {product.Value - amountToAdd} единиц продукции {product.Key}");
                }
            }
        }

        #endregion

        public override void OnBuildingPlaced()
        {
            // Запуск начальных химических процессов при размещении
            RunChemicalProcesses();
            Console.WriteLine($"Химический завод {PlantType} размещен и запущен!");
        }
    }

    /// <summary>
    /// Химический реактор - основная производственная единица
    /// </summary>
    public class ChemicalReactor
    {
        public string Name { get; set; }
        public ChemicalProcess ProcessType { get; set; }
        public int Capacity { get; set; }
        public List<ChemicalMaterial> InputMaterials { get; set; } = new List<ChemicalMaterial>();
        public List<ChemicalProduct> OutputProducts { get; set; } = new List<ChemicalProduct>();
        public float Efficiency { get; set; } = 1.0f;

        /// <summary>
        /// Проверить возможность запуска процесса
        /// </summary>
        public bool CanProcess(Dictionary<ChemicalMaterial, int> availableMaterials)
        {
            foreach (var material in InputMaterials)
            {
                if (!availableMaterials.ContainsKey(material) || availableMaterials[material] < Capacity)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Запустить химический процесс
        /// </summary>
        public Dictionary<ChemicalProduct, int> Process(Dictionary<ChemicalMaterial, int> availableMaterials, float productionEfficiency)
        {
            var products = new Dictionary<ChemicalProduct, int>();

            // Потребляем сырье
            foreach (var material in InputMaterials)
            {
                if (availableMaterials.ContainsKey(material))
                {
                    availableMaterials[material] -= Capacity;
                    if (availableMaterials[material] <= 0)
                        availableMaterials.Remove(material);
                }
            }

            // Производим продукцию
            foreach (var product in OutputProducts)
            {
                int outputAmount = (int)(Capacity * Efficiency * productionEfficiency);
                products[product] = outputAmount;
            }

            return products;
        }
    }
}