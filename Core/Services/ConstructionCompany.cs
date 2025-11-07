using Core.Enums;
using Core.Interfaces;
using Core.Models.Base;
using Core.Models.Map;
using Core.Resourses;
using System.Collections.Generic;
using System.Linq;

namespace Core.Services
{
    /// <summary>
    /// Представляет строительную компанию, ответственную за строительные проекты. (офис строительной компании, пока не уверен, будет ли это зданием)
    /// </summary>
    public class ConstructionCompany
    {
        // Игровые данные
        private readonly PlayerResources _resources;
        // Опционально: финансовая система города для учета расходов и проверки бюджета
        private readonly FinancialSystem? _financialSystem;

        public ConstructionCompany(PlayerResources resources, FinancialSystem? financialSystem = null)
        {
            _resources = resources;
            _financialSystem = financialSystem;
        }

        public decimal Balance => _resources.Balance;
        public Dictionary<ConstructionMaterial, int> StoredMaterials => _resources.StoredMaterials;

        /// <summary>
        /// Списание материалов с счета пользователя
        /// </summary>
        /// <param name="material">тип материала</param>
        /// <param name="amount">количество материала</param>
        /// <param name="balance">осталось материалов на счету</param>
        /// <returns>True, если материалы списаны успешно, false, если на счету не хватило материалов</returns>
        public bool TryRemoveMaterial(ConstructionMaterial material, int amount, out int balance)
        {
            if (amount > 0 && _resources.StoredMaterials.ContainsKey(material) && _resources.StoredMaterials[material] >= amount)
            {
                _resources.StoredMaterials[material] -= amount;
                balance = _resources.StoredMaterials[material];
                return true;
            }

            balance = _resources.StoredMaterials.ContainsKey(material) ? _resources.StoredMaterials[material] : -1;
            return false;
        }

        /// <summary>
        /// Попытка разместить здание на карте
        /// </summary>
        /// <typeparam name="T"> тип здания </typeparam>
        /// <param name="x"> ось абцисс </param>
        /// <param name="y"> ось ординат </param>
        /// <param name="map"> игровая карта </param>
        /// <param name="parameters"> список параметров для инициализации здания </param>
        /// <param name="building"> получаемый на выходе объект здания </param>
        /// <returns>True, если здание поставлено, false, если не удалось</returns>
        public bool TryBuild<T>(int x, int y, GameMap map, object[] parameters, out IConstructable<T>? building)
            where T : Building, IConstructable<T>
        {
            var requiredMaterials = T.RequiredMaterials;
            var buildCost = T.BuildCost;

            // 1) Проверяем материалы (без списания)
            foreach (var material in requiredMaterials)
            {
                if (!_resources.StoredMaterials.ContainsKey(material.Key) || _resources.StoredMaterials[material.Key] < material.Value)
                {
                    building = null;
                    return false;
                }
            }

            // 2) Проверяем бюджет (если подключена финансовая система — по ней, иначе по локальному балансу)
            var availableBudget = _financialSystem?.CityBudget ?? _resources.Balance;
            if (availableBudget < buildCost)
            {
                building = null;
                return false;
            }

            // 3) Создаем объект здания и пытаемся разместить
            building = (T)Activator.CreateInstance(typeof(T), parameters);
            if (!building.TryPlace(x, y, map))
            {
                building = null;
                return false;
            }

            // 4) Списываем материалы (после успешного размещения)
            foreach (var material in requiredMaterials)
            {
                TryRemoveMaterial(material.Key, material.Value, out _);
            }

            // 5) Списываем деньги: через финансовую систему (для отчетности) + синхронизируем локальный баланс для обратной совместимости
            if (_financialSystem != null)
            {
                // Категория расхода: Строительство: <Тип>
                _financialSystem.AddExpense(buildCost, $"Construction: {typeof(T).Name}");
            }

            // Поддерживаем старую логику баланса игрока
            _resources.Balance -= buildCost;

            return true;
        }
    }
}
