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
        /// Производственный цех стекольного завода
        /// Отвечает за преобразование сырья в готовую продукцию
        /// </summary>
        public class Workshop
        {
            /// <summary>
            /// Название производственного цеха
            /// </summary>
            public string Name { get; set; } = "Производственый цех стекольного завода";

            /// <summary>
            /// Время выполнения одного производственного цикла
            /// </summary>
            public int ProductionCycleTime { get; set; } = 1;

            /// <summary>
            /// Требования к материалам для производства
            /// </summary>
            public Dictionary<GlassMaterial, int> InputRequirements { get; private set; } = new();

            /// <summary>
            /// Выходная продукция цеха
            /// </summary>
            public Dictionary<GlassProduct, int> OutputProducts { get; private set; } = new();

            /// <summary>
            /// Выполнить производственный цикл цеха
            /// Потребляет ресурсы и создает готовую продукцыю
            /// </summary>
            /// <param name="availableResources">Доступные ресурсы на складе</param>
            /// <param name="outputProducts">Словарь для записи результатов производства</param>
            /// <returns>True если производство успешно завершено</returns>
            public bool Process(Dictionary<GlassMaterial, int> availableResources, Dictionary<GlassProduct, int> outputProducts)
            {
                if (availableResources == null)
                    throw new ArgumentNullException(nameof(availableResources), "Словарь ресурсов не может быть null");

                if (outputProducts == null)
                    throw new ArgumentNullException(nameof(outputProducts), "Словарь продукции не может быть null");

                // Проверяем, достаточно ли ресурсов для производства
                if (!CanProduce(availableResources))
                    return false;

                // Потребляем ресурсы из доступных запасов
                foreach (var requirement in InputRequirements)
                {
                    availableResources[requirement.Key] -= requirement.Value;
                    if (availableResources[requirement.Key] <= 0)
                        availableResources.Remove(requirement.Key);
                }

                // Производим готовую продукцыю
                foreach (var output in OutputProducts)
                {
                    if (outputProducts.ContainsKey(output.Key))
                        outputProducts[output.Key] += output.Value;
                    else
                        outputProducts[output.Key] = output.Value;
                }

                return true;
            }

            /// <summary>
            /// Проверить, достаточно ли ресурсов для запуска производства
            /// </summary>
            /// <param name="availableResources">Доступные материалы на складе</param>
            /// <returns>True если ресурсов достаточно для производства</returns>
            public bool CanProduce(Dictionary<GlassMaterial, int> availableResources)
            {
                if (availableResources == null)
                    throw new ArgumentNullException(nameof(availableResources), "Словарь ресурсов не может быть null");

                foreach (var requirement in InputRequirements)
                {
                    if (!availableResources.ContainsKey(requirement.Key) || availableResources[requirement.Key] < requirement.Value)
                        return false;
                }
                return true;
            }

            /// <summary>
            /// Получить основную информацию о цехе
            /// </summary>
            /// <returns>Словарь с параметрами цеха</returns>
            public Dictionary<string, object> GetWorkshopInfo()
            {
                return new Dictionary<string, object>
                {
                    { "Название цеха", Name },
                    { "Время цикла производства", ProductionCycleTime },
                    { "Требуемые материалы", string.Join(", ", InputRequirements.Select(x => $"{x.Key}: {x.Value}")) },
                    { "Выпускаемая продукция", string.Join(", ", OutputProducts.Select(x => $"{x.Key}: {x.Value}")) }
                };
            }
        }

        /// <summary>
        /// Основные виды сырья для стекольной промышленности
        /// Используется в производственном процессе
        /// </summary>
        public enum GlassMaterial
        {
            Sand,        // Кварцевый песок - основной компонент
            SodaAsh,     // Карбонат натрия - для снижения температуры плавления
            Limestone,   // Известняк - стабилизатор
            RecycledGlass, // Бой стекла - вторичное сырье
            Colorant,    // Краситель - для окрашивания стекла
            Tin,         // Олово - для флоат-процесса
            Gas,         // Топливо/энергия - для работы печей
            Clay,        // Глина - для изготовления форм
            Silica,      // Диоксид кремния - улучшает свойства
            Resin        // Смолы - для ламинирования и связывания
        }

        /// <summary>
        /// Готовая стеклянная продукция производимая заводом
        /// Различные виды изделий из стекла
        /// </summary>
        public enum GlassProduct
        {
            WindowGlass,     // Оконное стекло для строительства
            GlassBottle,     // Стеклянная бутылка для напитков
            GlassJar,        // Стеклянная банка для консервации
            GlassSheet,      // Листовое стекло общего назначения
            TemperedGlass,   // Закалённое стекло повышенной прочности
            LaminatedGlass,  // Триплекс - многослойное безопасное стекло
            GlassTube,       // Стеклянная трубка для лабораторий
            Fiberglass,      // Стекловолокно - композитный материал
            Mirror,          // Зеркало с отражающим покрытием
            GlassBrick       // Стеклоблок для декоративных целей
        }

        /// <summary>
        /// Склад сырья с текущими запасами материалов
        /// </summary>
        public Dictionary<GlassMaterial, int> MaterialStorage { get; private set; } = new();

        /// <summary>
        /// Склад готовой продукции с текущими запасами
        /// </summary>
        public Dictionary<GlassProduct, int> ProductStorage { get; private set; } = new();

        /// <summary>
        /// Максимальная вместимость склада сырья
        /// </summary>
        public int MaxMaterialStorage { get; private set; } = 1800;

        /// <summary>
        /// Максимальная вместимость склада готовой продукции
        /// </summary>
        public int MaxProductStorage { get; private set; } = 1400;

        /// <summary>
        /// Список всех производственных цехов завода
        /// </summary>
        public List<Workshop> Workshops { get; private set; } = new();

        /// <summary>
        /// Текущее количество рабочих на заводе
        /// </summary>
        public int WorkersCount { get; private set; }

        /// <summary>
        /// Максимальное количество рабочих которое может работать на заводе
        /// </summary>
        public int MaxWorkers { get; private set; } = 20;

        /// <summary>
        /// Эффективность производства зависящая от количества рабочих
        /// Рассчитывается динамически на основе текущей численности
        /// </summary>
        public float ProductionEfficiency => WorkersCount > 0 ? 0.35f + (WorkersCount / (float)MaxWorkers) * 0.65f : 0f;

        /// <summary>
        /// Конструктор стекольного завода
        /// Инициализирует цеха и начальные запасы сырья
        /// </summary>
        public GlassFactory()
        {
            WorkersCount = 0;
            InitializeWorkshops();
            InitializeStartingMaterials();
        }

        /// <summary>
        /// Инициализация производственных цехов завода
        /// Создает и настраивает все цеха с их параметрами
        /// </summary>
        private void InitializeWorkshops()
        {
            // Плавильная печь - основной цех для выплавки стекла
            var meltingWorkshop = new Workshop
            {
                Name = "Плавильная печ",
                ProductionCycleTime = 6
            };
            meltingWorkshop.InputRequirements.Add(GlassMaterial.Sand, 14);
            meltingWorkshop.InputRequirements.Add(GlassMaterial.SodaAsh, 6);
            meltingWorkshop.InputRequirements.Add(GlassMaterial.Limestone, 4);
            meltingWorkshop.OutputProducts.Add(GlassProduct.GlassSheet, 8);
            meltingWorkshop.OutputProducts.Add(GlassProduct.WindowGlass, 6);
            Workshops.Add(meltingWorkshop);

            // Линия для производства тарного стекла - бутылки и банки
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

            // Флоат-линия для производства высококачественного стекла
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

            // Цех обработки стекла - закалка и ламинирование
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

            // Цех специальных изделий - волокно и трубки
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

        /// <summary>
        /// Инициализация начальных запасов сырья на складе
        /// Заполняет склад стартовыми материалами для работы
        /// </summary>
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

        /// <summary>
        /// Установить количество рабочих на заводе
        /// Влияет на эффективность производства
        /// </summary>
        /// <param name="count">Количество рабочих для установки</param>
        public void SetWorkersCount(int count)
        {
            if (count < 0)
                throw new ArgumentException("Количество рабочих не может быть отрицательным", nameof(count));

            WorkersCount = Math.Clamp(count, 0, MaxWorkers);
        }

        /// <summary>
        /// Добавить материал на склад сырья
        /// Проверяет доступное место перед добавлением
        /// </summary>
        /// <param name="material">Тип добавляемого материала</param>
        /// <param name="amount">Количество для добавления</param>
        /// <returns>True если материал успешно добавлен</returns>
        public bool AddMaterial(GlassMaterial material, int amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Количество материала должно быть положительным числом", nameof(amount));

            int currentAmount = MaterialStorage.ContainsKey(material) ? MaterialStorage[material] : 0;
            if (GetTotalMaterialStorage() + amount > MaxMaterialStorage)
                return false;

            MaterialStorage[material] = currentAmount + amount;
            return true;
        }

        /// <summary>
        /// Удалить материал со склада сырья
        /// Используется для потребления материалов производством
        /// </summary>
        /// <param name="material">Тип удаляемого материала</param>
        /// <param name="amount">Количество для удаления</param>
        /// <returns>True если материал успешно удален</returns>
        public bool RemoveMaterial(GlassMaterial material, int amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Количество материала должно быть положительным числом", nameof(amount));

            if (!MaterialStorage.ContainsKey(material) || MaterialStorage[material] < amount)
                return false;

            MaterialStorage[material] -= amount;
            if (MaterialStorage[material] == 0)
                MaterialStorage.Remove(material);

            return true;
        }

        /// <summary>
        /// Запустить обработку всех цехов завода
        /// Выполняет производственные циклы всех цехов последовательно
        /// </summary>
        public void ProcessWorkshops()
        {
            if (WorkersCount == 0 || ProductionEfficiency <= 0)
                return;

            var availableResources = new Dictionary<GlassMaterial, int>(MaterialStorage);
            var producedOutputs = new Dictionary<GlassProduct, int>();

            foreach (var workshop in Workshops)
            {
                var workshopResources = new Dictionary<GlassMaterial, int>(availableResources);
                var workshopOutputs = new Dictionary<GlassProduct, int>();

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

        /// <summary>
        /// Получить текущую продукцию на складе
        /// </summary>
        /// <returns>Словарь с продукцией и ее количеством</returns>
        public Dictionary<GlassProduct, int> GetProductionOutput()
        {
            return new Dictionary<GlassProduct, int>(ProductStorage);
        }

        /// <summary>
        /// Получить текущие запасы сырья на складе
        /// </summary>
        /// <returns>Словарь с материалами и их количеством</returns>
        public Dictionary<GlassMaterial, int> GetMaterialStorage()
        {
            return new Dictionary<GlassMaterial, int>(MaterialStorage);
        }

        /// <summary>
        /// Потреблить готовую продукцию со склада
        /// Уменьшает количество продукции на складе
        /// </summary>
        /// <param name="product">Тип потребляемой продукции</param>
        /// <param name="amount">Количество для потребления</param>
        /// <returns>True если продукция успешно потреблена</returns>
        public bool ConsumeProduct(GlassProduct product, int amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Количество продукции должно быть положительным числом", nameof(amount));

            if (amount == 0)
                return true;

            if (!ProductStorage.ContainsKey(product) || ProductStorage[product] < amount)
                return false;

            ProductStorage[product] -= amount;
            if (ProductStorage[product] == 0)
                ProductStorage.Remove(product);

            return true;
        }

        /// <summary>
        /// Получить общую информацию о производстве
        /// Содержит основные показатели работы завода
        /// </summary>
        /// <returns>Словарь с производственной информацией</returns>
        public Dictionary<string, object> GetProductionInfo()
        {
            return new Dictionary<string, object>
            {
                { "Количество рабочих", WorkersCount },
                { "Максимальное количество рабочих", MaxWorkers },
                { "Эфективность производства", ProductionEfficiency },
                { "Общее сырьё на складе", GetTotalMaterialStorage() },
                { "Максимум сырья", MaxMaterialStorage },
                { "Общая продукция на складе", GetTotalProductStorage() },
                { "Максимум продукции", MaxProductStorage },
                { "Активные цеха", Workshops.Count },
                { "Типы материалов", MaterialStorage.Count },
                { "Типы продукции", ProductStorage.Count },
            };
        }

        /// <summary>
        /// Получить детальную информацию о конкретном цехе
        /// </summary>
        /// <param name="workshopIndex">Индекс цеха в списке</param>
        /// <returns>Словарь с информацией о цехе</returns>
        public Dictionary<string, object> GetWorkshopInfo(int workshopIndex)
        {
            if (workshopIndex < 0 || workshopIndex >= Workshops.Count)
                throw new ArgumentOutOfRangeException(nameof(workshopIndex), "Индекс цеха вне допустимого диапазона");

            return Workshops[workshopIndex].GetWorkshopInfo();
        }

        /// <summary>
        /// Рассчитать общее количество сырья на складе
        /// </summary>
        /// <returns>Сумма всех материалов на складе</returns>
        private int GetTotalMaterialStorage() => MaterialStorage.Values.Sum();

        /// <summary>
        /// Рассчитать общее количество продукции на складе
        /// </summary>
        /// <returns>Сумма всей продукции на складе</returns>
        private int GetTotalProductStorage() => ProductStorage.Values.Sum();

        /// <summary>
        /// Применить эффективность производства к выходной продукции
        /// Уменьшает выход продукции в зависимости от эффективности
        /// </summary>
        /// <param name="outputs">Выходная продукция для коррекции</param>
        private void ApplyProductionEfficiency(Dictionary<GlassProduct, int> outputs)
        {
            if (ProductionEfficiency >= 1f)
                return;

            var keys = outputs.Keys.ToList();
            foreach (var key in keys)
            {
                outputs[key] = (int)(outputs[key] * ProductionEfficiency);
                if (outputs[key] <= 0)
                    outputs.Remove(key);
            }
        }

        /// <summary>
        /// Обновить склад сырья после производственного цикла
        /// </summary>
        /// <param name="availableResources">Оставшиеся после производства ресурсы</param>
        private void UpdateMaterialsStorage(Dictionary<GlassMaterial, int> availableResources)
        {
            MaterialStorage.Clear();
            foreach (var resource in availableResources)
            {
                if (resource.Value > 0)
                    MaterialStorage[resource.Key] = resource.Value;
            }
        }

        /// <summary>
        /// Обновить склад готовой продукции новыми изделиями
        /// Учитывает ограничения по вместимости склада
        /// </summary>
        /// <param name="producedOutputs">Произведенная в цикле продукция</param>
        private void UpdateProductsStorage(Dictionary<GlassProduct, int> producedOutputs)
        {
            foreach (var output in producedOutputs)
            {
                int current = ProductStorage.ContainsKey(output.Key) ? ProductStorage[output.Key] : 0;
                int space = MaxProductStorage - GetTotalProductStorage();
                int toAdd = Math.Min(output.Value, space);

                if (toAdd > 0)
                    ProductStorage[output.Key] = current + toAdd;

                if (toAdd < output.Value)
                {
                    System.Diagnostics.Debug.WriteLine($"Превышена вместимость склада! Потеряно {output.Value - toAdd} единиц продукции {output.Key}");
                }
            }
        }
    }
}