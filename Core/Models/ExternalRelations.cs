using System;
using System.Collections.Generic;

namespace Core.External
{
    /// <summary>Направление торговой операции.</summary>
    public enum TradeDirection { Import, Export }
    /// <summary>Статус торговой сделки.</summary>
    public enum DealStatus { Draft, Approved, InTransit, Completed, Cancelled }
    /// <summary>Режим транспорта.</summary>
    public enum TransportMode { Land, Sea, Air }
    /// <summary>Статус отправления.</summary>
    public enum ShipmentStatus { Planned, Dispatched, Delayed, Arrived, Cancelled }
    /// <summary>Тип миграции.</summary>
    public enum MigrationType { Immigration, Emigration }
    /// <summary>Статус запроса на миграцию.</summary>
    public enum MigrationStatus { Submitted, Approved, Rejected }
    /// <summary>Тип пограничного пункта.</summary>
    public enum BorderType { Land, SeaPort, Airport }
}

namespace Core.Models.External
{
    using Core.External;
    using Core.Enums; 

    /// <summary>
    /// Условия международной торговли (инкотермс, валюта, сроки оплаты).
    /// </summary>
    public class TradeTerms
    {
        /// <summary>Условие поставки Incoterms (например, FOB, CIF).</summary>
        public string Incoterm { get; set; }
        /// <summary>Отсрочка платежа в днях.</summary>
        public int PaymentDueDays { get; set; }
        /// <summary>Ставка таможенной пошлины (0..1).</summary>
        public decimal CustomsDutyRate { get; set; }
        /// <summary>Валюта сделки.</summary>
        public string Currency { get; set; } = "CRD";
    }

    /// <summary>
    /// Международная торговая сделка (импорт/экспорт).
    /// </summary>
    public class TradeDeal
    {
        /// <summary>Идентификатор сделки.</summary>
        public string DealId { get; set; } = Guid.NewGuid().ToString();
        /// <summary>Страна-партнер.</summary>
        public string PartnerCountry { get; set; }
        /// <summary>Ресурс/товар.</summary>
        public ResourceType Resource { get; set; }
        /// <summary>Количество (объем).</summary>
        public decimal Quantity { get; set; }
        /// <summary>Цена за единицу.</summary>
        public decimal UnitPrice { get; set; }
        /// <summary>Направление сделки (импорт/экспорт).</summary>
        public TradeDirection Direction { get; set; }
        /// <summary>Условия сделки.</summary>
        public TradeTerms Terms { get; set; }
        /// <summary>Текущий статус сделки.</summary>
        public DealStatus Status { get; set; }
        /// <summary>Ожидаемая дата поставки.</summary>
        public DateTime? ExpectedDeliveryDate { get; set; }
        /// <summary>Идентификаторы связанных отправлений.</summary>
        public IList<string> ShipmentIds { get; set; } = new List<string>();
    }

    /// <summary>
    /// Отправление в рамках торговой сделки.
    /// </summary>
    public class Shipment
    {
        /// <summary>Идентификатор отправления.</summary>
        public string ShipmentId { get; set; } = Guid.NewGuid().ToString();
        /// <summary>Идентификатор сделки.</summary>
        public string DealId { get; set; }
        /// <summary>Отгружаемое количество.</summary>
        public decimal Quantity { get; set; }
        /// <summary>Вид транспорта.</summary>
        public TransportMode Mode { get; set; }
        /// <summary>Идентификатор маршрута.</summary>
        public string RouteId { get; set; }
        /// <summary>Статус отправления.</summary>
        public ShipmentStatus Status { get; set; }
        /// <summary>Дата отправки.</summary>
        public DateTime? DepartureDate { get; set; }
        /// <summary>Дата прибытия.</summary>
        public DateTime? ArrivalDate { get; set; }
        /// <summary>Стоимость логистики.</summary>
        public decimal LogisticsCost { get; set; }
    }

    /// <summary>
    /// Логистический маршрут между точками.
    /// </summary>
    public class LogisticsRoute
    {
        /// <summary>Идентификатор маршрута.</summary>
        public string RouteId { get; set; } = Guid.NewGuid().ToString();
        /// <summary>Пункт отправления.</summary>
        public string Origin { get; set; }
        /// <summary>Пункт назначения.</summary>
        public string Destination { get; set; }
        /// <summary>Тип пограничного перехода.</summary>
        public BorderType BorderType { get; set; }
        /// <summary>Дистанция, км.</summary>
        public double DistanceKm { get; set; }
        /// <summary>Оценка времени в пути.</summary>
        public TimeSpan EstimatedTime { get; set; }
        /// <summary>Список КПП на маршруте.</summary>
        public IList<string> BorderCheckpointIds { get; set; } = new List<string>();
        /// <summary>Режим транспорта.</summary>
        public TransportMode Mode { get; set; }
    }

