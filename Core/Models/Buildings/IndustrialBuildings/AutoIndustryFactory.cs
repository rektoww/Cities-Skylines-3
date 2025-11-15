using Core.Enums;
using Core.Enums.Core.Enums;
using Core.Interfaces;
using Core.Models.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Models.Buildings.IndustrialBuildings
{
    public class AutoIndustryFactory : CommercialBuilding
    {
        #region Static Properties - Construction Cost

        public static decimal BuildCost { get; protected set; } = 1200000m; // Дороже, т.к. автомобильный завод

        public static Dictionary<ConstructionMaterial, int> RequiredMaterials { get; protected set; }
            = new Dictionary<ConstructionMaterial, int>
            {
                { ConstructionMaterial.Steel, 25 },
                { ConstructionMaterial.Concrete, 20 },
                { ConstructionMaterial.Glass, 10 },
                { ConstructionMaterial.Plastic, 15 }
            };

        #endregion

        #region Simplified Enums

        public enum AutoMaterial
        {
            Steel,
            Aluminum,
            Plastic,
            Electronics,
            Rubber,
            EngineParts,
            ChassisParts
        }

        public enum AutoProduct
        {
            Sedan,          // Седан
            SUV,            // Внедорожник
            Hatchback,      // Хэтчбек
            Coupe,          // Купе
            Minivan,        // Минивэн
            Truck,          // Грузовик
            ElectricCar,    // Электромобиль
            Motorcycle      // Мотоцикл
        }

        #endregion

        public Dictionary<AutoMaterial, int> MaterialsStorage { get; private set; } = new Dictionary<AutoMaterial, int>();
        public Dictionary<AutoProduct, int> ProductsStorage { get; private set; } = new Dictionary<AutoProduct, int>();
        public int MaxMaterialStorage { get; private set; }
        public int MaxProductStorage { get; private set; }
        public List<Workshop> Workshops { get; private set; } = new List<Workshop>();
        public int WorkersCount { get; private set; }
        public int MaxWorkers { get; private set; }

        // Эффективность зависит от количества рабочих
        public float ProductionEfficiency => WorkersCount > 0 ? 0.4f + WorkersCount / (float)MaxWorkers * 0.6f : 0f;

        public AutoIndustryFactory() : base(CommercialBuildingType.Factory)
        {
            MaxMaterialStorage = 5000;  // Большой склад материалов
            MaxProductStorage = 100;    // Мало готовых автомобилей (они крупные)
            MaxWorkers = 100;           // Много рабочих на автозаводе
            WorkersCount = 0;

            InitializeWorkshops();
            InitializeStartingMaterials();
        }

        private void InitializeWorkshops()
        {
            // Цех кузовов
            var bodyWorkshop = new Workshop
            {
                Name = "Кузовной цех",
                ProductionCycleTime = 15
            };
            bodyWorkshop.InputRequirements.Add("Steel", 40);
            bodyWorkshop.InputRequirements.Add("Aluminum", 20);
            bodyWorkshop.OutputProducts.Add("ChassisParts", 5);
            Workshops.Add(bodyWorkshop);

            // Двигательный цех
            var engineWorkshop = new Workshop
            {
                Name = "Двигательный цех",
                ProductionCycleTime = 20
            };
            engineWorkshop.InputRequirements.Add("Steel", 30);
            engineWorkshop.InputRequirements.Add("Aluminum", 15);
            engineWorkshop.InputRequirements.Add("Plastic", 10);
            engineWorkshop.OutputProducts.Add("EngineParts", 3);
            Workshops.Add(engineWorkshop);

            // Сборочный цех (легковые автомобили)
            var assemblyWorkshop = new Workshop
            {
                Name = "Сборочный цех легковых авто",
                ProductionCycleTime = 25
            };
            assemblyWorkshop.InputRequirements.Add("ChassisParts", 2);
            assemblyWorkshop.InputRequirements.Add("EngineParts", 1);
            assemblyWorkshop.InputRequirements.Add("Plastic", 15);
            assemblyWorkshop.InputRequirements.Add("Electronics", 8);
            assemblyWorkshop.InputRequirements.Add("Rubber", 6);
            assemblyWorkshop.OutputProducts.Add("Sedan", 1);
            assemblyWorkshop.OutputProducts.Add("Hatchback", 1);
            assemblyWorkshop.OutputProducts.Add("Coupe", 1);
            Workshops.Add(assemblyWorkshop);

            // Цех внедорожников и грузовиков
            var heavyAssemblyWorkshop = new Workshop
            {
                Name = "Цех тяжелых авто",
                ProductionCycleTime = 30
            };
            heavyAssemblyWorkshop.InputRequirements.Add("ChassisParts", 4);
            heavyAssemblyWorkshop.InputRequirements.Add("EngineParts", 2);
            heavyAssemblyWorkshop.InputRequirements.Add("Steel", 20);
            heavyAssemblyWorkshop.InputRequirements.Add("Plastic", 10);
            heavyAssemblyWorkshop.InputRequirements.Add("Rubber", 8);
            heavyAssemblyWorkshop.OutputProducts.Add("SUV", 1);
            heavyAssemblyWorkshop.OutputProducts.Add("Minivan", 1);
            heavyAssemblyWorkshop.OutputProducts.Add("Truck", 1);
            Workshops.Add(heavyAssemblyWorkshop);

            // Цех электромобилей
            var electricWorkshop = new Workshop
            {
                Name = "Цех электромобилей",
                ProductionCycleTime = 20
            };
            electricWorkshop.InputRequirements.Add("ChassisParts", 2);
            electricWorkshop.InputRequirements.Add("Electronics", 20);
            electricWorkshop.InputRequirements.Add("Plastic", 12);
            electricWorkshop.InputRequirements.Add("Aluminum", 10);
            electricWorkshop.OutputProducts.Add("ElectricCar", 1);
            Workshops.Add(electricWorkshop);

            // Цех мотоциклов
            var motorcycleWorkshop = new Workshop
            {
                Name = "Цех мотоциклов",
                ProductionCycleTime = 10
            };
            motorcycleWorkshop.InputRequirements.Add("Steel", 15);
            motorcycleWorkshop.InputRequirements.Add("EngineParts", 1);
            motorcycleWorkshop.InputRequirements.Add("Plastic", 8);
            motorcycleWorkshop.InputRequirements.Add("Rubber", 4);
            motorcycleWorkshop.OutputProducts.Add("Motorcycle", 2);
            Workshops.Add(motorcycleWorkshop);
        }

        private void InitializeStartingMaterials()
        {
            AddMaterial(AutoMaterial.Steel, 800);
            AddMaterial(AutoMaterial.Aluminum, 500);
            AddMaterial(AutoMaterial.Plastic, 400);
            AddMaterial(AutoMaterial.Electronics, 300);
            AddMaterial(AutoMaterial.Rubber, 200);
            AddMaterial(AutoMaterial.EngineParts, 50);
            AddMaterial(AutoMaterial.ChassisParts, 30);
        }

        public void SetWorkersCount(int count)
        {
            WorkersCount = Math.Min(count, MaxWorkers);
        }

        public bool AddMaterial(AutoMaterial material, int amount)
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
                if (Enum.TryParse<AutoMaterial>(resource.Key.ToString(), out var material))
                {
                    MaterialsStorage[material] = resource.Value;
                }
            }
        }

        private void UpdateProductsStorage(Dictionary<object, int> producedOutputs)
        {
            foreach (var output in producedOutputs)
            {
                if (Enum.TryParse<AutoProduct>(output.Key.ToString(), out var product))
                {
                    int currentAmount = ProductsStorage.ContainsKey(product) ? ProductsStorage[product] : 0;
                    int availableSpace = MaxProductStorage - GetTotalProductStorage();
                    int amountToAdd = Math.Min(output.Value, availableSpace);

                    if (amountToAdd > 0)
                    {
                        ProductsStorage[product] = currentAmount + amountToAdd;
                    }

                    if (amountToAdd < output.Value)
                    {
                        System.Diagnostics.Debug.WriteLine($"Превышена вместимость склада! Потеряно {output.Value - amountToAdd} автомобилей {product}");
                    }
                }
            }
        }

        public void FullProductionCycle()
        {
            ProcessWorkshops();
        }

        public Dictionary<AutoProduct, int> GetProductionOutput()
        {
            return new Dictionary<AutoProduct, int>(ProductsStorage);
        }

        public Dictionary<AutoMaterial, int> GetMaterialStorage()
        {
            return new Dictionary<AutoMaterial, int>(MaterialsStorage);
        }

        public bool ConsumeProduct(AutoProduct product, int amount)
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
    }
}