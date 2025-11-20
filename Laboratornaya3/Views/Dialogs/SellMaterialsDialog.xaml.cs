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
    /// <summary>
    /// Диалоговое окно для продажи строительных материалов
    /// </summary>
    public partial class SellMaterialsDialog : Window
    {
        private readonly PlayerResources _playerResources;
        private readonly ResourceProductionService _productionService;
        private readonly ObservableCollection<MaterialSellItem> _materials;

        /// <summary>
        /// Флаг успешного завершения операции продажи
        /// </summary>
        public bool SoldSuccessfully { get; private set; }

        /// <summary>
        /// Общая выручка от продажи материалов
        /// </summary>
        public decimal TotalRevenue { get; private set; }

        /// <summary>
        /// Инициализация диалогового окна продажи материалов
        /// </summary>
        /// <param name="playerResources">Ресурсы игрока для продажи</param>
        /// <param name="productionService">Сервис производства и продажи ресурсов</param>
        public SellMaterialsDialog(PlayerResources playerResources, ResourceProductionService productionService)
        {
            InitializeComponent();
            _playerResources = playerResources;
            _productionService = productionService;
            _materials = new ObservableCollection<MaterialSellItem>();

            LoadMaterials();
            MaterialsItemsControl.ItemsSource = _materials;
            SubscribeToMaterialChanges();
            UpdateTotalRevenue();
        }

        /// <summary>
        /// Загрузка доступных для продажи материалов из ресурсов игрока
        /// </summary>
        private void LoadMaterials()
        {
            foreach (var kvp in _playerResources.StoredMaterials)
            {
                if (kvp.Value > 0 && EconomyConfig.MaterialPrices.TryGetValue(kvp.Key, out var buyPrice))
                {
                    var sellPrice = buyPrice * 0.7m; 
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

        /// <summary>
        /// Подписывается на события изменения количества материалов для продажи
        /// </summary>
        private void SubscribeToMaterialChanges()
        {
            foreach (var item in _materials)
            {
                item.PropertyChanged += Material_PropertyChanged;
            }
        }

        /// <summary>
        /// Обработчик изменения количества материала для продажи
        /// </summary>
        private void Material_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MaterialSellItem.QuantityToSell))
            {
                UpdateTotalRevenue();
            }
        }

        /// <summary>
        /// Обновление отображения общей выручки от продажи
        /// </summary>
        private void UpdateTotalRevenue()
        {
            TotalRevenue = _materials.Sum(m => m.TotalPrice);
            TotalRevenueText.Text = $"Общая выручка: {TotalRevenue:N0} валюты";
        }

        /// <summary>
        /// Обработчик нажатия кнопки продажи материалов
        /// </summary>
        private void SellButton_Click(object sender, RoutedEventArgs e)
        {
            var itemsToSell = _materials.Where(m => m.QuantityToSell > 0).ToList();

            if (!ValidateSale(itemsToSell))
                return;

            ExecuteSale(itemsToSell);
        }

        /// <summary>
        /// Проверка возможности совершения продажи
        /// </summary>
        /// <param name="itemsToSell">Список материалов для продажи</param>
        /// <returns>True если продажа возможна, иначе False</returns>
        private bool ValidateSale(List<MaterialSellItem> itemsToSell)
        {
            if (itemsToSell.Count == 0)
            {
                MessageBox.Show("Выберите материалы для продажи.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Выполнение операции продажи материалов
        /// </summary>
        /// <param name="itemsToSell">Список материалов для продажи</param>
        private void ExecuteSale(List<MaterialSellItem> itemsToSell)
        {
            foreach (var item in itemsToSell)
            {
                _productionService.TrySellMaterials(item.Material, item.QuantityToSell, item.PricePerUnit, out _);
            }

            SoldSuccessfully = true;
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Обработчик нажатия кнопки отмены продажи
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            SoldSuccessfully = false;
            DialogResult = false;
            Close();
        }
    }

    /// <summary>
    /// Элемент коллекции материалов для продажи
    /// </summary>
    public class MaterialSellItem : INotifyPropertyChanged
    {
        private int _quantityToSell;

        /// <summary>
        /// Тип строительного материала
        /// </summary>
        public ConstructionMaterial Material { get; set; }

        /// <summary>
        /// Доступное количество материала для продажи
        /// </summary>
        public int Available { get; set; }

        /// <summary>
        /// Цена продажи за единицу материала
        /// </summary>
        public decimal PricePerUnit { get; set; }

        /// <summary>
        /// Количество материала для продажи
        /// </summary>
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

        /// <summary>
        /// Название материала
        /// </summary>
        public string MaterialName => Material.ToString();

        /// <summary>
        /// Информация о доступном количестве
        /// </summary>
        public string AvailableText => $"Доступно: {Available} шт.";

        /// <summary>
        /// Информация о цене продажи
        /// </summary>
        public string PriceText => $"Цена: {PricePerUnit:N0} за шт.";

        /// <summary>
        /// Общая стоимость продажи материала
        /// </summary>
        public decimal TotalPrice => QuantityToSell * PricePerUnit;

        /// <summary>
        /// Информация об общей стоимости
        /// </summary>
        public string TotalText => $"= {TotalPrice:N0}";

        /// <summary>
        /// Событие изменения свойства
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Вызов события изменения свойства
        /// </summary>
        /// <param name="propertyName">Название измененного свойства</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}