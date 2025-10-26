using Core.Enums;
using System.Collections.Generic;

namespace Core.Interfaces
{
    /// <summary>
    /// Определяет контракт на здания, которые могут быть построены с течением времени.
    /// </summary>
    public interface IConstructable
    {
        /// <summary>
        /// Получает сбор материалов и их необходимое количество для строительства.
        /// </summary>
        Dictionary<ConstructionMaterial, int> RequiredMaterials { get; }

        /// <summary>
        /// Получает общее время (может в милисекундах, может в собственных единицмх измерения игры, как тики в майне),
        /// необходимое для строительства здания.
        /// </summary>
        int ConstructionTime { get; }

        /// <summary>
        /// Получает текущий прогресс строительства от 0,0 до 1,0. (как проценты)
        /// </summary>
        double ConstructionProgress { get; }

        /// <summary>
        /// Ход прогресса строительства на заданную сумму.
        /// </summary>
        /// <param name="amount">Количество прогресса, которое нужно добавить</param>
        void AdvanceConstruction(double amount);

        /// <summary>
        /// Флаг, указывающий, завершено ли строительство.
        /// </summary>
        bool IsConstructed { get; }
    }
}