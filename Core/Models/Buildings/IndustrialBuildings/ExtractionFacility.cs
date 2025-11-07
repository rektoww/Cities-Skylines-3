using System;
using System.Collections.Generic;
using Core.Enums;
using Core.Models.Mobs;
using Core.Models.Map;
using Core.Models.Components;

namespace Core.Models.Factories
{
    /// <summary>
    /// Универсальная производственная фабрика
    /// </summary>
    public class ProductionFactory
    {
        /// <summary>Сырьевые ресурсы фабрики</summary>
        public Dictionary<ResourceType, int> ResourceStorage { get; private set; } = new Dictionary<ResourceType, int>();

        /// <summary>Максимальная вместимость для каждого типа ресурсов</summary>
        public Dictionary<ResourceType, int> MaxStorageCapacity { get; private set; } = new Dictionary<ResourceType, int>();

        /// <summary>Природные ресурсы, с которыми работает фабрика</summary>
        public List<NaturalResource> ConnectedResources { get; private set; } = new List<NaturalResource>();

        /// <summary>Производственные цеха фабрики</summary>
        public List<Workshop> Workshops { get; private set; } = new List<Workshop>();

        /// <summary>Текущее количество рабочих</summary>
        public int WorkersCount { get; private set; }

        /// <summary>Максимальное количество рабочих</summary>
        public int MaxWorkers { get; private set; }

        /// <summary>Базовая скорость добычи</summary>
        public int BaseExtractionRate { get; private set; }

        /// <summary>
        /// Создает новую производственную фабрику
        /// </summary>
        public ProductionFactory(int maxWorkers = 10, int baseExtractionRate = 15)
        {
            MaxWorkers = maxWorkers;
            BaseExtractionRate = baseExtractionRate;
            WorkersCount = 0;
        }

        /// <summary>
        /// Инициализирует цеха фабрики
        /// </summary>
        public void InitializeWorkshops(List<Workshop> workshops)
        {
            Workshops = workshops ?? new List<Workshop>();
        }

        /// <summary>
        /// Добавляет цех на фабрику
        /// </summary>
        public void AddWorkshop(Workshop workshop)
        {
            if (workshop != null)
                Workshops.Add(workshop);
        }

        /// <summary>
        /// Устанавливает вместимость хранилища для типа ресурса
        /// </summary>
        public void SetStorageCapacity(ResourceType resourceType, int capacity)
        {
            if (MaxStorageCapacity.ContainsKey(resourceType))
                MaxStorageCapacity[resourceType] = capacity;
            else
                MaxStorageCapacity.Add(resourceType, capacity);

            // Инициализируем текущее количество, если его нет
            if (!ResourceStorage.ContainsKey(resourceType))
                ResourceStorage.Add(resourceType, 0);
        }

        /// <summary>
        /// Подключает природный ресурс к фабрике
        /// </summary>
        public void ConnectResource(NaturalResource resource)
        {
            if (resource != null && !ConnectedResources.Contains(resource))
                ConnectedResources.Add(resource);
        }

        /// <summary>
        /// Устанавливает количество рабочих
        /// </summary>
        public void SetWorkersCount(int count)
        {
            WorkersCount = Math.Min(Math.Max(0, count), MaxWorkers);
        }

        /// <summary>
        /// Добывает ресурсы из подключенных природных ресурсов
        /// </summary>
        public Dictionary<ResourceType, int> ExtractResources()
        {
            var extractedResources = new Dictionary<ResourceType, int>();

            if (WorkersCount == 0) return extractedResources;

            int extractionRate = CalculateExtractionRate();

            foreach (var resource in ConnectedResources)
            {
                if (resource.Amount <= 0) continue;

                ResourceType resourceType = resource.Type;
                int availableSpace = GetAvailableStorage(resourceType);

                if (availableSpace <= 0) continue;

                int amountToExtract = Math.Min(extractionRate, resource.Amount);
                amountToExtract = Math.Min(amountToExtract, availableSpace);

                resource.Amount -= amountToExtract;
                AddResource(resourceType, amountToExtract);

                extractedResources[resourceType] = amountToExtract;
            }

            return extractedResources;
        }

