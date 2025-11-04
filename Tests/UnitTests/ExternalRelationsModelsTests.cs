using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Core.Enums;
using Core.External;
using Core.Models.External;

namespace Tests.UnitTests
{
    [TestClass]
    public sealed class ExternalRelationsModelsTests
    {
        [TestMethod]
        public void TradeDeal_Assign_Values()
        {
            // Подготовка: формируем торговую сделку с основными параметрами
            var deal = new TradeDeal
            {
                PartnerCountry = "Freedonia",
                Resource = ResourceType.Iron,
                Quantity = 500m,
                UnitPrice = 12.5m,
                Direction = TradeDirection.Export,
                Terms = new TradeTerms { Incoterm = "FOB", PaymentDueDays = 30, CustomsDutyRate = 0.05m, Currency = "CRD" },
                Status = DealStatus.Draft
            };

            // Проверка: идентификатор создан, поля заполнены, условия заданы
            Assert.IsFalse(string.IsNullOrWhiteSpace(deal.DealId));
            Assert.AreEqual("Freedonia", deal.PartnerCountry);
            Assert.AreEqual(TradeDirection.Export, deal.Direction);
            Assert.IsNotNull(deal.Terms);
        }

        [TestMethod]
        public void Shipment_Status_Can_Be_Set()
        {
            // Подготовка: создаем отправление в статусе «Запланировано»
            var shipment = new Shipment
            {
                DealId = "D1",
                Quantity = 100m,
                Mode = TransportMode.Sea,
                RouteId = "R1",
                Status = ShipmentStatus.Planned,
                DepartureDate = DateTime.Today
            };

            // Изменение статуса отправления
            shipment.Status = ShipmentStatus.Dispatched;

            // Проверка: статус изменился
            Assert.AreEqual(ShipmentStatus.Dispatched, shipment.Status);
        }

        [TestMethod]
        public void LogisticsRoute_Properties_Assignable()
        {
            // Подготовка: описываем маршрут перевозки
            var route = new LogisticsRoute
            {
                Origin = "HarborA",
                Destination = "PortB",
                BorderType = BorderType.SeaPort,
                DistanceKm = 1200,
                EstimatedTime = TimeSpan.FromHours(36),
                Mode = TransportMode.Sea
            };

            // Проверка: идентификатор присвоен, ключевые поля заданы
            Assert.IsFalse(string.IsNullOrWhiteSpace(route.RouteId));
            Assert.AreEqual("HarborA", route.Origin);
            Assert.AreEqual("PortB", route.Destination);
        }

        [TestMethod]
        public void CustomsRegulation_Collections_NotNull()
        {
            // Подготовка: создаем набор таможенных регуляций
            var reg = new CustomsRegulation();

            // Проверка: коллекции инициализированы
            Assert.IsNotNull(reg.Tariffs);
            Assert.IsNotNull(reg.Quotas);
        }

        [TestMethod]
        public void MigrationRequest_Default_Status_Submitted()
        {
            // Подготовка: формируем заявку на иммиграцию
            var req = new MigrationRequest { PersonId = "P1", Type = MigrationType.Immigration, Reason = "Work", SubmittedAt = DateTime.UtcNow };

            // Проверка: идентификатор создан и статус по умолчанию «Подано»
            Assert.IsFalse(string.IsNullOrWhiteSpace(req.RequestId));
            Assert.AreEqual(MigrationStatus.Submitted, req.Status);
        }
    }
}
