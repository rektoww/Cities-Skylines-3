using Core.Models.Map;
using Core.Models.Buildings;
using Core.Interfaces;
using Core.Models.Mobs;
using Core.Services.Mobility;
using Core.Services.Education;
using Core.Services.Happiness;

namespace Core.Services
{
    /// <summary>
    /// Сервис управления населением города.
    /// Поддерживает список граждан, обновляет их поведение каждый тик и предоставляет простой API для добавления/получения жителей.
    /// </summary>
    public class PopulationService : IGameService
    {
        private readonly GameMap _map;
        private readonly List<Citizen> _citizens;
        private readonly Random _random;

        private readonly IMobilityService _mobilityService;
        private readonly IEducationService _educationService;
        private readonly IHappinessService _happinessService;

        public PopulationService(GameMap map)
        {
            _map = map ?? throw new ArgumentNullException(nameof(map));
            _citizens = new List<Citizen>();
            _random = new Random();

            _mobilityService = new MobilityService(_map);
            _educationService = new EducationService();
            _happinessService = new HappinessService(_map);

            Initialize();
        }
        
        /// <summary>
        /// Количество граждан в городе.
        /// Используется в UI (_gameEngine.PopulationService.CitizenCount).
        /// </summary>
        public int CitizenCount => _citizens.Count;

        /// <summary>
        /// Возвращает readonly-коллекцию граждан.
        /// </summary>
        public IReadOnlyList<Citizen> Citizens => _citizens;

        /// <summary>
        /// Инициализация сервиса (можно использовать для спауна стартовой популяции).
        /// Сейчас не спаунит по-умолчанию (оставлено для расширения).
        /// </summary>
        public void Initialize()
        {

        }

        /// <summary>
        /// Обновление сервиса — вызывается каждый игровой тик из GameEngine.Update().
        /// Выполняет шаги поведения граждан: обучение, перемещение, обновление счастья и дополнительные эффекты.
        /// </summary>
        public void Update()
        {
            var snapshot = _citizens.ToList();

            foreach (var citizen in snapshot)
            {
                _happinessService.UpdateHappiness(citizen);

                _educationService.StudyTick(citizen);

                try
                {
                    _mobilityService.MoveCitizen(citizen);
                }
                catch
                {
                }

                _happinessService.ApplyHappinessEffects(citizen);

                if (citizen.Health <= 0f)
                {
                    RemoveCitizen(citizen);
                    continue;
                }

                if (citizen.CanReproduce && citizen.IsMarried)
                {
                    var birthChance = 0.005 * (citizen.Happiness / 100.0);
                    if (_random.NextDouble() < birthChance)
                    {
                        var child = new Citizen(citizen.X, citizen.Y, _map)
                        {
                            Age = 0,
                            IsMale = _random.Next(0, 2) == 0,
                            Home = citizen.Home
                        };

                        AddCitizen(child);
                    }
                }
            }

        }

        /// <summary>
        /// Добавляет гражданина в систему.
        /// Устанавливает ссылку на карту у гражданина и по возможности добавляет в список жителей жилого здания.
        /// </summary>
        public void AddCitizen(Citizen citizen)
        {
            if (citizen == null) throw new ArgumentNullException(nameof(citizen));

            citizen.GameMap = _map;
            _citizens.Add(citizen);

            if (citizen.Home != null && citizen.Home is ResidentialBuilding home && home.TryAddResident(citizen))
            {
            }
        }

        /// <summary>
        /// Удаляет гражданина из системы (например, при смерти).
        /// </summary>
        public bool RemoveCitizen(Citizen citizen)
        {
            if (citizen == null) return false;

            if (citizen.Home != null && citizen.Home is ResidentialBuilding home)
            {
                if (home.CurrentResidents.Contains(citizen))
                    home.CurrentResidents.Remove(citizen);
            }

            if (citizen.TargetTransitStation != null)
            {
                citizen.TargetTransitStation.RemoveWaitingCitizen(citizen);
            }

            return _citizens.Remove(citizen);
        }

        /// <summary>
        /// Удобный метод — попытаться заселить граждан в ближайшее свободное жилое здание.
        /// Возвращает true, если заселение прошло успешно.
        /// </summary>
        public bool TryAssignToAnyVacancy(Citizen citizen)
        {
            if (citizen == null) return false;

            var residentials = _map.Buildings.OfType<ResidentialBuilding>().Where(b => b.HasVacancy).ToList();
            if (!residentials.Any()) return false;

            var nearest = residentials.OrderBy(b => Math.Abs(b.X - citizen.X) + Math.Abs(b.Y - citizen.Y)).First();
            if (nearest.TryAddResident(citizen))
            {
                citizen.Home = nearest;
                return true;
            }
            return false;
        }
    }
}