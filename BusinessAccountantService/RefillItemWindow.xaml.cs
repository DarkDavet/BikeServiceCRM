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

            NewPurchasePriceBox.Text = item.PurchasePrice.ToString();
        }

        private void SaveRefill_Click(object sender, RoutedEventArgs e)
        {
            // 1. Валидация количества
            if (!int.TryParse(RefillQtyBox.Text, out int qty) || qty <= 0)
            {
                MessageBox.Show("Введите корректное количество (целое число > 0)");
                return;
            }

            // 2. Валидация цены закупки (универсальный парсинг)
            if (!double.TryParse(NewPurchasePriceBox.Text.Replace(",", "."),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double price))
            {
                MessageBox.Show("Введите корректную цену закупки");
                return;
            }

            try
            {
                // 3. Обновляем количество и цену в карточке товара на складе
                _inventoryManager.RefillItem(_currentItem.Id, qty, price);

                // 4. ФИКСИРУЕМ РЕАЛЬНЫЙ РАСХОД ДЕНЕГ
                // Считаем общую сумму затрат на эту поставку
                double totalSpent = price * qty;

                _inventoryManager.AddExpense(
                    $"Пополнение: {_currentItem.Name} (x{qty})",
                    totalSpent,
                    "Запчасти"
                );

                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}");
            }
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RefillQtyBox.Focus();
            RefillQtyBox.SelectAll(); // Выделяем текст "1", чтобы его можно было сразу заменить
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
