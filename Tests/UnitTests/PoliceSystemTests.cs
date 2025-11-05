using Core.Enums;
using Core.Models.Map;
using Core.Models.Police;
using Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.UnitTests
{
    /// <summary>
    /// Тесты для полицейской системы
    /// </summary>
    [TestClass]
    public class PoliceSystemTests
    {
        private GameMap _map;
        private PoliceService _policeService;

        /// <summary>
        /// Инициализация тестового окружения перед каждым тестом
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            _map = new GameMap(50, 50);
            _policeService = new PoliceService(_map);
        }

        /// <summary>
        /// Проверяет корректность инициализации преступления
        /// </summary>
        [TestMethod]
        public void Crime_Creation_InitializesCorrectly()
        {
            var crime = new Crime(1, CrimeType.Theft, 10, 10, 100);

            Assert.AreEqual(1, crime.Id);
            Assert.AreEqual(CrimeType.Theft, crime.Type);
            Assert.AreEqual(10, crime.LocationX);
            Assert.AreEqual(10, crime.LocationY);
            Assert.IsFalse(crime.IsSolved);
            Assert.IsFalse(crime.PoliceDispatched);
            Assert.AreEqual(4, crime.SeverityLevel); // Мелкая кража имеет степень тяжести 4
        }

        /// <summary>
        /// Проверяет различные уровни серьезности для разных типов преступлений
        /// </summary>
        [TestMethod]
        public void Crime_DifferentTypes_HaveDifferentSeverity()
        {
            var disturbance = new Crime(1, CrimeType.Disturbance, 0, 0, 0);
            var assault = new Crime(2, CrimeType.Assault, 0, 0, 0);

            Assert.IsTrue(assault.SeverityLevel > disturbance.SeverityLevel);
        }

        /// <summary>
        /// Проверяет доступность офицера для новых заданий
        /// </summary>
        [TestMethod]
        public void PoliceOfficer_IsAvailable_ReturnsTrueWhenNoAssignment()
        {
            var station = new PoliceStation();
            station.X = 10;
            station.Y = 10;
            var officer = new PoliceOfficer(10, 10, _map, "Bob Johnson", station);

            var available = officer.IsAvailable();

            Assert.IsTrue(available);
        }

        /// <summary>
        /// Проверяет корректность инициализации полицейского участка
        /// </summary>
        [TestMethod]
        public void PoliceStation_Creation_InitializesCorrectly()
        {
            var station = new PoliceStation();

            Assert.AreEqual(3, station.Width);
            Assert.AreEqual(3, station.Height);
            Assert.AreEqual(30, station.CoverageRadius);
            Assert.AreEqual(0, station.Officers.Count);
            Assert.AreEqual(0, station.PatrolCars.Count);
            Assert.AreEqual(10, station.MaxOfficers);
            Assert.AreEqual(3, station.MaxPatrolCars);
        }

        /// <summary>
        /// Проверяет ограничение максимальной вместимости участка при найме офицеров
        /// </summary>
        [TestMethod]
        public void PoliceStation_HireOfficer_FailsWhenAtMaxCapacity()
        {
            var station = new PoliceStation();
            station.X = 20;
            station.Y = 20;

            // Нанимаем максимум офицеров
            for (int i = 0; i < station.MaxOfficers; i++)
            {
                var officer = new PoliceOfficer(20, 20, _map, $"Officer {i}", station);
                station.HireOfficer(officer);
            }

            var extraOfficer = new PoliceOfficer(20, 20, _map, "Extra Officer", station);

            var result = station.HireOfficer(extraOfficer);

            Assert.IsFalse(result);
            Assert.AreEqual(station.MaxOfficers, station.Officers.Count);
        }

        /// <summary>
        /// Проверяет определение нахождения локации в зоне покрытия участка
        /// </summary>
        [TestMethod]
        public void PoliceStation_IsInCoverageArea_ReturnsTrueForNearbyLocation()
        {
            var station = new PoliceStation();
            station.X = 20;
            station.Y = 20;

            var inRange = station.IsInCoverageArea(25, 25);

            Assert.IsTrue(inRange); // Расстояние = 10, радиус = 30
        }

        /// <summary>
        /// Проверяет корректность создания полицейского автомобиля
        /// </summary>
        [TestMethod]
        public void PoliceCar_Creation_InitializesCorrectly()
        {
            var station = new PoliceStation();
            station.X = 10;
            station.Y = 10;

            var car = new PoliceCar(10, 10, _map, station);

            Assert.AreEqual(10, car.X);
            Assert.AreEqual(10, car.Y);
            Assert.AreEqual(11f, car.Speed);
            Assert.IsFalse(car.IsOnPatrol);
            Assert.IsFalse(car.IsRespondingToCall);
            Assert.AreEqual(0, car.Officers.Count);
        }

        /// <summary>
        /// Проверяет добавление офицера в полицейский автомобиль
        /// </summary>
        [TestMethod]
        public void PoliceCar_AddOfficer_AddsOfficerToCar()
        {
            var station = new PoliceStation();
            station.X = 10;
            station.Y = 10;
            var car = new PoliceCar(10, 10, _map, station);
            var officer = new PoliceOfficer(10, 10, _map, "Test Officer", station);

            var result = car.AddOfficer(officer);

            Assert.IsTrue(result);
            Assert.AreEqual(1, car.Officers.Count);
        }

        /// <summary>
        /// Проверяет доступность полицейского автомобиля для заданий
        /// </summary>
        [TestMethod]
        public void PoliceCar_IsAvailable_ReturnsTrueWithOfficers()
        {
            var station = new PoliceStation();
            station.X = 10;
            station.Y = 10;
            var car = new PoliceCar(10, 10, _map, station);
            var officer = new PoliceOfficer(10, 10, _map, "Test Officer", station);
            car.AddOfficer(officer);

            var available = car.IsAvailable();

            Assert.IsTrue(available);
        }

        /// <summary>
        /// Проверяет получение списка активных преступлений
        /// </summary>
        [TestMethod]
        public void PoliceService_GetActiveCrimes_ReturnsAllActiveCrimes()
        {
            _policeService.CreateCrime(CrimeType.Theft, 5, 5);
            _policeService.CreateCrime(CrimeType.Robbery, 10, 10);

            var crimes = _policeService.GetActiveCrimes();

            Assert.AreEqual(2, crimes.Count);
        }

        /// <summary>
        /// Проверяет расчет уровня преступности
        /// </summary>
        [TestMethod]
        public void PoliceService_GetCrimeLevel_ReturnsValidValue()
        {
            for (int i = 0; i < 5; i++)
            {
                _policeService.CreateCrime(CrimeType.Theft, i, i);
            }

            var crimeLevel = _policeService.GetCrimeLevel();

            Assert.IsTrue(crimeLevel >= 0);
            Assert.IsTrue(crimeLevel <= 100);
        }
    }
}
