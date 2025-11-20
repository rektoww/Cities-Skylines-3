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
    /// <summary>
    /// Диалоговое окно для покупки строительных материалов
    /// </summary>
    public partial class BuyMaterialsDialog : Window
    {
        private readonly MarketService _marketService;
        private readonly FinancialSystem _financialSystem;
        private readonly PlayerResources _playerResources;

        /// <summary>
        /// Коллекция материалов доступных для покупки
        /// </summary>
        public ObservableCollection<MaterialBuyItem> Materials { get; set; }

        /// <summary>
        /// Доступный бюджет города для покупки материалов
        /// </summary>
        public decimal AvailableBudget => _financialSystem.CityBudget;

        /// <summary>
        /// Инициализация диалогового окна покупки материалов
        /// </summary>
        /// <param name="marketService">Сервис работы с рынком</param>
        /// <param name="financialSystem">Финансовая система города</param>
        /// <param name="playerResources">Ресурсы игрока</param>
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
            InitializeMaterials();

            MaterialsItemsControl.ItemsSource = Materials;
            DataContext = this;

            UpdateTotalCost();
        }

        /// <summary>
        /// Инициализация коллекции материалов из конфигурации
        /// </summary>
        private void InitializeMaterials()
        {
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
        }

        /// <summary>
        /// Обработчик изменения количества материала для покупки
        /// </summary>
        private void MaterialItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MaterialBuyItem.QuantityToBuy))
            {
                UpdateTotalCost();
            }
        }

        /// <summary>
        /// Обновление отображения общей стоимости покупки
        /// </summary>
        private void UpdateTotalCost()
        {
            decimal total = Materials.Sum(m => m.TotalCost);
            TotalRevenueText.Text = $"{total:N0}$";
        }

        /// <summary>
        /// Обработчик нажатия кнопки покупки материалов
        /// </summary>
        private void BuyButton_Click(object sender, RoutedEventArgs e)
        {
            var quantities = Materials
                .Where(m => m.QuantityToBuy > 0)
                .ToDictionary(m => m.Material, m => m.QuantityToBuy);

            if (!ValidatePurchase(quantities))
                return;

            ExecutePurchase(quantities);
        }

        /// <summary>
        /// Проверка возможности совершения покупки
        /// </summary>
        /// <param name="quantities">Словарь материалов и их количеств</param>
        /// <returns>True если покупка возможна, иначе False</returns>
        private bool ValidatePurchase(Dictionary<ConstructionMaterial, int> quantities)
        {
            if (quantities.Count == 0)
            {
                MessageBox.Show("Введите количество для покупки!", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            decimal totalCost = Materials.Sum(m => m.TotalCost);
            if (totalCost > _financialSystem.CityBudget)
            {
                MessageBox.Show($"Недостаточно средств!\nТребуется: {totalCost:N0}$\nДоступно: {_financialSystem.CityBudget:N0}$",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Выполнение операции покупки материалов
        /// </summary>
        /// <param name="quantities">Словарь материалов и их количеств</param>
        private void ExecutePurchase(Dictionary<ConstructionMaterial, int> quantities)
        {
            decimal totalCost = Materials.Sum(m => m.TotalCost);

            bool success = _marketService.TryBuyMaterials(
                quantities,
                EconomyConfig.MaterialPrices,
                out decimal actualCost);

            if (success)
            {
                MessageBox.Show($"Материалы успешно куплены!\nПотрачено: {actualCost:N0}$",
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

    /// <summary>
    /// Элемент коллекции материалов для покупки
    /// </summary>
    public class MaterialBuyItem : INotifyPropertyChanged
    {
        private int _quantityToBuy;

        /// <summary>
        /// Тип строительного материала
        /// </summary>
        public ConstructionMaterial Material { get; set; }

        /// <summary>
        /// Название материала
        /// </summary>
        public string MaterialName { get; set; } = string.Empty;

        /// <summary>
        /// Цена за единицу материала
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Количество для покупки
        /// </summary>
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

        /// <summary>
        /// Общая стоимость материала (цена × количество)
        /// </summary>
        public decimal TotalCost => Price * QuantityToBuy;

        /// <summary>
        /// Событие изменения свойства
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Вызов события изменения свойства
        /// </summary>
        /// <param name="propertyName">Название измененного свойства</param>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}