using System;
using System.Collections.Generic;
using System.Linq;
using Core.Enums;
using Core.Models.Base;

namespace Core.Services
{
    /// <summary>
    /// Финансовая система города
    /// Управляет бюджетом, налогами, расходами и субсидиями
    /// </summary>
    public class FinancialSystem
    {

        /// <summary>
        /// Текущий баланс городского бюджета
        /// </summary>
        private decimal _cityBudget;

        /// <summary>
        /// Словарь налоговых ставок по типам зданий
        /// Ключ - тип налога, Значение - ставка от 0 до 1 (например, 0.09 = 9%)
        /// </summary>
        private readonly Dictionary<string, decimal> _taxRates;

        /// <summary>
        /// Накопленные расходы по категориям за текущий период
        /// Словарь используется для отчетности и анализа
        /// </summary>
        private readonly Dictionary<string, decimal> _expenses;

        /// <summary>
        /// Накопленные доходы по источникам за текущий период
        /// Словарь используется для отчетности и анализа
        /// </summary>
        private readonly Dictionary<string, decimal> _incomes;

        /// <summary>
        /// Словарь активных субсидий
        /// Ключ - название субсидии, Значение - сумма выплаты за период
        /// </summary>
        private readonly Dictionary<string, decimal> _subsidies;

        /// <summary>
        /// История финансовых транзакций
        /// Хранит последние N операций для аудита
        /// </summary>
        private readonly List<FinancialTransaction> _transactionHistory;

        /// <summary>
        /// Максимальное количество транзакций в истории
        /// </summary>
        private const int MaxTransactionHistory = 1000;

        /// <summary>
        /// Текущий баланс бюджета 
        /// </summary>
        public decimal CityBudget => _cityBudget;

        /// <summary>
        /// Налоговые ставки 
        /// </summary>
        public IReadOnlyDictionary<string, decimal> TaxRates => _taxRates;

        /// <summary>
        /// Расходы по категориям 
        /// </summary>
        public IReadOnlyDictionary<string, decimal> Expenses => _expenses;

        /// <summary>
        /// Доходы по источникам 
        /// </summary>
        public IReadOnlyDictionary<string, decimal> Incomes => _incomes;

        /// <summary>
        /// Активные субсидии 
        /// </summary>
        public IReadOnlyDictionary<string, decimal> Subsidies => _subsidies;

        /// <summary>
        /// История транзакций 
        /// </summary>
        public IReadOnlyList<FinancialTransaction> TransactionHistory => _transactionHistory;


        /// <summary>
        /// Создание новой финансовой системы города
        /// </summary>
        /// <param name="initialBudget">Начальный бюджет города (по умолчанию 100,000)</param>
        public FinancialSystem(decimal initialBudget = 100000m)
        {
            // Проверка на корректность начального бюджета
            if (initialBudget < 0)
                throw new ArgumentException("Начальный бюджет не может быть отрицательным", nameof(initialBudget));

            _cityBudget = initialBudget;
            _taxRates = new Dictionary<string, decimal>();
            _expenses = new Dictionary<string, decimal>();
            _incomes = new Dictionary<string, decimal>();
            _subsidies = new Dictionary<string, decimal>();
            _transactionHistory = new List<FinancialTransaction>();

            // Инициализация базовых значений
            InitializeDefaultTaxRates();
            InitializeDefaultExpenseCategories();
            InitializeDefaultIncomeCategories();
        }


        /// <summary>
        /// Добавление дохода в городской бюджет
        /// </summary>
        /// <param name="amount">Сумма дохода</param>
        /// <param name="source">Источник дохода (например, "Налоги с жилых зданий")</param>
        /// <exception cref="ArgumentException">Если сумма отрицательная</exception>
        public void AddIncome(decimal amount, string source)
        {
            // Валидация входных данных
            if (amount < 0)
                throw new ArgumentException("Доход не может быть отрицательным", nameof(amount));

            if (string.IsNullOrWhiteSpace(source))
                throw new ArgumentException("Источник дохода должен быть указан", nameof(source));

            // Увеличение бюджета
            _cityBudget += amount;

            // Учет дохода в статистике по категориям
            if (_incomes.ContainsKey(source))
                _incomes[source] += amount;
            else
                _incomes[source] = amount;

            // Запись транзакции в историю
            AddTransaction(TransactionType.Income, amount, source);

            // Уведомление о получении дохода
            OnIncomeAdded?.Invoke(amount, source);
        }

