using Core.Enums;
using Core.Interfaces;
using Core.Models.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Models.Buildings.IndustrialBuildings
{
    public class EnergyFactory : CommercialBuilding, IConstructable<EnergyFactory>
    {
        #region Static Properties - Construction Cost

        public static decimal BuildCost { get; protected set; } = 400000m;

        public static Dictionary<ConstructionMaterial, int> RequiredMaterials { get; protected set; }
            = new Dictionary<ConstructionMaterial, int>
            {
                { ConstructionMaterial.Steel, 6 },
                { ConstructionMaterial.Concrete, 7 },
                { ConstructionMaterial.Glass, 3 },
                { ConstructionMaterial.Plastic, 2 }
            };

        #endregion

        
        public Dictionary<EnergySource, int> MaterialStorage { get; private set; } = new Dictionary<EnergySource, int>();
        public Dictionary<EnergyType, int> ProductStorage { get; private set; } = new Dictionary<EnergyType, int>();
        public int MaxMaterialStorage { get; private set; }
        public int MaxProductStorage { get; private set; }
        public List<Workshop> Workshops { get; private set; } = new List<Workshop>();
        public int WorkersCount { get; private set; }
        public int MaxWorkers { get; private set; }

        // Коэффициент эффективности в зависимости от количества рабочих
        public float ProductionEfficiency => WorkersCount > 0 ? 0.5f + WorkersCount / (float)MaxWorkers * 0.5f : 0f;

        public EnergyFactory() : base(CommercialBuildingType.Factory)
        {
            MaxMaterialStorage = 1000;
            MaxProductStorage = 900;
            MaxWorkers = 120;
            WorkersCount = 0;

            InitializeWorkshops();
            InitializeStartingMaterials();
        }

        private void InitializeWorkshops()
        {
            var electricityWorkshop = new Workshop
            {
                Name = "Цех производства электроэнергии",
                ProductionCycleTime = 3
            };
            electricityWorkshop.InputRequirements.Add("Gas", 6);
            electricityWorkshop.InputRequirements.Add("Coal", 20);
            electricityWorkshop.OutputProducts.Add("Electricity", 10);
            Workshops.Add(electricityWorkshop);            
            
            var hotwaterWorkshop = new Workshop
            {
                Name = "Цех производства горячей воды",
                ProductionCycleTime = 5
            };
            hotwaterWorkshop.InputRequirements.Add("Gas", 10);
            hotwaterWorkshop.OutputProducts.Add("ThermalEnergy", 20);
            Workshops.Add(hotwaterWorkshop);
        }

        private void InitializeStartingMaterials()
        {            
            AddMaterial(EnergySource.Coal, 200);
            AddMaterial(EnergySource.Gas, 250);
        }

        public void SetWorkersCount(int count)
        {
            WorkersCount = Math.Min(count, MaxWorkers);
        }

        public bool AddMaterial(EnergySource source, int amount)
        {
            int currentAmount = MaterialStorage.ContainsKey(source) ? MaterialStorage[source] : 0;
            if (GetTotalMaterialStorage() + amount > MaxMaterialStorage)
                return false;

            MaterialStorage[source] = currentAmount + amount;
            return true;
        }

        public int GetTotalMaterialStorage()
        {
            return MaterialStorage.Values.Sum();
        }

        public void ProcessWorkshops()
        {
            if (WorkersCount == 0 || ProductionEfficiency <= 0) return;

            var availableResources = new Dictionary<object, int>();

            // Конвертируем enum в строки для совместимости с Workshop
            foreach (var source in MaterialStorage)
            {
                availableResources.Add(source.Key.ToString(), source.Value);
            }

            var producedEnergy = new Dictionary<object, int>();

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
                        if (producedEnergy.ContainsKey(output.Key))
                            producedEnergy[output.Key] += output.Value;
                        else
                            producedEnergy[output.Key] = output.Value;
                    }
                }
            }

            // Обновляем хранилище материалов
            UpdateMaterialStorage(availableResources);

            // Обновляем хранилище продукции с проверкой вместимости
            UpdateProductStorage(producedEnergy);
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
        private void UpdateMaterialStorage(Dictionary<object, int> availableResources)
        {
            MaterialStorage.Clear();
            foreach (var resource in availableResources)
            {
                if (Enum.TryParse<EnergySource>(resource.Key.ToString(), out var source))
                {
                    MaterialStorage[source] = resource.Value;
                }
            }
        }

        /// <summary>
        /// Обновляет хранилище продукции с проверкой вместимости
        /// </summary>
        private void UpdateProductStorage(Dictionary<object, int> producedOutputs)
        {
            foreach (var output in producedOutputs)
            {
                if (Enum.TryParse<EnergyType>(output.Key.ToString(), out var product))
                {
                    int currentAmount = ProductStorage.ContainsKey(product) ? ProductStorage[product] : 0;
                    int availableSpace = MaxProductStorage - GetTotalProductStorage();
                    int amountToAdd = Math.Min(output.Value, availableSpace);

                    if (amountToAdd > 0)
                    {
                        ProductStorage[product] = currentAmount + amountToAdd;
                    }

                    // Логируем потерю выработанной энергии при превыении над потреблением
                    if (amountToAdd < output.Value)
                    {
                        System.Diagnostics.Debug.WriteLine($"Превышена выработка энергии! Потерян след. объем {output.Value - amountToAdd} энергии типа {product}");
                    }
                }
            }
        }

        public void FullProductionCycle()
        {
            ProcessWorkshops();
        }

        public Dictionary<EnergyType, int> GetConsumerOutput()
        {
            return new Dictionary<EnergyType, int>(ProductStorage);
        }

        public Dictionary<EnergySource, int> GetMaterialStorage()
        {
            return new Dictionary<EnergySource, int>(MaterialStorage);
        }

        public bool ConsumeProduct(EnergyType product, int amount)
        {
            if (!ProductStorage.ContainsKey(product) || ProductStorage[product] < amount)
                return false;

            ProductStorage[product] -= amount;
            if (ProductStorage[product] == 0)
                ProductStorage.Remove(product);

            return true;
        }

        public int GetTotalProductStorage()
        {
            return ProductStorage.Values.Sum();
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