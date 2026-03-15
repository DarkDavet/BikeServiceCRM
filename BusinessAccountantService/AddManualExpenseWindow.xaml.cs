using BusinessAccountantService.Managers;
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
    /// Interaction logic for AddManualExpenseWindow.xaml
    /// </summary>
    public partial class AddManualExpenseWindow : Window
    {
        private readonly InventoryManager _inventoryManager = new();

        public AddManualExpenseWindow()
        {
            InitializeComponent();
            CategoryBox.SelectedIndex = 0; // По умолчанию "Аренда"
            AmountBox.Focus();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // 1. Валидация суммы
            if (!decimal.TryParse(AmountBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Введите корректную сумму расхода!");
                return;
            }

            // 2. Валидация описания
            if (string.IsNullOrWhiteSpace(DescBox.Text))
            {
                MessageBox.Show("Введите описание расхода!");
                return;
            }

            // 3. Сохранение только в таблицу Expenses
            try
            {
                _inventoryManager.AddExpense(
                    DescBox.Text,
                    amount,
                    CategoryBox.Text
                );
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => this.DialogResult = false;
    }
}
