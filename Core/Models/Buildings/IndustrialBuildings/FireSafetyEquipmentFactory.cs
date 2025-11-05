using Core.Enums;
using Core.Interfaces;
using Core.Models.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Models.Buildings.IndustrialBuildings
{
    /// <summary>
    /// Фабрика по производству противопожарного оборудования.
    /// Производит широкий ассортимент средств пожаротушения и безопасности.
    /// </summary>
    /// <remarks>
    /// Наследует стандартную коммерческую структуру зданий и реализует интерфейс строительства.
    /// Использует систему цехов (Workshop) для организации производственных линий.
    /// </remarks>
    public class FireSafetyEquipmentFactory : CommercialBuilding, IConstructable<FireSafetyEquipmentFactory>
    {
        #region Construction Configuration

        /// <summary>
        /// Базовая стоимость строительства фабрики
        /// </summary>
        public static decimal BuildCost { get; protected set; } = 420000m;

        /// <summary>
        /// Материалы, необходимые для строительства фабрики
        /// </summary>
        public static Dictionary<ConstructionMaterial, int> RequiredMaterials { get; protected set; }
            = new Dictionary<ConstructionMaterial, int>
            {
                { ConstructionMaterial.Steel, 10 },     // Для несущих конструкций
                { ConstructionMaterial.Concrete, 8 },   // Для фундамента
                { ConstructionMaterial.Plastic, 12 },   // Для труб и коммуникаций
                { ConstructionMaterial.Glass, 2 }       // Для окон и смотровых оконц
            };

        #endregion

        #region Production Enums

        /// <summary>
        /// Компоненты, используемые в производстве противопожарного оборудования
        /// </summary>
        public enum FireSafetyComponent
        {
            /// <summary>Сталь для баллонов и корпусов оборудования</summary>
            Steel,
            /// <summary>Пластик для рукавов и защитных кожухов</summary>
            Plastic,
            /// <summary>Алюминий для легких конструкций и деталей</summary>
            Aluminum,
            /// <summary>Медь для электропроводки и датчиков</summary>
            Copper,
            /// <summary>Химикаты для огнетушащих составов</summary>
            Chemicals,
            /// <summary>Электронные компоненты для систем управления</summary>
            Electronics,
            /// <summary>Резина для уплотнений и гибких соединений</summary>
            Rubber,
            /// <summary>Текстиль для пожарных рукавов и защитной одежды</summary>
            Textile
        }

        /// <summary>
        /// Готовая продукция, производимая фабрикой
        /// </summary>
        public enum FireSafetyProduct
        {
            /// <summary>Переносные огнетушители различных типов</summary>
            FireExtinguisher,
            /// <summary>Пожарные рукава и шланги</summary>
            FireHose,
            /// <summary>Дымовые и тепловые датчики</summary>
            SmokeDetector,
            /// <summary>Центральные панели пожарной сигнализации</summary>
            FireAlarmPanel,
            /// <summary>Системы автоматического пожаротушения</summary>
            SprinklerSystem,
            /// <summary>Противопожарные одеяла</summary>
            FireBlanket,
            /// <summary>Дыхательные аппараты для пожарных</summary>
            BreathingApparatus,
            /// <summary>Защитные шлемы пожарных</summary>
            FireHelmet,
            /// <summary>Аварийные светильники</summary>
            EmergencyLight,
            /// <summary>Эвакуационные указатели и знаки</summary>
            FireExitSign
        }

        #endregion

        #region Factory State

        /// <summary>
        /// Склад сырья и компонентов для производства
        /// </summary>
        public Dictionary<FireSafetyComponent, int> ComponentsStorage { get; private set; }
            = new Dictionary<FireSafetyComponent, int>();

        /// <summary>
        /// Склад готовой продукции
        /// </summary>
        public Dictionary<FireSafetyProduct, int> ProductsStorage { get; private set; }
            = new Dictionary<FireSafetyProduct, int>();

        /// <summary>
        /// Максимальная вместимость склада компонентов
        /// </summary>
        public int MaxComponentStorage { get; private set; } = 1200;

        /// <summary>
        /// Максимальная вместимость склада готовой продукции
        /// </summary>
        public int MaxProductStorage { get; private set; } = 800;

        /// <summary>
        /// Производственные линии фабрики
        /// </summary>
        public List<Workshop> ProductionLines { get; private set; } = new List<Workshop>();

        /// <summary>
        /// Текущее количество рабочих
        /// </summary>
        public int WorkersCount { get; private set; }

        /// <summary>
        /// Максимальное количество рабочих
        /// </summary>
        public int MaxWorkers { get; private set; } = 12;

        /// <summary>
        /// Уровень стандартов безопасности (1-5)
        /// </summary>
        public int SafetyStandardsLevel { get; private set; } = 1;

        /// <summary>
        /// Эффективность производства, зависящая от рабочих и стандартов безопасности
        /// </summary>
        /// <remarks>
        /// Формула: база 50% + 40% от заполненности персонала + 2% за уровень стандартов
        /// </remarks>
        public float ProductionEfficiency => WorkersCount > 0 ?
            0.5f + (WorkersCount / (float)MaxWorkers * 0.4f) + (SafetyStandardsLevel * 0.02f) : 0f;

        #endregion

        #region Constructor

        /// <summary>
        /// Создает новый экземпляр фабрики противопожарного оборудования
        /// </summary>
        public FireSafetyEquipmentFactory() : base(CommercialBuildingType.Factory)
        {
            WorkersCount = 0;
            InitializeProductionLines();
            InitializeStartingComponents();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Инициализирует производственные линии фабрики
        /// </summary>
        private void InitializeProductionLines()
        {
            // Линия огнетушителей и рукавов - базовое оборудование
            var extinguisherLine = new Workshop
            {
                Name = "Линия огнетушителей и рукавов",
                ProductionCycleTime = 8
            };
            extinguisherLine.InputRequirements.Add(FireSafetyComponent.Steel.ToString(), 6);
            extinguisherLine.InputRequirements.Add(FireSafetyComponent.Plastic.ToString(), 4);
            extinguisherLine.InputRequirements.Add(FireSafetyComponent.Chemicals.ToString(), 3);
            extinguisherLine.InputRequirements.Add(FireSafetyComponent.Rubber.ToString(), 2);
            extinguisherLine.OutputProducts.Add(FireSafetyProduct.FireExtinguisher.ToString(), 4);
            extinguisherLine.OutputProducts.Add(FireSafetyProduct.FireHose.ToString(), 3);
            ProductionLines.Add(extinguisherLine);

            // Линия систем сигнализации - электронное оборудование
            var alarmSystemLine = new Workshop
            {
                Name = "Линия систем сигнализации",
                ProductionCycleTime = 12
            };
            alarmSystemLine.InputRequirements.Add(FireSafetyComponent.Electronics.ToString(), 8);
            alarmSystemLine.InputRequirements.Add(FireSafetyComponent.Plastic.ToString(), 3);
            alarmSystemLine.InputRequirements.Add(FireSafetyComponent.Copper.ToString(), 2);
            alarmSystemLine.InputRequirements.Add(FireSafetyComponent.Steel.ToString(), 2);
            alarmSystemLine.OutputProducts.Add(FireSafetyProduct.SmokeDetector.ToString(), 6);
            alarmSystemLine.OutputProducts.Add(FireSafetyProduct.FireAlarmPanel.ToString(), 2);
            alarmSystemLine.OutputProducts.Add(FireSafetyProduct.EmergencyLight.ToString(), 4);
            ProductionLines.Add(alarmSystemLine);

            // Линия систем пожаротушения - сложное оборудование
            var suppressionLine = new Workshop
            {
                Name = "Линия систем пожаротушения",
                ProductionCycleTime = 16
            };
            suppressionLine.InputRequirements.Add(FireSafetyComponent.Steel.ToString(), 10);
            suppressionLine.InputRequirements.Add(FireSafetyComponent.Plastic.ToString(), 6);
            suppressionLine.InputRequirements.Add(FireSafetyComponent.Chemicals.ToString(), 5);
            suppressionLine.InputRequirements.Add(FireSafetyComponent.Rubber.ToString(), 3);
            suppressionLine.OutputProducts.Add(FireSafetyProduct.SprinklerSystem.ToString(), 2);
            ProductionLines.Add(suppressionLine);

            // Линия средств индивидуальной защиты
            var protectionLine = new Workshop
            {
                Name = "Линия средств индивидуальной защиты",
                ProductionCycleTime = 10
            };
            protectionLine.InputRequirements.Add(FireSafetyComponent.Textile.ToString(), 8);
            protectionLine.InputRequirements.Add(FireSafetyComponent.Plastic.ToString(), 4);
            protectionLine.InputRequirements.Add(FireSafetyComponent.Aluminum.ToString(), 3);
            protectionLine.InputRequirements.Add(FireSafetyComponent.Rubber.ToString(), 2);
            protectionLine.OutputProducts.Add(FireSafetyProduct.FireBlanket.ToString(), 5);
            protectionLine.OutputProducts.Add(FireSafetyProduct.BreathingApparatus.ToString(), 2);
            protectionLine.OutputProducts.Add(FireSafetyProduct.FireHelmet.ToString(), 4);
            ProductionLines.Add(protectionLine);

            // Линия знаков и указателей - простая продукция
            var signageLine = new Workshop
            {
                Name = "Линия знаков и указателей",
                ProductionCycleTime = 6
            };
            signageLine.InputRequirements.Add(FireSafetyComponent.Plastic.ToString(), 5);
            signageLine.InputRequirements.Add(FireSafetyComponent.Electronics.ToString(), 2);
            signageLine.InputRequirements.Add(FireSafetyComponent.Aluminum.ToString(), 3);
            signageLine.OutputProducts.Add(FireSafetyProduct.FireExitSign.ToString(), 8);
            ProductionLines.Add(signageLine);
        }

        /// <summary>
        /// Инициализирует стартовый запас компонентов на складе
        /// </summary>
        private void InitializeStartingComponents()
        {
            // Инициализируем все компоненты нулевыми значениями
            foreach (FireSafetyComponent component in Enum.GetValues(typeof(FireSafetyComponent)))
            {
                ComponentsStorage[component] = 0;
            }

            // Стартовый набор компонентов для начала производства
            AddComponent(FireSafetyComponent.Steel, 150);
            AddComponent(FireSafetyComponent.Plastic, 120);
            AddComponent(FireSafetyComponent.Aluminum, 80);
            AddComponent(FireSafetyComponent.Copper, 50);
            AddComponent(FireSafetyComponent.Chemicals, 60);
            AddComponent(FireSafetyComponent.Electronics, 40);
            AddComponent(FireSafetyComponent.Rubber, 70);
            AddComponent(FireSafetyComponent.Textile, 90);
        }

        #endregion

        #region Workforce Management

        /// <summary>
        /// Устанавливает количество рабочих на фабрике
        /// </summary>
        /// <param name="count">Количество рабочих (автоматически ограничивается максимумом)</param>
        public void SetWorkersCount(int count)
        {
            WorkersCount = Math.Clamp(count, 0, MaxWorkers);
        }

        #endregion

        #region Component Management

        /// <summary>
        /// Добавляет компоненты на склад
        /// </summary>
        /// <param name="component">Тип компонента</param>
        /// <param name="amount">Количество для добавления</param>
        /// <returns>True если компоненты успешно добавлены, false если недостаточно места</returns>
        public bool AddComponent(FireSafetyComponent component, int amount)
        {
            int currentAmount = ComponentsStorage.ContainsKey(component) ? ComponentsStorage[component] : 0;
            if (GetTotalComponentStorage() + amount > MaxComponentStorage)
                return false;

            ComponentsStorage[component] = currentAmount + amount;
            return true;
        }

        /// <summary>
        /// Возвращает общее количество компонентов на складе
        /// </summary>
        public int GetTotalComponentStorage()
        {
            return ComponentsStorage.Values.Sum();
        }

        #endregion

        #region Production Core

        /// <summary>
        /// Запускает производственный цикл на всех линиях
        /// </summary>
        /// <remarks>
        /// Процесс:
        /// 1. Проверяет наличие рабочих и эффективность
        /// 2. Для каждой линии проверяет доступность ресурсов
        /// 3. Запускает производство с учетом эффективности
        /// 4. Обновляет склады компонентов и продукции
        /// </remarks>
        public void ProcessProductionLines()
        {
            if (WorkersCount == 0 || ProductionEfficiency <= 0) return;

            var availableResources = new Dictionary<object, int>();
            foreach (var component in ComponentsStorage)
            {
                availableResources.Add(component.Key.ToString(), component.Value);
            }

            var producedOutputs = new Dictionary<object, int>();

            foreach (var productionLine in ProductionLines)
            {
                var lineResources = new Dictionary<object, int>(availableResources);
                var lineOutputs = new Dictionary<object, int>();

                if (productionLine.Process(lineResources, lineOutputs))
                {
                    ApplyProductionEfficiency(lineOutputs);
                    availableResources = lineResources;

                    foreach (var output in lineOutputs)
                    {
                        if (producedOutputs.ContainsKey(output.Key))
                            producedOutputs[output.Key] += output.Value;
                        else
                            producedOutputs[output.Key] = output.Value;
                    }
                }
            }

            UpdateComponentsStorage(availableResources);
            UpdateProductsStorage(producedOutputs);
        }

        /// <summary>
        /// Применяет коэффициент эффективности к произведенной продукции
        /// </summary>
        /// <param name="outputs">Словарь с произведенной продукцией</param>
        private void ApplyProductionEfficiency(Dictionary<object, int> outputs)
        {
            if (ProductionEfficiency >= 1f) return;

            var keys = outputs.Keys.ToList();
            foreach (var key in keys)
            {
                outputs[key] = (int)(outputs[key] * ProductionEfficiency);
                if (outputs[key] <= 0)
                    outputs.Remove(key);
            }
        }

        /// <summary>
        /// Выполняет полный производственный цикл
        /// </summary>
        public void FullProductionCycle()
        {
            ProcessProductionLines();
        }

        #endregion

        #region Storage Management

        /// <summary>
        /// Обновляет склад компонентов на основе результатов производства
        /// </summary>
        private void UpdateComponentsStorage(Dictionary<object, int> availableResources)
        {
            ComponentsStorage.Clear();
            foreach (var resource in availableResources)
            {
                if (Enum.TryParse<FireSafetyComponent>(resource.Key.ToString(), out var component))
                {
                    ComponentsStorage[component] = resource.Value;
                }
            }
        }

        /// <summary>
        /// Обновляет склад продукции с учетом ограничений вместимости
        /// </summary>
        private void UpdateProductsStorage(Dictionary<object, int> producedOutputs)
        {
            foreach (var output in producedOutputs)
            {
                if (Enum.TryParse<FireSafetyProduct>(output.Key.ToString(), out var product))
                {
                    int currentAmount = ProductsStorage.ContainsKey(product) ? ProductsStorage[product] : 0;
                    int availableSpace = MaxProductStorage - GetTotalProductStorage();
                    int amountToAdd = Math.Min(output.Value, availableSpace);

                    if (amountToAdd > 0)
                    {
                        ProductsStorage[product] = currentAmount + amountToAdd;
                    }

                    if (amountToAdd < output.Value)
                    {
                        System.Diagnostics.Debug.WriteLine($"Превышена вместимость склада! Потеряно {output.Value - amountToAdd} единиц продукции {product}");
                    }
                }
            }
        }

        /// <summary>
        /// Возвращает общее количество продукции на складе
        /// </summary>
        public int GetTotalProductStorage()
        {
            return ProductsStorage.Values.Sum();
        }

        #endregion

        #region Factory Upgrades

        /// <summary>
        /// Повышает уровень стандартов безопасности фабрики
        /// </summary>
        /// <returns>True если улучшение выполнено, false если достигнут максимум</returns>
        public bool UpgradeSafetyStandards()
        {
            if (SafetyStandardsLevel >= 5) return false;

            decimal upgradeCost = SafetyStandardsLevel * 150000m;
            // TODO: Интеграция с системой финансов игрока
            // if (PlayerResources.Money < upgradeCost) return false;

            SafetyStandardsLevel++;
            return true;
        }

        #endregion

        #region Product Management

        /// <summary>
        /// Отгружает продукцию со склада
        /// </summary>
        /// <param name="product">Тип продукции</param>
        /// <param name="amount">Количество для отгрузки</param>
        /// <returns>True если отгрузка выполнена, false если недостаточно продукции</returns>
        public bool ShipProduct(FireSafetyProduct product, int amount)
        {
            if (!ProductsStorage.ContainsKey(product) || ProductsStorage[product] < amount)
                return false;

            ProductsStorage[product] -= amount;
            if (ProductsStorage[product] == 0)
                ProductsStorage.Remove(product);

            return true;
        }

        /// <summary>
        /// Возвращает копию склада готовой продукции
        /// </summary>
        public Dictionary<FireSafetyProduct, int> GetProductionOutput()
        {
            return new Dictionary<FireSafetyProduct, int>(ProductsStorage);
        }

        /// <summary>
        /// Возвращает копию склада компонентов
        /// </summary>
        public Dictionary<FireSafetyComponent, int> GetComponentStorage()
        {
            return new Dictionary<FireSafetyComponent, int>(ComponentsStorage);
        }

        #endregion

        #region Safety & Quality System

        /// <summary>
        /// Проверяет соответствие фабрики минимальным требованиям безопасности
        /// </summary>
        /// <returns>True если фабрика соответствует стандартам</returns>
        public bool MeetsSafetyRegulations()
        {
            return SafetyStandardsLevel >= 2;
        }

        /// <summary>
        /// Рассчитывает рейтинг безопасности производимой продукции
        /// </summary>
        /// <returns>Рейтинг от 0.7 до 1.0</returns>
        public float GetProductSafetyRating()
        {
            float baseRating = 0.7f;
            float standardsBonus = SafetyStandardsLevel * 0.06f;
            return Math.Min(baseRating + standardsBonus, 1.0f);
        }

        #endregion

        #region Economics

        /// <summary>
        /// Рассчитывает стоимость обслуживания фабрики
        /// </summary>
        /// <returns>Месячная стоимость содержания</returns>
        public decimal CalculateMaintenanceCost()
        {
            decimal baseCost = 12000m;
            decimal workersCost = WorkersCount * 2000m;
            decimal standardsCost = SafetyStandardsLevel * 3000m;
            return baseCost + workersCost + standardsCost;
        }

        #endregion

        #region Reporting

        /// <summary>
        /// Возвращает подробную информацию о состоянии производства
        /// </summary>
        /// <returns>Словарь с ключевыми показателями</returns>
        public Dictionary<string, object> GetProductionInfo()
        {
            return new Dictionary<string, object>
            {
                { "WorkersCount", WorkersCount },
                { "MaxWorkers", MaxWorkers },
                { "ProductionEfficiency", ProductionEfficiency },
                { "SafetyStandardsLevel", SafetyStandardsLevel },
                { "TotalComponentStorage", GetTotalComponentStorage() },
                { "MaxComponentStorage", MaxComponentStorage },
                { "TotalProductStorage", GetTotalProductStorage() },
                { "MaxProductStorage", MaxProductStorage },
                { "ActiveProductionLines", ProductionLines.Count },
                { "ProductsInStorage", ProductsStorage.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value) },
                { "MeetsSafetyRegulations", MeetsSafetyRegulations() },
                { "ProductSafetyRating", GetProductSafetyRating() }
            };
        }

        #endregion

        #region Building Events

        /// <summary>
        /// Вызывается при размещении здания на карте
        /// </summary>
        public override void OnBuildingPlaced()
        {
            FullProductionCycle();
        }

        #endregion

        #region Static Construction Interface

        /// <summary>
        /// Возвращает стоимость строительства фабрики
        /// </summary>
        public static decimal GetConstructionCost()
        {
            return BuildCost;
        }

        /// <summary>
        /// Возвращает материалы, необходимые для строительства
        /// </summary>
        public static Dictionary<ConstructionMaterial, int> GetRequiredMaterials()
        {
            return new Dictionary<ConstructionMaterial, int>(RequiredMaterials);
        }

        #endregion
    }
}