using Core.Enums;
using Core.Enums.Core.Enums;
using Core.Interfaces;
using Core.Models.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Models.Buildings.IndustrialBuildings
{
    public class PaintAndVarnishFactory : CommercialBuilding
    {
        #region Static Properties - Construction Cost

        public static decimal BuildCost { get; protected set; } = 350000m;

        public static Dictionary<ConstructionMaterial, int> RequiredMaterials { get; protected set; }
            = new Dictionary<ConstructionMaterial, int>
            {
                { ConstructionMaterial.Steel, 6 },
                { ConstructionMaterial.Concrete, 10 },
                { ConstructionMaterial.Glass, 5 },
                { ConstructionMaterial.Plastic, 4 }
            };

        #endregion

        #region Enums for Paint Industry

        /// <summary>
        /// Типы сырья для производства красок и лаков
        /// </summary>
        public enum PaintMaterial
        {
            Iron,           // Железо (для пигментов)
            Copper,         // Медь (для антикоррозийных покрытий)
            Oil,            // Нефть (основа для масляных красок)
            Gas,            // Газ (для синтеза полимеров)
            Resins,         // Смолы (импорт)
            Solvents,       // Растворители (импорт)
            Additives,      // Добавки (импорт)
            Titanium,       // Диоксид титана (импорт)
            Pigments        // Пигменты (импорт)
        }

        /// <summary>
        /// Типы производимой лакокрасочной продукции
        /// </summary>
        public enum PaintProduct
        {
            // Для строительной промышленности
            InteriorWallPaint,      // Краска для внутренних стен
            ExteriorFacadePaint,    // Фасадная краска
            FloorEnamel,           // Эмаль для полов

            // Для автомобильной промышленности  
            AutomotivePaint,        // Автомобильная эмаль
            AntiCorrosiveCoating,   // Антикоррозийное покрытие

            // Для мебельной промышленности
            FurnitureVarnish,       // Мебельный лак
            WoodStain,              // Морилка для дерева

            // Для судостроения
            MarinePaint,            // Судостроительная краска

            // Для авиационной промышленности
            AerospaceCoating,       // Авиационное покрытие

            // Универсальные продукты
            UniversalEnamel,        // Универсальная эмаль
            ProtectivePrimer        // Грунтовка-праймер
        }

        /// <summary>
        /// Отрасли промышленности-потребители
        /// </summary>
        public enum IndustrialSector
        {
            Construction,    // Строительная промышленность
            Automotive,      // Автомобильная промышленность  
            Furniture,       // Мебельная промышленность
            Shipbuilding,    // Судостроение
            Aerospace,       // Авиационная промышленность
            GeneralIndustry  // Общее машиностроение
        }

        #endregion

        #region Instance Properties

        public Dictionary<PaintMaterial, int> MaterialsStorage { get; private set; } = new Dictionary<PaintMaterial, int>();
        public Dictionary<PaintProduct, int> ProductsStorage { get; private set; } = new Dictionary<PaintProduct, int>();
        public int MaxMaterialStorage { get; private set; }
        public int MaxProductStorage { get; private set; }
        public List<Workshop> Workshops { get; private set; } = new List<Workshop>();
        public int WorkersCount { get; private set; }
        public int MaxWorkers { get; private set; }

        // Стоимость лакокрасочной продукции
        public Dictionary<PaintProduct, decimal> ProductPrices { get; private set; } = new Dictionary<PaintProduct, decimal>();

        // Словарь для отслеживания отраслей-потребителей для каждого продукта
        public Dictionary<PaintProduct, List<IndustrialSector>> ProductSectors { get; private set; } 
            = new Dictionary<PaintProduct, List<IndustrialSector>>();

        // Коэффициент эффективности производства
        public float ProductionEfficiency => WorkersCount > 0 ? 0.4f + (WorkersCount / (float)MaxWorkers) * 0.6f : 0f;

        #endregion

        public PaintAndVarnishFactory() : base(CommercialBuildingType.Factory)
        {
            MaxMaterialStorage = 1500;
            MaxProductStorage = 1000;
            MaxWorkers = 15;
            WorkersCount = 0;

            InitializeProductSectors();
            InitializeWorkshops();
            InitializeProductPrices();
            InitializeStartingMaterials();
        }

        /// <summary>
        /// Инициализация отраслей-потребителей для каждого продукта
        /// </summary>
        private void InitializeProductSectors()
        {
            ProductSectors[PaintProduct.InteriorWallPaint] = new List<IndustrialSector> { IndustrialSector.Construction };
            ProductSectors[PaintProduct.ExteriorFacadePaint] = new List<IndustrialSector> { IndustrialSector.Construction };
            ProductSectors[PaintProduct.FloorEnamel] = new List<IndustrialSector> { 
                IndustrialSector.Construction, IndustrialSector.GeneralIndustry };

            ProductSectors[PaintProduct.AutomotivePaint] = new List<IndustrialSector> { IndustrialSector.Automotive };
            ProductSectors[PaintProduct.AntiCorrosiveCoating] = new List<IndustrialSector> {
                IndustrialSector.Automotive, IndustrialSector.Shipbuilding, IndustrialSector.GeneralIndustry };

            ProductSectors[PaintProduct.FurnitureVarnish] = new List<IndustrialSector> { IndustrialSector.Furniture };
            ProductSectors[PaintProduct.WoodStain] = new List<IndustrialSector> { 
                IndustrialSector.Furniture, IndustrialSector.Construction };

            ProductSectors[PaintProduct.MarinePaint] = new List<IndustrialSector> { IndustrialSector.Shipbuilding };
            ProductSectors[PaintProduct.AerospaceCoating] = new List<IndustrialSector> { IndustrialSector.Aerospace };

            ProductSectors[PaintProduct.UniversalEnamel] = new List<IndustrialSector> { 
                IndustrialSector.GeneralIndustry, IndustrialSector.Construction, IndustrialSector.Automotive };
            ProductSectors[PaintProduct.ProtectivePrimer] = new List<IndustrialSector> { 
                IndustrialSector.Construction, IndustrialSector.Shipbuilding, IndustrialSector.Automotive };
        }
        /// <summary>
        /// Инициализация цен на продукцию
        /// </summary>
        private void InitializeProductPrices()
        {
            // Строительные краски
            ProductPrices[PaintProduct.InteriorWallPaint] = 800m;      // Краска для внутренних стен
            ProductPrices[PaintProduct.ExteriorFacadePaint] = 1200m;   // Фасадная краска
            ProductPrices[PaintProduct.FloorEnamel] = 1500m;           // Эмаль для полов
            ProductPrices[PaintProduct.WoodStain] = 600m;              // Морилка для дерева

            // Автомобильные покрытия
            ProductPrices[PaintProduct.AutomotivePaint] = 2500m;       // Автомобильная эмаль
            ProductPrices[PaintProduct.AntiCorrosiveCoating] = 1800m;  // Антикоррозийное покрытие

            // Мебельные покрытия
            ProductPrices[PaintProduct.FurnitureVarnish] = 2000m;      // Мебельный лак

            // Специализированные покрытия
            ProductPrices[PaintProduct.MarinePaint] = 3000m;           // Судостроительная краска
            ProductPrices[PaintProduct.AerospaceCoating] = 5000m;      // Авиационное покрытие

            // Универсальные продукты
            ProductPrices[PaintProduct.UniversalEnamel] = 1000m;       // Универсальная эмаль
            ProductPrices[PaintProduct.ProtectivePrimer] = 500m;       // Грунтовка-праймер
        }

        /// <summary>
        /// Инициализация производственных цехов
        /// </summary>
        private void InitializeWorkshops()
        {
            // Цех водно-дисперсионных красок (для строительства)
            var waterBasedWorkshop = new Workshop
            {
                Name = "Цех водно-дисперсионных красок",
                ProductionCycleTime = 8
            };
            waterBasedWorkshop.InputRequirements.Add(PaintMaterial.Titanium.ToString(), 8);
            waterBasedWorkshop.InputRequirements.Add(PaintMaterial.Resins.ToString(), 6);
            waterBasedWorkshop.InputRequirements.Add(PaintMaterial.Additives.ToString(), 3);
            waterBasedWorkshop.InputRequirements.Add(PaintMaterial.Pigments.ToString(), 5);
            waterBasedWorkshop.OutputProducts.Add(PaintProduct.InteriorWallPaint.ToString(), 12);
            waterBasedWorkshop.OutputProducts.Add(PaintProduct.ExteriorFacadePaint.ToString(), 10);
            Workshops.Add(waterBasedWorkshop);

            // Цех эмалей и лаков (для автомобильной и мебельной промышленности)
            var enamelWorkshop = new Workshop
            {
                Name = "Цех эмалей и лаков",
                ProductionCycleTime = 10
            };
            enamelWorkshop.InputRequirements.Add(PaintMaterial.Oil.ToString(), 10);
            enamelWorkshop.InputRequirements.Add(PaintMaterial.Solvents.ToString(), 8);
            enamelWorkshop.InputRequirements.Add(PaintMaterial.Resins.ToString(), 7);
            enamelWorkshop.InputRequirements.Add(PaintMaterial.Pigments.ToString(), 6);
            enamelWorkshop.OutputProducts.Add(PaintProduct.AutomotivePaint.ToString(), 8);
            enamelWorkshop.OutputProducts.Add(PaintProduct.FurnitureVarnish.ToString(), 9);
            enamelWorkshop.OutputProducts.Add(PaintProduct.UniversalEnamel.ToString(), 11);
            Workshops.Add(enamelWorkshop);

            // Цех антикоррозийных покрытий (для автомобилей и судостроения)
            var antiCorrosionWorkshop = new Workshop
            {
                Name = "Цех антикоррозийных покрытий",
                ProductionCycleTime = 12
            };
            antiCorrosionWorkshop.InputRequirements.Add(PaintMaterial.Copper.ToString(), 6);
            antiCorrosionWorkshop.InputRequirements.Add(PaintMaterial.Iron.ToString(), 8);
            antiCorrosionWorkshop.InputRequirements.Add(PaintMaterial.Resins.ToString(), 5);
            antiCorrosionWorkshop.InputRequirements.Add(PaintMaterial.Solvents.ToString(), 4);
            antiCorrosionWorkshop.OutputProducts.Add(PaintProduct.AntiCorrosiveCoating.ToString(), 7);
            antiCorrosionWorkshop.OutputProducts.Add(PaintProduct.ProtectivePrimer.ToString(), 10);
            Workshops.Add(antiCorrosionWorkshop);

            // Цех специализированных покрытий (для судостроения и авиации)
            var specializedWorkshop = new Workshop
            {
                Name = "Цех специализированных покрытий",
                ProductionCycleTime = 15
            };
            specializedWorkshop.InputRequirements.Add(PaintMaterial.Gas.ToString(), 8);
            specializedWorkshop.InputRequirements.Add(PaintMaterial.Resins.ToString(), 10);
            specializedWorkshop.InputRequirements.Add(PaintMaterial.Additives.ToString(), 6);
            specializedWorkshop.InputRequirements.Add(PaintMaterial.Pigments.ToString(), 7);
            specializedWorkshop.OutputProducts.Add(PaintProduct.MarinePaint.ToString(), 6);
            specializedWorkshop.OutputProducts.Add(PaintProduct.AerospaceCoating.ToString(), 5);
            Workshops.Add(specializedWorkshop);

            // Цех отделочных материалов (для полов и дерева)
            var finishingWorkshop = new Workshop
            {
                Name = "Цех отделочных материалов",
                ProductionCycleTime = 9
            };
            finishingWorkshop.InputRequirements.Add(PaintMaterial.Oil.ToString(), 7);
            finishingWorkshop.InputRequirements.Add(PaintMaterial.Resins.ToString(), 5);
            finishingWorkshop.InputRequirements.Add(PaintMaterial.Pigments.ToString(), 4);
            finishingWorkshop.InputRequirements.Add(PaintMaterial.Solvents.ToString(), 3);
            finishingWorkshop.OutputProducts.Add(PaintProduct.FloorEnamel.ToString(), 8);
            finishingWorkshop.OutputProducts.Add(PaintProduct.WoodStain.ToString(), 9);
            Workshops.Add(finishingWorkshop);
        }

        /// <summary>
        /// Инициализация стартовых материалов
        /// </summary>
        private void InitializeStartingMaterials()
        {
            // Локальные ресурсы
            AddMaterial(PaintMaterial.Iron, 200);
            AddMaterial(PaintMaterial.Copper, 150);
            AddMaterial(PaintMaterial.Oil, 300);
            AddMaterial(PaintMaterial.Gas, 250);

            // Импортные материалы
            AddMaterial(PaintMaterial.Resins, 180);
            AddMaterial(PaintMaterial.Solvents, 220);
            AddMaterial(PaintMaterial.Additives, 120);
            AddMaterial(PaintMaterial.Titanium, 100);
            AddMaterial(PaintMaterial.Pigments, 200);
        }

        #region Public Methods

        public void SetWorkersCount(int count)
        {
            WorkersCount = Math.Min(count, MaxWorkers);
        }

        public bool AddMaterial(PaintMaterial material, int amount)
        {
            int currentAmount = MaterialsStorage.ContainsKey(material) ? MaterialsStorage[material] : 0;
            if (GetTotalMaterialStorage() + amount > MaxMaterialStorage)
                return false;

            MaterialsStorage[material] = currentAmount + amount;
            return true;
        }

        public int GetTotalMaterialStorage()
        {
            return MaterialsStorage.Values.Sum();
        }

        public void ProcessWorkshops()
        {
            if (WorkersCount == 0 || ProductionEfficiency <= 0) return;

            var availableResources = new Dictionary<object, int>();
            foreach (var material in MaterialsStorage)
            {
                availableResources.Add(material.Key.ToString(), material.Value);
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

        public void FullProductionCycle()
        {
            ProcessWorkshops();
        }

        public Dictionary<PaintProduct, int> GetProductionOutput()
        {
            return new Dictionary<PaintProduct, int>(ProductsStorage);
        }

        public Dictionary<PaintMaterial, int> GetMaterialStorage()
        {
            return new Dictionary<PaintMaterial, int>(MaterialsStorage);
        }

        public bool ConsumeProduct(PaintProduct product, int amount)
        {
            if (!ProductsStorage.ContainsKey(product) || ProductsStorage[product] < amount)
                return false;

            ProductsStorage[product] -= amount;
            if (ProductsStorage[product] == 0)
                ProductsStorage.Remove(product);

            return true;
        }

        public int GetTotalProductStorage()
        {
            return ProductsStorage.Values.Sum();
        }

        /// <summary>
        /// Получает продукты для конкретной отрасли промышленности
        /// </summary>
        public Dictionary<PaintProduct, int> GetProductsForSector(IndustrialSector sector)
        {
            return ProductsStorage
                .Where(kvp => ProductSectors.ContainsKey(kvp.Key) && ProductSectors[kvp.Key].Contains(sector))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        /// <summary>
        /// Получает информацию о производственных возможностях для отраслей
        /// </summary>
        public Dictionary<IndustrialSector, int> GetSectorProductionCapability()
        {
            var sectorCapability = new Dictionary<IndustrialSector, int>();

            foreach (var sector in Enum.GetValues(typeof(IndustrialSector)).Cast<IndustrialSector>())
            {
                int totalProducts = GetProductsForSector(sector).Values.Sum();
                sectorCapability[sector] = totalProducts;
            }

            return sectorCapability;
        }

        /// <summary>
        /// Получает детальную информацию о производстве
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
                { "SectorCapability", GetSectorProductionCapability() },
                { "InventoryValue", CalculateInventoryValue() }
            };
        }
        /// <summary>
        /// Получает общую стоимость продукции на складе
        /// </summary>
        public decimal CalculateInventoryValue()
        {
            decimal totalValue = 0;
            foreach (var product in ProductsStorage)
            {
                if (ProductPrices.ContainsKey(product.Key))
                {
                    totalValue += ProductPrices[product.Key] * product.Value;
                }
            }
            return totalValue;
        }

        /// <summary>
        /// Получает наиболее прибыльные продукты
        /// </summary>
        public List<KeyValuePair<PaintProduct, decimal>> GetMostProfitableProducts()
        {
            return ProductPrices
                .OrderByDescending(x => x.Value)
                .ToList();
        }

        /// <summary>
        /// Получает стоимость конкретного продукта
        /// </summary>
        public decimal GetProductPrice(PaintProduct product)
        {
            return ProductPrices.ContainsKey(product) ? ProductPrices[product] : 0m;
        }

        #endregion

        #region Private Methods

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

        private void UpdateMaterialsStorage(Dictionary<object, int> availableResources)
        {
            MaterialsStorage.Clear();
            foreach (var resource in availableResources)
            {
                if (Enum.TryParse<PaintMaterial>(resource.Key.ToString(), out var material))
                {
                    MaterialsStorage[material] = resource.Value;
                }
            }
        }

        private void UpdateProductsStorage(Dictionary<object, int> producedOutputs)
        {
            foreach (var output in producedOutputs)
            {
                if (Enum.TryParse<PaintProduct>(output.Key.ToString(), out var product))
                {
                    int currentAmount = ProductsStorage.ContainsKey(product) ? ProductsStorage[product] : 0;
                    int availableSpace = MaxProductStorage - GetTotalProductStorage();
                    int amountToAdd = Math.Min(output.Value, availableSpace);

                    if (amountToAdd > 0)
                    {
                        ProductsStorage[product] = currentAmount + amountToAdd;
                    }
                }
            }
        }

        public override void OnBuildingPlaced()
        {
            FullProductionCycle();
        }

        #endregion
    }
}