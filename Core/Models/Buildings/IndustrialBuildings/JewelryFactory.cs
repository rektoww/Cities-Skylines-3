using Core.Models.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Models.Buildings.IndustrialBuildings
{
    /// <summary>
    /// Завод ювелирных изделий: склад сырья, производственные цеха и склад готовой продукции
    /// Простой и понятный по структуре (по аналогии с PackagingFactory)
    /// </summary>
    public class JewelryFactory
    {
        
        /// <summary>
        /// Сырьё для ювелирного производства
        /// </summary>
        public enum JewelryMaterial
        {
            Gold,       // Золото
            Silver,     // Серебро
            Platinum,   // Платина
            Palladium,  // Палладий
            Copper,     // Медь (сплавы)
            Steel,      // Сталь (корпуса, фурнитура)
            Diamond,    // Алмаз
            Gemstone,   // Драгоценный/полудрагоценный камень
            Enamel,     // Эмаль
            Leather     // Кожа (ремешки)
        }

        /// <summary>
        /// Готовая ювелирная продукция (10 видов)
        /// </summary>
        public enum JewelryProduct
        {
            GoldRing,           // Золотое кольцо
            PlatinumRing,       // Платиновое кольцо
            SilverNecklace,     // Серебряное ожерелье
            DiamondEarrings,    // Серьги с алмазами
            GoldBracelet,       // Золотой браслет
            Pendant,            // Подвеска
            Brooch,             // Брошь
            GoldChain,          // Золотая цепь
            WatchCase,          // Корпус часов
            JewelryBox          // Украшение в подарочной коробке
        }
        
        
        public Dictionary<JewelryMaterial, int> MaterialStorage { get; private set; } = new();
        public Dictionary<JewelryProduct, int> ProductStorage { get; private set; } = new();

        public int MaxMaterialStorage { get; private set; } = 1200;
        public int MaxProductStorage  { get; private set; } = 900;

        public List<Workshop> Workshops { get; private set; } = new();

        public int WorkersCount { get; private set; }
        public int MaxWorkers   { get; private set; } = 18;

        public float ProductionEfficiency => WorkersCount > 0 ? 0.4f + (WorkersCount / (float)MaxWorkers) * 0.6f : 0f;
        

        
        public JewelryFactory()
        {
            WorkersCount = 0;
            InitializeWorkshops();
            InitializeStartingMaterials();
        }
       
        private void InitializeWorkshops()
        {
            // Цех колец (золото/платина + камни)
            var ringWorkshop = new Workshop
            {
                Name = "Цех колец",
                ProductionCycleTime = 6
            };
            ringWorkshop.InputRequirements.Add(JewelryMaterial.Gold, 6);
            ringWorkshop.InputRequirements.Add(JewelryMaterial.Platinum, 4);
            ringWorkshop.InputRequirements.Add(JewelryMaterial.Gemstone, 4);
            ringWorkshop.OutputProducts.Add(JewelryProduct.GoldRing, 6);
            ringWorkshop.OutputProducts.Add(JewelryProduct.PlatinumRing, 3);
            Workshops.Add(ringWorkshop);

            // Цех ожерелий и цепей (серебро/золото)
            var necklaceWorkshop = new Workshop
            {
                Name = "Цех ожерелий и цепей",
                ProductionCycleTime = 7
            };
            necklaceWorkshop.InputRequirements.Add(JewelryMaterial.Silver, 10);
            necklaceWorkshop.InputRequirements.Add(JewelryMaterial.Gold, 8);
            necklaceWorkshop.OutputProducts.Add(JewelryProduct.SilverNecklace, 7);
            necklaceWorkshop.OutputProducts.Add(JewelryProduct.GoldChain, 6);
            Workshops.Add(necklaceWorkshop);

            // Цех серёг и брошей (золото + алмазы/эмаль)
            var earringsWorkshop = new Workshop
            {
                Name = "Цех серёг и брошей",
                ProductionCycleTime = 8
            };
            earringsWorkshop.InputRequirements.Add(JewelryMaterial.Gold, 8);
            earringsWorkshop.InputRequirements.Add(JewelryMaterial.Diamond, 4);
            earringsWorkshop.InputRequirements.Add(JewelryMaterial.Enamel, 2);
            earringsWorkshop.OutputProducts.Add(JewelryProduct.DiamondEarrings, 4);
            earringsWorkshop.OutputProducts.Add(JewelryProduct.Brooch, 5);
            Workshops.Add(earringsWorkshop);

            // Цех браслетов и подвесок (золото/кожа/эмаль/камни)
            var braceletWorkshop = new Workshop
            {
                Name = "Цех браслетов и подвесок",
                ProductionCycleTime = 7
            };
            braceletWorkshop.InputRequirements.Add(JewelryMaterial.Gold, 10);
            braceletWorkshop.InputRequirements.Add(JewelryMaterial.Leather, 6);
            braceletWorkshop.InputRequirements.Add(JewelryMaterial.Gemstone, 3);
            braceletWorkshop.InputRequirements.Add(JewelryMaterial.Enamel, 2);
            braceletWorkshop.OutputProducts.Add(JewelryProduct.GoldBracelet, 6);
            braceletWorkshop.OutputProducts.Add(JewelryProduct.Pendant, 6);
            Workshops.Add(braceletWorkshop);

            // Цех корпусов часов и подарочной упаковки (сталь/золото/кожа)
            var watchWorkshop = new Workshop
            {
                Name = "Цех корпусов часов и упаковки",
                ProductionCycleTime = 6
            };
            watchWorkshop.InputRequirements.Add(JewelryMaterial.Steel, 10);
            watchWorkshop.InputRequirements.Add(JewelryMaterial.Gold, 4);
            watchWorkshop.InputRequirements.Add(JewelryMaterial.Leather, 4);
            watchWorkshop.OutputProducts.Add(JewelryProduct.WatchCase, 6);
            watchWorkshop.OutputProducts.Add(JewelryProduct.JewelryBox, 6);
            Workshops.Add(watchWorkshop);
        }

        private void InitializeStartingMaterials()
        {
            MaterialStorage.Clear();
            AddMaterial(JewelryMaterial.Gold, 180);
            AddMaterial(JewelryMaterial.Silver, 220);
            AddMaterial(JewelryMaterial.Platinum, 90);
            AddMaterial(JewelryMaterial.Palladium, 60);
            AddMaterial(JewelryMaterial.Copper, 140);
            AddMaterial(JewelryMaterial.Steel, 160);
            AddMaterial(JewelryMaterial.Diamond, 50);
            AddMaterial(JewelryMaterial.Gemstone, 120);
            AddMaterial(JewelryMaterial.Enamel, 80);
            AddMaterial(JewelryMaterial.Leather, 70);
        }
        
        public void SetWorkersCount(int count)
        {
            WorkersCount = Math.Clamp(count, 0, MaxWorkers);
        }

        public bool AddMaterial(JewelryMaterial material, int amount)
        {
            if (amount <= 0) return false;
            int currentAmount = MaterialStorage.ContainsKey(material) ? MaterialStorage[material] : 0;
            if (GetTotalMaterialStorage() + amount > MaxMaterialStorage) return false;
            MaterialStorage[material] = currentAmount + amount;
            return true;
        }

        public void ProcessWorkshops()
        {
            if (WorkersCount == 0 || ProductionEfficiency <= 0) return;

            var availableResources = new Dictionary<object, int>();
            foreach (var material in MaterialStorage)
                availableResources.Add(material.Key, material.Value);

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

        public Dictionary<JewelryProduct, int> GetProductionOutput()
        {
            return new Dictionary<JewelryProduct, int>(ProductStorage);
        }

        public Dictionary<JewelryMaterial, int> GetMaterialStorage()
        {
            return new Dictionary<JewelryMaterial, int>(MaterialStorage);
        }

        public bool ConsumeProduct(JewelryProduct product, int amount)
        {
            if (amount <= 0) return true;
            if (!ProductStorage.ContainsKey(product) || ProductStorage[product] < amount) return false;
            ProductStorage[product] -= amount;
            if (ProductStorage[product] == 0) ProductStorage.Remove(product);
            return true;
        }

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
                { "ProductTypes", ProductStorage.Count },
            };
        }
      
        private int GetTotalMaterialStorage() => MaterialStorage.Values.Sum();
        private int GetTotalProductStorage()  => ProductStorage.Values.Sum();

        private void ApplyProductionEfficiency(Dictionary<object, int> outputs)
        {
            if (ProductionEfficiency >= 1f) return;
            var keys = outputs.Keys.ToList();
            foreach (var key in keys)
            {
                outputs[key] = (int)(outputs[key] * ProductionEfficiency);
                if (outputs[key] <= 0) outputs.Remove(key);
            }
        }

        private void UpdateMaterialsStorage(Dictionary<object, int> availableResources)
        {
            MaterialStorage.Clear();
            foreach (var resource in availableResources)
            {
                if (resource.Key is JewelryMaterial m)
                    MaterialStorage[m] = resource.Value;
            }
        }

        private void UpdateProductsStorage(Dictionary<object, int> producedOutputs)
        {
            foreach (var output in producedOutputs)
            {
                if (output.Key is JewelryProduct p)
                {
                    int current = ProductStorage.ContainsKey(p) ? ProductStorage[p] : 0;
                    int space = MaxProductStorage - GetTotalProductStorage();
                    int toAdd = Math.Min(output.Value, space);
                    if (toAdd > 0) ProductStorage[p] = current + toAdd;
                    if (toAdd < output.Value)
                    {
                        System.Diagnostics.Debug.WriteLine($"Превышена вместимость склада! Потеряно {output.Value - toAdd} единиц продукции {p}");
                    }
                }
            }
        }

    }
}
