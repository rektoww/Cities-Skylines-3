using System;
using System.Collections.Generic;
using System.Linq;
using Core.Enums;
using Core.Enums.Core.Enums;
using Core.Models.Buildings;
using Core.Models.Map;
using Core.Services;

namespace Core.Models.Base
{
    /// <summary>
    /// Представляет событие бедствия на карте. Обрабатывает применение эффектов и распространение бедствия.
    /// </summary>
    public class DisasterEvent : GameObject
    {
        // Множители урона
        private const float EARTHQUAKE_DAMAGE_MULTIPLIER = 25f;
        private const float FIRE_DAMAGE_MULTIPLIER = 35f;
        private const float GAS_LEAK_EXPLOSION_DAMAGE_MULTIPLIER = 60f;
        private const float POWER_FAILURE_DAMAGE_MULTIPLIER = 15f;
        private const float INDUSTRIAL_ACCIDENT_INDUSTRIAL_DAMAGE_MULTIPLIER = 60f;
        private const float INDUSTRIAL_ACCIDENT_NON_INDUSTRIAL_DAMAGE_MULTIPLIER = 20f;

        // Вероятности событий
        private const float EARTHQUAKE_UTILITY_DISRUPTION_CHANCE = 0.3f;
        private const float EARTHQUAKE_ROAD_DESTRUCTION_CHANCE = 0.1f;
        private const float FIRE_TREE_DESTRUCTION_CHANCE = 0.8f;
        private const float FIRE_PARK_DESTRUPTION_CHANCE = 0.7f;
        private const float FIRE_ELECTRICITY_DISRUPTION_CHANCE = 0.8f;
        private const float GAS_LEAK_EXPLOSION_CHANCE = 0.5f;
        private const float INDUSTRIAL_ACCIDENT_UTILITY_DISRUPTION_CHANCE = 0.7f;
        private const float INDUSTRIAL_ACCIDENT_ENVIRONMENT_DAMAGE_CHANCE = 0.4f;
        private const float POWER_FAILURE_ELECTRICITY_DISRUPTION_CHANCE = 1.0f;

        // Влияние на жителей
        private const float FIRE_HEALTH_IMPACT = 12f;
        private const float FIRE_HAPPINESS_IMPACT = 20f;
        private const float GAS_HEALTH_IMPACT = 8f;
        private const float GAS_HAPPINESS_IMPACT = 15f;
        private const float INDUSTRIAL_HEALTH_IMPACT = 8f;
        private const float INDUSTRIAL_HAPPINESS_IMPACT = 15f;

        // Распространение пожара
        private const float FIRE_SPREAD_CHANCE = 0.4f;
        private const float FIRE_SPREAD_INTENSITY_FACTOR = 0.7f;
        private const int FIRE_SPREAD_DURATION = 4;
        private const float FIRE_SPREAD_RADIUS = 1.5f;

        // Границы карты
        private const int MAP_MIN_COORDINATE = 0;

        /// <summary>Тип бедствия.</summary>
        public DisasterType Type { get; set; }

        /// <summary>Название бедствия.</summary>
        public string Name { get; set; }

        /// <summary>Интенсивность бедствия.</summary>
        public float Intensity { get; set; }

        /// <summary>Длительность бедствия в тактах.</summary>
        public int DurationTicks { get; set; }

        /// <summary>Текущий такт бедствия.</summary>
        public int CurrentTick { get; set; } = 0;

        /// <summary>Флаг активности бедствия.</summary>
        public bool IsActive => CurrentTick < DurationTicks;

        /// <summary>X-координата эпицентра бедствия.</summary>
        public int EpicenterX { get; set; }

        /// <summary>Y-координата эпицентра бедствия.</summary>
        public int EpicenterY { get; set; }

        /// <summary>Радиус воздействия бедствия.</summary>
        public float Radius { get; set; }

        /// <summary>Игровая карта для применения эффектов.</summary>
        public GameMap Map { get; set; }

        /// <summary>Менеджер бедствий для координации событий.</summary>
        public DisasterManager Manager { get; set; }

        /// <summary>Событие, возникающее при распространении пожара.</summary>
        public event Action<DisasterEvent> OnFireSpread;

        /// <summary>Генератор случайных чисел для расчета вероятностей.</summary>
        private static readonly Random Random = new Random();

