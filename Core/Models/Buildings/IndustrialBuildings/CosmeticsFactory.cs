using Core.Enums;
using Core.Interfaces;
using Core.Models.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Models.Buildings.IndustrialBuildings
{
    public class CosmeticsFactory : CommercialBuilding, IConstructable<CosmeticsFactory>
    {
        #region Static Properties - Construction Cost

        public static decimal BuildCost { get; protected set; } = 300000m;

        public static Dictionary<ConstructionMaterial, int> RequiredMaterials { get; protected set; }
            = new Dictionary<ConstructionMaterial, int>
            {
                { ConstructionMaterial.Steel, 5 },
                { ConstructionMaterial.Concrete, 8 },
                { ConstructionMaterial.Glass, 4 },
                { ConstructionMaterial.Plastic, 3 }
            };

        #endregion

        #region Simplified Enums

        public enum CosmeticMaterial
        {
            Alcohol,
            Plastic
        }

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

        public Dictionary<CosmeticMaterial, int> MaterialsStorage { get; private set; } = new Dictionary<CosmeticMaterial, int>();
        public Dictionary<CosmeticProduct, int> ProductsStorage { get; private set; } = new Dictionary<CosmeticProduct, int>();
        public int MaxMaterialStorage { get; private set; }
        public int MaxProductStorage { get; private set; }
        public List<Workshop> Workshops { get; private set; } = new List<Workshop>();
        public int WorkersCount { get; private set; }
        public int MaxWorkers { get; private set; }

        // Коэффициент эффективности в зависимости от количества рабочих
        public float ProductionEfficiency => WorkersCount > 0 ? 0.5f + WorkersCount / (float)MaxWorkers * 0.5f : 0f;

        public CosmeticsFactory() : base(CommercialBuildingType.Factory)
        {
            MaxMaterialStorage = 1000;
            MaxProductStorage = 800;
            MaxWorkers = 12;
            WorkersCount = 0;

            InitializeWorkshops();
            InitializeStartingMaterials();
        }

        private void InitializeWorkshops()
        {
            var creamWorkshop = new Workshop
            {
                Name = "Цех кремов и лосьонов",
                ProductionCycleTime = 6
            };
            creamWorkshop.InputRequirements.Add("Alcohol", 6);
            creamWorkshop.InputRequirements.Add("Plastic", 2);
            creamWorkshop.OutputProducts.Add("FaceCream", 5);
            creamWorkshop.OutputProducts.Add("BodyLotion", 4);
            Workshops.Add(creamWorkshop);

            var makeupWorkshop = new Workshop
            {
                Name = "Цех декоративной косметики",
                ProductionCycleTime = 8
            };
            makeupWorkshop.InputRequirements.Add("Alcohol", 10);
            makeupWorkshop.InputRequirements.Add("Plastic", 4);
            makeupWorkshop.OutputProducts.Add("Lipstick", 6);
            makeupWorkshop.OutputProducts.Add("EyeShadow", 8);
            Workshops.Add(makeupWorkshop);

            var perfumeWorkshop = new Workshop
            {
                Name = "Цех парфюмерии",
                ProductionCycleTime = 10
            };
            perfumeWorkshop.InputRequirements.Add("Alcohol", 12);
            perfumeWorkshop.InputRequirements.Add("Plastic", 3);
            perfumeWorkshop.OutputProducts.Add("Perfume", 3);
            perfumeWorkshop.OutputProducts.Add("EauDeToilette", 5);
            Workshops.Add(perfumeWorkshop);

            var hairCareWorkshop = new Workshop
            {
                Name = "Цех ухода за волосами",
                ProductionCycleTime = 7
            };
            hairCareWorkshop.InputRequirements.Add("Alcohol", 8);
            hairCareWorkshop.InputRequirements.Add("Plastic", 5);
            hairCareWorkshop.OutputProducts.Add("Shampoo", 8);
            hairCareWorkshop.OutputProducts.Add("Conditioner", 6);
            Workshops.Add(hairCareWorkshop);
        }

        private void InitializeStartingMaterials()
        {
            AddMaterial(CosmeticMaterial.Alcohol, 200);
            AddMaterial(CosmeticMaterial.Plastic, 150);
        }

        public void SetWorkersCount(int count)
        {
            WorkersCount = Math.Min(count, MaxWorkers);
        }

        public bool AddMaterial(CosmeticMaterial material, int amount)
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

            // Конвертируем enum в строки для совместимости с Workshop
            foreach (var material in MaterialsStorage)
            {
                availableResources.Add(material.Key.ToString(), material.Value);
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
                if (Enum.TryParse<CosmeticMaterial>(resource.Key.ToString(), out var material))
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
                if (Enum.TryParse<CosmeticProduct>(output.Key.ToString(), out var product))
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

        public void FullProductionCycle()
        {
            ProcessWorkshops();
        }

        public Dictionary<CosmeticProduct, int> GetProductionOutput()
        {
            return new Dictionary<CosmeticProduct, int>(ProductsStorage);
        }

        public Dictionary<CosmeticMaterial, int> GetMaterialStorage()
        {
            return new Dictionary<CosmeticMaterial, int>(MaterialsStorage);
        }

        public bool ConsumeProduct(CosmeticProduct product, int amount)
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
        /// Получает информацию о производственной эффективности
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