        /// <summary>
        /// Рассчет и добавление налогового дохода от здания
        /// </summary>
        /// <param name="building">Здание, с которого собираются налоги</param>
        /// <param name="taxCategory">Категория налога (Residential, Commercial, Industrial)</param>
        /// <param name="baseValue">Базовая стоимость для расчета налога</param>
        /// <returns>Сумма собранного налога</returns>
        public decimal CollectTaxFromBuilding(Building building, string taxCategory, decimal baseValue)
        {
            if (building == null)
                throw new ArgumentNullException(nameof(building));

            if (baseValue < 0)
                throw new ArgumentException("Базовая стоимость не может быть отрицательной", nameof(baseValue));

            // Проверка, работоспособно ли здание
            if (!building.IsOperational)
                return 0m; // Налоги не собираются с неработающих зданий

            // Получение налоговой ставки для данной категории
            if (!_taxRates.TryGetValue(taxCategory, out decimal taxRate))
                taxRate = 0.1m; // Ставка по умолчанию 10%

            // Рассчет суммы налога
            decimal taxAmount = baseValue * taxRate;

            // Добавление дохода
            AddIncome(taxAmount, $"Налог: {taxCategory} - {building.Name}");

            return taxAmount;
        }

        /// <summary>
        /// Добавление расхода из городского бюджета
        /// </summary>
        /// <param name="amount">Сумма расхода</param>
        /// <param name="category">Категория расхода (например, "Образование")</param>
        /// <returns>true, если расход успешно проведен; false, если недостаточно средств</returns>
        public bool AddExpense(decimal amount, string category)
        {
            // Валидация входных данных
            if (amount < 0)
                throw new ArgumentException("Расход не может быть отрицательным", nameof(amount));

            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("Категория расхода должна быть указана", nameof(category));

            // Проверка, достаточно ли средств
            if (_cityBudget < amount)
                return false; // Недостаточно средств

            // Списывание средств с бюджета
            _cityBudget -= amount;

            // Учет расхода в статистике по категориям
            if (_expenses.ContainsKey(category))
                _expenses[category] += amount;
            else
                _expenses[category] = amount;

            // Запись транзакции в историю
            AddTransaction(TransactionType.Expense, amount, category);

            // Уведомление о расходе
            OnExpenseAdded?.Invoke(amount, category);

            return true;
        }

        /// <summary>
        /// Попытка провести расход с возможностью отрицательного баланса
        /// </summary>
        /// <param name="amount">Сумма расхода</param>
        /// <param name="category">Категория расхода</param>
        /// <param name="allowNegativeBalance">Разрешить отрицательный баланс</param>
        /// <returns>true, если расход проведен</returns>
        public bool TryAddExpense(decimal amount, string category, bool allowNegativeBalance)
        {
            if (amount < 0)
                throw new ArgumentException("Расход не может быть отрицательным", nameof(amount));

            // Если отрицательный баланс не разрешен, проверка наличия средств
            if (!allowNegativeBalance && _cityBudget < amount)
                return false;

            // Списание средств
            _cityBudget -= amount;

            // Учет расхода
            if (_expenses.ContainsKey(category))
                _expenses[category] += amount;
            else
                _expenses[category] = amount;

            // Запись транзакции
            AddTransaction(TransactionType.Expense, amount, category);

            // Если баланс стал отрицательным, уведомление об этом
            if (_cityBudget < 0)
                OnBudgetDeficit?.Invoke(_cityBudget);

            OnExpenseAdded?.Invoke(amount, category);
            return true;
        }

     
        /// <summary>
        /// Устанавка налоговой ставки для категории
        /// </summary>
        /// <param name="taxType">Тип налога (Residential, Commercial, Industrial)</param>
        /// <param name="rate">Ставка от 0 до 1 (например, 0.09 для 9%)</param>
        /// <exception cref="ArgumentException">Если ставка вне диапазона [0, 1]</exception>
        public void SetTaxRate(string taxType, decimal rate)
        {
            if (string.IsNullOrWhiteSpace(taxType))
                throw new ArgumentException("Тип налога должен быть указан", nameof(taxType));

            if (rate < 0 || rate > 1)
                throw new ArgumentException("Налоговая ставка должна быть от 0 до 1 (0% - 100%)", nameof(rate));

            _taxRates[taxType] = rate;
            OnTaxRateChanged?.Invoke(taxType, rate);
        }

        /// <summary>
        /// Получение налоговой ставки для категории
        /// </summary>
        /// <param name="taxType">Тип налога</param>
        /// <returns>Налоговая ставка или 0, если не установлена</returns>
        public decimal GetTaxRate(string taxType)
        {
            return _taxRates.TryGetValue(taxType, out decimal rate) ? rate : 0m;
        }

