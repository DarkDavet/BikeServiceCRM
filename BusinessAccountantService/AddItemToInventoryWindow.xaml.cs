using BusinessAccountantService.Data;
using BusinessAccountantService.Managers;
using Microsoft.Data.Sqlite;
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
    /// Interaction logic for AddItemToInventoryWindow.xaml
    /// </summary>
    public partial class AddItemToInventoryWindow : Window
    {
        private readonly InventoryManager _inventoryManager = new();
        public AddItemToInventoryWindow()
        {
            InitializeComponent();
        }

        private void CategoryBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BulkModeCheck == null) return;

            string cat = (CategoryBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "";

            // Показываем чекбокс только для запчастей
            BulkModeCheck.Visibility = cat == "Запчасти" ? Visibility.Visible : Visibility.Collapsed;

            // Если категория сменилась на другую — выключаем оптовый режим
            if (cat != "Запчасти")
            {
                BulkModeCheck.IsChecked = false;
            }
        }

        private void SaveItem_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ItemNameBox.Text))
            {
                MessageBox.Show("Введите название товара!");
                return;
            }

            string itemName = ItemNameBox.Text;
            string category = (CategoryBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Разное";

            // Парсим количество
            int.TryParse(QuantityBox.Text, out int qty);

            // Переменные для финальных расчетов
            decimal finalUnitPrice = 0;
            decimal totalSpent = 0;

            // ЛОГИКА РАСЧЕТА ЦЕНЫ
            if (BulkModeCheck.IsChecked == true)
            {
                // Режим ОПТ: парсим общую сумму всей партии
                decimal.TryParse(BulkTotalBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out totalSpent);

                if (qty > 0 && totalSpent > 0)
                {
                    finalUnitPrice = totalSpent / qty; // Считаем цену за 1 шт
                }
            }
            else
            {
                // ОБЫЧНЫЙ режим: парсим цену за штуку
                decimal.TryParse(PurchasePriceBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out finalUnitPrice);
                totalSpent = finalUnitPrice * qty;
            }

            // Парсим розничную цену
            decimal.TryParse(RetailPriceBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal rPrice);

            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();

                command.CommandText = @"
                    INSERT INTO Inventory (Name, Quantity, PurchasePrice, RetailPrice, Category) 
                    VALUES ($name, $qty, $pPrice, $rPrice, $cat)";

                command.Parameters.AddWithValue("$name", itemName);
                command.Parameters.AddWithValue("$qty", qty);
                command.Parameters.AddWithValue("$pPrice", (decimal)finalUnitPrice); // Сохраняем вычисленную цену за 1 шт
                command.Parameters.AddWithValue("$rPrice", (decimal)rPrice);
                command.Parameters.AddWithValue("$cat", category);

                command.ExecuteNonQuery();

                // ФИКСИРУЕМ РАСХОД В АНАЛИТИКЕ
                if (totalSpent > 0)
                {
                    _inventoryManager.AddExpense($"Закупка: {itemName} (x{qty})", totalSpent, category);
                }
            }

            // ТУТ БЫЛ ЛИШНИЙ БЛОК, ЕГО МЫ УДАЛИЛИ

            this.DialogResult = true;
        }


        private void Cancel_Click(object sender, RoutedEventArgs e) => this.DialogResult = false;


    }
}
