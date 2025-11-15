using Core.Enums;
using Core.Interfaces;
using Core.Models.Map;
using Core.Models.Police;

namespace Core.Services
{
    /// <summary>
    /// MarkinaVS
    /// Сервис для управления полицейскими службами города.
    /// Управляет преступлениями, участками и координирует работу полиции.
    /// </summary>
    public class PoliceService : IGameService
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
            CrimeGenerationRate = 0.01f;
            TotalCrimesCommitted = 0;
            TotalCrimesSolved = 0;
        }

        /// <summary>
        /// Инициализация сервиса (часть IGameService).
        /// </summary>
        public void Initialize()
        {
            // Currently nothing to init beyond ctor. Kept for future use.
        }

        /// <summary>
        /// Обновление системы полиции (вызывается каждый игровой тик)
        /// </summary>
        public void Update()
        {
            _currentGameTime++;

            TryGenerateCrime();

            DispatchPoliceToActiveCrimes();

            CleanupSolvedCrimes();
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

        private void TryGenerateCrime()
        {
            if (_random.NextDouble() < CrimeGenerationRate)
            {
                GenerateRandomCrime();
            }
        }

        private void GenerateRandomCrime()
        {
            int x = _random.Next(0, _map.Width);
            int y = _random.Next(0, _map.Height);

            var crimeTypes = Enum.GetValues(typeof(CrimeType));
            CrimeType type = (CrimeType)crimeTypes.GetValue(_random.Next(crimeTypes.Length));

            Crime crime = new Crime(_nextCrimeId++, type, x, y, _currentGameTime);
            _activeCrimes.Add(crime);
            TotalCrimesCommitted++;
        }

        public Crime CreateCrime(CrimeType type, int x, int y)
        {
            Crime crime = new Crime(_nextCrimeId++, type, x, y, _currentGameTime);
            _activeCrimes.Add(crime);
            TotalCrimesCommitted++;
            return crime;
        }

        private void DispatchPoliceToActiveCrimes()
        {
            var undispatchedCrimes = _activeCrimes
                .Where(c => !c.PoliceDispatched && !c.IsSolved)
                .OrderByDescending(c => c.SeverityLevel)
                .ToList();

            foreach (var crime in undispatchedCrimes)
            {
                var nearestStation = FindNearestPoliceStation(crime.LocationX, crime.LocationY);

                if (nearestStation != null && nearestStation.IsInCoverageArea(crime.LocationX, crime.LocationY))
                {
                    nearestStation.DispatchToCrime(crime);
                }
            }
        }

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

        private void CleanupSolvedCrimes()
        {
            var solvedCrimes = _activeCrimes.Where(c => c.IsSolved).ToList();

            foreach (var crime in solvedCrimes)
            {
                _activeCrimes.Remove(crime);
                TotalCrimesSolved++;
            }
        }

        public List<Crime> GetActiveCrimes()
        {
            return new List<Crime>(_activeCrimes);
        }

        public float GetCrimeSolveRate()
        {
            if (TotalCrimesCommitted == 0)
                return 0f;

            return (float)TotalCrimesSolved / TotalCrimesCommitted * 100f;
        }

        public int GetActiveCrimeCount()
        {
            return _activeCrimes.Count(c => !c.IsSolved);
        }

        public float GetCrimeLevel()
        {
            int activeCrimes = GetActiveCrimeCount();
            int population = 1000;
            float crimeLevel = (float)activeCrimes / population * 1000f;
            return Math.Min(100f, crimeLevel);
        }
    }
}