        /// <summary>
        /// Создание события бедствия с указанными параметрами.
        /// </summary>
        /// <param name="type">Тип бедствия.</param>
        /// <param name="intensity">Интенсивность бедствия.</param>
        /// <param name="duration">Длительность бедствия в тактах.</param>
        /// <param name="x">X-координата эпицентра.</param>
        /// <param name="y">Y-координата эпицентра.</param>
        /// <param name="radius">Радиус воздействия бедствия.</param>
        /// <param name="map">Игровая карта.</param>
        /// <param name="manager">Менеджер бедствий.</param>
        public DisasterEvent(DisasterType type, float intensity, int duration, int x, int y, float radius, GameMap map, DisasterManager manager = null)
        {
            Type = type;
            Name = type.ToString();
            Intensity = intensity;
            DurationTicks = duration;
            EpicenterX = x;
            EpicenterY = y;
            Radius = radius;
            Map = map;
            Manager = manager;
            X = x;
            Y = y;
        }

        /// <summary>
        /// Обновление состояния бедствия. Применяет эффекты и обрабатывает распространение.
        /// </summary>
        public void Update()
        {
            if (!IsActive) return;

            ApplyEffects();

            // УПРОЩЕННАЯ логика распространения пожара
            if (Type == DisasterType.Fire)
            {
                TrySpreadFire();
            }

            CurrentTick++;

            if (!IsActive)
            {
                OnEventEnded();
            }
        }

        /// <summary>
        /// Применение эффектов бедствия ко всем тайлам в радиусе воздействия.
        /// </summary>
        private void ApplyEffects()
        {
            for (int x = Math.Max(MAP_MIN_COORDINATE, (int)(EpicenterX - Radius)); x <= Math.Min(Map.Width - 1, EpicenterX + Radius); x++)
            {
                for (int y = Math.Max(MAP_MIN_COORDINATE, (int)(EpicenterY - Radius)); y <= Math.Min(Map.Height - 1, EpicenterY + Radius); y++)
                {
                    float distance = CalculateDistance(x, y, EpicenterX, EpicenterY);
                    if (distance <= Radius)
                    {
                        float localIntensity = Intensity * (1 - (distance / Radius));
                        AffectTile(Map.Tiles[x, y], localIntensity);
                    }
                }
            }
        }

        /// <summary>
        /// Расчет расстояния между двумя точками на карте.
        /// </summary>
        /// <param name="x1">X-координата первой точки.</param>
        /// <param name="y1">Y-координата первой точки.</param>
        /// <param name="x2">X-координата второй точки.</param>
        /// <param name="y2">Y-координата второй точки.</param>
        /// <returns>Расстояние между точками.</returns>
        private float CalculateDistance(int x1, int y1, int x2, int y2)
        {
            return (float)Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        }

        /// <summary>
        /// Воздействие на указанный тайл с заданной интенсивностью.
        /// </summary>
        /// <param name="tile">Тайл для воздействия.</param>
        /// <param name="localIntensity">Локальная интенсивность воздействия.</param>
        private void AffectTile(Tile tile, float localIntensity)
        {
            switch (Type)
            {
                case DisasterType.Earthquake:
                    AffectTile_Earthquake(tile, localIntensity);
                    break;
                case DisasterType.Fire:
                    AffectTile_Fire(tile, localIntensity);
                    break;
                case DisasterType.PowerGridFailure:
                    AffectTile_PowerFailure(tile, localIntensity);
                    break;
                case DisasterType.GasLeak:
                    AffectTile_GasLeak(tile, localIntensity);
                    break;
                case DisasterType.IndustrialAccident:
                    AffectTile_IndustrialAccident(tile, localIntensity);
                    break;
            }
        }

        /// <summary>
        /// Воздействие землетрясения на тайл. Наносит урон зданиям и нарушает коммунальные услуги.
        /// </summary>
        /// <param name="tile">Тайл для воздействия.</param>
        /// <param name="intensity">Интенсивность воздействия.</param>
        private void AffectTile_Earthquake(Tile tile, float intensity)
        {
            if (tile.Building != null)
            {
                float damage = intensity * EARTHQUAKE_DAMAGE_MULTIPLIER;
                tile.Building.Condition = Math.Max(0, tile.Building.Condition - damage);

                if (Random.NextDouble() < intensity * EARTHQUAKE_UTILITY_DISRUPTION_CHANCE)
                {
                    var utilityTypes = Enum.GetValues(typeof(UtilityType));
                    var utility = (UtilityType)Random.Next(0, utilityTypes.Length);
                    switch (utility)
                    {
                        case UtilityType.Electricity: tile.Building.HasElectricity = false; break;
                        case UtilityType.Water: tile.Building.HasWater = false; break;
                        case UtilityType.Gas: tile.Building.HasGas = false; break;
                        case UtilityType.Sewage: tile.Building.HasSewage = false; break;
                    }
                }
            }

            if (tile.HasRoad && Random.NextDouble() < intensity * EARTHQUAKE_ROAD_DESTRUCTION_CHANCE)
            {
                tile.HasRoad = false;
            }
        }

