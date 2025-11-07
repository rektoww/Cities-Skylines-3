using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Core.Config;
using Core.Enums;
using Core.Resourses;
using Core.Services;

namespace Laboratornaya3.Views
{
    public partial class BuyMaterialsDialog : Window
    {
        private readonly MarketService _marketService;
        private readonly FinancialSystem _financialSystem;
        private readonly PlayerResources _playerResources;

        public ObservableCollection<MaterialBuyItem> Materials { get; set; }
        public decimal AvailableBudget => _financialSystem.CityBudget;

        public BuyMaterialsDialog(
            MarketService marketService,
            FinancialSystem financialSystem,
            PlayerResources playerResources)
        {
            InitializeComponent();

            _marketService = marketService;
            _financialSystem = financialSystem;
            _playerResources = playerResources;

            Materials = new ObservableCollection<MaterialBuyItem>();

            // Добавляем все материалы из конфига
            foreach (var priceEntry in EconomyConfig.MaterialPrices)
            {
                var item = new MaterialBuyItem
                {
                    Material = priceEntry.Key,
                    MaterialName = priceEntry.Key.ToString(),
                    Price = priceEntry.Value,
                    QuantityToBuy = 0
                };
                item.PropertyChanged += MaterialItem_PropertyChanged;
                Materials.Add(item);
            }

            MaterialsItemsControl.ItemsSource = Materials;
            DataContext = this;

            UpdateTotalCost();
        }

        private void MaterialItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MaterialBuyItem.QuantityToBuy))
            {
                UpdateTotalCost();
            }
        }

        private void UpdateTotalCost()
        {
            decimal total = Materials.Sum(m => m.TotalCost);
            TotalRevenueText.Text = $"{total:N0}$";
        }

        private void BuyButton_Click(object sender, RoutedEventArgs e)
        {
            var quantities = Materials
                .Where(m => m.QuantityToBuy > 0)
                .ToDictionary(m => m.Material, m => m.QuantityToBuy);

            if (quantities.Count == 0)
            {
                MessageBox.Show("Введите количество для покупки!", "Предупреждение", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal totalCost = Materials.Sum(m => m.TotalCost);
            if (totalCost > _financialSystem.CityBudget)
            {
                MessageBox.Show($"Недостаточно средств!\nТребуется: {totalCost:N0}$\nДоступно: {_financialSystem.CityBudget:N0}$", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            bool success = _marketService.TryBuyMaterials(
                quantities,
                EconomyConfig.MaterialPrices,
                _financialSystem,
                _playerResources,
                "Purchase: Materials");

            if (success)
            {
                MessageBox.Show($"Материалы успешно куплены!\nПотрачено: {totalCost:N0}$", 
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Не удалось выполнить покупку!", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class MaterialBuyItem : INotifyPropertyChanged
    {
        private int _quantityToBuy;

        public ConstructionMaterial Material { get; set; }
        public string MaterialName { get; set; } = string.Empty;
        public decimal Price { get; set; }

        public int QuantityToBuy
        {
            get => _quantityToBuy;
            set
            {
                if (_quantityToBuy != value)
                {
                    _quantityToBuy = value < 0 ? 0 : value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TotalCost));
                }
            }
        }

        public decimal TotalCost => Price * QuantityToBuy;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
