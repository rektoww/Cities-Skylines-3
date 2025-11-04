using Core.Models.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Models.Buildings.IndustrialBuildings
{
    /// <summary>
    /// Стекольное производство: склад сырья, цеха и склад готовых изделий
    /// Простая реализация по образцу PackagingFactory
    /// </summary>
    public class GlassFactory
    {
        
        /// <summary>
        /// Сырьё для стекольной промышленности
        /// </summary>
        public enum GlassMaterial
        {
            Sand,        // Кварцевый песок
            SodaAsh,     // Карбонат натрия
            Limestone,   // Известняк
            RecycledGlass, // Бой стекла
            Colorant,    // Краситель
            Tin,         // Олово (флоат-линия)
            Gas,         // Топливо/энергия
            Clay,        // Глина (формы, вспомогательное)
            Silica,      // Диоксид кремния (добавка)
            Resin        // Смолы/связующие (ламинат/стеклопластик)
        }

        /// <summary>
        /// Готовая стеклянная продукция (10 видов)
        /// </summary>
        public enum GlassProduct
        {
            WindowGlass,     // Оконное стекло
            GlassBottle,     // Стеклянная бутылка
            GlassJar,        // Стеклянная банка
            GlassSheet,      // Листовое стекло
            TemperedGlass,   // Закалённое стекло
            LaminatedGlass,  // Триплекс
            GlassTube,       // Стеклянная трубка
            Fiberglass,      // Стекловолокно
            Mirror,          // Зеркало
            GlassBrick       // Стеклоблок
        }
      
        public Dictionary<GlassMaterial, int> MaterialStorage { get; private set; } = new();
        public Dictionary<GlassProduct, int>  ProductStorage  { get; private set; } = new();

        public int MaxMaterialStorage { get; private set; } = 1800;
        public int MaxProductStorage  { get; private set; } = 1400;

        public List<Workshop> Workshops { get; private set; } = new();

        public int WorkersCount { get; private set; }
        public int MaxWorkers   { get; private set; } = 20;

        public float ProductionEfficiency => WorkersCount > 0 ? 0.35f + (WorkersCount / (float)MaxWorkers) * 0.65f : 0f;
      

       
        public GlassFactory()
        {
            WorkersCount = 0;
            InitializeWorkshops();
            InitializeStartingMaterials();
        }
       

     
        private void InitializeWorkshops()
        {
            // Плавильная печь (базовая смесь) -> лист/оконное стекло
            var meltingWorkshop = new Workshop
            {
                Name = "Плавильная печь",
                ProductionCycleTime = 6
            };
            meltingWorkshop.InputRequirements.Add(GlassMaterial.Sand, 14);
            meltingWorkshop.InputRequirements.Add(GlassMaterial.SodaAsh, 6);
            meltingWorkshop.InputRequirements.Add(GlassMaterial.Limestone, 4);
            meltingWorkshop.OutputProducts.Add(GlassProduct.GlassSheet, 8);
            meltingWorkshop.OutputProducts.Add(GlassProduct.WindowGlass, 6);
            Workshops.Add(meltingWorkshop);

            // Линия бутылок и банок (с добавлением боя и красителя)
            var containerWorkshop = new Workshop
            {
                Name = "Линия тарного стекла",
                ProductionCycleTime = 7
            };
            containerWorkshop.InputRequirements.Add(GlassMaterial.RecycledGlass, 12);
            containerWorkshop.InputRequirements.Add(GlassMaterial.Colorant, 2);
            containerWorkshop.InputRequirements.Add(GlassMaterial.Gas, 4);
            containerWorkshop.OutputProducts.Add(GlassProduct.GlassBottle, 10);
            containerWorkshop.OutputProducts.Add(GlassProduct.GlassJar, 8);
            Workshops.Add(containerWorkshop);

            // Флоат-линия (лист -> зеркала и оконное стекло высокого качества)
            var floatWorkshop = new Workshop
            {
                Name = "Флоат-линия",
                ProductionCycleTime = 8
            };
            floatWorkshop.InputRequirements.Add(GlassMaterial.Tin, 2);
            floatWorkshop.InputRequirements.Add(GlassMaterial.Silica, 4);
            floatWorkshop.OutputProducts.Add(GlassProduct.WindowGlass, 10);
            floatWorkshop.OutputProducts.Add(GlassProduct.Mirror, 6);
            Workshops.Add(floatWorkshop);

            // Закалка и ламинирование
            var temperLaminateWorkshop = new Workshop
            {
                Name = "Закалка и ламинирование",
                ProductionCycleTime = 9
            };
            temperLaminateWorkshop.InputRequirements.Add(GlassMaterial.Gas, 6);
            temperLaminateWorkshop.InputRequirements.Add(GlassMaterial.Resin, 4);
            temperLaminateWorkshop.OutputProducts.Add(GlassProduct.TemperedGlass, 8);
            temperLaminateWorkshop.OutputProducts.Add(GlassProduct.LaminatedGlass, 6);
            Workshops.Add(temperLaminateWorkshop);

            // Волокно и трубки
            var fiberTubeWorkshop = new Workshop
            {
                Name = "Вытяжка волокна и трубок",
                ProductionCycleTime = 7
            };
            fiberTubeWorkshop.InputRequirements.Add(GlassMaterial.RecycledGlass, 10);
            fiberTubeWorkshop.InputRequirements.Add(GlassMaterial.Clay, 2);
            fiberTubeWorkshop.OutputProducts.Add(GlassProduct.Fiberglass, 12);
            fiberTubeWorkshop.OutputProducts.Add(GlassProduct.GlassTube, 8);
            fiberTubeWorkshop.OutputProducts.Add(GlassProduct.GlassBrick, 4);
            Workshops.Add(fiberTubeWorkshop);
        }

        private void InitializeStartingMaterials()
        {
            MaterialStorage.Clear();
            AddMaterial(GlassMaterial.Sand, 600);
            AddMaterial(GlassMaterial.SodaAsh, 300);
            AddMaterial(GlassMaterial.Limestone, 240);
            AddMaterial(GlassMaterial.RecycledGlass, 400);
            AddMaterial(GlassMaterial.Colorant, 120);
            AddMaterial(GlassMaterial.Tin, 60);
            AddMaterial(GlassMaterial.Gas, 200);
            AddMaterial(GlassMaterial.Clay, 140);
            AddMaterial(GlassMaterial.Silica, 180);
            AddMaterial(GlassMaterial.Resin, 100);
        }
       

       
        public void SetWorkersCount(int count)
        {
            WorkersCount = Math.Clamp(count, 0, MaxWorkers);
        }

        public bool AddMaterial(GlassMaterial material, int amount)
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

        public Dictionary<GlassProduct, int> GetProductionOutput()
        {
            return new Dictionary<GlassProduct, int>(ProductStorage);
        }

        public Dictionary<GlassMaterial, int> GetMaterialStorage()
        {
            return new Dictionary<GlassMaterial, int>(MaterialStorage);
        }

        public bool ConsumeProduct(GlassProduct product, int amount)
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
                if (resource.Key is GlassMaterial m)
                    MaterialStorage[m] = resource.Value;
            }
        }

        private void UpdateProductsStorage(Dictionary<object, int> producedOutputs)
        {
            foreach (var output in producedOutputs)
            {
                if (output.Key is GlassProduct p)
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
