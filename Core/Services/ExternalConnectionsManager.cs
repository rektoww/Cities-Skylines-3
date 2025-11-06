using Core.Enums;
using Core.Models.Buildings;
using Core.Models.Mobs;
using Core.Resourses;

namespace Core.Services
{
    /// <summary>
    /// Управление внешними связями города: импорт и экспорт ресурсов, миграция населения
    /// Вызов из основного цикла симуляции для обработки торговли и перемещения граждан
    /// </summary>
    public class ExternalConnectionsManager
    {
        // Зависимости от других систем города
        private readonly PlayerResources _playerResources;
        private readonly List<Citizen> _citizens;
        private readonly List<ResidentialBuilding> _residentialBuildings;

        // Данные об импорте/экспорте за текущий период
        private Dictionary<ConstructionMaterial, int> _importVolumes;
        private Dictionary<ConstructionMaterial, int> _exportVolumes;

        // Данные о миграции за текущий период
        private int _netMigrationThisMonth;
        private int _immigrantsCount;
        private int _emigrantsCount;

        // Константы для расчетов
        private const decimal IMPORT_PRICE_MULTIPLIER = 1.5m; // Импорт дороже на 50%
        private const decimal EXPORT_PRICE_MULTIPLIER = 0.7m; // Экспорт дешевле на 30%
        private const int BASE_MATERIAL_PRICE = 100; // Базовая цена за единицу материала
        
        private const int MATERIAL_SHORTAGE_THRESHOLD = 50; // Порог для запуска импорта
        private const int MATERIAL_SURPLUS_THRESHOLD = 500; // Порог для запуска экспорта
        
        private const float UNEMPLOYMENT_MIGRATION_FACTOR = 200f; // Влияние безработицы на миграцию
        private const float HOUSING_MIGRATION_FACTOR = 0.1f; // Влияние жилья на миграцию

        /// <summary>
        /// Конструктор менеджера внешних связей
        /// </summary>
        /// <param name="playerResources">Ссылка на ресурсы игрока (бюджет и материалы)</param>
        /// <param name="citizens">Ссылка на список граждан города</param>
        /// <param name="residentialBuildings">Ссылка на список жилых зданий</param>
        public ExternalConnectionsManager(
            PlayerResources playerResources, 
            List<Citizen> citizens, 
            List<ResidentialBuilding> residentialBuildings)
        {
            _playerResources = playerResources;
            _citizens = citizens;
            _residentialBuildings = residentialBuildings;

            // Инициализация словарей для всех типов материалов
            _importVolumes = new Dictionary<ConstructionMaterial, int>();
            _exportVolumes = new Dictionary<ConstructionMaterial, int>();
            
            foreach (ConstructionMaterial material in Enum.GetValues(typeof(ConstructionMaterial)))
            {
                _importVolumes[material] = 0;
                _exportVolumes[material] = 0;
            }
        }

        /// <summary>
        /// Выполнение симуляции внешних связей за один игровой период (месяц)
        /// Обработка импорта/экспорта материалов и миграции населения
        /// </summary>
        public void SimulateTick()
        {
            ClearMonthlyData();
            SimulateTrade();
            SimulateMigration();
        }

        /// <summary>
        /// Симуляция торговли материалами с внешним миром
        /// Импорт недостающих материалов и экспорт излишков
        /// </summary>
        private void SimulateTrade()
        {
            // Проверка каждого типа строительного материала
            foreach (ConstructionMaterial material in Enum.GetValues(typeof(ConstructionMaterial)))
            {
                // Получение текущего количества материала
                int currentAmount = GetMaterialAmount(material);

                // Импорт: если материала мало
                if (currentAmount < MATERIAL_SHORTAGE_THRESHOLD)
                {
                    int amountToImport = MATERIAL_SHORTAGE_THRESHOLD - currentAmount;
                    TryImportMaterial(material, amountToImport);
                }
                // Экспорт: если материала много
                else if (currentAmount > MATERIAL_SURPLUS_THRESHOLD)
                {
                    int amountToExport = currentAmount - MATERIAL_SURPLUS_THRESHOLD;
                    TryExportMaterial(material, amountToExport);
                }
            }
        }

        /// <summary>
        /// Попытка импортировать указанное количество материала
        /// </summary>
        /// <param name="material">Тип материала для импорта</param>
        /// <param name="amount">Количество для импорта</param>
        private void TryImportMaterial(ConstructionMaterial material, int amount)
        {
            decimal importCost = CalculateImportCost(material, amount);

            // Проверка, хватит ли денег
            if (_playerResources.Balance >= importCost)
            {
                // Списывание денег
                _playerResources.Balance -= importCost;

                // Добавление материала
                if (_playerResources.StoredMaterials.ContainsKey(material))
                {
                    _playerResources.StoredMaterials[material] += amount;
                }
                else
                {
                    _playerResources.StoredMaterials[material] = amount;
                }

                // Запись статистики импорта
                _importVolumes[material] += amount;
            }
        }

