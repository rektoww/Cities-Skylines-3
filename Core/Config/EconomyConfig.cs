using System.Collections.Generic;
using Core.Enums;

namespace Core.Config
{
    /// <summary>
    /// Конфигурация экономики: цены на материалы, базовые настройки
    /// </summary>
    public static class EconomyConfig
    {
        /// <summary>
        /// Цены за единицу строительного материала (в валюте игры)
        /// </summary>
        public static readonly Dictionary<ConstructionMaterial, decimal> MaterialPrices = new()
        {
            { ConstructionMaterial.Steel, 120m },
            { ConstructionMaterial.Concrete, 50m },
            { ConstructionMaterial.Glass, 80m },
            { ConstructionMaterial.Plastic, 40m }
        };

        /// <summary>
        /// Начальный бюджет игрока по умолчанию
        /// </summary>
        public const decimal DefaultCityBudget = 8_000_000m;

        /// <summary>
        /// Стартовые строительные материалы для игрока
        /// </summary>
        public static readonly Dictionary<ConstructionMaterial, int> DefaultStartMaterials = new()
        {
            { ConstructionMaterial.Steel, 10 },
            { ConstructionMaterial.Concrete, 10 },
            { ConstructionMaterial.Glass, 10 }
        };
    }
}