        /// <summary>
        /// Добавление новой субсидии или изменение существующей
        /// </summary>
        /// <param name="subsidyName">Название субсидии</param>
        /// <param name="amount">Сумма выплаты за период</param>
        public void AddSubsidy(string subsidyName, decimal amount)
        {
            if (string.IsNullOrWhiteSpace(subsidyName))
                throw new ArgumentException("Название субсидии должно быть указано", nameof(subsidyName));

            if (amount < 0)
                throw new ArgumentException("Сумма субсидии не может быть отрицательной", nameof(amount));

            _subsidies[subsidyName] = amount;
            OnSubsidyAdded?.Invoke(subsidyName, amount);
        }

        /// <summary>
        /// Удаление субсидии
        /// </summary>
        /// <param name="subsidyName">Название субсидии для удаления</param>
        /// <returns>true, если субсидия была удалена</returns>
        public bool RemoveSubsidy(string subsidyName)
        {
            if (_subsidies.Remove(subsidyName))
            {
                OnSubsidyRemoved?.Invoke(subsidyName);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Выплата всех активных субсидий
        /// Должна вызываться периодически (например, каждый игровой месяц)
        /// </summary>
        /// <returns>Общая сумма выплаченных субсидий</returns>
        public decimal PaySubsidies()
        {
            decimal totalPaid = 0m;

            foreach (var subsidy in _subsidies)
            {
                // Попытка провести расход на каждую субсидию
                if (AddExpense(subsidy.Value, $"Субсидия: {subsidy.Key}"))
                {
                    totalPaid += subsidy.Value;
                }
            }

            return totalPaid;
        }


        /// <summary>
        /// Генерация финансового отчета за текущий период
        /// </summary>
        /// <returns>Объект с финансовым отчетом</returns>
        public FinancialReport GetFinancialReport()
        {
            return new FinancialReport
            {
                CurrentBudget = _cityBudget,
                TotalIncome = CalculateTotalIncome(),
                TotalExpenses = CalculateTotalExpenses(),
                TotalSubsidies = CalculateTotalSubsidies(),
                TaxRates = new Dictionary<string, decimal>(_taxRates),
                ExpensesByCategory = new Dictionary<string, decimal>(_expenses),
                IncomesBySource = new Dictionary<string, decimal>(_incomes),
                NetBalance = _cityBudget,
                IsDeficit = _cityBudget < 0
            };
        }

        /// <summary>
        /// Получение последних N транзакций из истории
        /// </summary>
        /// <param name="count">Количество транзакций</param>
        /// <returns>Список транзакций</returns>
        public List<FinancialTransaction> GetRecentTransactions(int count)
        {
            if (count <= 0)
                return new List<FinancialTransaction>();

            return _transactionHistory
                .OrderByDescending(t => t.Timestamp)
                .Take(count)
                .ToList();
        }

        /// <summary>
        /// Сброс статистики доходов и расходов за период
        /// Вызов в начале нового отчетного периода
        /// </summary>
        public void ResetPeriodStatistics()
        {
            _expenses.Clear();
            _incomes.Clear();
            InitializeDefaultExpenseCategories();
            InitializeDefaultIncomeCategories();
            OnPeriodReset?.Invoke();
        }


        /// <summary>
        /// Установка налоговых ставок по умолчанию
        /// </summary>
        private void InitializeDefaultTaxRates()
        {
            _taxRates["Residential"] = 0.09m;    // 9% налог на жилую недвижимость
            _taxRates["Commercial"] = 0.12m;     // 12% налог на коммерческую недвижимость
            _taxRates["Industrial"] = 0.11m;     // 11% налог на промышленность
        }

        /// <summary>
        /// Инициализация категории расходов нулевыми значениями
        /// </summary>
        private void InitializeDefaultExpenseCategories()
        {
            _expenses["Education"] = 0m;         // Образование
            _expenses["Healthcare"] = 0m;        // Здравоохранение
            _expenses["Police"] = 0m;            // Полиция
            _expenses["FireDepartment"] = 0m;    // Пожарная служба
            _expenses["Utilities"] = 0m;         // ЖКХ (электричество, вода, газ)
            _expenses["Infrastructure"] = 0m;    // Инфраструктура
            _expenses["Maintenance"] = 0m;       // Обслуживание зданий
        }

        /// <summary>
        /// Инициализация категории доходов нулевыми значениями
        /// </summary>
        private void InitializeDefaultIncomeCategories()
        {
            _incomes["TaxResidential"] = 0m;     // Налоги с жилья
            _incomes["TaxCommercial"] = 0m;      // Налоги с коммерции
            _incomes["TaxIndustrial"] = 0m;      // Налоги с промышленности
            _incomes["Services"] = 0m;           // Оплата услуг
        }

     

        /// <summary>
        /// Рассчет общей суммы расходов за период
        /// </summary>
        private decimal CalculateTotalExpenses()
        {
            return _expenses.Values.Sum();
        }

        /// <summary>
        /// Рассчет общей суммы доходов за период
        /// </summary>
        private decimal CalculateTotalIncome()
        {
            return _incomes.Values.Sum();
        }

        /// <summary>
        /// Рассчет общей суммы субсидий
        /// </summary>
        private decimal CalculateTotalSubsidies()
        {
            return _subsidies.Values.Sum();
        }

        /// <summary>
        /// Добавление транзакций в историю с ограничением размера
        /// </summary>
        private void AddTransaction(TransactionType type, decimal amount, string description)
        {
            var transaction = new FinancialTransaction
            {
                Type = type,
                Amount = amount,
                Description = description,
                Timestamp = DateTime.Now
            };

            _transactionHistory.Add(transaction);

            // Ограничение размера истории
            if (_transactionHistory.Count > MaxTransactionHistory)
            {
                _transactionHistory.RemoveAt(0); // Удаление самой старой транзакции
            }
        }

        /// <summary>
        /// Событие при добавлении дохода
        /// Параметры: сумма, источник
        /// </summary>
        public event Action<decimal, string> OnIncomeAdded;

        /// <summary>
        /// Событие при добавлении расхода
        /// Параметры: сумма, категория
        /// </summary>
        public event Action<decimal, string> OnExpenseAdded;

        /// <summary>
        /// Событие при изменении налоговой ставки
        /// Параметры: тип налога, новая ставка
        /// </summary>
        public event Action<string, decimal> OnTaxRateChanged;

        /// <summary>
        /// Событие при добавлении субсидии
        /// Параметры: название, сумма
        /// </summary>
        public event Action<string, decimal> OnSubsidyAdded;

        /// <summary>
        /// Событие при удалении субсидии
        /// Параметры: название
        /// </summary>
        public event Action<string> OnSubsidyRemoved;

        /// <summary>
        /// Событие при возникновении дефицита бюджета (отрицательного баланса)
        /// Параметры: текущий баланс
        /// </summary>
        public event Action<decimal> OnBudgetDeficit;

        /// <summary>
        /// Событие при сбросе статистики периода
        /// </summary>
        public event Action OnPeriodReset;

     
    }

   
    /// <summary>
    /// Финансовый отчет города за период
    /// Содержит сводную информацию о доходах, расходах и балансе
    /// </summary>
    public class FinancialReport
    {
        /// <summary>
        /// Текущий баланс бюджета
        /// </summary>
        public decimal CurrentBudget { get; set; }

        /// <summary>
        /// Общая сумма доходов за период
        /// </summary>
        public decimal TotalIncome { get; set; }

        /// <summary>
        /// Общая сумма расходов за период
        /// </summary>
        public decimal TotalExpenses { get; set; }

        /// <summary>
        /// Общая сумма субсидий
        /// </summary>
        public decimal TotalSubsidies { get; set; }

        /// <summary>
        /// Текущие налоговые ставки
        /// </summary>
        public Dictionary<string, decimal> TaxRates { get; set; }

        /// <summary>
        /// Расходы по категориям
        /// </summary>
        public Dictionary<string, decimal> ExpensesByCategory { get; set; }

        /// <summary>
        /// Доходы по источникам
        /// </summary>
        public Dictionary<string, decimal> IncomesBySource { get; set; }

        /// <summary>
        /// Чистый баланс за период (доходы - расходы)
        /// </summary>
        public decimal NetBalance { get; set; }

        /// <summary>
        /// Признак дефицита бюджета
        /// </summary>
        public bool IsDeficit { get; set; }

        /// <summary>
        /// Доступный бюджет (текущий бюджет минус планируемые расходы)
        /// </summary>
        public decimal AvailableBudget => CurrentBudget;

        /// <summary>
        /// Профицит/дефицит за период
        /// </summary>
        public decimal PeriodBalance => TotalIncome - TotalExpenses;
    }

    /// <summary>
    /// Запись о финансовой транзакции
    /// </summary>
    public class FinancialTransaction
    {
        /// <summary>
        /// Тип транзакции (доход или расход)
        /// </summary>
        public TransactionType Type { get; set; }

        /// <summary>
        /// Сумма транзакции
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Описание транзакции
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Временная метка транзакции
        /// </summary>
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Тип финансовой транзакции
    /// </summary>
    public enum TransactionType
    {
        /// <summary>
        /// Доход в бюджет
        /// </summary>
        Income,

        /// <summary>
        /// Расход из бюджета
        /// </summary>
        Expense
    }

 
}
