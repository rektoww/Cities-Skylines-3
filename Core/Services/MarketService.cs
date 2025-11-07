using System;
using System.Collections.Generic;
using Core.Enums;
using Core.Resourses;

namespace Core.Services
{
    /// <summary>
    /// Сервис импорта материалов из внешнего мира.
    /// Использует ExternalConnectionsManager для управления торговлей.
    /// </summary>
    public class MarketService
    {
        private readonly ExternalConnectionsManager _externalConnections;

        public MarketService(ExternalConnectionsManager externalConnections)
        {
            _externalConnections = externalConnections;
        }

        /// <summary>
        /// Импорт (покупка) материалов из внешнего мира
        /// </summary>
        public bool TryBuyMaterials(
            Dictionary<ConstructionMaterial, int> quantities,
            Dictionary<ConstructionMaterial, decimal> prices,
            out decimal totalCost)
        {
            return _externalConnections.TryImportMaterials(quantities, prices, out totalCost);
        }
    }
}
