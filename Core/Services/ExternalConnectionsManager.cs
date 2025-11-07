using Core.Enums;
using Core.Models.Buildings;
using Core.Models.Mobs;
using Core.Resourses;

namespace Core.Services
{
    /// <summary>
    /// Управление внешними связями города: импорт/экспорт ресурсов и миграция населения
    /// </summary>
    public class ExternalConnectionsManager
    {
        // Зависимости от других систем города
        private readonly PlayerResources _playerResources;
        private readonly FinancialSystem _financialSystem;

        // Константы для расчетов
        private const decimal IMPORT_PRICE_MULTIPLIER = 1.5m; // Импорт дороже на 50%
        private const decimal EXPORT_PRICE_MULTIPLIER = 0.7m; // Экспорт дешевле на 30%

        /// <summary>
        /// Конструктор менеджера внешних связей
        /// </summary>
        /// <param name="playerResources">Ссылка на ресурсы игрока (бюджет и материалы)</param>
        /// <param name="financialSystem">Финансовая система</param>
        public ExternalConnectionsManager(
            PlayerResources playerResources,
            FinancialSystem financialSystem)
        {
            _playerResources = playerResources;
            _financialSystem = financialSystem;
        }



        /// <summary>
        /// Ручной импорт материалов из внешнего мира (покупка по запросу игрока)
        /// </summary>
        /// <param name="quantities">Материалы и их количество для импорта</param>
        /// <param name="prices">Цены импорта за единицу</param>
        /// <param name="totalCost">Общая стоимость импорта</param>
        /// <returns>true, если импорт успешен</returns>
        public bool TryImportMaterials(
            Dictionary<ConstructionMaterial, int> quantities,
            Dictionary<ConstructionMaterial, decimal> prices,
            out decimal totalCost)
        {
            totalCost = 0m;

            if (quantities == null || quantities.Count == 0)
                return false;

            // Расчёт общей стоимости импорта
            foreach (var item in quantities)
            {
                if (item.Value <= 0) return false;
                if (!prices.TryGetValue(item.Key, out var unitPrice)) return false;
                totalCost += unitPrice * item.Value;
            }

            // Проверка бюджета (приоритет FinancialSystem)
            decimal availableBudget = _financialSystem?.CityBudget ?? _playerResources.Balance;
            if (availableBudget < totalCost)
                return false;

            // Списание средств через финансовую систему
            if (_financialSystem != null)
            {
                if (!_financialSystem.AddExpense(totalCost, "Import: Materials"))
                    return false;
            }
            else
            {
                _playerResources.Balance -= totalCost;
            }

            // Добавление импортированных материалов в инвентарь
            foreach (var item in quantities)
            {
                if (_playerResources.StoredMaterials.ContainsKey(item.Key))
                    _playerResources.StoredMaterials[item.Key] += item.Value;
                else
                    _playerResources.StoredMaterials[item.Key] = item.Value;

            }

            // Синхронизация баланса игрока
            if (_financialSystem != null)
                _playerResources.Balance -= totalCost;

            return true;
        }

        /// <summary>
        /// Ручной экспорт материалов во внешний мир (продажа по запросу игрока)
        /// </summary>
        /// <param name="material">Тип материала для экспорта</param>
        /// <param name="quantity">Количество для экспорта</param>
        /// <param name="pricePerUnit">Цена экспорта за единицу</param>
        /// <param name="totalRevenue">Общая выручка от экспорта</param>
        /// <returns>true, если экспорт успешен</returns>
        public bool TryExportMaterials(
            ConstructionMaterial material,
            int quantity,
            decimal pricePerUnit,
            out decimal totalRevenue)
        {
            totalRevenue = 0m;

            if (quantity <= 0)
                return false;

            // Проверка наличия материалов для экспорта
            if (!_playerResources.StoredMaterials.ContainsKey(material) ||
                _playerResources.StoredMaterials[material] < quantity)
                return false;

            totalRevenue = pricePerUnit * quantity;

            // Удаление материалов из инвентаря
            _playerResources.StoredMaterials[material] -= quantity;

            // Добавление дохода от экспорта
            if (_financialSystem != null)
            {
                _financialSystem.AddIncome(totalRevenue, $"Export: {material} x{quantity}");
            }
            _playerResources.Balance += totalRevenue;

            return true;
        }

