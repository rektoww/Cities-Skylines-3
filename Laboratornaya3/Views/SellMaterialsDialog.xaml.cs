using System;
using System.Collections.Generic;
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
    public partial class SellMaterialsDialog : Window
    {
        private readonly PlayerResources _playerResources;
        private readonly ResourceProductionService _productionService;
        private readonly ObservableCollection<MaterialSellItem> _materials;

        public bool SoldSuccessfully { get; private set; }
        public decimal TotalRevenue { get; private set; }

        public SellMaterialsDialog(PlayerResources playerResources, ResourceProductionService productionService)
        {
            InitializeComponent();
            _playerResources = playerResources;
            _productionService = productionService;
            _materials = new ObservableCollection<MaterialSellItem>();

            LoadMaterials();
            MaterialsItemsControl.ItemsSource = _materials;

            // Подписываемся на изменения количества для обновления итога
            foreach (var item in _materials)
            {
                item.PropertyChanged += Material_PropertyChanged;
            }

            UpdateTotalRevenue();
        }

        private void LoadMaterials()
        {
            foreach (var kvp in _playerResources.StoredMaterials)
            {
                if (kvp.Value > 0 && EconomyConfig.MaterialPrices.TryGetValue(kvp.Key, out var buyPrice))
                {
                    var sellPrice = buyPrice * 0.7m; // Продажа по 70% от закупки
                    _materials.Add(new MaterialSellItem
                    {
                        Material = kvp.Key,
                        Available = kvp.Value,
                        PricePerUnit = sellPrice,
                        QuantityToSell = 0
                    });
                }
            }
        }

        private void Material_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MaterialSellItem.QuantityToSell))
            {
                UpdateTotalRevenue();
            }
        }

        private void UpdateTotalRevenue()
        {
            TotalRevenue = _materials.Sum(m => m.TotalPrice);
            TotalRevenueText.Text = $"Общая выручка: {TotalRevenue:N0} валюты";
        }

        private void SellButton_Click(object sender, RoutedEventArgs e)
        {
            var itemsToSell = _materials.Where(m => m.QuantityToSell > 0).ToList();
            if (itemsToSell.Count == 0)
            {
                MessageBox.Show("Выберите материалы для продажи.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Продаем каждый материал
            foreach (var item in itemsToSell)
            {
                _productionService.TrySellMaterials(item.Material, item.QuantityToSell, item.PricePerUnit, out _);
            }

            SoldSuccessfully = true;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            SoldSuccessfully = false;
            DialogResult = false;
            Close();
        }
    }

    public class MaterialSellItem : INotifyPropertyChanged
    {
        private int _quantityToSell;

        public ConstructionMaterial Material { get; set; }
        public int Available { get; set; }
        public decimal PricePerUnit { get; set; }

        public int QuantityToSell
        {
            get => _quantityToSell;
            set
            {
                if (value < 0) value = 0;
                if (value > Available) value = Available;
                
                if (_quantityToSell != value)
                {
                    _quantityToSell = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TotalPrice));
                    OnPropertyChanged(nameof(TotalText));
                }
            }
        }

        public string MaterialName => Material.ToString();
        public string AvailableText => $"Доступно: {Available} шт.";
        public string PriceText => $"Цена: {PricePerUnit:N0} за шт.";
        public decimal TotalPrice => QuantityToSell * PricePerUnit;
        public string TotalText => $"= {TotalPrice:N0}";

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
