﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Enums;
using Core.Models.Base;
using Core.Models.Map;

namespace Core.Services
{
    /// <summary>
    /// Управление бедствиями в игре. Обрабатывает создание, обновление и завершение событий бедствий.
    /// </summary>
    public class DisasterManager
    {
        // Базовые шансы возникновения бедствий
        private const double POWER_GRID_FAILURE_CHANCE = 0.005;
        private const double GAS_LEAK_CHANCE = 0.003;
        private const double FIRE_CHANCE = 0.004;
        private const double INDUSTRIAL_ACCIDENT_CHANCE = 0.002;
        private const double EARTHQUAKE_CHANCE = 0.0005;

        // Параметры интенсивности по умолчанию
        private const float DEFAULT_INTENSITY_BASE = 0.5f;
        private const float DEFAULT_INTENSITY_RANDOM_RANGE = 0.5f;

        // Параметры длительности по умолчанию
        private const int DEFAULT_DURATION_BASE = 3;
        private const int DEFAULT_DURATION_RANDOM_RANGE = 4;

        // Параметры радиуса по умолчанию
        private const float DEFAULT_RADIUS_BASE = 2f;
        private const float DEFAULT_RADIUS_RANDOM_RANGE = 2f;

        // Специальные значения для параметров
        private const float INTENSITY_NOT_SET = -1f;
        private const int DURATION_NOT_SET = -1;
        private const int COORDINATE_NOT_SET = -1;
        private const float RADIUS_NOT_SET = -1f;

        // Сообщения
        private const string NO_DISASTERS_MESSAGE = "No active disasters.";
        private const string DISASTER_REPORT_HEADER = "Active Disasters: {0}\n";
        private const string DISASTER_EVENT_FORMAT = "- {0} at ({1}, {2})";
        private const string DISASTER_TRIGGERED_MESSAGE = "DISASTER! {0} at ({1}, {2})";
        private const string FIRE_SPREAD_MESSAGE = "FIRE SPREAD! New fire at ({0}, {1})";
        private const string ALL_DISASTERS_STOPPED_MESSAGE = "All disasters stopped.";

        /// <summary>Список активных событий бедствий.</summary>
        public List<DisasterEvent> ActiveEvents { get; private set; } = new List<DisasterEvent>();

        /// <summary>Игровая карта для применения эффектов бедствий.</summary>
        public GameMap Map { get; set; }

        /// <summary>Менеджер утилит для управления коммунальными услугами.</summary>
        public UtilityManager UtilityManager { get; set; }

        /// <summary>Базовые шансы возникновения различных типов бедствий.</summary>
        public Dictionary<DisasterType, double> BaseDisasterChances { get; set; } = new Dictionary<DisasterType, double>()
        {
            { DisasterType.PowerGridFailure, POWER_GRID_FAILURE_CHANCE },
            { DisasterType.GasLeak, GAS_LEAK_CHANCE },
            { DisasterType.Fire, FIRE_CHANCE },
            { DisasterType.IndustrialAccident, INDUSTRIAL_ACCIDENT_CHANCE },
            { DisasterType.Earthquake, EARTHQUAKE_CHANCE },
        };

        /// <summary>Генератор случайных чисел для определения возникновения бедствий.</summary>
        private static readonly Random Random = new Random();

        /// <summary>Событие, возникающее при начале бедствия.</summary>
        public event Action<DisasterEvent> OnDisasterStarted;

        /// <summary>Событие, возникающее при завершении бедствия.</summary>
        public event Action<DisasterEvent> OnDisasterEnded;

        /// <summary>
        /// Создание менеджера бедствий с указанной картой и менеджером утилит.
        /// </summary>
        /// <param name="map">Игровая карта.</param>
        /// <param name="utilityManager">Менеджер утилит.</param>
        public DisasterManager(GameMap map, UtilityManager utilityManager)
        {
            Map = map;
            UtilityManager = utilityManager;
        }

        /// <summary>
        /// Обновление состояния менеджера бедствий. Обрабатывает активные события и проверяет новые случайные бедствия.
        /// </summary>
        public void Update()
        {
            // Обновляем активные события
            foreach (var disasterEvent in ActiveEvents.ToList())
            {
                disasterEvent.Update();

                if (!disasterEvent.IsActive)
                {
                    ActiveEvents.Remove(disasterEvent);
                    OnDisasterEnded?.Invoke(disasterEvent);
                }
            }

            // Проверяем новые случайные события
            TryTriggerRandomDisaster();
        }

