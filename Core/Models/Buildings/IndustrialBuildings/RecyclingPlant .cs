using Core.Enums;
using Core.Enums.Core.Enums;
using Core.Interfaces;
using Core.Models.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Buildings.IndustrialBuildings
{
    public class RecyclingPlant : CommercialBuilding
    {
        #region Static Properties - Construction Cost

        /// <summary>
        /// Стоимость постройки завода переработки
        /// </summary>
        public static decimal BuildCost => 280000m;

        /// <summary>
        /// Материалы, необходимые для строительства завода переработки
        /// </summary>
        public static Dictionary<ConstructionMaterial, int> RequiredMaterials =>
            new Dictionary<ConstructionMaterial, int>
            {
                { ConstructionMaterial.Steel, 6 },
                { ConstructionMaterial.Concrete, 8 },
                { ConstructionMaterial.Glass, 4 }
            };

        #endregion

        #region Simplified Enums

        /// <summary>
        /// Типы отходов для переработки
        /// </summary>
        public enum WasteMaterial
        {
            Plastic,    // Пластиковые отходы
            Paper,      // Бумажные отходы
            Glass,      // Стеклянные отходы
            Metal,      // Металлические отходы
            Organic     // Органические отходы
        }

        /// <summary>
        /// Типы производимой переработанной продукции
        /// </summary>
        public enum RecycledProduct
        {
            RecycledPlastic,    // Переработанный пластик
            RecycledPaper,      // Переработанная бумага
            RecycledGlass,      // Переработанное стекло
            RecycledMetal,      // Переработанный металл
            Compost,           // Компост
            Biogas            // Биогаз
        }

        #endregion

        /// <summary>Количество отходов на складе</summary>
        public Dictionary<WasteMaterial, int> WasteStorage { get; private set; } = new Dictionary<WasteMaterial, int>();

        /// <summary>Количество переработанной продукции на складе</summary>
        public Dictionary<RecycledProduct, int> ProductsStorage { get; private set; } = new Dictionary<RecycledProduct, int>();

        /// <summary>Максимальная вместимость склада отходов</summary>
        public int MaxWasteStorage { get; private set; }

        /// <summary>Максимальная вместимость склада продукции</summary>
        public int MaxProductStorage { get; private set; }

        /// <summary>Производственные цеха завода</summary>
        public List<Workshop> Workshops { get; private set; } = new List<Workshop>();

        /// <summary>Текущее количество рабочих</summary>
        public int WorkersCount { get; private set; }

        /// <summary>Максимальное количество рабочих</summary>
        public int MaxWorkers { get; private set; }

        /// <summary>Уровень экологической эффективности</summary>
        public float EcoEfficiency { get; private set; } = 0.7f;

        // Коэффициент эффективности переработки
        public float RecyclingEfficiency => WorkersCount > 0 ?
            (0.4f + (WorkersCount / (float)MaxWorkers) * 0.4f) * EcoEfficiency : 0f;

        /// <summary>
        /// Создает новый завод переработки
        /// </summary>
        public RecyclingPlant() : base(CommercialBuildingType.Factory)
        {
            MaxWasteStorage = 2000;
            MaxProductStorage = 1200;
            MaxWorkers = 12;
            WorkersCount = 0;

            InitializeWorkshops();
            InitializeStartingWaste();
        }

        /// <summary>
        /// Инициализирует цеха завода переработки
        /// </summary>
        private void InitializeWorkshops()
        {
            // Цех переработки пластика
            var plasticWorkshop = new Workshop
            {
                Name = "Цех переработки пластика",
                ProductionCycleTime = 5
            };
            plasticWorkshop.InputRequirements.Add("Plastic", 8);
            plasticWorkshop.OutputProducts.Add("RecycledPlastic", 6);
            Workshops.Add(plasticWorkshop);

            // Цех переработки бумаги
            var paperWorkshop = new Workshop
            {
                Name = "Цех переработки бумаги",
                ProductionCycleTime = 4
            };
            paperWorkshop.InputRequirements.Add("Paper", 10);
            paperWorkshop.OutputProducts.Add("RecycledPaper", 8);
            Workshops.Add(paperWorkshop);

            // Цех переработки стекла
            var glassWorkshop = new Workshop
            {
                Name = "Цех переработки стекла",
                ProductionCycleTime = 6
            };
            glassWorkshop.InputRequirements.Add("Glass", 12);
            glassWorkshop.OutputProducts.Add("RecycledGlass", 10);
            Workshops.Add(glassWorkshop);

            // Цех переработки металла
            var metalWorkshop = new Workshop
            {
                Name = "Цех переработки металла",
                ProductionCycleTime = 7
            };
            metalWorkshop.InputRequirements.Add("Metal", 15);
            metalWorkshop.OutputProducts.Add("RecycledMetal", 12);
            Workshops.Add(metalWorkshop);

            // Цех переработки органики
            var organicWorkshop = new Workshop
            {
                Name = "Цех переработки органики",
                ProductionCycleTime = 8
            };
            organicWorkshop.InputRequirements.Add("Organic", 20);
            organicWorkshop.OutputProducts.Add("Compost", 15);
            organicWorkshop.OutputProducts.Add("Biogas", 8);
            Workshops.Add(organicWorkshop);
        }

        /// <summary>
        /// Инициализирует стартовые отходы
        /// </summary>
        private void InitializeStartingWaste()
        {
            AddWaste(WasteMaterial.Plastic, 400);
            AddWaste(WasteMaterial.Paper, 300);
            AddWaste(WasteMaterial.Glass, 250);
            AddWaste(WasteMaterial.Metal, 200);
            AddWaste(WasteMaterial.Organic, 500);
        }

        /// <summary>
        /// Устанавливает количество рабочих на заводе
        /// </summary>
        /// <param name="count">Количество рабочих для установки</param>
        public void SetWorkersCount(int count)
        {
            WorkersCount = Math.Min(count, MaxWorkers);
        }

        /// <summary>
        /// Устанавливает уровень экологической эффективности
        /// </summary>
        /// <param name="efficiency">Уровень эффективности от 0.1 до 1.0</param>
        public void SetEcoEfficiency(float efficiency)
        {
            EcoEfficiency = Math.Clamp(efficiency, 0.1f, 1.0f);
        }

        /// <summary>
        /// Добавляет отходы на склад завода
        /// </summary>
        /// <param name="material">Тип добавляемых отходов</param>
        /// <param name="amount">Количество отходов для добавления</param>
        /// <returns>True если отходы успешно добавлены, false в противном случае</returns>
        public bool AddWaste(WasteMaterial material, int amount)
        {
            int currentAmount = WasteStorage.ContainsKey(material) ? WasteStorage[material] : 0;
            if (GetTotalWasteStorage() + amount > MaxWasteStorage)
                return false;

            WasteStorage[material] = currentAmount + amount;
            return true;
        }

        /// <summary>
        /// Получает общее количество отходов на складе
        /// </summary>
        /// <returns>Общее количество отходов</returns>
        public int GetTotalWasteStorage()
        {
            return WasteStorage.Values.Sum();
        }

        /// <summary>
        /// Запускает процессы переработки во всех цехах завода
        /// </summary>
        public void ProcessWorkshops()
        {
            if (WorkersCount == 0 || RecyclingEfficiency <= 0) return;

            var availableResources = new Dictionary<object, int>();

            // Конвертируем enum в строки для совместимости с Workshop
            foreach (var waste in WasteStorage)
            {
                availableResources.Add(waste.Key.ToString(), waste.Value);
            }

            // Добавляем существующую продукцию как доступный ресурс
            foreach (var product in ProductsStorage)
            {
                availableResources.Add(product.Key.ToString(), product.Value);
            }

            var producedOutputs = new Dictionary<object, int>();

            foreach (var workshop in Workshops)
            {
                // Создаем копию ресурсов для каждого цеха
                var workshopResources = new Dictionary<object, int>(availableResources);
                var workshopOutputs = new Dictionary<object, int>();

                if (workshop.Process(workshopResources, workshopOutputs))
                {
                    // Учитываем эффективность переработки
                    ApplyRecyclingEfficiency(workshopOutputs);

                    // Обновляем ресурсы только если переработка успешна
                    availableResources = workshopResources;

                    // Добавляем выходы цеха в общие выходы
                    foreach (var output in workshopOutputs)
                    {
                        if (producedOutputs.ContainsKey(output.Key))
                            producedOutputs[output.Key] += output.Value;
                        else
                            producedOutputs[output.Key] = output.Value;
                    }
                }
            }

            // Обновляем хранилище отходов
            UpdateWasteStorage(availableResources);

            // Обновляем хранилище продукции с проверкой вместимости
            UpdateProductsStorage(producedOutputs);
        }

        /// <summary>
        /// Применяет коэффициент эффективности к переработанной продукции
        /// </summary>
        private void ApplyRecyclingEfficiency(Dictionary<object, int> outputs)
        {
            if (RecyclingEfficiency >= 1f) return;

            var keys = outputs.Keys.ToList();
            foreach (var key in keys)
            {
                outputs[key] = (int)(outputs[key] * RecyclingEfficiency);
                if (outputs[key] <= 0)
                    outputs.Remove(key);
            }
        }

        /// <summary>
        /// Обновляет хранилище отходов из результатов обработки
        /// </summary>
        private void UpdateWasteStorage(Dictionary<object, int> availableResources)
        {
            WasteStorage.Clear();
            foreach (var resource in availableResources)
            {
                if (Enum.TryParse<WasteMaterial>(resource.Key.ToString(), out var waste))
                {
                    WasteStorage[waste] = resource.Value;
                }
            }
        }

        /// <summary>
        /// Обновляет хранилище продукции с проверкой вместимости
        /// </summary>
        private void UpdateProductsStorage(Dictionary<object, int> producedOutputs)
        {
            foreach (var output in producedOutputs)
            {
                if (Enum.TryParse<RecycledProduct>(output.Key.ToString(), out var product))
                {
                    int currentAmount = ProductsStorage.ContainsKey(product) ? ProductsStorage[product] : 0;
                    int availableSpace = MaxProductStorage - GetTotalProductStorage();
                    int amountToAdd = Math.Min(output.Value, availableSpace);

                    if (amountToAdd > 0)
                    {
                        ProductsStorage[product] = currentAmount + amountToAdd;
                    }

                    // Логируем потерю продукции при переполнении
                    if (amountToAdd < output.Value)
                    {
                        System.Diagnostics.Debug.WriteLine($"Превышена вместимость склада! Потеряно {output.Value - amountToAdd} единиц продукции {product}");
                    }
                }
            }
        }

        /// <summary>
        /// Выполняет полный рабочий цикл завода
        /// </summary>
        public void FullRecyclingCycle()
        {
            ProcessWorkshops();
        }

        /// <summary>
        /// Получает текущие запасы переработанной продукции
        /// </summary>
        /// <returns>Словарь с текущими запасами продукции</returns>
        public Dictionary<RecycledProduct, int> GetRecyclingOutput()
        {
            return new Dictionary<RecycledProduct, int>(ProductsStorage);
        }

        /// <summary>
        /// Получает текущее количество отходов на складе
        /// </summary>
        /// <returns>Словарь с текущими запасами отходов</returns>
        public Dictionary<WasteMaterial, int> GetWasteStorage()
        {
            return new Dictionary<WasteMaterial, int>(WasteStorage);
        }

        /// <summary>
        /// Потребляет продукцию (продажа или использование)
        /// </summary>
        /// <param name="product">Тип потребляемой продукции</param>
        /// <param name="amount">Количество продукции для потребления</param>
        /// <returns>True если продукция успешно потреблена, false в противном случае</returns>
        public bool ConsumeProduct(RecycledProduct product, int amount)
        {
            if (!ProductsStorage.ContainsKey(product) || ProductsStorage[product] < amount)
                return false;

            ProductsStorage[product] -= amount;
            if (ProductsStorage[product] == 0)
                ProductsStorage.Remove(product);

            return true;
        }

        /// <summary>
        /// Получает общее количество продукции на складе
        /// </summary>
        /// <returns>Общее количество продукции</returns>
        public int GetTotalProductStorage()
        {
            return ProductsStorage.Values.Sum();
        }

        /// <summary>
        /// Получает информацию о производственной эффективности
        /// </summary>
        public Dictionary<string, object> GetRecyclingInfo()
        {
            return new Dictionary<string, object>
            {
                { "WorkersCount", WorkersCount },
                { "MaxWorkers", MaxWorkers },
                { "EcoEfficiency", EcoEfficiency },
                { "RecyclingEfficiency", RecyclingEfficiency },
                { "TotalWasteStorage", GetTotalWasteStorage() },
                { "MaxWasteStorage", MaxWasteStorage },
                { "TotalProductStorage", GetTotalProductStorage() },
                { "MaxProductStorage", MaxProductStorage },
                { "ActiveWorkshops", Workshops.Count }
            };
        }

        public override void OnBuildingPlaced()
        {
            // Базовая логика при размещении
            FullRecyclingCycle();
        }
    }
}
