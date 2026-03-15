using BusinessAccountantService.Managers;
using BusinessAccountantService.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BusinessAccountantService
{
    /// <summary>
    /// Interaction logic for AddPartToRepairRecordWindow.xaml
    /// </summary>
    public partial class AddPartToRepairRecordWindow : Window
    {
        private readonly InventoryManager _inventoryManager = new();
        public RepairItem SelectedResult { get; private set; }
        private Item _foundItem;
        private ICollectionView _partView;

        public AddPartToRepairRecordWindow()
        {
            InitializeComponent();

            // Загружаем полный список ОДИН РАЗ при открытии
            var allServices =
            _partView = CollectionViewSource.GetDefaultView(_inventoryManager.GetAllItems());
            PartSearchBox.ItemsSource = _partView;

            PartSearchBox.Focus();
        }

        private void PartSearchBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Enter || e.Key == Key.Escape) return;

            string query = PartSearchBox.Text.ToLower();

            _partView.Filter = obj =>
            {
                if (string.IsNullOrEmpty(query)) return true;

                // ВАЖНО: Приводим к классу Item (тот, что в вашем Inventory)
                var item = obj as Item;
                if (item == null) return false;

                // Фильтруем по названию
                return item.Name.ToLower().Contains(query);
            };
            var textBox = (TextBox)PartSearchBox.Template.FindName("PART_EditableTextBox", PartSearchBox);
            if (textBox != null)
            {
                textBox.SelectionStart = textBox.Text.Length;
                textBox.SelectionLength = 0;
            }
            PartSearchBox.IsDropDownOpen = true;

        }

        private void PartSearchBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PartSearchBox.SelectedItem is Item item)
            {
                _foundItem = item;
                StockText.Text = $"{item.Quantity} шт.";
                QtyBox.Focus();
                QtyBox.SelectAll();
            }
        }

        private void PartSearchBox_DropDownOpened(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PartSearchBox.Text))
            {
                // Загружаем все товары, где количество > 0
                var allItems = _inventoryManager.GetAllItems()
                                                .Where(i => i.Quantity > 0)
                                                .OrderBy(i => i.Name)
                                                .ToList();
                PartSearchBox.ItemsSource = allItems;
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (_foundItem == null) return;

            if (!int.TryParse(QtyBox.Text, out int qty) || qty <= 0)
            {
                MessageBox.Show("Введите корректное количество.");
                return;
            }

            if (qty > _foundItem.Quantity)
            {
                MessageBox.Show("На складе недостаточно товара!");
                return;
            }

            SelectedResult = new RepairItem
            {
                ProductId = _foundItem.Id,
                Name = _foundItem.Name,
                Quantity = qty,
                Price = _foundItem.RetailPrice,
                PurchasePrice = _foundItem.PurchasePrice 
            };

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
