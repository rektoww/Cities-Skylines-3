using System;
using System.Collections.Generic;

namespace Core.Finance
{
    /// <summary>
    /// Категории финансовых операций и статей бюджета.
    /// </summary>
    public enum FinanceCategory
    {
        /// <summary>Доходы.</summary>
        Revenue,
        /// <summary>Расходы.</summary>
        Expense,
        /// <summary>Обслуживание долга.</summary>
        DebtService,
        /// <summary>Инфраструктура.</summary>
        Infrastructure,
        /// <summary>Здравоохранение.</summary>
        Healthcare,
        /// <summary>Транспорт.</summary>
        Transport,
        /// <summary>Образование.</summary>
        Education,
        /// <summary>Социальная поддержка.</summary>
        Welfare,
        /// <summary>Общественная безопасность.</summary>
        PublicSafety,
        /// <summary>Администрирование.</summary>
        Administration,
        /// <summary>Прочее.</summary>
        Other
    }
}

namespace Core.Models.Finance
{
    using Core.Finance;

    /// <summary>
    /// Финансовая транзакция: перевод средств между счетами или запись о доходе/расходе.
    /// </summary>
    public class Transaction
    {
        /// <summary>Уникальный идентификатор транзакции.</summary>
        public string TransactionId { get; set; } = Guid.NewGuid().ToString();
        /// <summary>Дата транзакции.</summary>
        public DateTime Date { get; set; }
        /// <summary>Сумма (положительная для доходов, отрицательная для расходов).</summary>
        public decimal Amount { get; set; }
        /// <summary>Категория транзакции.</summary>
        public FinanceCategory Category { get; set; }
        /// <summary>Описание назначения платежа.</summary>
        public string Description { get; set; }
        /// <summary>Идентификатор счета-источника.</summary>
        public string FromAccountId { get; set; }
        /// <summary>Идентификатор счета-получателя.</summary>
        public string ToAccountId { get; set; }
        /// <summary>Признак, что транзакция является доходом.</summary>
        public bool IsRevenue => Amount > 0;
    }

    /// <summary>
    /// Главная книга за период с набором транзакций.
    /// </summary>
    public class Ledger
    {
        /// <summary>Уникальный идентификатор книги.</summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();
        /// <summary>Начало отчетного периода.</summary>
        public DateTime PeriodStart { get; set; }
        /// <summary>Конец отчетного периода.</summary>
        public DateTime PeriodEnd { get; set; }
        /// <summary>Список транзакций периода.</summary>
        public IList<Transaction> Transactions { get; set; } = new List<Transaction>();
    }

    /// <summary>
    /// Статья бюджета (доходная или расходная).
    /// </summary>
    public class BudgetItem
    {
        /// <summary>Идентификатор статьи.</summary>
        public string Id { get; set; }
        /// <summary>Категория статьи.</summary>
        public FinanceCategory Category { get; set; }
        /// <summary>Описание статьи.</summary>
        public string Description { get; set; }
        /// <summary>Сумма по статье.</summary>
        public decimal Amount { get; set; }
        /// <summary>Признак, что статья относится к доходам.</summary>
        public bool IsRevenue { get; set; }
    }

    /// <summary>
    /// Бюджет города на год, включает агрегированные итоги.
    /// </summary>
    public class Budget
    {
        /// <summary>Год бюджета.</summary>
        public int Year { get; set; }
        /// <summary>Статус утверждения бюджета.</summary>
        public bool IsApproved { get; set; }
        /// <summary>Список статей бюджета.</summary>
        public IList<BudgetItem> Items { get; set; } = new List<BudgetItem>();
        /// <summary>Итоговые доходы.</summary>
        public decimal TotalRevenue { get; set; }
        /// <summary>Итоговые расходы.</summary>
        public decimal TotalExpenses { get; set; }
        /// <summary>Профицит/дефицит (доходы - расходы).</summary>
        public decimal SurplusOrDeficit { get; set; }
    }