        /// <summary>
        /// Воздействие пожара на тайл. Уничтожает деревья, парки и наносит урон зданиям.
        /// </summary>
        /// <param name="tile">Тайл для воздействия.</param>
        /// <param name="intensity">Интенсивность воздействия.</param>
        private void AffectTile_Fire(Tile tile, float intensity)
        {
            // Уничтожение деревьев
            if (tile.TreeCount > 0 && Random.NextDouble() < intensity * FIRE_TREE_DESTRUCTION_CHANCE)
            {
                tile.TreeCount = 0;
                tile.TreeType = null;
            }

            // Уничтожение парков
            if (tile.HasPark && Random.NextDouble() < intensity * FIRE_PARK_DESTRUPTION_CHANCE)
            {
                tile.HasPark = false;
            }

            // Воздействие на здания
            if (tile.Building != null)
            {
                float damage = intensity * FIRE_DAMAGE_MULTIPLIER;
                tile.Building.Condition = Math.Max(0, tile.Building.Condition - damage);

                if (Random.NextDouble() < intensity * FIRE_ELECTRICITY_DISRUPTION_CHANCE)
                {
                    tile.Building.HasElectricity = false;
                }

                // Влияние на жителей
                if (tile.Building is ResidentialBuilding residential)
                {
                    foreach (var citizen in residential.CurrentResidents)
                    {
                        citizen.Health -= FIRE_HEALTH_IMPACT * intensity;
                        citizen.Happiness -= FIRE_HAPPINESS_IMPACT * intensity;
                    }
                }
            }
        }

        /// <summary>
        /// Попытка распространения пожара на соседние тайлы.
        /// </summary>
        private void TrySpreadFire()
        {
            if (Type != DisasterType.Fire || !IsActive) return;

            // Проверяем всех соседей
            var neighbors = new (int, int)[] { (-1, 0), (1, 0), (0, -1), (0, 1) };

            foreach (var (dx, dy) in neighbors)
            {
                int newX = EpicenterX + dx;
                int newY = EpicenterY + dy;

                // Проверяем границы карты
                if (newX < MAP_MIN_COORDINATE || newX >= Map.Width ||
                    newY < MAP_MIN_COORDINATE || newY >= Map.Height)
                    continue;

                var neighborTile = Map.Tiles[newX, newY];

                // УПРОЩЕННОЕ УСЛОВИЕ: распространяемся только если есть здания или деревья
                bool canSpread = neighborTile.Building != null || neighborTile.TreeCount > 0;

                // УПРОЩЕННАЯ ПРОВЕРКА: шанс распространения
                if (canSpread && Random.NextDouble() < FIRE_SPREAD_CHANCE && !IsTileAlreadyBurning(newX, newY))
                {
                    CreateNewFire(newX, newY);
                    break; // Распространяемся только на одного соседа за такт
                }
            }
        }

        /// <summary>
        /// Проверка, не горит ли уже указанный тайл.
        /// </summary>
        /// <param name="x">X-координата тайла.</param>
        /// <param name="y">Y-координата тайла.</param>
        /// <returns>True если тайл уже горит, иначе False.</returns>
        private bool IsTileAlreadyBurning(int x, int y)
        {
            return Manager?.ActiveEvents.Any(e =>
                e.Type == DisasterType.Fire &&
                e.EpicenterX == x &&
                e.EpicenterY == y) == true;
        }

        /// <summary>
        /// Создание нового очага пожара в указанных координатах.
        /// </summary>
        /// <param name="x">X-координата нового пожара.</param>
        /// <param name="y">Y-координата нового пожара.</param>
        private void CreateNewFire(int x, int y)
        {
            var newFire = new DisasterEvent(
                DisasterType.Fire,
                Intensity * FIRE_SPREAD_INTENSITY_FACTOR,
                FIRE_SPREAD_DURATION,
                x, y,
                FIRE_SPREAD_RADIUS,
                Map,
                Manager
            );

            OnFireSpread?.Invoke(newFire);
        }

