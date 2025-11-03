using Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Resourses
{
    /// <summary>
    /// Счет ресурсов у игрока
    /// </summary>
    public class PlayerResources
    {
        public decimal Balance = 0; // Баланс игрока (кстати что это и как заработать?)
        public Dictionary<ConstructionMaterial, int> StoredMaterials; // Словарь доступных материалов, полученных через заводы переработки

        public PlayerResources(decimal balance, Dictionary<ConstructionMaterial, int> materials)
        {
            Balance = balance;
            StoredMaterials = materials;
        }
    }
}
