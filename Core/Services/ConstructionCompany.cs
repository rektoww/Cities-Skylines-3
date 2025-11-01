using Core.Enums;
using Core.Interfaces;
using Core.Models.Base;
using Core.Models.Map;
using System.Collections.Generic;
using System.Linq;

namespace Core.Services
{
    /// <summary>
    /// Представляет строительную компанию, ответственную за строительные проекты. (офис строительной компании, пока не уверен, будет ли это зданием)
    /// </summary>
    public class ConstructionCompany
    {
        public Dictionary<ConstructionMaterial, int> StoredMaterials { get; private set; } // Кол-во материалов у игрока

        public decimal Balance { get; private set; } // Баланс игрока (деньги)

        public ConstructionCompany(ref Dictionary<ConstructionMaterial, int> StoredMaterials, ref decimal balance)
        {
            this.StoredMaterials = StoredMaterials;
            Balance = balance;
        }

        /// <summary>
        /// Пытается удалить из хранилища указанное количество строительного материала.
        /// </summary>
        /// <param name="material">Материал, который нужно удалить.</param>
        /// <param name="amount">Количество для удаления.</param>
        /// <param name="balance">Остаток материала после удаления.</param>
        /// <returns>True, если материал был успешно удален, в противном случае — false..</returns>
        public bool TryRemoveMaterial(ConstructionMaterial material, int amount, out int balance)
        {
            if (amount > 0 && StoredMaterials.ContainsKey(material) && StoredMaterials[material] >= amount)
            {
                StoredMaterials[material] -= amount;
                balance = StoredMaterials[material];
                return true;
            }
            balance = StoredMaterials[material];
            return false;
        }

        /// <summary>
        /// Создание объекта здания
        /// </summary>
        /// <typeparam name="T"> класс здания </typeparam>
        /// <param name="x"> позиция x </param>
        /// <param name="y"> позиция y </param>
        /// <param name="map"> игровое поле </param>
        /// <param name="parameters"> параметры для конструктора здания</param>
        /// <param name="building"> объект здания, если true, иначе null </param>
        /// <returns>true, если успешно установлено, иначе false </returns>
        public bool TryBuild<T>(int x, int y, GameMap map, object[] parameters, out IConstructable? building) where T : Building, IConstructable
        {
            var RequiredMaterials = T.RequiredMaterials;
            var BuildCost = T.BuildCost;

            // Дополню логику, когда узнаю, что нужно для постройки здания

            // Проверка достаточности материалов для начала строительства
            foreach (var material in RequiredMaterials)
            {
                if (!StoredMaterials.ContainsKey(material.Key) || StoredMaterials[material.Key] < material.Value)
                {
                    // Недостаточно материалов
                    building = null;
                    return false;
                }
            }

            // Вычитание материалов из баланса игрока
            foreach (var material in RequiredMaterials)
            {
                TryRemoveMaterial(material.Key, material.Value, out int balance); // пока не используем balance
            }

            // Попытка поставить здание
            building = (T)Activator.CreateInstance(typeof(T), parameters);

            if (building.TryPlace(x, y, map))
            {
                Balance -= BuildCost;
                return true;
            }


            return false;
        }
    }
}