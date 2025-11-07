using System;
using System.Collections.Generic;
using Core.Enums;
using Core.Resourses;

namespace Core.Services
{
    /// <summary>
    /// Простой сервис рынка: покупка строительных материалов за деньги.
    /// </summary>
    public class MarketService
    {
        /// <summary>
        /// Попытка купить набор материалов по указанным ценам.
        /// </summary>
        /// <param name="quantities">Материалы и их количество для покупки</param>
        /// <param name="prices">Цены за единицу материалов</param>
        /// <param name="finance">Финансовая система (для списания средств и отчетности)</param>
        /// <param name="playerResources">Инвентарь игрока (для зачисления материалов)</param>
        /// <param name="category">Категория расхода для отчета</param>
        /// <returns>true, если покупка прошла успешно</returns>
        public bool TryBuyMaterials(
            Dictionary<ConstructionMaterial, int> quantities,
            Dictionary<ConstructionMaterial, decimal> prices,
            FinancialSystem finance,
            PlayerResources playerResources,
            string category = "Materials: Purchase")
        {
            if (quantities == null || quantities.Count == 0)
                return false;

            decimal total = 0m;
            foreach (var item in quantities)
            {
                if (item.Value <= 0) return false;
                if (!prices.TryGetValue(item.Key, out var unitPrice)) return false;
                total += unitPrice * item.Value;
            }

            // Проверяем и списываем деньги через финансовую систему (она сама проверит нехватку средств)
            if (!finance.AddExpense(total, category))
                return false;

            // Обновляем инвентарь игрока
            foreach (var item in quantities)
            {
                if (playerResources.StoredMaterials.ContainsKey(item.Key))
                    playerResources.StoredMaterials[item.Key] += item.Value;
                else
                    playerResources.StoredMaterials[item.Key] = item.Value;
            }

            // Поддержка старого поля баланса (держим его в синхронизации)
            playerResources.Balance -= total;
            return true;
        }
    }
}