    /// <summary>
    /// Налоговая политика города (ставки налогов и взносов).
    /// </summary>
    public class TaxPolicy
    {
        /// <summary>Ставка налога на прибыль организаций (0..1).</summary>
        public decimal CorporateTaxRate { get; set; }
        /// <summary>Ставка НДФЛ (0..1).</summary>
        public decimal PersonalIncomeTaxRate { get; set; }
        /// <summary>Ставка НДС (0..1).</summary>
        public decimal ValueAddedTaxRate { get; set; }
        /// <summary>Ставка налога на имущество (0..1).</summary>
        public decimal PropertyTaxRate { get; set; }
        /// <summary>Ставка социальных взносов (0..1).</summary>
        public decimal SocialContributionsRate { get; set; }
    }

    /// <summary>
    /// Банковский счет.
    /// </summary>
    public class Account
    {
        /// <summary>Идентификатор счета.</summary>
        public string AccountId { get; set; }
        /// <summary>Владелец счета.</summary>
        public string OwnerId { get; set; }
        /// <summary>Баланс.</summary>
        public decimal Balance { get; set; }
        /// <summary>Валюта счета.</summary>
        public string Currency { get; set; } = "CRD";
    }

    /// <summary>
    /// Кредит.
    /// </summary>
    public class Loan
    {
        /// <summary>Идентификатор кредита.</summary>
        public string LoanId { get; set; } = Guid.NewGuid().ToString();
        /// <summary>Идентификатор заемщика.</summary>
        public string BorrowerId { get; set; }
        /// <summary>Сумма основного долга.</summary>
        public decimal Principal { get; set; }
        /// <summary>Годовая процентная ставка.</summary>
        public double AnnualInterestRate { get; set; }
        /// <summary>Дата начала.</summary>
        public DateTime StartDate { get; set; }
        /// <summary>Дата погашения.</summary>
        public DateTime MaturityDate { get; set; }
        /// <summary>Остаток основного долга.</summary>
        public decimal OutstandingPrincipal { get; set; }
    }

    /// <summary>
    /// Банковский вклад.
    /// </summary>
    public class Deposit
    {
        /// <summary>Идентификатор вклада.</summary>
        public string DepositId { get; set; } = Guid.NewGuid().ToString();
        /// <summary>Владелец вклада.</summary>
        public string OwnerId { get; set; }
        /// <summary>Сумма вклада.</summary>
        public decimal Principal { get; set; }
        /// <summary>Годовая ставка по вкладу.</summary>
        public double AnnualInterestRate { get; set; }
        /// <summary>Дата открытия.</summary>
        public DateTime StartDate { get; set; }
        /// <summary>Дата окончания.</summary>
        public DateTime MaturityDate { get; set; }
    }

    /// <summary>
    /// Инструмент долга (кредит, облигация и т.п.).
    /// </summary>
    public class DebtInstrument
    {
        /// <summary>Идентификатор инструмента.</summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();
        /// <summary>Тип инструмента ("Loan", "Bond" и т.д.).</summary>
        public string Kind { get; set; }
        /// <summary>Номинал (основной долг).</summary>
        public decimal Principal { get; set; }
        /// <summary>Годовая процентная ставка.</summary>
        public double AnnualInterestRate { get; set; }
        /// <summary>Дата выпуска.</summary>
        public DateTime IssueDate { get; set; }
        /// <summary>Дата погашения.</summary>
        public DateTime MaturityDate { get; set; }
        /// <summary>Остаток долга.</summary>
        public decimal OutstandingPrincipal { get; set; }
    }

    /// <summary>
    /// Реестр долгов города.
    /// </summary>
    public class DebtLedger
    {
        /// <summary>Список долговых инструментов.</summary>
        public IList<DebtInstrument> Instruments { get; set; } = new List<DebtInstrument>();
        /// <summary>Совокупный непогашенный долг.</summary>
        public decimal TotalOutstandingDebt { get; set; }
    }

    /// <summary>
    /// Программа субсидий и дотаций.
    /// </summary>
    public class SubsidyProgram
    {
        /// <summary>Идентификатор программы.</summary>
        public string ProgramId { get; set; } = Guid.NewGuid().ToString();
        /// <summary>Название программы.</summary>
        public string Name { get; set; }
        /// <summary>Целевой сектор.</summary>
        public string TargetSector { get; set; }
        /// <summary>Бюджет программы.</summary>
        public decimal Budget { get; set; }
        /// <summary>Дата начала.</summary>
        public DateTime StartDate { get; set; }
        /// <summary>Дата окончания (если есть).</summary>
        public DateTime? EndDate { get; set; }
        /// <summary>Распределения по получателям: получатель → сумма.</summary>
        public IDictionary<string, decimal> Allocations { get; set; } = new Dictionary<string, decimal>();
    }

