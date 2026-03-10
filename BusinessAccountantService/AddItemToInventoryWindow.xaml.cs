using BusinessAccountantService.Data;
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

            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
            INSERT INTO Inventory (Name, Quantity, PurchasePrice, RetailPrice, Category) 
            VALUES ($name, $qty, $pPrice, $rPrice, $cat)";

                command.Parameters.AddWithValue("$name", ItemNameBox.Text);
                command.Parameters.AddWithValue("$qty", int.TryParse(QuantityBox.Text, out int q) ? q : 0);

                // Парсим цены (с защитой от запятых, как мы делали раньше)
                double.TryParse(PurchasePriceBox.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double pPrice);
                double.TryParse(RetailPriceBox.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double rPrice);

                command.Parameters.AddWithValue("$pPrice", pPrice);
                command.Parameters.AddWithValue("$rPrice", rPrice);
                command.Parameters.AddWithValue("$cat", CategoryBox.Text);

                command.ExecuteNonQuery();
            }
            this.DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => this.DialogResult = false;

    }
}
