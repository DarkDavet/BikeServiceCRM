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

            var allItems = _inventoryManager.GetAllItems();
            _partView = CollectionViewSource.GetDefaultView(allItems);

            _partView.Filter = obj =>
            {
                if (obj is not Item item) return false;

                string itemCat = (item.Category ?? "").Trim().ToLower();
                string query = (PartSearchBox.Text ?? "").Trim().ToLower();

                bool isPart = itemCat == "запчасти";
                bool hasStock = item.Quantity > 0;
                bool matchesQuery = string.IsNullOrEmpty(query) || item.Name.ToLower().Contains(query);

                return isPart && hasStock && matchesQuery;
            };

            PartSearchBox.ItemsSource = _partView;
            PartSearchBox.Focus();
        }

        private void PartSearchBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Enter || e.Key == Key.Escape) return;

            _partView.Refresh();

            PartSearchBox.IsDropDownOpen = true;

            var textBox = (TextBox)PartSearchBox.Template.FindName("PART_EditableTextBox", PartSearchBox);
            if (textBox != null)
            {
                textBox.SelectionStart = textBox.Text.Length;
                textBox.SelectionLength = 0;
            }

        }

        private void PartSearchBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PartSearchBox.SelectedItem is Item item)
            {
                _foundItem = item;
                StockText.Text = $"{item.Quantity} шт.";

                PartSearchBox.Text = item.Name;

                QtyBox.Focus();
                QtyBox.SelectAll();
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