    /// <summary>
    /// Ключевые экономические показатели города.
    /// </summary>
    public class EconomicIndicators
    {
        /// <summary>Валовой внутренний продукт.</summary>
        public decimal GDP { get; set; }
        /// <summary>Уровень инфляции (0..1 или %/100).</summary>
        public double InflationRate { get; set; }
        /// <summary>Уровень безработицы (0..1 или %/100).</summary>
        public double UnemploymentRate { get; set; }
        /// <summary>Дата фиксации показателей.</summary>
        public DateTime AsOf { get; set; }
    }

    /// <summary>
    /// Банк, через который выполняются муниципальные операции.
    /// </summary>
    public class Bank
    {
        /// <summary>Название банка.</summary>
        public string Name { get; set; }
        /// <summary>Счета клиентов.</summary>
        public IList<Account> Accounts { get; set; } = new List<Account>();
        /// <summary>Выданные кредиты.</summary>
        public IList<Loan> Loans { get; set; } = new List<Loan>();
        /// <summary>Открытые вклады.</summary>
        public IList<Deposit> Deposits { get; set; } = new List<Deposit>();
        /// <summary>Норма резервирования.</summary>
        public double ReserveRatio { get; set; }
    }
}

namespace Core.Interfaces
{
    using System;
    using System.Collections.Generic;
    using Core.Models.Finance;

    /// <summary>
    /// Провайдер доходов из других модулей (Коммерция, Население и т.п.).
    /// </summary>
    public interface IIncomeProvider
    {
        /// <summary>Человекочитаемое имя источника.</summary>
        string SourceName { get; }
        /// <summary>Прогнозные доходы за период.</summary>
        IEnumerable<Transaction> GetProjectedRevenues(DateTime periodStart, DateTime periodEnd);
        /// <summary>Фактические доходы за период.</summary>
        IEnumerable<Transaction> GetActualRevenues(DateTime periodStart, DateTime periodEnd);
    }

    /// <summary>
    /// Провайдер расходов из других модулей (Строительство, Транспорт, ...).
    /// </summary>
    public interface IExpenseProvider
    {
        /// <summary>Человекочитаемое имя источника.</summary>
        string SourceName { get; }
        /// <summary>Прогнозные расходы за период.</summary>
        IEnumerable<Transaction> GetProjectedExpenses(DateTime periodStart, DateTime periodEnd);
        /// <summary>Фактические расходы за период.</summary>
        IEnumerable<Transaction> GetActualExpenses(DateTime periodStart, DateTime periodEnd);
    }

    /// <summary>
    /// Интерфейс для предоставления данных модулю «Внешние связи».
    /// </summary>
    public interface IExternalRelationsFinanceProvider
    {
        /// <summary>Текущий бюджет.</summary>
        Budget GetCurrentBudget();
        /// <summary>Список активных кредитов.</summary>
        IEnumerable<Loan> GetOutstandingLoans();
        /// <summary>Совокупный непогашенный долг.</summary>
        decimal GetTotalOutstandingDebt();
    }

    /// <summary>
    /// Сервис формирования и исполнения бюджета.
    /// </summary>
    public interface IBudgetService
    {
        /// <summary>Получить текущий бюджет.</summary>
        Budget GetCurrentBudget();
        /// <summary>Спланировать годовой бюджет.</summary>
        void PlanAnnualBudget(int year, IEnumerable<IIncomeProvider> incomeProviders, IEnumerable<IExpenseProvider> expenseProviders);
        /// <summary>Утвердить бюджет.</summary>
        void ApproveBudget(Budget budget);
        /// <summary>Исполнить бюджет за месяц.</summary>
        void ExecuteMonthlyBudget(int year, int month, IEnumerable<IIncomeProvider> incomeProviders, IEnumerable<IExpenseProvider> expenseProviders);
    }

