using System;
using System.Linq;
using Core.Enums;
using Core.Models.Base;
using Core.Models.Buildings;
using Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    /// <summary>
    /// Тесты для проверки функциональности финансовой системы города
    /// </summary>
    [TestClass]
    public class FinancialSystemTests
    {
        // Константы для тестовых значений
        private const decimal DEFAULT_BUDGET = 100000m;
        private const decimal TEST_INCOME = 5000m;
        private const decimal TEST_EXPENSE = 3000m;
        private const decimal SMALL_AMOUNT = 100m;
        private const decimal LARGE_AMOUNT = 150000m;
        private const decimal TAX_RATE_9_PERCENT = 0.09m;
        private const decimal TAX_RATE_12_PERCENT = 0.12m;
        private const decimal TAX_RATE_15_PERCENT = 0.15m;
        private const decimal BASE_VALUE_10000 = 10000m;
        private const decimal SUBSIDY_AMOUNT = 2000m;

        private FinancialSystem _financialSystem;

        /// <summary>
        /// Инициализация тестового окружения перед каждым тестом
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            _financialSystem = new FinancialSystem(DEFAULT_BUDGET);
        }

        /// <summary>
        /// Проверка корректной инициализации финансовой системы с начальным бюджетом
        /// </summary>
        [TestMethod]
        public void Constructor_WithInitialBudget_ShouldInitializeCorrectly()
        {
            decimal initialBudget = 50000m;

            var system = new FinancialSystem(initialBudget);

            Assert.AreEqual(initialBudget, system.CityBudget, "Начальный бюджет должен быть установлен корректно");
            Assert.IsNotNull(system.TaxRates, "Налоговые ставки должны быть инициализированы");
            Assert.IsNotNull(system.Expenses, "Расходы должны быть инициализированы");
            Assert.IsNotNull(system.Incomes, "Доходы должны быть инициализированы");
        }

        /// <summary>
        /// Проверка, что конструктор выбрасывает исключение при отрицательном начальном бюджете
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_WithNegativeBudget_ShouldThrowException()
        {
            var system = new FinancialSystem(-1000m);

        }

        /// <summary>
        /// Проверка инициализации налоговых ставок по умолчанию
        /// </summary>
        [TestMethod]
        public void Constructor_ShouldInitializeDefaultTaxRates()
        {
            var system = new FinancialSystem();

            Assert.AreEqual(TAX_RATE_9_PERCENT, system.TaxRates["Residential"], "Налоговая ставка для жилья должна быть 9%");
            Assert.AreEqual(TAX_RATE_12_PERCENT, system.TaxRates["Commercial"], "Налоговая ставка для коммерции должна быть 12%");
            Assert.AreEqual(0.11m, system.TaxRates["Industrial"], "Налоговая ставка для промышленности должна быть 11%");
        }

        /// <summary>
        /// Проверка добавления дохода в бюджет
        /// </summary>
        [TestMethod]
        public void AddIncome_WithValidAmount_ShouldIncreaseBudget()
        {
            decimal expectedBudget = DEFAULT_BUDGET + TEST_INCOME;

            _financialSystem.AddIncome(TEST_INCOME, "Налоги");

            Assert.AreEqual(expectedBudget, _financialSystem.CityBudget, "Бюджет должен увеличиться на сумму дохода");
        }

        /// <summary>
        /// Проверка, что доход учитывается в статистике по источникам
        /// </summary>
        [TestMethod]
        public void AddIncome_ShouldTrackIncomeBySource()
        {
            string source = "Налоги с жилья";

            _financialSystem.AddIncome(TEST_INCOME, source);

            Assert.IsTrue(_financialSystem.Incomes.ContainsKey(source), "Источник дохода должен быть записан");
            Assert.AreEqual(TEST_INCOME, _financialSystem.Incomes[source], "Сумма дохода должна быть корректной");
        }

        /// <summary>
        /// Проверка суммирования доходов из одного источника
        /// </summary>
        [TestMethod]
        public void AddIncome_MultipleTimes_ShouldSumUpBySource()
        {
            string source = "Налоги";
            decimal firstIncome = 1000m;
            decimal secondIncome = 2000m;
            decimal expectedTotal = firstIncome + secondIncome;

            _financialSystem.AddIncome(firstIncome, source);
            _financialSystem.AddIncome(secondIncome, source);

            Assert.AreEqual(expectedTotal, _financialSystem.Incomes[source], "Доходы из одного источника должны суммироваться");
        }

        /// <summary>
        /// Проверка, что отрицательный доход вызывает исключение
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddIncome_WithNegativeAmount_ShouldThrowException()
        {
            _financialSystem.AddIncome(-100m, "Тест");

        }

        /// <summary>
        /// Проверка, что пустой источник дохода вызывает исключение
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddIncome_WithEmptySource_ShouldThrowException()
        {
            _financialSystem.AddIncome(TEST_INCOME, "");

        }

        /// <summary>
        /// Проверка вызова события при добавлении дохода
        /// </summary>
        [TestMethod]
        public void AddIncome_ShouldTriggerOnIncomeAddedEvent()
        {
            bool eventTriggered = false;
            decimal receivedAmount = 0m;
            string receivedSource = "";

            _financialSystem.OnIncomeAdded += (amount, source) =>
            {
                eventTriggered = true;
                receivedAmount = amount;
                receivedSource = source;
            };
            _financialSystem.AddIncome(TEST_INCOME, "Тестовый доход");
            Assert.IsTrue(eventTriggered, "Событие OnIncomeAdded должно быть вызвано");
            Assert.AreEqual(TEST_INCOME, receivedAmount, "Сумма в событии должна совпадать");
            Assert.AreEqual("Тестовый доход", receivedSource, "Источник в событии должен совпадать");
        }

        /// <summary>
        /// Проверка добавления расхода при достаточном бюджете
        /// </summary>
        [TestMethod]
        public void AddExpense_WithSufficientBudget_ShouldDecreaseBudget()
        {
            decimal expectedBudget = DEFAULT_BUDGET - TEST_EXPENSE;

            bool result = _financialSystem.AddExpense(TEST_EXPENSE, "Образование");

            Assert.IsTrue(result, "Расход должен быть успешно проведен");
            Assert.AreEqual(expectedBudget, _financialSystem.CityBudget, "Бюджет должен уменьшиться на сумму расхода");
        }

        /// <summary>
        /// Проверка отклонения расхода при недостаточном бюджете
        /// </summary>
        [TestMethod]
        public void AddExpense_WithInsufficientBudget_ShouldReturnFalse()
        {
            decimal initialBudget = _financialSystem.CityBudget;

            bool result = _financialSystem.AddExpense(LARGE_AMOUNT, "Образование");

            Assert.IsFalse(result, "Расход должен быть отклонен при недостаточном бюджете");
            Assert.AreEqual(initialBudget, _financialSystem.CityBudget, "Бюджет не должен измениться");
        }

        /// <summary>
        /// Проверка учета расходов по категориям
        /// </summary>
        [TestMethod]
        public void AddExpense_ShouldTrackExpensesByCategory()
        {
            string category = "Здравоохранение";

            _financialSystem.AddExpense(TEST_EXPENSE, category);

            Assert.IsTrue(_financialSystem.Expenses.ContainsKey(category), "Категория расхода должна быть записана");
            Assert.AreEqual(TEST_EXPENSE, _financialSystem.Expenses[category], "Сумма расхода должна быть корректной");
        }

        /// <summary>
        /// Проверка суммирования расходов по одной категории
        /// </summary>
        [TestMethod]
        public void AddExpense_MultipleTimes_ShouldSumUpByCategory()
        {
            string category = "Полиция";
            decimal firstExpense = 1000m;
            decimal secondExpense = 1500m;
            decimal expectedTotal = firstExpense + secondExpense;

            _financialSystem.AddExpense(firstExpense, category);
            _financialSystem.AddExpense(secondExpense, category);

            Assert.AreEqual(expectedTotal, _financialSystem.Expenses[category], "Расходы по категории должны суммироваться");
        }

        /// <summary>
        /// Проверка, что отрицательный расход вызывает исключение
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddExpense_WithNegativeAmount_ShouldThrowException()
        {
            _financialSystem.AddExpense(-500m, "Тест");

        }

        /// <summary>
        /// Проверка вызова события при добавлении расхода
        /// </summary>
        [TestMethod]
        public void AddExpense_ShouldTriggerOnExpenseAddedEvent()
        {
            bool eventTriggered = false;
            decimal receivedAmount = 0m;
            string receivedCategory = "";

            _financialSystem.OnExpenseAdded += (amount, category) =>
            {
                eventTriggered = true;
                receivedAmount = amount;
                receivedCategory = category;
            };

            _financialSystem.AddExpense(TEST_EXPENSE, "Тестовая категория");

            Assert.IsTrue(eventTriggered, "Событие OnExpenseAdded должно быть вызвано");
            Assert.AreEqual(TEST_EXPENSE, receivedAmount, "Сумма в событии должна совпадать");
            Assert.AreEqual("Тестовая категория", receivedCategory, "Категория в событии должна совпадать");
        }

        /// <summary>
        /// Проверка расхода с отрицательным балансом
        /// </summary>
        [TestMethod]
        public void TryAddExpense_WithOverdraftAllowed_ShouldAllowNegativeBalance()
        {
            decimal largeExpense = DEFAULT_BUDGET + 10000m;
            decimal expectedBudget = DEFAULT_BUDGET - largeExpense;

            bool result = _financialSystem.TryAddExpense(largeExpense, "Срочные расходы", allowNegativeBalance: true);

            Assert.IsTrue(result, "Расход должен быть проведен с овердрафтом");
            Assert.AreEqual(expectedBudget, _financialSystem.CityBudget, "Баланс должен стать отрицательным");
            Assert.IsTrue(_financialSystem.CityBudget < 0, "Бюджет должен быть отрицательным");
        }

        /// <summary>
        /// Проверка события дефицита бюджета при отрицательном балансе
        /// </summary>
        [TestMethod]
        public void TryAddExpense_WithNegativeBalance_ShouldTriggerBudgetDeficitEvent()
        {
            bool deficitEventTriggered = false;
            decimal deficitBalance = 0m;

            _financialSystem.OnBudgetDeficit += (balance) =>
            {
                deficitEventTriggered = true;
                deficitBalance = balance;
            };

            decimal largeExpense = DEFAULT_BUDGET + 5000m;

            _financialSystem.TryAddExpense(largeExpense, "Срочные расходы", allowNegativeBalance: true);

            Assert.IsTrue(deficitEventTriggered, "Событие дефицита бюджета должно быть вызвано");
            Assert.IsTrue(deficitBalance < 0, "Баланс в событии должен быть отрицательным");
        }

        /// <summary>
        /// Проверка установки налоговой ставки
        /// </summary>
        [TestMethod]
        public void SetTaxRate_WithValidRate_ShouldUpdateTaxRate()
        {
            string taxType = "Residential";
            decimal newRate = TAX_RATE_15_PERCENT;

            _financialSystem.SetTaxRate(taxType, newRate);

            Assert.AreEqual(newRate, _financialSystem.TaxRates[taxType], "Налоговая ставка должна быть обновлена");
        }

        /// <summary>
        /// Проверка, что налоговая ставка больше 1 вызывает исключение
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SetTaxRate_WithRateAboveOne_ShouldThrowException()
        {
            _financialSystem.SetTaxRate("Commercial", 1.5m);

        }

        /// <summary>
        /// Проверка, что отрицательная налоговая ставка вызывает исключение
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SetTaxRate_WithNegativeRate_ShouldThrowException()
        {
            _financialSystem.SetTaxRate("Industrial", -0.1m);

        }

        /// <summary>
        /// Проверка получения налоговой ставки
        /// </summary>
        [TestMethod]
        public void GetTaxRate_ForExistingType_ShouldReturnCorrectRate()
        {
            string taxType = "Commercial";

            decimal rate = _financialSystem.GetTaxRate(taxType);

            Assert.AreEqual(TAX_RATE_12_PERCENT, rate, "Должна вернуться корректная налоговая ставка");
        }

        /// <summary>
        /// Проверка получения налоговой ставки для несуществующего типа
        /// </summary>
        [TestMethod]
        public void GetTaxRate_ForNonExistingType_ShouldReturnZero()
        {
            decimal rate = _financialSystem.GetTaxRate("НесуществующийТип");

            Assert.AreEqual(0m, rate, "Для несуществующего типа должна вернуться ставка 0");
        }

        /// <summary>
        /// Проверка вызова события при изменении налоговой ставки
        /// </summary>
        [TestMethod]
        public void SetTaxRate_ShouldTriggerOnTaxRateChangedEvent()
        {
            bool eventTriggered = false;
            string receivedTaxType = "";
            decimal receivedRate = 0m;

            _financialSystem.OnTaxRateChanged += (taxType, rate) =>
            {
                eventTriggered = true;
                receivedTaxType = taxType;
                receivedRate = rate;
            };

            _financialSystem.SetTaxRate("Residential", TAX_RATE_15_PERCENT);

            Assert.IsTrue(eventTriggered, "Событие изменения налоговой ставки должно быть вызвано");
            Assert.AreEqual("Residential", receivedTaxType, "Тип налога в событии должен совпадать");
            Assert.AreEqual(TAX_RATE_15_PERCENT, receivedRate, "Ставка в событии должна совпадать");
        }


        /// <summary>
        /// Проверка добавления субсидии
        /// </summary>
        [TestMethod]
        public void AddSubsidy_WithValidData_ShouldAddSubsidy()
        {
            string subsidyName = "Субсидия на образование";

            _financialSystem.AddSubsidy(subsidyName, SUBSIDY_AMOUNT);

            Assert.IsTrue(_financialSystem.Subsidies.ContainsKey(subsidyName), "Субсидия должна быть добавлена");
            Assert.AreEqual(SUBSIDY_AMOUNT, _financialSystem.Subsidies[subsidyName], "Сумма субсидии должна быть корректной");
        }

        /// <summary>
        /// Проверка обновления существующей субсидии
        /// </summary>
        [TestMethod]
        public void AddSubsidy_ExistingSubsidy_ShouldUpdateAmount()
        {
            string subsidyName = "Субсидия на здравоохранение";
            decimal newAmount = 3000m;

            _financialSystem.AddSubsidy(subsidyName, SUBSIDY_AMOUNT);

            _financialSystem.AddSubsidy(subsidyName, newAmount);

            Assert.AreEqual(newAmount, _financialSystem.Subsidies[subsidyName], "Сумма субсидии должна быть обновлена");
        }

        /// <summary>
        /// Проверка, что отрицательная субсидия вызывает исключение
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddSubsidy_WithNegativeAmount_ShouldThrowException()
        {
            _financialSystem.AddSubsidy("Тестовая субсидия", -1000m);

        }

        /// <summary>
        /// Проверка удаления субсидии
        /// </summary>
        [TestMethod]
        public void RemoveSubsidy_ExistingSubsidy_ShouldRemoveSuccessfully()
        {
            string subsidyName = "Временная субсидия";
            _financialSystem.AddSubsidy(subsidyName, SUBSIDY_AMOUNT);

            bool result = _financialSystem.RemoveSubsidy(subsidyName);

            Assert.IsTrue(result, "Субсидия должна быть успешно удалена");
            Assert.IsFalse(_financialSystem.Subsidies.ContainsKey(subsidyName), "Субсидия не должна присутствовать в списке");
        }

        /// <summary>
        /// Проверка удаления несуществующей субсидии
        /// </summary>
        [TestMethod]
        public void RemoveSubsidy_NonExistingSubsidy_ShouldReturnFalse()
        {
            bool result = _financialSystem.RemoveSubsidy("Несуществующая субсидия");

            Assert.IsFalse(result, "Удаление несуществующей субсидии должно вернуть false");
        }

        /// <summary>
        /// Проверка выплаты всех активных субсидий
        /// </summary>
        [TestMethod]
        public void PaySubsidies_WithMultipleSubsidies_ShouldPayAll()
        {
            _financialSystem.AddSubsidy("Субсидия 1", 1000m);
            _financialSystem.AddSubsidy("Субсидия 2", 1500m);
            _financialSystem.AddSubsidy("Субсидия 3", 2000m);

            decimal totalSubsidies = 4500m;
            decimal expectedBudget = DEFAULT_BUDGET - totalSubsidies;

            decimal totalPaid = _financialSystem.PaySubsidies();

            Assert.AreEqual(totalSubsidies, totalPaid, "Общая сумма выплат должна быть корректной");
            Assert.AreEqual(expectedBudget, _financialSystem.CityBudget, "Бюджет должен уменьшиться на сумму субсидий");
        }

        /// <summary>
        /// Проверка, что субсидии не выплачиваются при недостаточном бюджете
        /// </summary>
        [TestMethod]
        public void PaySubsidies_WithInsufficientBudget_ShouldPayPartially()
        {
            var smallBudgetSystem = new FinancialSystem(1000m);
            smallBudgetSystem.AddSubsidy("Большая субсидия", 5000m);

            decimal initialBudget = smallBudgetSystem.CityBudget;

            decimal totalPaid = smallBudgetSystem.PaySubsidies();

            Assert.AreEqual(0m, totalPaid, "Субсидия не должна быть выплачена при недостатке средств");
            Assert.AreEqual(initialBudget, smallBudgetSystem.CityBudget, "Бюджет не должен измениться");
        }

     
        /// <summary>
        /// Проверка генерации финансового отчета
        /// </summary>
        [TestMethod]
        public void GetFinancialReport_ShouldReturnCorrectReport()
        {
            _financialSystem.AddIncome(5000m, "Налоги");
            _financialSystem.AddExpense(2000m, "Образование");
            _financialSystem.AddSubsidy("Тестовая субсидия", 500m);

            var report = _financialSystem.GetFinancialReport();

            Assert.IsNotNull(report, "Отчет не должен быть null");
            Assert.AreEqual(_financialSystem.CityBudget, report.CurrentBudget, "Текущий бюджет в отчете должен совпадать");
            Assert.AreEqual(5000m, report.TotalIncome, "Общий доход должен быть корректным");
            Assert.AreEqual(2000m, report.TotalExpenses, "Общие расходы должны быть корректными");
            Assert.AreEqual(500m, report.TotalSubsidies, "Общие субсидии должны быть корректными");
            Assert.IsFalse(report.IsDeficit, "Не должно быть дефицита при положительном балансе");
        }

        /// <summary>
        /// Проверка расчета профицита/дефицита за период
        /// </summary>
        [TestMethod]
        public void GetFinancialReport_ShouldCalculatePeriodBalance()
        {
            _financialSystem.AddIncome(10000m, "Налоги");
            _financialSystem.AddExpense(3000m, "Расходы");

            decimal expectedPeriodBalance = 10000m - 3000m;

            var report = _financialSystem.GetFinancialReport();

            Assert.AreEqual(expectedPeriodBalance, report.PeriodBalance, "Баланс периода должен быть корректным");
        }

        /// <summary>
        /// Проверка определения дефицита в отчете
        /// </summary>
        [TestMethod]
        public void GetFinancialReport_WithNegativeBalance_ShouldIndicateDeficit()
        {
            _financialSystem.TryAddExpense(DEFAULT_BUDGET + 1000m, "Большой расход", allowNegativeBalance: true);

            var report = _financialSystem.GetFinancialReport();

            Assert.IsTrue(report.IsDeficit, "Должен быть указан дефицит при отрицательном балансе");
            Assert.IsTrue(report.CurrentBudget < 0, "Текущий бюджет должен быть отрицательным");
        }

        /// <summary>
        /// Проверка получения истории транзакций
        /// </summary>
        [TestMethod]
        public void GetRecentTransactions_ShouldReturnCorrectCount()
        {
            _financialSystem.AddIncome(1000m, "Доход 1");
            _financialSystem.AddIncome(2000m, "Доход 2");
            _financialSystem.AddExpense(500m, "Расход 1");

            var transactions = _financialSystem.GetRecentTransactions(2);

            Assert.AreEqual(2, transactions.Count, "Должно вернуться 2 последние транзакции");
        }

        /// <summary>
        /// Проверка сортировки транзакций по времени
        /// </summary>
        [TestMethod]
        public void GetRecentTransactions_ShouldReturnInDescendingOrder()
        {
            _financialSystem.AddIncome(1000m, "Первая");
            System.Threading.Thread.Sleep(10); 
            _financialSystem.AddIncome(2000m, "Вторая");
            System.Threading.Thread.Sleep(10);
            _financialSystem.AddIncome(3000m, "Третья");

            var transactions = _financialSystem.GetRecentTransactions(3);

            Assert.AreEqual("Третья", transactions[0].Description, "Транзакции должны быть отсортированы от новых к старым");
            Assert.AreEqual("Вторая", transactions[1].Description);
            Assert.AreEqual("Первая", transactions[2].Description);
        }

        /// <summary>
        /// Проверка сброса статистики периода
        /// </summary>
        [TestMethod]
        public void ResetPeriodStatistics_ShouldClearExpensesAndIncomes()
        {
            _financialSystem.AddIncome(5000m, "Доходы");
            _financialSystem.AddExpense(2000m, "Расходы");

            _financialSystem.ResetPeriodStatistics();

            var report = _financialSystem.GetFinancialReport();
            Assert.AreEqual(0m, report.TotalIncome, "Доходы должны быть сброшены");
            Assert.AreEqual(0m, report.TotalExpenses, "Расходы должны быть сброшены");
        }

        /// <summary>
        /// Проверка вызова события при сбросе статистики
        /// </summary>
        [TestMethod]
        public void ResetPeriodStatistics_ShouldTriggerOnPeriodResetEvent()
        {
            bool eventTriggered = false;
            _financialSystem.OnPeriodReset += () => eventTriggered = true;

            _financialSystem.ResetPeriodStatistics();

            Assert.IsTrue(eventTriggered, "Событие сброса периода должно быть вызвано");
        }

        /// <summary>
        /// Проверка добавления транзакций в историю
        /// </summary>
        [TestMethod]
        public void TransactionHistory_ShouldRecordAllTransactions()
        {
            _financialSystem.AddIncome(1000m, "Доход");
            _financialSystem.AddExpense(500m, "Расход");

            Assert.AreEqual(2, _financialSystem.TransactionHistory.Count, "Должно быть записано 2 транзакции");
        }

        /// <summary>
        /// Проверка типов транзакций в истории
        /// </summary>
        [TestMethod]
        public void TransactionHistory_ShouldRecordCorrectTypes()
        {
            _financialSystem.AddIncome(1000m, "Тестовый доход");
            _financialSystem.AddExpense(500m, "Тестовый расход");

            var incomeTransaction = _financialSystem.TransactionHistory.FirstOrDefault(t => t.Type == TransactionType.Income);
            var expenseTransaction = _financialSystem.TransactionHistory.FirstOrDefault(t => t.Type == TransactionType.Expense);

            Assert.IsNotNull(incomeTransaction, "Транзакция дохода должна быть записана");
            Assert.IsNotNull(expenseTransaction, "Транзакция расхода должна быть записана");
            Assert.AreEqual(1000m, incomeTransaction.Amount);
            Assert.AreEqual(500m, expenseTransaction.Amount);
        }

      
    }

}
