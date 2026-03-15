using BusinessAccountantService.Managers;
using BusinessAccountantService.Models;
using System;
using System.Collections.Generic;
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

        public AddPartToRepairRecordWindow()
        {
            InitializeComponent();
            PartSearchBox.Focus();
        }

        private void PartSearchBox_KeyUp(object sender, KeyEventArgs e)
        {
            string query = PartSearchBox.Text;
            if (query.Length < 2) return;

            // Фильтруем только то, что есть в наличии
            var items = _inventoryManager.GetAllItems()
                .Where(i => i.Name.ToLower().Contains(query.ToLower()) && i.Quantity > 0)
                .ToList();

            PartSearchBox.ItemsSource = items;
            PartSearchBox.IsDropDownOpen = items.Any();
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