    /// <summary>
    /// Сервис налогообложения.
    /// </summary>
    public interface ITaxationService
    {
        /// <summary>Задать налоговую политику.</summary>
        void SetPolicy(TaxPolicy policy);
        /// <summary>Получить текущую политику.</summary>
        TaxPolicy GetPolicy();
        /// <summary>Рассчитать налог для бизнеса.</summary>
        decimal CalculateBusinessTax(decimal businessRevenue);
        /// <summary>Рассчитать НДФЛ.</summary>
        decimal CalculatePersonalIncomeTax(decimal personalIncome);
        /// <summary>Рассчитать налог на имущество.</summary>
        decimal CalculatePropertyTax(decimal propertyValue);
    }

    /// <summary>
    /// Банковский сервис: платежи, кредиты, депозиты.
    /// </summary>
    public interface IBankingService
    {
        /// <summary>Муниципальный счет.</summary>
        Account GetMunicipalAccount();
        /// <summary>Запросить кредит.</summary>
        Loan RequestLoan(string borrowerId, decimal amount, double annualInterestRate, TimeSpan term);
        /// <summary>Открыть депозит.</summary>
        Deposit OpenDeposit(string ownerId, decimal amount, double annualInterestRate, DateTime maturityDate);
        /// <summary>Провести входящие/исходящие платежи.</summary>
        IEnumerable<Transaction> ProcessPayments(IEnumerable<Transaction> outgoingPayments, IEnumerable<Transaction> incomingPayments);
    }

    /// <summary>
    /// Сервис управления государственным долгом.
    /// </summary>
    public interface IDebtService
    {
        /// <summary>Зарегистрировать новый долг.</summary>
        void RegisterDebt(DebtInstrument instrument);
        /// <summary>Зарегистрировать платеж по долгу.</summary>
        void RegisterDebtPayment(string instrumentId, decimal amount);
        /// <summary>Получить совокупный остаток долга.</summary>
        decimal GetOutstandingDebt();
        /// <summary>Получить расписание долгов.</summary>
        IEnumerable<DebtInstrument> GetDebtSchedule();
    }

    /// <summary>
    /// Сервис субсидий и дотаций.
    /// </summary>
    public interface ISubsidyService
    {
        /// <summary>Создать программу.</summary>
        void CreateProgram(SubsidyProgram program);
        /// <summary>Получить активные программы.</summary>
        IEnumerable<SubsidyProgram> GetActivePrograms();
        /// <summary>Распределить средства.</summary>
        void Allocate(string programId, string recipientId, decimal amount);
    }

    /// <summary>
    /// Сервис ключевых экономических индикаторов.
    /// </summary>
    public interface IEconomicIndicatorService
    {
        /// <summary>Получить текущие индикаторы.</summary>
        EconomicIndicators GetCurrent();
        /// <summary>Обновить пакет индикаторов.</summary>
        void Update(EconomicIndicators indicators);
        /// <summary>Зафиксировать ВВП.</summary>
        void RecordGDP(decimal gdp, DateTime asOf);
        /// <summary>Зафиксировать уровень безработицы.</summary>
        void RecordUnemploymentRate(double unemploymentRate, DateTime asOf);
        /// <summary>Зафиксировать уровень инфляции.</summary>
        void RecordInflationRate(double inflationRate, DateTime asOf);
    }

    /// <summary>
    /// Фасад финансовой системы.
    /// </summary>
    public interface IFinancialSystem
    {
        /// <summary>Сервис бюджета.</summary>
        IBudgetService BudgetService { get; }
        /// <summary>Сервис налогообложения.</summary>
        ITaxationService TaxationService { get; }
        /// <summary>Банковский сервис.</summary>
        IBankingService BankingService { get; }
        /// <summary>Сервис долгов.</summary>
        IDebtService DebtService { get; }
        /// <summary>Сервис субсидий.</summary>
        ISubsidyService SubsidyService { get; }
        /// <summary>Сервис экономических индикаторов.</summary>
        IEconomicIndicatorService EconomicIndicatorService { get; }

        /// <summary>Подключить провайдеров доходов/расходов.</summary>
        void IntegrateProviders(IEnumerable<IIncomeProvider> incomeProviders, IEnumerable<IExpenseProvider> expenseProviders);
        /// <summary>Открыть доступ модулю внешних связей.</summary>
        void ExposeToExternalRelations(IExternalRelationsFinanceProvider externalGateway);
    }
}