        /// <summary>
        /// Рассчитывает текущую скорость добычи
        /// </summary>
        private int CalculateExtractionRate()
        {
            return BaseExtractionRate + (WorkersCount * 3);
        }

        /// <summary>
        /// Получает доступное место для хранения типа ресурса
        /// </summary>
        private int GetAvailableStorage(ResourceType resourceType)
        {
            if (!MaxStorageCapacity.ContainsKey(resourceType))
                return 0;

            int currentAmount = ResourceStorage.ContainsKey(resourceType) ? ResourceStorage[resourceType] : 0;
            return MaxStorageCapacity[resourceType] - currentAmount;
        }

        /// <summary>
        /// Добавляет ресурс в хранилище
        /// </summary>
        public bool AddResource(ResourceType resourceType, int amount)
        {
            int availableSpace = GetAvailableStorage(resourceType);
            if (availableSpace < amount) return false;

            if (ResourceStorage.ContainsKey(resourceType))
                ResourceStorage[resourceType] += amount;
            else
                ResourceStorage.Add(resourceType, amount);

            return true;
        }

        /// <summary>
        /// Забирает ресурс из хранилища
        /// </summary>
        public bool TakeResource(ResourceType resourceType, int amount)
        {
            if (!ResourceStorage.ContainsKey(resourceType) || ResourceStorage[resourceType] < amount)
                return false;

            ResourceStorage[resourceType] -= amount;
            return true;
        }

        /// <summary>
        /// Запускает производственные циклы во всех цехах
        /// </summary>
        public Dictionary<ResourceType, int> ProcessWorkshops()
        {
            var totalOutput = new Dictionary<ResourceType, int>();

            if (WorkersCount == 0) return totalOutput;

            // Создаем копию доступных ресурсов для обработки
            var availableResources = new Dictionary<object, int>();
            foreach (var resource in ResourceStorage)
            {
                availableResources[resource.Key] = resource.Value;
            }

            foreach (var workshop in Workshops)
            {
                var workshopOutputs = new Dictionary<object, int>();
                bool productionSuccess = workshop.Process(availableResources, workshopOutputs);

                if (productionSuccess)
                {
                    // Добавляем выходные продукты в общий результат
                    foreach (var output in workshopOutputs)
                    {
                        if (output.Key is ResourceType resourceType)
                        {
                            if (totalOutput.ContainsKey(resourceType))
                                totalOutput[resourceType] += output.Value;
                            else
                                totalOutput[resourceType] = output.Value;
                        }
                    }
                }
            }

            // Обновляем основное хранилище ресурсов
            ResourceStorage.Clear();
            foreach (var resource in availableResources)
            {
                if (resource.Key is ResourceType resourceType)
                {
                    ResourceStorage[resourceType] = resource.Value;
                }
            }

            return totalOutput;
        }

        /// <summary>
        /// Полный рабочий цикл фабрики
        /// </summary>
        public void FullProductionCycle()
        {
            ExtractResources();
            ProcessWorkshops();
        }

        /// <summary>
        /// Получает текущие запасы ресурсов
        /// </summary>
        public Dictionary<ResourceType, int> GetResourceStorage()
        {
            return new Dictionary<ResourceType, int>(ResourceStorage);
        }

        /// <summary>
        /// Получает количество конкретного ресурса
        /// </summary>
        public int GetResourceAmount(ResourceType resourceType)
        {
            return ResourceStorage.ContainsKey(resourceType) ? ResourceStorage[resourceType] : 0;
        }

        /// <summary>
        /// Проверяет, достаточно ли ресурсов для производства
        /// </summary>
        public bool CanProduce(Workshop workshop)
        {
            if (workshop == null) return false;

            foreach (var requirement in workshop.InputRequirements)
            {
                if (requirement.Key is ResourceType resourceType)
                {
                    if (GetResourceAmount(resourceType) < requirement.Value)
                        return false;
                }
            }
            return true;
        }
    }
}