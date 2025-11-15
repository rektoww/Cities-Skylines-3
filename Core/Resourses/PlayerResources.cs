using CommunityToolkit.Mvvm.ComponentModel;
using Core.Enums;
using System.Collections.Generic;
using System.Linq;

namespace Core.Resourses
{
    public partial class PlayerResources : ObservableObject
    {
        [ObservableProperty]
        private decimal _balance = 0;

        [ObservableProperty]
        private Dictionary<ConstructionMaterial, int> _storedMaterials;

        public PlayerResources(decimal balance, Dictionary<ConstructionMaterial, int> materials)
        {
            Balance = balance;
            StoredMaterials = materials ?? new Dictionary<ConstructionMaterial, int>();
        }

        public bool HasEnoughMaterials(Dictionary<ConstructionMaterial, int> required)
        {
            return required.All(x =>
                StoredMaterials.ContainsKey(x.Key) && StoredMaterials[x.Key] >= x.Value);
        }

        public void AddMaterials(ConstructionMaterial material, int amount)
        {
            if (StoredMaterials.ContainsKey(material))
                StoredMaterials[material] += amount;
            else
                StoredMaterials[material] = amount;

            OnPropertyChanged(nameof(StoredMaterials));
        }

        public bool TryRemoveMaterials(Dictionary<ConstructionMaterial, int> required)
        {
            if (!HasEnoughMaterials(required))
                return false;

            foreach (var material in required)
            {
                StoredMaterials[material.Key] -= material.Value;
            }

            OnPropertyChanged(nameof(StoredMaterials));
            return true;
        }
    }
}