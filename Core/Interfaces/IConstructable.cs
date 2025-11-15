using Core.Enums;
using Core.Models.Map;
using System.Collections.Generic;

namespace Core.Interfaces
{
    /// <summary>
    /// Определяет контракт на здания, которые могут быть построены с течением времени.
    /// </summary>
    public interface IConstructable
    {
        /// <summary>
        /// Цена на строительство
        /// </summary>
        decimal BuildCost { get; }

        /// <summary>
        /// Необходимое количество материалов для строительства.
        /// </summary>
        Dictionary<ConstructionMaterial, int> RequiredMaterials { get; }

        /// <summary>
        /// Размещение на карте
        /// </summary>
        bool TryPlace(int x, int y, GameMap map);
    }
}