        /// <summary>
        /// Попытка экспортировать указанное количество материала
        /// </summary>
        /// <param name="material">Тип материала для экспорта</param>
        /// <param name="amount">Количество для экспорта</param>
        private void TryExportMaterial(ConstructionMaterial material, int amount)
        {
            int currentAmount = GetMaterialAmount(material);

            // Проверка, есть ли достаточно материала для экспорта
            if (currentAmount >= amount)
            {
                decimal exportRevenue = CalculateExportRevenue(material, amount);

                // Исключение материала
                _playerResources.StoredMaterials[material] -= amount;

                // Добавление денег
                _playerResources.Balance += exportRevenue;

                // Запись статистики экспорта
                _exportVolumes[material] += amount;
            }
        }

        /// <summary>
        /// Симуляция миграции населения на основе условий в городе
        /// </summary>
        private void SimulateMigration()
        {
            // Расчет безработицы
            float unemploymentRate = CalculateUnemploymentRate();

            // Расчет свободного жилья
            int availableHousing = CalculateAvailableHousing();

            // Базовая формула миграции:
            // - Высокая безработица отпугивает мигрантов и вызывает эмиграцию
            // - Наличие свободного жилья привлекает мигрантов
            int migrationBalance = (int)((0.5f - unemploymentRate) * UNEMPLOYMENT_MIGRATION_FACTOR + 
                                          availableHousing * HOUSING_MIGRATION_FACTOR);

            _netMigrationThisMonth = migrationBalance;

            if (migrationBalance > 0)
            {
                // Иммиграция: добавление новых граждан
                _immigrantsCount = migrationBalance;
                _emigrantsCount = 0;
                AddNewCitizens(migrationBalance);
            }
            else if (migrationBalance < 0)
            {
                // Эмиграция: исключение граждан
                _emigrantsCount = Math.Abs(migrationBalance);
                _immigrantsCount = 0;
                RemoveCitizens(Math.Abs(migrationBalance));
            }
            else
            {
                // Нулевая миграция
                _immigrantsCount = 0;
                _emigrantsCount = 0;
            }
        }

        /// <summary>
        /// Добавление новых граждан в город (иммигранты)
        /// </summary>
        /// <param name="count">Количество граждан для добавления</param>
        private void AddNewCitizens(int count)
        {
            // Создание граждан без размещения на карте
            for (int i = 0; i < count; i++)
            {
                var newCitizen = new Citizen(0, 0, null)
                {
                    Age = 25, // Иммигранты обычно молодые и трудоспособные
                    IsMale = i % 2 == 0, // Чередование пола
                    Education = EducationLevel.School,
                    IsEmployed = false,
                    Health = 100f,
                    Happiness = 50f
                };

                _citizens.Add(newCitizen);

                // Попытка найти жилье для нового гражданина
                AssignHousingToCitizen(newCitizen);
            }
        }

        /// <summary>
        /// Удаление граждан из города (эмигранты)
        /// </summary>
        /// <param name="count">Количество граждан для удаления</param>
        private void RemoveCitizens(int count)
        {
            // Сортировка граждан по приоритету эмиграции:
            // 1. Безработные с низким счастьем (первые кандидаты)
            // 2. Безработные с нормальным счастьем
            // 3. Работающие с низким счастьем
            // 4. Работающие со средним счастьем
            // 5. Работающие с высоким счастьем (последние кандидаты)
            var sortedCitizens = _citizens
                .OrderBy(c => c.IsEmployed ? 1 : 0)  // Сначала безработные
                .ThenBy(c => c.Happiness)             // Потом по уровню счастья (от меньшего к большему)
                .Take(count)
                .ToList();

            // Удаление выбранных граждан
            foreach (var citizen in sortedCitizens)
            {
                RemoveCitizen(citizen);
            }
        }

        /// <summary>
        /// Удаление конкретного гражданина из города
        /// </summary>
        /// <param name="citizen">Гражданин для удаления</param>
        private void RemoveCitizen(Citizen citizen)
        {
            // Освобождение жилья
            if (citizen.Home != null)
            {
                citizen.Home.CurrentResidents.Remove(citizen);
            }

            // Удаление из списка граждан
            _citizens.Remove(citizen);
        }

        /// <summary>
        /// Попытка назначить жилье новому гражданину
        /// </summary>
        /// <param name="citizen">Гражданин, которому нужно жилье</param>
        private void AssignHousingToCitizen(Citizen citizen)
        {
            foreach (var building in _residentialBuildings)
            {
                if (building.HasVacancy)
                {
                    building.TryAddResident(citizen);
                    break;
                }
            }
        }

