using Core.Enums;
using Core.Models.Map;
using Core.Models.Police;

namespace Core.Services
{
    /// <summary>
    /// MarkinaVS
    /// Сервис для управления полицейскими службами города.
    /// Управляет преступлениями, участками и координирует работу полиции.
    /// </summary>
    public class PoliceService
    {
        private GameMap _map;
        private List<Crime> _activeCrimes;
        private List<PoliceStation> _policeStations;
        private int _nextCrimeId;
        private int _currentGameTime;
        private Random _random;

        public float CrimeGenerationRate { get; set; }
        public int TotalCrimesCommitted { get; private set; }
        public int TotalCrimesSolved { get; private set; }

        /// <summary>
        /// Конструктор сервиса полиции
        /// </summary>
        public PoliceService(GameMap map)
        {
            _map = map;
            _activeCrimes = new List<Crime>();
            _policeStations = new List<PoliceStation>();
            _nextCrimeId = 1;
            _currentGameTime = 0;
            _random = new Random();
            CrimeGenerationRate = 0.01f; // 1% шанс на тик
            TotalCrimesCommitted = 0;
            TotalCrimesSolved = 0;
        }

        /// <summary>
        /// Регистрирует полицейский участок в системе
        /// </summary>
        public void RegisterPoliceStation(PoliceStation station)
        {
            if (!_policeStations.Contains(station))
            {
                _policeStations.Add(station);
            }
        }

        /// <summary>
        /// Удаляет полицейский участок из системы
        /// </summary>
        public void UnregisterPoliceStation(PoliceStation station)
        {
            _policeStations.Remove(station);
        }

        /// <summary>
        /// Обновление системы полиции (вызывается каждый игровой тик)
        /// </summary>
        public void Update()
        {
            _currentGameTime++;

            // Генерация новых преступлений
            TryGenerateCrime();

            // Отправка полиции на нераскрытые преступления
            DispatchPoliceToActiveCrimes();

            // Очистка раскрытых преступлений
            CleanupSolvedCrimes();
        }

        /// <summary>
        /// Пытается сгенерировать случайное преступление
        /// </summary>
        private void TryGenerateCrime()
        {
            if (_random.NextDouble() < CrimeGenerationRate)
            {
                GenerateRandomCrime();
            }
        }

        /// <summary>
        /// Генерирует случайное преступление на карте
        /// </summary>
        private void GenerateRandomCrime()
        {
            // Случайная позиция на карте
            int x = _random.Next(0, _map.Width);
            int y = _random.Next(0, _map.Height);

            // Случайный тип преступления
            var crimeTypes = Enum.GetValues(typeof(CrimeType));
            CrimeType type = (CrimeType)crimeTypes.GetValue(_random.Next(crimeTypes.Length));

            // Создаём преступление
            Crime crime = new Crime(_nextCrimeId++, type, x, y, _currentGameTime);
            _activeCrimes.Add(crime);
            TotalCrimesCommitted++;
        }

        /// <summary>
        /// Создаёт преступление в указанном месте
        /// </summary>
        public Crime CreateCrime(CrimeType type, int x, int y)
        {
            Crime crime = new Crime(_nextCrimeId++, type, x, y, _currentGameTime);
            _activeCrimes.Add(crime);
            TotalCrimesCommitted++;
            return crime;
        }

        /// <summary>
        /// Отправляет полицию на активные преступления
        /// </summary>
        private void DispatchPoliceToActiveCrimes()
        {
            // Получаем все преступления без назначенной полиции
            var undispatchedCrimes = _activeCrimes
                .Where(c => !c.PoliceDispatched && !c.IsSolved)
                .OrderByDescending(c => c.SeverityLevel) // Сначала серьёзные
                .ToList();

            foreach (var crime in undispatchedCrimes)
            {
                // Ищем ближайший участок
                var nearestStation = FindNearestPoliceStation(crime.LocationX, crime.LocationY);

                if (nearestStation != null && nearestStation.IsInCoverageArea(crime.LocationX, crime.LocationY))
                {
                    nearestStation.DispatchToCrime(crime);
                }
            }
        }

        /// <summary>
        /// Находит ближайший полицейский участок к указанной точке
        /// </summary>
        private PoliceStation FindNearestPoliceStation(int x, int y)
        {
            PoliceStation nearest = null;
            int minDistance = int.MaxValue;

            foreach (var station in _policeStations)
            {
                int distance = Math.Abs(station.X - x) + Math.Abs(station.Y - y);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = station;
                }
            }

            return nearest;
        }

        /// <summary>
        /// Очищает список от раскрытых преступлений
        /// </summary>
        private void CleanupSolvedCrimes()
        {
            var solvedCrimes = _activeCrimes.Where(c => c.IsSolved).ToList();

            foreach (var crime in solvedCrimes)
            {
                _activeCrimes.Remove(crime);
                TotalCrimesSolved++;
            }
        }

        /// <summary>
        /// Получает все активные преступления
        /// </summary>
        public List<Crime> GetActiveCrimes()
        {
            return new List<Crime>(_activeCrimes);
        }

        /// <summary>
        /// Получает статистику преступности
        /// </summary>
        public float GetCrimeSolveRate()
        {
            if (TotalCrimesCommitted == 0)
                return 0f;

            return (float)TotalCrimesSolved / TotalCrimesCommitted * 100f;
        }

        /// <summary>
        /// Получает количество активных (нераскрытых) преступлений
        /// </summary>
        public int GetActiveCrimeCount()
        {
            return _activeCrimes.Count(c => !c.IsSolved);
        }

        /// <summary>
        /// Получает уровень преступности в городе (от 0 до 100)
        /// </summary>
        public float GetCrimeLevel()
        {
            int activeCrimes = GetActiveCrimeCount();
            int population = 1000; // Можно связать с реальным населением

            // Уровень преступности зависит от количества активных преступлений
            float crimeLevel = (float)activeCrimes / population * 1000f;
            return Math.Min(100f, crimeLevel);
        }
    }
}
