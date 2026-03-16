using BusinessAccountantService.Managers;
using BusinessAccountantService.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Interaction logic for RefillItemWindow.xaml
    /// </summary>
    public partial class RefillItemWindow : Window
    {
        private Item _currentItem;
        private readonly InventoryManager _inventoryManager = new();

        public RefillItemWindow(Item item)
        {
            InitializeComponent();
            _currentItem = item;
            ItemTitleText.Text = $"ПОПОЛНЕНИЕ: {item.Name.ToUpper()}";

            // Показываем опцию опта, если это запчасти
            if (item.Category != null && item.Category.Trim().Equals("Запчасти", StringComparison.OrdinalIgnoreCase))
            {
                BulkModeCheck.Visibility = Visibility.Visible;
            }

            NewPurchasePriceBox.Text = item.PurchasePrice.ToString("F2");
        }

        private void SaveRefill_Click(object sender, RoutedEventArgs e)
        {
            int.TryParse(RefillQuantityBox.Text, out int qty);
            decimal finalUnitPrice = 0;
            decimal totalSpent = 0;

            if (BulkModeCheck.IsChecked == true)
            {
                decimal.TryParse(BulkTotalBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out totalSpent);
                if (qty > 0) finalUnitPrice = totalSpent / qty;
            }
            else
            {
                decimal.TryParse(NewPurchasePriceBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out finalUnitPrice);
                totalSpent = finalUnitPrice * qty;
            }

            if (qty <= 0 || finalUnitPrice <= 0)
            {
                MessageBox.Show("Введите корректные данные!");
                return;
            }

            try
            {
                // Обновляем склад и фиксируем расход (все в decimal)
                _inventoryManager.RefillItem(_currentItem.Id, qty, finalUnitPrice);
                _inventoryManager.AddExpense($"Пополнение: {_currentItem.Name} (x{qty})", totalSpent, _currentItem.Category);

                this.DialogResult = true;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
