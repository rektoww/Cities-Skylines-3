using Core.Enums;
using Core.Enums.Core.Enums;
using Core.Interfaces;
using Core.Models.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Models.Buildings.IndustrialBuildings
{
    public class HouseholdAppliancesFactory : CommercialBuilding
    {
        #region Static Properties - Construction Cost

        public static decimal BuildCost { get; protected set; } = 400000m;

        public static Dictionary<ConstructionMaterial, int> RequiredMaterials { get; protected set; }
            = new Dictionary<ConstructionMaterial, int>
            {
                { ConstructionMaterial.Steel, 8 },
                { ConstructionMaterial.Concrete, 12 },
                { ConstructionMaterial.Glass, 6 },
                { ConstructionMaterial.Plastic, 10 }
            };

        #endregion

        #region Enums for Appliances Factory

        /// <summary>
        /// Типы комплектующих для сборки бытовой техники
        /// </summary>
        public enum ApplianceComponent
        {
            SteelSheets,        // Стальные листы (корпуса)
            PlasticParts,       // Пластиковые детали
            ElectricMotors,     // Электродвигатели
            Compressors,        // Компрессоры (для холодильников)
            ElectronicBoards,   // Электронные платы
            GlassPanels,        // Стеклянные панели
            CopperTubes,        // Медные трубки
            InsulationMaterials // Изоляционные материалы
        }

        /// <summary>
        /// Типы производимой бытовой техники
        /// </summary>
        public enum ApplianceProduct
        {
            // Холодильное оборудование
            Refrigerator,       // Холодильник
            Freezer,           // Морозильная камера

            // Стиральное оборудование
            WashingMachine,    // Стиральная машина
            Dryer,             // Сушильная машина

            // Кухонная техника
            Oven,              // Духовой шкаф
            Dishwasher,        // Посудомоечная машина
            Microwave,         // Микроволновая печь

            // Климатическая техника
            AirConditioner,    // Кондиционер
            WaterHeater,       // Водонагреватель

            // Мелкая бытовая техника
            VacuumCleaner      // Пылесос
        }

        /// <summary>
        /// Категории бытовой техники
        /// </summary>
        public enum ApplianceCategory
        {
            Refrigeration,     // Холодильное оборудование
            Laundry,          // Стиральное оборудование
            Kitchen,          // Кухонная техника
            Climate,          // Климатическая техника
            SmallAppliances   // Мелкая бытовая техника
        }

        #endregion

        #region Instance Properties

        public Dictionary<ApplianceComponent, int> ComponentsStorage { get; private set; } 
            = new Dictionary<ApplianceComponent, int>();
        public Dictionary<ApplianceProduct, int> ProductsStorage { get; private set; } 
            = new Dictionary<ApplianceProduct, int>();
        public int MaxComponentsStorage { get; private set; }
        public int MaxProductsStorage { get; private set; }
        public List<Workshop> AssemblyLines { get; private set; } = new List<Workshop>();
        public int WorkersCount { get; private set; }
        public int MaxWorkers { get; private set; }

        // Словарь для категорий техники
        public Dictionary<ApplianceProduct, ApplianceCategory> ProductCategories { get; private set; } 
            = new Dictionary<ApplianceProduct, ApplianceCategory>();

        // Стоимость продукции
        public Dictionary<ApplianceProduct, decimal> ProductPrices { get; private set; } 
            = new Dictionary<ApplianceProduct, decimal>();

        // Коэффициент эффективности сборки
        public float AssemblyEfficiency => WorkersCount > 0 ? 0.3f + (WorkersCount / (float)MaxWorkers) * 0.7f : 0f;

        // Уровень автоматизации (влияет на качество и скорость)
        public float AutomationLevel { get; private set; } = 0.5f;

        #endregion

        public HouseholdAppliancesFactory() : base(CommercialBuildingType.Factory)
        {
            MaxComponentsStorage = 2000;
            MaxProductsStorage = 500; // Техника занимает больше места
            MaxWorkers = 20;
            WorkersCount = 0;

            InitializeProductCategories();
            InitializeProductPrices();
            InitializeAssemblyLines();
            InitializeStartingComponents();
        }

        /// <summary>
        /// Инициализация категорий техники
        /// </summary>
        private void InitializeProductCategories()
        {
            ProductCategories[ApplianceProduct.Refrigerator] = ApplianceCategory.Refrigeration;
            ProductCategories[ApplianceProduct.Freezer] = ApplianceCategory.Refrigeration;

            ProductCategories[ApplianceProduct.WashingMachine] = ApplianceCategory.Laundry;
            ProductCategories[ApplianceProduct.Dryer] = ApplianceCategory.Laundry;

            ProductCategories[ApplianceProduct.Oven] = ApplianceCategory.Kitchen;
            ProductCategories[ApplianceProduct.Dishwasher] = ApplianceCategory.Kitchen;
            ProductCategories[ApplianceProduct.Microwave] = ApplianceCategory.Kitchen;

            ProductCategories[ApplianceProduct.AirConditioner] = ApplianceCategory.Climate;
            ProductCategories[ApplianceProduct.WaterHeater] = ApplianceCategory.Climate;

            ProductCategories[ApplianceProduct.VacuumCleaner] = ApplianceCategory.SmallAppliances;
        }

        /// <summary>
        /// Инициализация цен на продукцию
        /// </summary>
        private void InitializeProductPrices()
        {
            ProductPrices[ApplianceProduct.Refrigerator] = 25000m;
            ProductPrices[ApplianceProduct.Freezer] = 18000m;
            ProductPrices[ApplianceProduct.WashingMachine] = 20000m;
            ProductPrices[ApplianceProduct.Dryer] = 15000m;
            ProductPrices[ApplianceProduct.Oven] = 12000m;
            ProductPrices[ApplianceProduct.Dishwasher] = 18000m;
            ProductPrices[ApplianceProduct.Microwave] = 5000m;
            ProductPrices[ApplianceProduct.AirConditioner] = 22000m;
            ProductPrices[ApplianceProduct.WaterHeater] = 8000m;
            ProductPrices[ApplianceProduct.VacuumCleaner] = 6000m;
        }

        /// <summary>
        /// Инициализация сборочных линий
        /// </summary>
        private void InitializeAssemblyLines()
        {
            // Линия сборки холодильного оборудования
            var refrigerationLine = new Workshop
            {
                Name = "Линия сборки холодильного оборудования",
                ProductionCycleTime = 12
            };
            refrigerationLine.InputRequirements.Add(ApplianceComponent.SteelSheets.ToString(), 15);
            refrigerationLine.InputRequirements.Add(ApplianceComponent.PlasticParts.ToString(), 10);
            refrigerationLine.InputRequirements.Add(ApplianceComponent.Compressors.ToString(), 2);
            refrigerationLine.InputRequirements.Add(ApplianceComponent.InsulationMaterials.ToString(), 8);
            refrigerationLine.InputRequirements.Add(ApplianceComponent.ElectronicBoards.ToString(), 3);
            refrigerationLine.OutputProducts.Add(ApplianceProduct.Refrigerator.ToString(), 1);
            refrigerationLine.OutputProducts.Add(ApplianceProduct.Freezer.ToString(), 1);
            AssemblyLines.Add(refrigerationLine);

            // Линия сборки стирального оборудования
            var laundryLine = new Workshop
            {
                Name = "Линия сборки стирального оборудования",
                ProductionCycleTime = 10
            };
            laundryLine.InputRequirements.Add(ApplianceComponent.SteelSheets.ToString(), 12);
            laundryLine.InputRequirements.Add(ApplianceComponent.PlasticParts.ToString(), 8);
            laundryLine.InputRequirements.Add(ApplianceComponent.ElectricMotors.ToString(), 2);
            laundryLine.InputRequirements.Add(ApplianceComponent.ElectronicBoards.ToString(), 4);
            laundryLine.InputRequirements.Add(ApplianceComponent.GlassPanels.ToString(), 1);
            laundryLine.OutputProducts.Add(ApplianceProduct.WashingMachine.ToString(), 1);
            laundryLine.OutputProducts.Add(ApplianceProduct.Dryer.ToString(), 1);
            AssemblyLines.Add(laundryLine);

            // Линия сборки кухонной техники
            var kitchenLine = new Workshop
            {
                Name = "Линия сборки кухонной техники",
                ProductionCycleTime = 8
            };
            kitchenLine.InputRequirements.Add(ApplianceComponent.SteelSheets.ToString(), 8);
            kitchenLine.InputRequirements.Add(ApplianceComponent.PlasticParts.ToString(), 6);
            kitchenLine.InputRequirements.Add(ApplianceComponent.ElectronicBoards.ToString(), 3);
            kitchenLine.InputRequirements.Add(ApplianceComponent.GlassPanels.ToString(), 2);
            kitchenLine.InputRequirements.Add(ApplianceComponent.CopperTubes.ToString(), 4);
            kitchenLine.OutputProducts.Add(ApplianceProduct.Oven.ToString(), 1);
            kitchenLine.OutputProducts.Add(ApplianceProduct.Dishwasher.ToString(), 1);
            kitchenLine.OutputProducts.Add(ApplianceProduct.Microwave.ToString(), 1);
            AssemblyLines.Add(kitchenLine);

            // Линия сборки климатической техники
            var climateLine = new Workshop
            {
                Name = "Линия сборки климатической техники",
                ProductionCycleTime = 14
            };
            climateLine.InputRequirements.Add(ApplianceComponent.SteelSheets.ToString(), 10);
            climateLine.InputRequirements.Add(ApplianceComponent.PlasticParts.ToString(), 8);
            climateLine.InputRequirements.Add(ApplianceComponent.Compressors.ToString(), 1);
            climateLine.InputRequirements.Add(ApplianceComponent.CopperTubes.ToString(), 6);
            climateLine.InputRequirements.Add(ApplianceComponent.ElectronicBoards.ToString(), 3);
            climateLine.OutputProducts.Add(ApplianceProduct.AirConditioner.ToString(), 1);
            climateLine.OutputProducts.Add(ApplianceProduct.WaterHeater.ToString(), 1);
            AssemblyLines.Add(climateLine);

            // Линия сборки мелкой бытовой техники
            var smallAppliancesLine = new Workshop
            {
                Name = "Линия сборки мелкой бытовой техники",
                ProductionCycleTime = 6
            };
            smallAppliancesLine.InputRequirements.Add(ApplianceComponent.PlasticParts.ToString(), 5);
            smallAppliancesLine.InputRequirements.Add(ApplianceComponent.ElectricMotors.ToString(), 1);
            smallAppliancesLine.InputRequirements.Add(ApplianceComponent.ElectronicBoards.ToString(), 2);
            smallAppliancesLine.InputRequirements.Add(ApplianceComponent.SteelSheets.ToString(), 3);
            smallAppliancesLine.OutputProducts.Add(ApplianceProduct.VacuumCleaner.ToString(), 3);
            AssemblyLines.Add(smallAppliancesLine);
        }

        /// <summary>
        /// Инициализация стартовых комплектующих
        /// </summary>
        private void InitializeStartingComponents()
        {
            AddComponent(ApplianceComponent.SteelSheets, 300);
            AddComponent(ApplianceComponent.PlasticParts, 250);
            AddComponent(ApplianceComponent.ElectricMotors, 50);
            AddComponent(ApplianceComponent.Compressors, 30);
            AddComponent(ApplianceComponent.ElectronicBoards, 80);
            AddComponent(ApplianceComponent.GlassPanels, 60);
            AddComponent(ApplianceComponent.CopperTubes, 40);
            AddComponent(ApplianceComponent.InsulationMaterials, 70);
        }

        #region Public Methods

        public void SetWorkersCount(int count)
        {
            WorkersCount = Math.Min(count, MaxWorkers);
        }

        /// <summary>
        /// Устанавливает уровень автоматизации (0.0 - 1.0)
        /// </summary>
        public void SetAutomationLevel(float level)
        {
            AutomationLevel = Math.Max(0f, Math.Min(1f, level));
        }

        public bool AddComponent(ApplianceComponent component, int amount)
        {
            int currentAmount = ComponentsStorage.ContainsKey(component) ? ComponentsStorage[component] : 0;
            if (GetTotalComponentsStorage() + amount > MaxComponentsStorage)
                return false;

            ComponentsStorage[component] = currentAmount + amount;
            return true;
        }

        public int GetTotalComponentsStorage()
        {
            return ComponentsStorage.Values.Sum();
        }

        /// <summary>
        /// Запуск сборочных линий
        /// </summary>
        public void RunAssemblyLines()
        {
            if (WorkersCount == 0 || AssemblyEfficiency <= 0) return;

            var availableComponents = new Dictionary<object, int>();
            foreach (var component in ComponentsStorage)
            {
                availableComponents.Add(component.Key.ToString(), component.Value);
            }

            var assembledProducts = new Dictionary<object, int>();

            foreach (var assemblyLine in AssemblyLines)
            {
                var lineComponents = new Dictionary<object, int>(availableComponents);
                var lineOutputs = new Dictionary<object, int>();

                if (assemblyLine.Process(lineComponents, lineOutputs))
                {
                    // Учитываем эффективность сборки и автоматизацию
                    ApplyAssemblyEfficiency(lineOutputs);
                    availableComponents = lineComponents;

                    foreach (var output in lineOutputs)
                    {
                        if (assembledProducts.ContainsKey(output.Key))
                            assembledProducts[output.Key] += output.Value;
                        else
                            assembledProducts[output.Key] = output.Value;
                    }
                }
            }

            UpdateComponentsStorage(availableComponents);
            UpdateProductsStorage(assembledProducts);
        }

        public void FullProductionCycle()
        {
            RunAssemblyLines();
        }

        public Dictionary<ApplianceProduct, int> GetProductionOutput()
        {
            return new Dictionary<ApplianceProduct, int>(ProductsStorage);
        }

        public Dictionary<ApplianceComponent, int> GetComponentsStorage()
        {
            return new Dictionary<ApplianceComponent, int>(ComponentsStorage);
        }

        public bool SellProduct(ApplianceProduct product, int quantity)
        {
            if (!ProductsStorage.ContainsKey(product) || ProductsStorage[product] < quantity)
                return false;

            ProductsStorage[product] -= quantity;
            if (ProductsStorage[product] == 0)
                ProductsStorage.Remove(product);

            return true;
        }

        public int GetTotalProductsStorage()
        {
            return ProductsStorage.Values.Sum();
        }

        /// <summary>
        /// Получает продукты по категории
        /// </summary>
        public Dictionary<ApplianceProduct, int> GetProductsByCategory(ApplianceCategory category)
        {
            return ProductsStorage
                .Where(kvp => ProductCategories.ContainsKey(kvp.Key) && ProductCategories[kvp.Key] == category)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
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
        /// Получает статистику по категориям
        /// </summary>
        public Dictionary<ApplianceCategory, int> GetCategoryStatistics()
        {
            var statistics = new Dictionary<ApplianceCategory, int>();

            foreach (var category in Enum.GetValues(typeof(ApplianceCategory)).Cast<ApplianceCategory>())
            {
                int totalProducts = GetProductsByCategory(category).Values.Sum();
                statistics[category] = totalProducts;
            }

            return statistics;
        }

        /// <summary>
        /// Получает информацию о производстве
        /// </summary>
        public Dictionary<string, object> GetProductionInfo()
        {
            return new Dictionary<string, object>
            {
                { "WorkersCount", WorkersCount },
                { "MaxWorkers", MaxWorkers },
                { "AssemblyEfficiency", AssemblyEfficiency },
                { "AutomationLevel", AutomationLevel },
                { "TotalComponentsStorage", GetTotalComponentsStorage() },
                { "MaxComponentsStorage", MaxComponentsStorage },
                { "TotalProductsStorage", GetTotalProductsStorage() },
                { "MaxProductsStorage", MaxProductsStorage },
                { "ActiveAssemblyLines", AssemblyLines.Count },
                { "InventoryValue", CalculateInventoryValue() },
                { "CategoryStatistics", GetCategoryStatistics() }
            };
        }

        /// <summary>
        /// Получает наиболее прибыльные продукты
        /// </summary>
        public List<KeyValuePair<ApplianceProduct, decimal>> GetMostProfitableProducts()
        {
            return ProductPrices
                .OrderByDescending(x => x.Value)
                .ToList();
        }

        #endregion

        #region Private Methods

        private void ApplyAssemblyEfficiency(Dictionary<object, int> outputs)
        {
            if (AssemblyEfficiency >= 1f && AutomationLevel >= 1f) return;

            float totalEfficiency = AssemblyEfficiency * (0.7f + AutomationLevel * 0.3f);

            var keys = outputs.Keys.ToList();
            foreach (var key in keys)
            {
                outputs[key] = (int)(outputs[key] * totalEfficiency);
                if (outputs[key] <= 0)
                    outputs.Remove(key);
            }
        }

        private void UpdateComponentsStorage(Dictionary<object, int> availableComponents)
        {
            ComponentsStorage.Clear();
            foreach (var component in availableComponents)
            {
                if (Enum.TryParse<ApplianceComponent>(component.Key.ToString(), out var comp))
                {
                    ComponentsStorage[comp] = component.Value;
                }
            }
        }

        private void UpdateProductsStorage(Dictionary<object, int> assembledProducts)
        {
            foreach (var output in assembledProducts)
            {
                if (Enum.TryParse<ApplianceProduct>(output.Key.ToString(), out var product))
                {
                    int currentAmount = ProductsStorage.ContainsKey(product) ? ProductsStorage[product] : 0;
                    int availableSpace = MaxProductsStorage - GetTotalProductsStorage();
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
            // При размещении запускаем тестовую сборку
            FullProductionCycle();
        }

        #endregion
    }
}