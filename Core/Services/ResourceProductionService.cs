using System;
using System.Collections.Generic;
using Core.Enums;
using Core.Resourses;

namespace Core.Services
{
    /// <summary>
    /// Сервис производства ресурсов и экспорта во внешний мир.
    /// Управляет производством материалов через шахты/заводы и экспортом через ExternalConnectionsManager.
    /// </summary>
    public class ResourceProductionService
    {
        private readonly PlayerResources _playerResources;
        private readonly ExternalConnectionsManager _externalConnections;

        /// <summary>
        /// Количество ресурсов, добываемых шахтой за тик (для теста).
        /// </summary>
        public int MineProductionRate { get; set; } = 5;

        public ResourceProductionService(PlayerResources playerResources, ExternalConnectionsManager externalConnections)
        {
            _playerResources = playerResources;
            _externalConnections = externalConnections;
        }

        /// <summary>
        /// Симулирует работу шахты: добывает случайные материалы.
        /// </summary>
        public void SimulateMineProduction()
        {
            // Для теста: шахта добывает случайный материал
            var random = new Random();
            var materials = new[] { ConstructionMaterial.Steel, ConstructionMaterial.Concrete };
            var material = materials[random.Next(materials.Length)];

            if (_playerResources.StoredMaterials.ContainsKey(material))
                _playerResources.StoredMaterials[material] += MineProductionRate;
            else
                _playerResources.StoredMaterials[material] = MineProductionRate;
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