        /// <summary>
        /// Попытка запуска случайного бедствия на основе базовых шансов.
        /// </summary>
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

        /// <summary>
        /// Запуск бедствия указанного типа с возможностью настройки параметров.
        /// </summary>
        /// <param name="type">Тип бедствия.</param>
        /// <param name="intensity">Интенсивность бедствия.</param>
        /// <param name="duration">Длительность бедствия в тактах.</param>
        /// <param name="x">X-координата эпицентра.</param>
        /// <param name="y">Y-координата эпицентра.</param>
        /// <param name="radius">Радиус воздействия бедствия.</param>
        public void TriggerDisaster(DisasterType type, float intensity = INTENSITY_NOT_SET, int duration = DURATION_NOT_SET, int x = COORDINATE_NOT_SET, int y = COORDINATE_NOT_SET, float radius = RADIUS_NOT_SET)
        {
            if (intensity == INTENSITY_NOT_SET) intensity = GetDefaultIntensity(type);
            if (duration == DURATION_NOT_SET) duration = GetDefaultDuration(type);
            if (radius == RADIUS_NOT_SET) radius = GetDefaultRadius(type);

            // Случайные координаты если не указаны
            if (x == COORDINATE_NOT_SET) x = Random.Next(0, Map.Width);
            if (y == COORDINATE_NOT_SET) y = Random.Next(0, Map.Height);

            var newEvent = new DisasterEvent(type, intensity, duration, x, y, radius, Map, this);
            newEvent.OnFireSpread += HandleFireSpread;

            ActiveEvents.Add(newEvent);
            OnDisasterStarted?.Invoke(newEvent);

            Console.WriteLine(string.Format(DISASTER_TRIGGERED_MESSAGE, type, x, y));
        }

        /// <summary>
        /// Обработка распространения пожара. Добавляет новое событие пожара в активные события.
        /// </summary>
        /// <param name="newFireEvent">Новое событие пожара.</param>
        private void HandleFireSpread(DisasterEvent newFireEvent)
        {
            ActiveEvents.Add(newFireEvent);
            Console.WriteLine(string.Format(FIRE_SPREAD_MESSAGE, newFireEvent.EpicenterX, newFireEvent.EpicenterY));
        }

        /// <summary>
        /// Получение интенсивности по умолчанию для указанного типа бедствия.
        /// </summary>
        /// <param name="type">Тип бедствия.</param>
        /// <returns>Значение интенсивности.</returns>
        private float GetDefaultIntensity(DisasterType type)
        {
            return DEFAULT_INTENSITY_BASE + (float)Random.NextDouble() * DEFAULT_INTENSITY_RANDOM_RANGE;
        }

        /// <summary>
        /// Получение длительности по умолчанию для указанного типа бедствия.
        /// </summary>
        /// <param name="type">Тип бедствия.</param>
        /// <returns>Длительность в тактах.</returns>
        private int GetDefaultDuration(DisasterType type)
        {
            return DEFAULT_DURATION_BASE + Random.Next(0, DEFAULT_DURATION_RANDOM_RANGE);
        }

        /// <summary>
        /// Получение радиуса по умолчанию для указанного типа бедствия.
        /// </summary>
        /// <param name="type">Тип бедствия.</param>
        /// <returns>Значение радиуса.</returns>
        private float GetDefaultRadius(DisasterType type)
        {
            return DEFAULT_RADIUS_BASE + (float)Random.NextDouble() * DEFAULT_RADIUS_RANDOM_RANGE;
        }

        /// <summary>
        /// Получение отчета о текущих активных бедствиях.
        /// </summary>
        /// <returns>Строка с информацией о активных бедствиях.</returns>
        public string GetDisasterReport()
        {
            if (ActiveEvents.Count == 0) return NO_DISASTERS_MESSAGE;

            var report = new StringBuilder(string.Format(DISASTER_REPORT_HEADER, ActiveEvents.Count));
            foreach (var evt in ActiveEvents)
            {
                report.AppendLine(string.Format(DISASTER_EVENT_FORMAT, evt.Name, evt.EpicenterX, evt.EpicenterY));
            }
            return report.ToString();
        }

        /// <summary>
        /// Остановка всех активных бедствий. Очищает список активных событий.
        /// </summary>
        public void StopAllDisasters()
        {
            ActiveEvents.Clear();
            Console.WriteLine(ALL_DISASTERS_STOPPED_MESSAGE);
        }
    }
}