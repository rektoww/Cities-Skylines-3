namespace Core.Models.Components
{
    /// <summary>
    /// Представляет цех на заводе, отвечающий за определенный этап производства.
    /// </summary>
    public class Workshop
    {
        /// <summary>
        /// Описательное название функции мастерской (например, «Сборочная линия»).
        /// </summary>
        public string Name { get; set; } = "DefaultName"; // Пока необязательно, но может пригодиться при поиске определенного цеха

        /// <summary>
        /// Ресурсы, необходимые для одного производственного цикла.
        /// </summary>
        public Dictionary<object, int> InputRequirements { get; protected set; } = new();

        /// <summary>
        /// Продукция, производимая в одном производственном цикле.
        /// </summary>
        public Dictionary<object, int> OutputProducts { get; protected set; } = new();

        /// <summary>
        /// Время (в тактах моделирования), необходимое для одного производственного цикла.
        /// Пока не уверен, что пригодится
        /// </summary>
        public int ProductionCycleTime { get; set; } = 10; // Заглушка магическое число

        /// <summary>
        /// Обрабатывает один такт производственного цикла.
        /// </summary>
        /// <param name="availableInputs">Словарь доступных ресурсов.</param>
        /// <param name="generatedOutputs">Словарь для размещения произведенных материалов.</param>
        /// <returns>True, если производственный цикл был завершен, в противном случае — false.</returns>
        public bool Process(Dictionary<object, int> availableInputs, Dictionary<object, int> generatedOutputs)
        {
            // 1. Проверка на наличие ресурсов
            foreach (var required in InputRequirements)
            {
                if (!availableInputs.ContainsKey(required.Key) || availableInputs[required.Key] < required.Value)
                {
                    return false; // Недостаточно ресурсов для производства
                }
            }

            // Выполнение цикла производства
            foreach (var required in InputRequirements)
            {
                availableInputs[required.Key] -= required.Value;
            }

            foreach (var product in OutputProducts)
            {
                if (generatedOutputs.ContainsKey(product.Key))
                {
                    generatedOutputs[product.Key] += product.Value;
                }
                else
                {
                    generatedOutputs[product.Key] = product.Value;
                }
            }

            return true;
        }
    }
}