using Core.Enums;
using Core.Interfaces;
using Core.Models.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Models.Buildings.IndustrialBuildings
{
    /// <summary>
    /// Представляет промышленное здание – фабрику по производству роботов.
    /// Использует компоненты (микрочипы, алюминий и т.д.) для сборки различных типов роботов.
    /// </summary>
    public class RoboticsFactory : CommercialBuilding, IConstructable<RoboticsFactory>
    {
        #region Статические параметры строительства

        /// <summary>
        /// Базовая стоимость постройки фабрики.
        /// </summary>
        public static decimal BuildCost { get; protected set; } = 850000m;

        /// <summary>
        /// Список материалов, необходимых для строительства фабрики.
        /// </summary>
        public static Dictionary<ConstructionMaterial, int> RequiredMaterials { get; protected set; }
            = new Dictionary<ConstructionMaterial, int>
            {
                { ConstructionMaterial.Steel, 15 },
                { ConstructionMaterial.Concrete, 12 },
                { ConstructionMaterial.Plastic, 6 },
                { ConstructionMaterial.Glass, 3 }
            };

        #endregion

        #region Внутренние перечисления

        /// <summary>
        /// Компоненты, используемые в производстве роботов.
        /// </summary>
        public enum RoboticsComponent
        {
            Microchips,
            Actuators,
            Sensors,
            Aluminum,
            CopperWiring
        }

        /// <summary>
        /// Доступные типы производимых роботов.
        /// </summary>
        public enum RobotType
        {
            AssemblyRobot,      // Робот для сборочных линий
            WeldingRobot,       // Сварочный робот
            PaintingRobot,      // Робот-окрасчик
            PackagingRobot,     // Робот для упаковки
            InspectionRobot,    // Робот для контроля качества
            LogisticsRobot,     // Логистический робот
            MedicalRobot,       // Медицинский робот
            ResearchRobot       // Исследовательский робот
        }

        #endregion

        #region Свойства состояния фабрики

        /// <summary>
        /// Текущий склад компонентов (микрочипы, алюминий и т.п.).
        /// </summary>
        public Dictionary<RoboticsComponent, int> ComponentsStorage { get; private set; } = new();

        /// <summary>
        /// Склад готовых роботов.
        /// </summary>
        public Dictionary<RobotType, int> RobotsStorage { get; private set; } = new();

        /// <summary>
        /// Максимальная вместимость склада компонентов.
        /// </summary>
        public int MaxComponentStorage { get; private set; }

        /// <summary>
        /// Максимальная вместимость склада готовых роботов.
        /// </summary>
        public int MaxRobotStorage { get; private set; }

        /// <summary>
        /// Список производственных линий фабрики.
        /// </summary>
        public List<Workshop> AssemblyLines { get; private set; } = new();

        /// <summary>
        /// Количество инженеров, работающих на фабрике.
        /// </summary>
        public int EngineersCount { get; private set; }

        /// <summary>
        /// Максимально возможное количество инженеров.
        /// </summary>
        public int MaxEngineers { get; private set; }

        /// <summary>
        /// Текущий уровень исследований фабрики (влияет на эффективность).
        /// </summary>
        public int ResearchLevel { get; private set; } = 1;

        /// <summary>
        /// Финансирование, выделенное на исследования.
        /// </summary>
        public decimal ResearchBudget { get; private set; }

        /// <summary>
        /// Эффективность производства, зависящая от инженеров и уровня исследований.
        /// </summary>
        public float ProductionEfficiency =>
            EngineersCount > 0
                ? 0.4f + (EngineersCount / (float)MaxEngineers * 0.4f) + (ResearchLevel * 0.05f)
                : 0f;

        #endregion

        #region Конструктор

        /// <summary>
        /// Создаёт новый экземпляр фабрики роботов с базовыми параметрами и стартовыми материалами.
        /// </summary>
        public RoboticsFactory() : base(CommercialBuildingType.Factory)
        {
            MaxComponentStorage = 800;
            MaxRobotStorage = 50;
            MaxEngineers = 8;
            EngineersCount = 0;

            InitializeAssemblyLines();
            InitializeStartingComponents();
        }

        #endregion

        #region Инициализация

        /// <summary>
        /// Создаёт линии сборки роботов с разными циклами и требованиями к ресурсам.
        /// </summary>
        private void InitializeAssemblyLines()
        {
            // Промышленные роботы
            var industrialLine = new Workshop
            {
                Name = "Линия промышленных роботов",
                ProductionCycleTime = 24
            };
            industrialLine.InputRequirements.Add(RoboticsComponent.Microchips.ToString(), 8);
            industrialLine.InputRequirements.Add(RoboticsComponent.Actuators.ToString(), 6);
            industrialLine.InputRequirements.Add(RoboticsComponent.Sensors.ToString(), 4);
            industrialLine.InputRequirements.Add(RoboticsComponent.Aluminum.ToString(), 12);
            industrialLine.InputRequirements.Add(RoboticsComponent.CopperWiring.ToString(), 10);
            industrialLine.OutputProducts.Add(RobotType.AssemblyRobot.ToString(), 1);
            industrialLine.OutputProducts.Add(RobotType.WeldingRobot.ToString(), 1);
            AssemblyLines.Add(industrialLine);

            // Специализированные роботы
            var specializedLine = new Workshop
            {
                Name = "Линия специализированных роботов",
                ProductionCycleTime = 32
            };
            specializedLine.InputRequirements.Add(RoboticsComponent.Microchips.ToString(), 12);
            specializedLine.InputRequirements.Add(RoboticsComponent.Actuators.ToString(), 8);
            specializedLine.InputRequirements.Add(RoboticsComponent.Sensors.ToString(), 6);
            specializedLine.InputRequirements.Add(RoboticsComponent.Aluminum.ToString(), 15);
            specializedLine.InputRequirements.Add(RoboticsComponent.CopperWiring.ToString(), 12);
            specializedLine.OutputProducts.Add(RobotType.PaintingRobot.ToString(), 1);
            specializedLine.OutputProducts.Add(RobotType.PackagingRobot.ToString(), 1);
            specializedLine.OutputProducts.Add(RobotType.InspectionRobot.ToString(), 1);
            AssemblyLines.Add(specializedLine);

            // Продвинутые роботы
            var advancedLine = new Workshop
            {
                Name = "Линия продвинутых роботов",
                ProductionCycleTime = 48
            };
            advancedLine.InputRequirements.Add(RoboticsComponent.Microchips.ToString(), 20);
            advancedLine.InputRequirements.Add(RoboticsComponent.Actuators.ToString(), 12);
            advancedLine.InputRequirements.Add(RoboticsComponent.Sensors.ToString(), 10);
            advancedLine.InputRequirements.Add(RoboticsComponent.Aluminum.ToString(), 20);
            advancedLine.InputRequirements.Add(RoboticsComponent.CopperWiring.ToString(), 15);
            advancedLine.OutputProducts.Add(RobotType.LogisticsRobot.ToString(), 1);
            advancedLine.OutputProducts.Add(RobotType.MedicalRobot.ToString(), 1);
            advancedLine.OutputProducts.Add(RobotType.ResearchRobot.ToString(), 1);
            AssemblyLines.Add(advancedLine);
        }

        /// <summary>
        /// Заполняет склад начальными компонентами.
        /// </summary>
        private void InitializeStartingComponents()
        {
            // Инициализируем все компоненты нулевыми значениями для гарантии наличия ключей
            foreach (RoboticsComponent component in Enum.GetValues(typeof(RoboticsComponent)))
            {
                ComponentsStorage[component] = 0;
            }

            // Стартовый набор компонентов для начала производства
            AddComponent(RoboticsComponent.Microchips, 50);
            AddComponent(RoboticsComponent.Actuators, 30);
            AddComponent(RoboticsComponent.Sensors, 25);
            AddComponent(RoboticsComponent.Aluminum, 100);
            AddComponent(RoboticsComponent.CopperWiring, 80);
        }

        #endregion

        #region Управление персоналом и ресурсами

        /// <summary>
        /// Устанавливает текущее количество инженеров (не выше лимита).
        /// </summary>
        public void SetEngineersCount(int count)
        {
            EngineersCount = Math.Min(count, MaxEngineers);
        }

        /// <summary>
        /// Добавляет компонент на склад, если есть место.
        /// </summary>
        public bool AddComponent(RoboticsComponent component, int amount)
        {
            int currentAmount = ComponentsStorage.ContainsKey(component) ? ComponentsStorage[component] : 0;
            if (GetTotalComponentStorage() + amount > MaxComponentStorage)
                return false;

            ComponentsStorage[component] = currentAmount + amount;
            return true;
        }

        /// <summary>
        /// Возвращает текущее количество занятых ячеек на складе компонентов.
        /// </summary>
        public int GetTotalComponentStorage() => ComponentsStorage.Values.Sum();

        #endregion

        #region Производство

        /// <summary>
        /// Выполняет производственный цикл для всех линий сборки.
        /// </summary>
        public void ProcessAssemblyLines()
        {
            if (EngineersCount == 0 || ProductionEfficiency <= 0) return;

            var availableResources = ComponentsStorage.ToDictionary(c => (object)c.Key.ToString(), c => c.Value);
            var producedOutputs = new Dictionary<object, int>();

            foreach (var assemblyLine in AssemblyLines)
            {
                var lineResources = new Dictionary<object, int>(availableResources);
                var lineOutputs = new Dictionary<object, int>();

                if (assemblyLine.Process(lineResources, lineOutputs))
                {
                    ApplyProductionEfficiency(lineOutputs);
                    availableResources = lineResources;

                    foreach (var output in lineOutputs)
                    {
                        producedOutputs[output.Key] = producedOutputs.ContainsKey(output.Key)
                            ? producedOutputs[output.Key] + output.Value
                            : output.Value;
                    }
                }
            }

            UpdateComponentsStorage(availableResources);
            UpdateRobotsStorage(producedOutputs);
        }

        /// <summary>
        /// Применяет коэффициент эффективности к объёму готовой продукции.
        /// </summary>
        private void ApplyProductionEfficiency(Dictionary<object, int> outputs)
        {
            if (ProductionEfficiency >= 1f) return;

            var keys = outputs.Keys.ToList();
            foreach (var key in keys)
            {
                float adjustedOutput = outputs[key] * ProductionEfficiency;
                outputs[key] = (int)Math.Ceiling(adjustedOutput);
                if (outputs[key] <= 0)
                    outputs.Remove(key);
            }
        }

        /// <summary>
        /// Полный производственный цикл (один рабочий период).
        /// </summary>
        public void FullProductionCycle() => ProcessAssemblyLines();

        #endregion

        #region Обновление складов

        /// <summary>
        /// Обновляет склад компонентов на основе результатов производственного цикла.
        /// </summary>
        private void UpdateComponentsStorage(Dictionary<object, int> availableResources)
        {
            ComponentsStorage.Clear();
            foreach (var resource in availableResources)
            {
                if (Enum.TryParse(resource.Key.ToString(), out RoboticsComponent component))
                    ComponentsStorage[component] = resource.Value;
            }
        }

        /// <summary>
        /// Обновляет склад готовых роботов с учётом ограничений вместимости.
        /// </summary>
        private void UpdateRobotsStorage(Dictionary<object, int> producedOutputs)
        {
            foreach (var output in producedOutputs)
            {
                if (Enum.TryParse(output.Key.ToString(), out RobotType robot))
                {
                    int currentAmount = RobotsStorage.ContainsKey(robot) ? RobotsStorage[robot] : 0;
                    int availableSpace = MaxRobotStorage - GetTotalRobotStorage();
                    int amountToAdd = Math.Min(output.Value, availableSpace);

                    if (amountToAdd > 0)
                        RobotsStorage[robot] = currentAmount + amountToAdd;

                    if (amountToAdd < output.Value)
                        System.Diagnostics.Debug.WriteLine($"Превышена вместимость склада роботов! Потеряно {output.Value - amountToAdd} единиц {robot}");
                }
            }
        }

        #endregion

        #region Исследования

        /// <summary>
        /// Повышает уровень исследований, если не достигнут максимум.
        /// </summary>
        public bool UpgradeResearch()
        {
            if (ResearchLevel >= 5) return false;

            decimal upgradeCost = ResearchLevel * 200000m;
            // TODO: Добавить логику проверки бюджета игрока (PlayerResources)

            ResearchLevel++;
            return true;
        }

        /// <summary>
        /// Устанавливает бюджет на исследования.
        /// </summary>
        public void SetResearchBudget(decimal budget)
        {
            ResearchBudget = budget;
        }

        #endregion

        #region Логистика и отчётность

        /// <summary>
        /// Отгружает указанное количество роботов со склада.
        /// </summary>
        public bool ShipRobot(RobotType robotType, int amount)
        {
            if (!RobotsStorage.ContainsKey(robotType) || RobotsStorage[robotType] < amount)
                return false;

            RobotsStorage[robotType] -= amount;
            if (RobotsStorage[robotType] == 0)
                RobotsStorage.Remove(robotType);

            return true;
        }

        /// <summary>
        /// Возвращает общее количество роботов на складе.
        /// </summary>
        public int GetTotalRobotStorage() => RobotsStorage.Values.Sum();

        /// <summary>
        /// Возвращает копию склада готовой продукции.
        /// </summary>
        public Dictionary<RobotType, int> GetProductionOutput() => new(RobotsStorage);

        /// <summary>
        /// Возвращает копию склада компонентов.
        /// </summary>
        public Dictionary<RoboticsComponent, int> GetComponentStorage() => new(ComponentsStorage);

        /// <summary>
        /// Возвращает детальную информацию о состоянии производства.
        /// </summary>
        public Dictionary<string, object> GetProductionInfo() => new()
        {
            { "EngineersCount", EngineersCount },
            { "MaxEngineers", MaxEngineers },
            { "ProductionEfficiency", ProductionEfficiency },
            { "ResearchLevel", ResearchLevel },
            { "ResearchBudget", ResearchBudget },
            { "TotalComponentStorage", GetTotalComponentStorage() },
            { "MaxComponentStorage", MaxComponentStorage },
            { "TotalRobotStorage", GetTotalRobotStorage() },
            { "MaxRobotStorage", MaxRobotStorage },
            { "ActiveAssemblyLines", AssemblyLines.Count },
            { "RobotsInStorage", RobotsStorage.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value) }
        };

        #endregion

        #region Экономика

        /// <summary>
        /// Рассчитывает стоимость обслуживания фабрики.
        /// </summary>
        public decimal CalculateMaintenanceCost()
        {
            decimal baseCost = 15000m;
            decimal engineersCost = EngineersCount * 2500m;
            decimal researchCost = ResearchLevel * 5000m;
            return baseCost + engineersCost + researchCost;
        }

        #endregion

        #region События

        /// <summary>
        /// Вызывается при размещении здания на карте.
        /// </summary>
        public override void OnBuildingPlaced()
        {
            // Производство начинается сразу после размещения
            FullProductionCycle();
        }

        #endregion

        #region Вспомогательные методы (статические)

        /// <summary>
        /// Возвращает стоимость строительства фабрики.
        /// </summary>
        public static decimal GetConstructionCost() => BuildCost;

        /// <summary>
        /// Возвращает материалы, необходимые для строительства.
        /// </summary>
        public static Dictionary<ConstructionMaterial, int> GetRequiredMaterials() =>
            new(RequiredMaterials);

        #endregion
    }
}