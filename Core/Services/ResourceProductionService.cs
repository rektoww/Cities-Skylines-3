using System;
using System.Collections.Generic;
using Core.Enums;
using Core.Resourses;

namespace Core.Services
{
    /// <summary>
    /// Сервис производства ресурсов и продажи за деньги.
    /// Управляет производством материалов через шахты/заводы и их продажей.
    /// </summary>
    public class ResourceProductionService
    {
        private readonly PlayerResources _playerResources;
        private readonly FinancialSystem _financialSystem;

        /// <summary>
        /// Количество ресурсов, добываемых шахтой за тик (для теста).
        /// </summary>
        public int MineProductionRate { get; set; } = 5;

        public ResourceProductionService(PlayerResources playerResources, FinancialSystem financialSystem)
        {
            _playerResources = playerResources;
            _financialSystem = financialSystem;
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
        /// Продает указанное количество материалов по рыночным ценам.
        /// Деньги зачисляются в FinancialSystem как доход от торговли.
        /// </summary>
        /// <param name="material">Тип материала</param>
        /// <param name="quantity">Количество для продажи</param>
        /// <param name="pricePerUnit">Цена за единицу</param>
        /// <param name="totalRevenue">Общая выручка от продажи</param>
        /// <returns>true, если продажа прошла успешно</returns>
        public bool TrySellMaterials(ConstructionMaterial material, int quantity, decimal pricePerUnit, out decimal totalRevenue)
        {
            totalRevenue = 0m;
            
            if (quantity <= 0)
                return false;

            if (!_playerResources.StoredMaterials.ContainsKey(material) || 
                _playerResources.StoredMaterials[material] < quantity)
                return false;

            totalRevenue = pricePerUnit * quantity;

            // Списываем материалы
            _playerResources.StoredMaterials[material] -= quantity;

            // Зачисляем деньги через финансовую систему (доход от продажи)
            _financialSystem.AddIncome(totalRevenue, $"Sale: {material} x{quantity}");

            // Синхронизируем PlayerResources.Balance
            _playerResources.Balance += totalRevenue;

            return true;
        }

        /// <summary>
        /// Продает все доступные материалы одного типа.
        /// </summary>
        public bool TrySellAllMaterials(ConstructionMaterial material, decimal pricePerUnit, out decimal totalRevenue)
        {
            totalRevenue = 0m;
            
            if (!_playerResources.StoredMaterials.ContainsKey(material))
                return false;

            int quantity = _playerResources.StoredMaterials[material];
            if (quantity == 0)
                return false;

            return TrySellMaterials(material, quantity, pricePerUnit, out totalRevenue);
        }
    }
}
