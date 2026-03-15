using BusinessAccountantService.Data;
using BusinessAccountantService.Managers;
using Microsoft.Data.Sqlite;
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
    /// Interaction logic for AddItemToInventoryWindow.xaml
    /// </summary>
    public partial class AddItemToInventoryWindow : Window
    {
        private readonly InventoryManager _inventoryManager = new();
        public AddItemToInventoryWindow()
        {
            InitializeComponent();
        }

        private void SaveItem_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ItemNameBox.Text))
            {
                MessageBox.Show("Введите название товара!");
                return;
            }

            string itemName = ItemNameBox.Text;
            int qty = int.TryParse(QuantityBox.Text, out int q) ? q : 0;

            double.TryParse(PurchasePriceBox.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double pPrice);
            double.TryParse(RetailPriceBox.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double rPrice);

            string category = CategoryBox.Text ?? "Разное";

            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();

                // 1. Сохраняем товар в инвентарь
                command.CommandText = @"
                   INSERT INTO Inventory (Name, Quantity, PurchasePrice, RetailPrice, Category) 
                   VALUES ($name, $qty, $pPrice, $rPrice, $cat)";

                command.Parameters.AddWithValue("$name", itemName);
                command.Parameters.AddWithValue("$qty", qty);
                command.Parameters.AddWithValue("$pPrice", pPrice);
                command.Parameters.AddWithValue("$rPrice", rPrice);
                command.Parameters.AddWithValue("$cat", category);

                command.ExecuteNonQuery();

                // 2. ФИКСИРУЕМ РАСХОД (ОДИН РАЗ!)
                double totalSpent = pPrice * qty;
                if (totalSpent > 0 && pPrice > 0)
                {
                    _inventoryManager.AddExpense($"Приход: {itemName} (x{qty})", totalSpent, category);
                }
            }

            // ТУТ БЫЛ ЛИШНИЙ БЛОК, ЕГО МЫ УДАЛИЛИ

            this.DialogResult = true;
        }


        private void Cancel_Click(object sender, RoutedEventArgs e) => this.DialogResult = false;


    }
}