    /// <summary>
    /// Таможенная пошлина для ресурса и направления торговли.
    /// </summary>
    public class CustomsTariff
    {
        /// <summary>Ресурс.</summary>
        public ResourceType Resource { get; set; }
        /// <summary>Применяется к импорту или экспорту.</summary>
        public TradeDirection AppliesTo { get; set; }
        /// <summary>Ставка пошлины (0..1).</summary>
        public decimal Rate { get; set; }
        /// <summary>Дата начала действия.</summary>
        public DateTime AppliesFrom { get; set; }
    }

    /// <summary>
    /// Квота на импорт/экспорт.
    /// </summary>
    public class Quota
    {
        /// <summary>Ресурс.</summary>
        public ResourceType Resource { get; set; }
        /// <summary>Направление.</summary>
        public TradeDirection Direction { get; set; }
        /// <summary>Лимит в месяц.</summary>
        public decimal LimitPerMonth { get; set; }
    }

    /// <summary>
    /// Набор таможенных регуляций (пошлины, квоты).
    /// </summary>
    public class CustomsRegulation
    {
        /// <summary>Список пошлин.</summary>
        public IList<CustomsTariff> Tariffs { get; set; } = new List<CustomsTariff>();
        /// <summary>Список квот.</summary>
        public IList<Quota> Quotas { get; set; } = new List<Quota>();
    }

    /// <summary>
    /// Политика миграции (визы, лимиты, критерии приема).
    /// </summary>
    public class MigrationPolicy
    {
        /// <summary>Требуется ли виза.</summary>
        public bool VisaRequired { get; set; }
        /// <summary>Максимум иммигрантов в месяц.</summary>
        public int MaxImmigrantsPerMonth { get; set; }
        /// <summary>Максимум эмигрантов в месяц.</summary>
        public int MaxEmigrantsPerMonth { get; set; }
        /// <summary>Порог баллов (обучение/навыки и т.п.).</summary>
        public int PointsThreshold { get; set; }
    }

    /// <summary>
    /// Заявка на иммиграцию/эмиграцию.
    /// </summary>
    public class MigrationRequest
    {
        /// <summary>Идентификатор заявки.</summary>
        public string RequestId { get; set; } = Guid.NewGuid().ToString();
        /// <summary>Идентификатор гражданина.</summary>
        public string PersonId { get; set; }
        /// <summary>Тип миграции.</summary>
        public MigrationType Type { get; set; }
        /// <summary>Причина переезда.</summary>
        public string Reason { get; set; }
        /// <summary>Статус заявки.</summary>
        public MigrationStatus Status { get; set; }
        /// <summary>Дата подачи.</summary>
        public DateTime SubmittedAt { get; set; }
        /// <summary>Дата решения (если есть).</summary>
        public DateTime? DecisionAt { get; set; }
    }

    /// <summary>
    /// Пограничный пункт пропуска.
    /// </summary>
    public class BorderCheckpoint
    {
        /// <summary>Идентификатор КПП.</summary>
        public string CheckpointId { get; set; } = Guid.NewGuid().ToString();
        /// <summary>Название КПП.</summary>
        public string Name { get; set; }
        /// <summary>Тип КПП.</summary>
        public BorderType Type { get; set; }
        /// <summary>Пропускная способность в день.</summary>
        public int CapacityPerDay { get; set; }
        /// <summary>Страна расположения.</summary>
        public string Country { get; set; }
    }

    /// <summary>
    /// Курс валюты к базовой валюте города.
    /// </summary>
    public class ForexRate
    {
        /// <summary>Базовая валюта.</summary>
        public string BaseCurrency { get; set; } = "CRD";
        /// <summary>Котируемая валюта.</summary>
        public string QuoteCurrency { get; set; }
        /// <summary>Курс.</summary>
        public decimal Rate { get; set; }
        /// <summary>Момент времени, на который установлен курс.</summary>
        public DateTime AsOf { get; set; }
    }

    /// <summary>
    /// Торговый баланс за период.
    /// </summary>
    public class TradeBalance
    {
        /// <summary>Начало периода.</summary>
        public DateTime PeriodStart { get; set; }
        /// <summary>Конец периода.</summary>
        public DateTime PeriodEnd { get; set; }
        /// <summary>Суммарная стоимость экспорта.</summary>
        public decimal TotalExportsValue { get; set; }
        /// <summary>Суммарная стоимость импорта.</summary>
        public decimal TotalImportsValue { get; set; }
        /// <summary>Сальдо торгового баланса.</summary>
        public decimal Balance => TotalExportsValue - TotalImportsValue;
    }
}

namespace Core.Interfaces
{
    using System;
    using System.Collections.Generic;
    using Core.External;
    using Core.Enums;
    using Core.Models.External;

