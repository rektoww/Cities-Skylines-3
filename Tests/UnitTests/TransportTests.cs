using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core.Models.Map;
using Core.Models.Mobs;
using Core.Models.Base;
using Core.Models.Vehicles;
using Core.Models.Buildings;
using System.Collections.Generic;

namespace Tests.UnitTests
{
    [TestClass]
    public sealed class TransportTests
    {
        [TestMethod]
        public void AddWaitingCitizen_SetsTargetAndPosition()
        {
            // Arrange
            var map = new GameMap(5, 1);
            var station = new TestTransitStation();
            station.X = 2; station.Y = 0;
            map.Tiles[2, 0].Building = station;

            var citizen = new Citizen(0, 0, map);

            // Act
            station.AddWaitingCitizen(citizen);

            // Assert
            Assert.AreEqual(station, citizen.TargetTransitStation);
            Assert.AreEqual(2, citizen.X);
            Assert.AreEqual(0, citizen.Y);
            Assert.IsTrue(station.WaitingCitizens.Contains(citizen));
        }

        [TestMethod]
        public void Bus_ArrivingAtStation_BoardsWaitingCitizen()
        {
            // Arrange
            var map = new GameMap(5, 1);
            var station = new TestTransitStation();
            station.X = 2; station.Y = 0;
            map.Tiles[2, 0].Building = station;

            var citizen = new Citizen(0, 0, map);
            station.AddWaitingCitizen(citizen);

            // Создаём маршрут, где первая точка — станция
            var route = new List<Tile> { map.Tiles[2, 0] };

            // Автобус расположен на той же плитке станции
            var bus = new Bus(2, 0, map, route);

            // Pre-check
            Assert.IsTrue(station.WaitingCitizens.Contains(citizen));
            Assert.AreEqual(0, bus.Passengers.Count);

            // Act - вызов Move должен обработать посадку (dwell time = 1 по умолчанию)
            bus.Move();

            // Assert - пассажир должен оказаться в автобусе и быть снят со списка ожидания
            Assert.IsTrue(bus.Passengers.Contains(citizen));
            Assert.AreEqual(bus, citizen.CurrentTransport);
            Assert.IsFalse(station.WaitingCitizens.Contains(citizen));
        }

        [TestMethod]
        public void Bus_AtStation_DisembarksPassenger_WhenDestinationReached()
        {
            // Arrange
            var map = new GameMap(5, 1);

            // Создаём здание-цель на станции
            var targetBuilding = new TestBuilding();
            targetBuilding.X = 2; targetBuilding.Y = 0;
            map.Tiles[2, 0].Building = targetBuilding;

            var route = new List<Tile> { map.Tiles[2, 0] };

            var bus = new Bus(2, 0, map, route);

            // Пассажир, уже в автобусе, с целью — targetBuilding
            var passenger = new Citizen(2, 0, map);
            passenger.DestinationBuilding = targetBuilding;

            // Сажаем пассажира вручную через TryBoard
            bool boarded = bus.TryBoard(passenger);
            Assert.IsTrue(boarded);
            Assert.IsTrue(bus.Passengers.Contains(passenger));
            Assert.AreEqual(bus, passenger.CurrentTransport);

            // Act - автобус на станции, вызов Move должен инициировать высадку
            bus.Move();

            // Assert - пассажир высажен и связь с транспортом сброшена
            Assert.IsFalse(bus.Passengers.Contains(passenger));
            Assert.IsNull(passenger.CurrentTransport);
            Assert.AreEqual(bus.X, passenger.X);
            Assert.AreEqual(bus.Y, passenger.Y);
        }

        // Вспомогательные тестовые классы
        private class TestTransitStation : TransitStation
        {
            public override void OnBuildingPlaced() { }
        }

        private class TestBuilding : Building
        {
            public TestBuilding() { Width = 1; Height = 1; }
            public override void OnBuildingPlaced() { }
        }
    }
}