        /// <summary>
        /// Рассчет уровеня безработицы в городе
        /// </summary>
        /// <returns>Уровень безработицы (от 0.0 до 1.0)</returns>
        private float CalculateUnemploymentRate()
        {
            if (_citizens.Count == 0)
                return 0f;

            int workingAge = _citizens.Count(c => c.Age >= 18 && c.Age <= 65);
            if (workingAge == 0)
                return 0f;

            int unemployed = _citizens.Count(c => c.Age >= 18 && c.Age <= 65 && !c.IsEmployed);
            return (float)unemployed / workingAge;
        }

        /// <summary>
        /// Рассчет количества свободного жилья
        /// </summary>
        /// <returns>Количество свободных мест</returns>
        private int CalculateAvailableHousing()
        {
            int totalCapacity = _residentialBuildings.Sum(b => b.Capacity);
            int occupied = _residentialBuildings.Sum(b => b.CurrentResidents.Count);
            return totalCapacity - occupied;
        }

        /// <summary>
        /// Получение текущего количества материала из хранилища игрока
        /// </summary>
        /// <param name="material">Тип материала</param>
        /// <returns>Количество материала</returns>
        private int GetMaterialAmount(ConstructionMaterial material)
        {
            return _playerResources.StoredMaterials.ContainsKey(material) 
                ? _playerResources.StoredMaterials[material] 
                : 0;
        }

        /// <summary>
        /// Рассчет стоимости импорта материала
        /// </summary>
        /// <param name="material">Тип материала</param>
        /// <param name="amount">Количество</param>
        /// <returns>Стоимость импорта</returns>
        private decimal CalculateImportCost(ConstructionMaterial material, int amount)
        {
            return BASE_MATERIAL_PRICE * amount * IMPORT_PRICE_MULTIPLIER;
        }

        /// <summary>
        /// Рассчет доходов от экспорта материала
        /// </summary>
        /// <param name="material">Тип материала</param>
        /// <param name="amount">Количество</param>
        /// <returns>Доход от экспорта</returns>
        private decimal CalculateExportRevenue(ConstructionMaterial material, int amount)
        {
            return BASE_MATERIAL_PRICE * amount * EXPORT_PRICE_MULTIPLIER;
        }

        /// <summary>
        /// Очистка статистики за текущий месяц для подготовки к следующему периоду
        /// </summary>
        private void ClearMonthlyData()
        {
            foreach (var material in _importVolumes.Keys.ToList())
            {
                _importVolumes[material] = 0;
                _exportVolumes[material] = 0;
            }

            _netMigrationThisMonth = 0;
            _immigrantsCount = 0;
            _emigrantsCount = 0;
        }

        /// <summary>
        /// Получение объема импорта материала за текущий месяц
        /// </summary>
        /// <param name="material">Тип материала</param>
        /// <returns>Объем импорта</returns>
        public int GetImportRate(ConstructionMaterial material)
        {
            return _importVolumes.ContainsKey(material) ? _importVolumes[material] : 0;
        }

        /// <summary>
        /// Получение объема экспорта материала за текущий месяц
        /// </summary>
        /// <param name="material">Тип материала</param>
        /// <returns>Объем экспорта</returns>
        public int GetExportRate(ConstructionMaterial material)
        {
            return _exportVolumes.ContainsKey(material) ? _exportVolumes[material] : 0;
        }

        /// <summary>
        /// Получение чистого миграционного баланса за текущий месяц
        /// Положительное значение = иммиграция, отрицательное = эмиграция
        /// </summary>
        /// <returns>Миграционный баланс</returns>
        public int GetMigrationBalance()
        {
            return _netMigrationThisMonth;
        }

        /// <summary>
        /// Получение количества иммигрантов за текущий месяц
        /// </summary>
        /// <returns>Количество иммигрантов</returns>
        public int GetImmigrantsCount()
        {
            return _immigrantsCount;
        }

        /// <summary>
        /// Получение количества эмигрантов за текущий месяц
        /// </summary>
        /// <returns>Количество эмигрантов</returns>
        public int GetEmigrantsCount()
        {
            return _emigrantsCount;
        }

        /// <summary>
        /// Получение общего объема торговли (импорт + экспорт) за текущий месяц
        /// </summary>
        /// <returns>Общий объем торговли в денежном эквиваленте</returns>
        public decimal GetTotalTradeVolume()
        {
            decimal total = 0m;

            foreach (var material in _importVolumes.Keys)
            {
                total += CalculateImportCost(material, _importVolumes[material]);
                total += CalculateExportRevenue(material, _exportVolumes[material]);
            }

            return total;
        }
    }
}