    /// <summary>
    /// Поставщик товаров/ресурсов из внутренних модулей города.
    /// </summary>
    public interface IGoodsProvider
    {
        /// <summary>Зарезервировать объем ресурса до указанной даты.</summary>
        bool ReserveGoods(ResourceType resource, decimal quantity, DateTime until);
        /// <summary>Освободить ранее зарезервированный объем.</summary>
        void ReleaseGoods(ResourceType resource, decimal quantity);
        /// <summary>Получить доступный объем.</summary>
        decimal GetAvailable(ResourceType resource);
    }

    /// <summary>
    /// Сервис населения для учета миграции.
    /// </summary>
    public interface IPopulationService
    {
        /// <summary>Текущее население.</summary>
        int GetCurrentPopulation();
        /// <summary>Зарегистрировать иммиграцию.</summary>
        void RegisterImmigration(int count);
        /// <summary>Зарегистрировать эмиграцию.</summary>
        void RegisterEmigration(int count);
    }

    /// <summary>
    /// Сервис международной торговли.
    /// </summary>
    public interface ITradeService
    {
        /// <summary>Создать сделку.</summary>
        TradeDeal CreateDeal(TradeDeal deal);
        /// <summary>Утвердить сделку.</summary>
        void ApproveDeal(string dealId);
        /// <summary>Отменить сделку.</summary>
        void CancelDeal(string dealId);
        /// <summary>Спланировать отправление по сделке.</summary>
        Shipment PlanShipment(string dealId, decimal quantity, TransportMode mode, string routeId);
        /// <summary>Получить торговый баланс за период.</summary>
        TradeBalance GetTradeBalance(DateTime from, DateTime to);
        /// <summary>Получить список сделок.</summary>
        IEnumerable<TradeDeal> GetDeals();
    }

    /// <summary>
    /// Сервис логистики и маршрутизации.
    /// </summary>
    public interface ILogisticsService
    {
        /// <summary>Спланировать маршрут.</summary>
        LogisticsRoute PlanRoute(string origin, string destination, TransportMode mode);
        /// <summary>Запланировать отправление.</summary>
        void ScheduleShipment(string shipmentId, DateTime departure);
        /// <summary>Получить статус отправления.</summary>
        ShipmentStatus GetShipmentStatus(string shipmentId);
    }

    /// <summary>
    /// Сервис таможенного регулирования.
    /// </summary>
    public interface ICustomsService
    {
        /// <summary>Рассчитать пошлину.</summary>
        decimal CalculateDuty(ResourceType resource, TradeDirection direction, decimal declaredValue);
        /// <summary>Проверить соблюдение квоты.</summary>
        bool ValidateQuota(ResourceType resource, TradeDirection direction, decimal quantity);
        /// <summary>Получить действующие регуляции.</summary>
        CustomsRegulation GetRegulation();
    }

    /// <summary>
    /// Сервис миграции населения.
    /// </summary>
    public interface IMigrationService
    {
        /// <summary>Задать политику.</summary>
        void SetPolicy(MigrationPolicy policy);
        /// <summary>Получить политику.</summary>
        MigrationPolicy GetPolicy();
        /// <summary>Подать заявку.</summary>
        MigrationRequest Submit(MigrationRequest request);
        /// <summary>Одобрить заявку.</summary>
        void Approve(string requestId);
        /// <summary>Отклонить заявку.</summary>
        void Reject(string requestId, string reason);
    }

    /// <summary>
    /// Сервис валютного курса.
    /// </summary>
    public interface IForexService
    {
        /// <summary>Получить курс для валюты.</summary>
        ForexRate GetRate(string quoteCurrency);
        /// <summary>Обновить курс.</summary>
        void UpdateRate(ForexRate rate);
    }

    /// <summary>
    /// Фасад модуля «Внешние связи».
    /// </summary>
    public interface IExternalRelationsSystem
    {
        /// <summary>Подсистема торговли.</summary>
        ITradeService Trade { get; }
        /// <summary>Подсистема логистики.</summary>
        ILogisticsService Logistics { get; }
        /// <summary>Подсистема таможни.</summary>
        ICustomsService Customs { get; }
        /// <summary>Подсистема миграции.</summary>
        IMigrationService Migration { get; }
        /// <summary>Подсистема валютного курса.</summary>
        IForexService Forex { get; }

        /// <summary>Подключить поставщиков товаров.</summary>
        void IntegrateGoodsProviders(IEnumerable<IGoodsProvider> providers);
        /// <summary>Подключить сервис населения.</summary>
        void ConnectPopulation(IPopulationService populationService);
        /// <summary>Подключить финансовую систему.</summary>
        void ConnectFinance(IFinancialSystem financialSystem);
    }
}
