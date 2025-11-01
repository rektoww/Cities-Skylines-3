using Core.Enums;
using Core.Models.Base;
using System.Collections.Generic;

namespace Core.Models.Buildings
{
    /// <summary>
    /// Представляет строительную площадку, которая производит или хранит строительные материалы.
    /// Хранилище строительных материалов.
    /// </summary>
    public class ConstructionYard : Building
    {
        /// <summary>
        /// Словарь для хранения количества каждого доступного строительного материала.
        /// </summary>
        public Dictionary<ConstructionMaterial, int> StoredMaterials { get; private set; } = new();

        public ConstructionYard() : base()
        {

            // Инициализируйте хранилище, установив для всех типов материалов значение 0.   
            foreach (ConstructionMaterial material in Enum.GetValues<ConstructionMaterial>())
            {
                StoredMaterials[material] = 0;
            }
        }

        /// <summary>
        /// Добавляет в хранилище указанное количество строительного материала.
        /// </summary>
        /// <param name="material">Материал, который нужно добавить.</param>
        /// <param name="amount">Количество для добавления.</param>
        public void AddMaterial(ConstructionMaterial material, int amount)
        {
            if (amount > 0) // Нельзя уйти в минус
            {
                StoredMaterials[material] += amount;
            }
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

        public override void OnBuildingPlaced()
        {
            // Пока неясная логика
        }
    }
}