        /// <summary>
        /// Экспорт всех доступных материалов одного типа
        /// </summary>
        public bool TryExportAllMaterials(
            ConstructionMaterial material,
            decimal pricePerUnit,
            out decimal totalRevenue)
        {
            totalRevenue = 0m;

            if (!_playerResources.StoredMaterials.ContainsKey(material))
                return false;

            int quantity = _playerResources.StoredMaterials[material];
            if (quantity == 0)
                return false;

            return TryExportMaterials(material, quantity, pricePerUnit, out totalRevenue);
        }

        /// <summary>
        /// Получение цены импорта для конкретного материала
        /// </summary>
        public decimal GetImportPrice(ConstructionMaterial material, int basePrice)
        {
            return basePrice * IMPORT_PRICE_MULTIPLIER;
        }

        /// <summary>
        /// Получение цены экспорта для конкретного материала
        /// </summary>
        public decimal GetExportPrice(ConstructionMaterial material, int basePrice)
        {
            return basePrice * EXPORT_PRICE_MULTIPLIER;
        }

        /// <summary>
        /// Добавление новых иммигрантов в город
        /// </summary>
        /// <param name="citizens">Список граждан города</param>
        /// <param name="residentialBuildings">Список жилых зданий</param>
        /// <param name="count">Количество иммигрантов</param>
        /// <returns>Количество фактически добавленных граждан</returns>
        public int AddImmigrants(
            List<Citizen> citizens,
            List<ResidentialBuilding> residentialBuildings,
            int count)
        {
            if (citizens == null || count <= 0)
                return 0;

            int added = 0;
            for (int i = 0; i < count; i++)
            {
                // Создание нового гражданина
                var newCitizen = new Citizen(0, 0, null)
                {
                    Age = 25, // Иммигранты обычно молодые и трудоспособные
                    IsMale = i % 2 == 0, // Чередование пола
                    Education = EducationLevel.School,
                    IsEmployed = false,
                    Health = 100f,
                    Happiness = 50f
                };

                citizens.Add(newCitizen);
                added++;

                // Попытка найти жилье
                if (residentialBuildings != null)
                {
                    foreach (var building in residentialBuildings)
                    {
                        if (building.HasVacancy && building.TryAddResident(newCitizen))
                        {
                            break;
                        }
                    }
                }
            }

            return added;
        }

        /// <summary>
        /// Удаление эмигрантов из города (приоритет безработным с низким счастьем)
        /// </summary>
        /// <param name="citizens">Список граждан города</param>
        /// <param name="count">Количество эмигрантов</param>
        /// <returns>Количество фактически удаленных граждан</returns>
        public int RemoveEmigrants(List<Citizen> citizens, int count)
        {
            if (citizens == null || citizens.Count == 0 || count <= 0)
                return 0;

            // Сортировка по приоритету: сначала безработные, потом по уровню счастья
            var candidatesToRemove = citizens
                .OrderBy(c => c.IsEmployed ? 1 : 0)
                .ThenBy(c => c.Happiness)
                .Take(Math.Min(count, citizens.Count))
                .ToList();

            int removed = 0;
            foreach (var citizen in candidatesToRemove)
            {
                // Освобождение жилья
                if (citizen.Home != null)
                {
                    citizen.Home.CurrentResidents.Remove(citizen);
                }

                citizens.Remove(citizen);
                removed++;
            }

            return removed;
        }

        /// <summary>
        /// Расчет уровня безработицы в городе
        /// </summary>
        /// <param name="citizens">Список граждан</param>
        /// <returns>Уровень безработицы (от 0.0 до 1.0)</returns>
        public float CalculateUnemploymentRate(List<Citizen> citizens)
        {
            if (citizens == null || citizens.Count == 0)
                return 0f;

            int workingAge = citizens.Count(c => c.Age >= 18 && c.Age <= 65);
            if (workingAge == 0)
                return 0f;

            int unemployed = citizens.Count(c => c.Age >= 18 && c.Age <= 65 && !c.IsEmployed);
            return (float)unemployed / workingAge;
        }

        /// <summary>
        /// Расчет количества свободного жилья
        /// </summary>
        /// <param name="residentialBuildings">Список жилых зданий</param>
        /// <returns>Количество свободных мест</returns>
        public int CalculateAvailableHousing(List<ResidentialBuilding> residentialBuildings)
        {
            if (residentialBuildings == null || residentialBuildings.Count == 0)
                return 0;

            int totalCapacity = residentialBuildings.Sum(b => b.Capacity);
            int occupied = residentialBuildings.Sum(b => b.CurrentResidents.Count);
            return totalCapacity - occupied;
        }
    }
}
