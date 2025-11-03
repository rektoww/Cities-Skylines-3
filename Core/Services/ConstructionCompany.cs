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

        public ConstructionCompany(PlayerResources resources)
        {
            _resources = resources;
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

            balance = _resources.StoredMaterials.ContainsKey(material) ? _resources.StoredMaterials[material] : 0;
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
            var RequiredMaterials = T.RequiredMaterials;
            var BuildCost = T.BuildCost;

            // Проверка достаточности материалов
            foreach (var material in RequiredMaterials)
            {
                if (!_resources.StoredMaterials.ContainsKey(material.Key) || _resources.StoredMaterials[material.Key] < material.Value)
                {
                    building = null;
                    return false;
                }
            }

            // Вычитание материалов
            foreach (var material in RequiredMaterials)
            {
                TryRemoveMaterial(material.Key, material.Value, out _);
            }

            // Создание объекта здания
            building = (T)Activator.CreateInstance(typeof(T), parameters);

            if (!building.TryPlace(x, y, map))
            {
                building = null;
                return false;
            }

            // Вычитание стоимости
            _resources.Balance -= BuildCost;

            return true;
        }
    }
}