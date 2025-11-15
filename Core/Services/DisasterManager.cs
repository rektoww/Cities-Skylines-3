using Core.Enums;
using Core.Interfaces;
using Core.Models.Base;
using Core.Models.Map;
using System.Text;

namespace Core.Services
{
    /// <summary>
    /// Управление бедствиями в игре. Обрабатывает создание, обновление и завершение событий бедствий.
    /// </summary>
    public class DisasterManager : IGameService
    {
        public List<DisasterEvent> ActiveEvents { get; private set; } = new List<DisasterEvent>();
        public GameMap Map { get; set; }
        public UtilityManager UtilityManager { get; set; }

        public Dictionary<DisasterType, double> BaseDisasterChances { get; set; } = new Dictionary<DisasterType, double>()
        {
            { DisasterType.PowerGridFailure, 0.005 },
            { DisasterType.GasLeak, 0.003 },
            { DisasterType.Fire, 0.004 },
            { DisasterType.IndustrialAccident, 0.002 },
            { DisasterType.Earthquake, 0.0005 },
        };

        private static readonly Random Random = new Random();

        public event Action<DisasterEvent> OnDisasterStarted;
        public event Action<DisasterEvent> OnDisasterEnded;

        public DisasterManager(GameMap map, UtilityManager utilityManager)
        {
            Map = map;
            UtilityManager = utilityManager;
        }

        /// <summary>
        /// Инициализация (IGameService)
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Обновление состояния менеджера бедствий (IGameService.Update)
        /// </summary>
        public void Update()
        {
            foreach (var disasterEvent in ActiveEvents.ToList())
            {
                disasterEvent.Update();

                if (!disasterEvent.IsActive)
                {
                    ActiveEvents.Remove(disasterEvent);
                    OnDisasterEnded?.Invoke(disasterEvent);
                }
            }

            TryTriggerRandomDisaster();
        }

        private void TryTriggerRandomDisaster()
        {
            foreach (var disasterChance in BaseDisasterChances)
            {
                if (Random.NextDouble() < disasterChance.Value)
                {
                    TriggerDisaster(disasterChance.Key);
                }
            }
        }

        public void TriggerDisaster(DisasterType type, float intensity = -1f, int duration = -1, int x = -1, int y = -1, float radius = -1f)
        {
            if (intensity == -1f) intensity = GetDefaultIntensity(type);
            if (duration == -1) duration = GetDefaultDuration(type);
            if (radius == -1f) radius = GetDefaultRadius(type);

            if (x == -1) x = Random.Next(0, Map.Width);
            if (y == -1) y = Random.Next(0, Map.Height);

            var newEvent = new DisasterEvent(type, intensity, duration, x, y, radius, Map, this);
            newEvent.OnFireSpread += HandleFireSpread;

            ActiveEvents.Add(newEvent);
            OnDisasterStarted?.Invoke(newEvent);

        }

        private void HandleFireSpread(DisasterEvent newFireEvent)
        {
            ActiveEvents.Add(newFireEvent);
        }

        private float GetDefaultIntensity(DisasterType type) => 0.5f + (float)Random.NextDouble() * 0.5f;
        private int GetDefaultDuration(DisasterType type) => 3 + Random.Next(0, 4);
        private float GetDefaultRadius(DisasterType type) => 2f + (float)Random.NextDouble() * 2f;

        public string GetDisasterReport()
        {
            if (ActiveEvents.Count == 0) return "No active disasters.";

            var report = new StringBuilder(string.Format("Active Disasters: {0}\n", ActiveEvents.Count));
            foreach (var evt in ActiveEvents)
            {
                report.AppendLine(string.Format("- {0} at ({1}, {2})", evt.Name, evt.EpicenterX, evt.EpicenterY));
            }
            return report.ToString();
        }

        public void StopAllDisasters()
        {
            ActiveEvents.Clear();
        }
    }
}