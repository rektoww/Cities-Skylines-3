using System;
using System.Collections.Generic;
using Core.Enums;
using Core.Resourses;

namespace Core.Services
{
    /// <summary>
    /// Экспорта во внешний мир.
    /// </summary>
    public class ResourceProductionService
    {
        private readonly PlayerResources _playerResources;
        private readonly ExternalConnectionsManager _externalConnections;

       

        public ResourceProductionService(PlayerResources playerResources, ExternalConnectionsManager externalConnections)
        {
            _playerResources = playerResources;
            _externalConnections = externalConnections;
        }

 
        /// <summary>
        /// Экспорт (продажа) материалов во внешний мир
        /// </summary>
        public bool TrySellMaterials(ConstructionMaterial material, int quantity, decimal pricePerUnit, out decimal totalRevenue)
        {
            return _externalConnections.TryExportMaterials(material, quantity, pricePerUnit, out totalRevenue);
        }

        /// <summary>
        /// Экспорт всех доступных материалов одного типа
        /// </summary>
        public bool TrySellAllMaterials(ConstructionMaterial material, decimal pricePerUnit, out decimal totalRevenue)
        {
            return _externalConnections.TryExportAllMaterials(material, pricePerUnit, out totalRevenue);
        }
    }
}