        /// <summary>
        /// Воздействие утечки газа на тайл. Отключает газ и может вызвать взрыв с созданием пожара.
        /// </summary>
        /// <param name="tile">Тайл для воздействия.</param>
        /// <param name="intensity">Интенсивность воздействия.</param>
        private void AffectTile_GasLeak(Tile tile, float intensity)
        {
            if (tile.Building != null)
            {
                tile.Building.HasGas = false;

                if (Random.NextDouble() < intensity * GAS_LEAK_EXPLOSION_CHANCE)
                {
                    tile.Building.Condition = Math.Max(0, tile.Building.Condition - intensity * GAS_LEAK_EXPLOSION_DAMAGE_MULTIPLIER);

                    // Создание пожара при взрыве
                    Manager?.TriggerDisaster(
                        DisasterType.Fire,
                        intensity * FIRE_SPREAD_INTENSITY_FACTOR,
                        FIRE_SPREAD_DURATION,
                        tile.X,
                        tile.Y,
                        FIRE_SPREAD_RADIUS
                    );
                }
            }

            if (tile.Building is ResidentialBuilding residential)
            {
                foreach (var citizen in residential.CurrentResidents)
                {
                    citizen.Health -= GAS_HEALTH_IMPACT * intensity;
                    citizen.Happiness -= GAS_HAPPINESS_IMPACT * intensity;
                }
            }
        }

        /// <summary>
        /// Воздействие аварии в энергосети на тайл. Отключает электричество и наносит урон промышленным зданиям.
        /// </summary>
        /// <param name="tile">Тайл для воздействия.</param>
        /// <param name="intensity">Интенсивность воздействия.</param>
        private void AffectTile_PowerFailure(Tile tile, float intensity)
        {
            if (tile.Building != null && Random.NextDouble() < intensity * POWER_FAILURE_ELECTRICITY_DISRUPTION_CHANCE)
            {
                tile.Building.HasElectricity = false;

                if (tile.Building is CommercialBuilding commercial &&
                    commercial.Type == CommercialBuildingType.Factory)
                {
                    tile.Building.Condition = Math.Max(0, tile.Building.Condition - intensity * POWER_FAILURE_DAMAGE_MULTIPLIER);
                }
            }
        }

        /// <summary>
        /// Воздействие промышленной аварии на тайл. Наносит значительный урон промышленным зданиям и нарушает коммунальные услуги.
        /// </summary>
        /// <param name="tile">Тайл для воздействия.</param>
        /// <param name="intensity">Интенсивность воздействия.</param>
        private void AffectTile_IndustrialAccident(Tile tile, float intensity)
        {
            bool isIndustrial = tile.Building is CommercialBuilding commercial &&
                               commercial.Type == CommercialBuildingType.Factory;

            if (tile.Building != null)
            {
                float damageMultiplier = isIndustrial ?
                    INDUSTRIAL_ACCIDENT_INDUSTRIAL_DAMAGE_MULTIPLIER :
                    INDUSTRIAL_ACCIDENT_NON_INDUSTRIAL_DAMAGE_MULTIPLIER;

                float damage = intensity * damageMultiplier;
                tile.Building.Condition = Math.Max(0, tile.Building.Condition - damage);

                if (Random.NextDouble() < intensity * INDUSTRIAL_ACCIDENT_UTILITY_DISRUPTION_CHANCE)
                {
                    tile.Building.HasElectricity = false;
                    tile.Building.HasWater = false;
                    tile.Building.HasGas = false;
                }

                if (tile.Building is ResidentialBuilding residential)
                {
                    foreach (var citizen in residential.CurrentResidents)
                    {
                        citizen.Health -= INDUSTRIAL_HEALTH_IMPACT * intensity;
                        citizen.Happiness -= INDUSTRIAL_HAPPINESS_IMPACT * intensity;
                    }
                }
            }

            if (Random.NextDouble() < intensity * INDUSTRIAL_ACCIDENT_ENVIRONMENT_DAMAGE_CHANCE)
            {
                tile.TreeCount = 0;
                tile.TreeType = null;
                tile.HasPark = false;
            }
        }

        /// <summary>
        /// Обработка завершения события бедствия. Выводит информацию в консоль.
        /// </summary>
        private void OnEventEnded()
        {
            Console.WriteLine($"Disaster event {Name} has ended at ({EpicenterX}, {EpicenterY}).");
        }
    }
}