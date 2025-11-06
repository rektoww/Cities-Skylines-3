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
        /// Производственный цех ювелирного завода
        /// Отвечает за создание различных видов ювелирных изделий
        /// </summary>
        public class Workshop
        {
            /// <summary>
            /// Название производственного цеха
            /// </summary>
            public string Name { get; set; } = "Производственый цех ювелирного завода";

            /// <summary>
            /// Время выполнения одного производственного цикла
            /// </summary>
            public int ProductionCycleTime { get; set; } = 1;

            /// <summary>
            /// Требования к материалам для производства
            /// </summary>
            public Dictionary<JewelryMaterial, int> InputRequirements { get; private set; } = new();

            /// <summary>
            /// Выходная продукция цеха
            /// </summary>
            public Dictionary<JewelryProduct, int> OutputProducts { get; private set; } = new();

            /// <summary>
            /// Выполнить производственный цикл цеха
            /// Потребляет ресурсы и создает готовые ювелирные изделия
            /// </summary>
            /// <param name="availableResources">Доступные ресурсы на складе</param>
            /// <param name="outputProducts">Словарь для записи результатов производства</param>
            /// <returns>True если производство успешно завершено</returns>
            public bool Process(Dictionary<JewelryMaterial, int> availableResources, Dictionary<JewelryProduct, int> outputProducts)
            {
                if (availableResources == null)
                    throw new ArgumentNullException(nameof(availableResources), "Словарь ресурсов не может быть null");

                if (outputProducts == null)
                    throw new ArgumentNullException(nameof(outputProducts), "Словарь продукции не может быть null");

                // Проверяем, достаточно ли ресурсов для запуска производства
                if (!CanProduce(availableResources))
                    return false;

                // Потребляем ресурсы из доступных запасов
                foreach (var requirement in InputRequirements)
                {
                    availableResources[requirement.Key] -= requirement.Value;
                    if (availableResources[requirement.Key] <= 0)
                        availableResources.Remove(requirement.Key);
                }

                // Создаем готовые ювелирные изделия
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
            public bool CanProduce(Dictionary<JewelryMaterial, int> availableResources)
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
        /// Основные виды сырья для ювелирного производства
        /// Драгоценные металлы, камни и вспомогательные материалы
        /// </summary>
        public enum JewelryMaterial
        {
            Gold,       // Золото - основной драгоценный металл
            Silver,     // Серебро - для украшений и сплавов
            Platinum,   // Платина - редкий драгоценный металл
            Palladium,  // Палладий - для белого золота и сплавов
            Copper,     // Медь - для создания сплавов и пайки
            Steel,      // Сталь - для корпусов и фурнитуры
            Diamond,    // Алмаз - самый твердый драгоценный камень
            Gemstone,   // Драгоценный камень - различные виды камней
            Enamel,     // Эмаль - для цветного покрытия украшений
            Leather     // Кожа - для ремешков и отделки
        }

        /// <summary>
        /// Готовая ювелирная продукция производимая заводом
        /// Различные виды украшений и аксессуаров
        /// </summary>
        public enum JewelryProduct
        {
            GoldRing,           // Золотое кольцо - классическое украшение
            PlatinumRing,       // Платиновое кольцо - премиум сегмент
            SilverNecklace,     // Серебряное ожерелье - женское украшение
            DiamondEarrings,    // Серьги с алмазами - элитные украшения
            GoldBracelet,       // Золотой браслет - наручное украшение
            Pendant,            // Подвеска - для ношения на цепочке
            Brooch,             // Брошь - декоративное украшение
            GoldChain,          // Золотая цепь - шейное украшение
            WatchCase,          // Корпус часов - для механизмов часов
            JewelryBox          // Подарочная упаковка - для презентации
        }

        /// <summary>
        /// Склад сырья с текущими запасами материалов
        /// </summary>
        public Dictionary<JewelryMaterial, int> MaterialStorage { get; private set; } = new();

        /// <summary>
        /// Склад готовой продукции с текущими запасами
        /// </summary>
        public Dictionary<JewelryProduct, int> ProductStorage { get; private set; } = new();

        /// <summary>
        /// Максимальная вместимость склада сырья
        /// </summary>
        public int MaxMaterialStorage { get; private set; } = 1200;

        /// <summary>
        /// Максимальная вместимость склада готовой продукции
        /// </summary>
        public int MaxProductStorage { get; private set; } = 900;

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
        public int MaxWorkers { get; private set; } = 18;

        /// <summary>
        /// Эффективность производства зависящая от количества рабочих
        /// Рассчитывается динамически на основе текущей численности
        /// </summary>
        public float ProductionEfficiency => WorkersCount > 0 ? 0.4f + (WorkersCount / (float)MaxWorkers) * 0.6f : 0f;

        /// <summary>
        /// Конструктор ювелирного завода
        /// Инициализирует цеха и начальные запасы сырья
        /// </summary>
        public JewelryFactory()
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
            // Цех колец - производство золотых и платиновых колец
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

            // Цех ожерелий и цепей - производство шейных украшений
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

            // Цех серёг и брошей - производство украшений с камнями
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

            // Цех браслетов и подвесок - производство различных аксессуаров
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

            // Цех часов и упаковки - производство корпусов и подарочных наборов
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

        /// <summary>
        /// Инициализация начальных запасов сырья на складе
        /// Заполняет склад стартовыми материалами для работы
        /// </summary>
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
        public bool AddMaterial(JewelryMaterial material, int amount)
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
        public bool RemoveMaterial(JewelryMaterial material, int amount)
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

            var availableResources = new Dictionary<JewelryMaterial, int>(MaterialStorage);
            var producedOutputs = new Dictionary<JewelryProduct, int>();

            foreach (var workshop in Workshops)
            {
                var workshopResources = new Dictionary<JewelryMaterial, int>(availableResources);
                var workshopOutputs = new Dictionary<JewelryProduct, int>();

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
        public Dictionary<JewelryProduct, int> GetProductionOutput()
        {
            return new Dictionary<JewelryProduct, int>(ProductStorage);
        }

        /// <summary>
        /// Получить текущие запасы сырья на складе
        /// </summary>
        /// <returns>Словарь с материалами и их количеством</returns>
        public Dictionary<JewelryMaterial, int> GetMaterialStorage()
        {
            return new Dictionary<JewelryMaterial, int>(MaterialStorage);
        }

        /// <summary>
        /// Потребить готовую продукцию со склада
        /// Уменьшает количество продукции на складе
        /// </summary>
        /// <param name="product">Тип потребляемой продукции</param>
        /// <param name="amount">Количество для потребления</param>
        /// <returns>True если продукция успешно потреблена</returns>
        public bool ConsumeProduct(JewelryProduct product, int amount)
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
        private void ApplyProductionEfficiency(Dictionary<JewelryProduct, int> outputs)
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
        private void UpdateMaterialsStorage(Dictionary<JewelryMaterial, int> availableResources)
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
        private void UpdateProductsStorage(Dictionary<JewelryProduct, int> producedOutputs)
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