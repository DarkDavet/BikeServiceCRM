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
    /// Interaction logic for AddRepairWindow.xaml
    /// </summary>
    public partial class AddRepairWindow : Window
    {
        private int _clientId;

        public AddRepairWindow(int clientId)
        {
            InitializeComponent();
            _clientId = clientId; 
        }

        // Убираем текст-подсказку при клике
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb && tb.Text.StartsWith("Например:")) tb.Clear();
        }

        // Калькулятор (AddItem_Click) - точно такой же, как в EditRepairWindow
        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ItemNameBox.Text) || !double.TryParse(ItemPriceBox.Text, out double price)) return;

            WorksBox.Text += $"— {ItemNameBox.Text}: {price:N2} руб.\n";

            var result = MessageBox.Show("Это запчасть (Расход)?", "Тип записи", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                double.TryParse(PartsCostBox.Text, out double current);
                PartsCostBox.Text = (current + price).ToString();
            }
            else
            {
                double.TryParse(CostBox.Text, out double current);
                CostBox.Text = (current + price).ToString();
            }
            ItemNameBox.Clear(); ItemPriceBox.Clear(); ItemNameBox.Focus();
        }

        // Живой подсчет прибыли
        private void CostFields_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ProfitText == null) return;
            double.TryParse(PartsCostBox.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double p);
            double.TryParse(CostBox.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double t);
            ProfitText.Text = $"Предварительная прибыль: {t - p:N2} руб.";
        }


        private void SaveRepair_Click(object sender, RoutedEventArgs e)
        {
            // 1. Проверяем, введено ли хоть что-то по велосипеду
            if (string.IsNullOrWhiteSpace(BikeInfoBox.Text) || BikeInfoBox.Text.StartsWith("Например:"))
            {
                MessageBox.Show("Пожалуйста, укажите марку и модель велосипеда.");
                return;
            }

            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();

                // 2. SQL запрос со всеми новыми колонками (включая WorksPerformed и PartsCost)
                command.CommandText = @"
            INSERT INTO Repairs (ClientId, BikeInfo, ProblemDescription, WorksPerformed, PartsCost, TotalCost, Status, DateCreated) 
            VALUES ($cid, $bike, $prob, $works, $parts, $cost, 'Принят', $date)";

                // 3. Привязываем параметры
                command.Parameters.AddWithValue("$cid", _clientId); // _clientId должен быть передан в конструктор окна
                command.Parameters.AddWithValue("$bike", BikeInfoBox.Text);
                command.Parameters.AddWithValue("$prob", ProblemBox.Text);
                command.Parameters.AddWithValue("$works", WorksBox.Text);

                // Парсим деньги (с защитой от запятых)
                double.TryParse(PartsCostBox.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double parts);
                double.TryParse(CostBox.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double total);

                command.Parameters.AddWithValue("$parts", parts);
                command.Parameters.AddWithValue("$cost", total);
                command.Parameters.AddWithValue("$date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                command.ExecuteNonQuery();


            }

            // Закрываем окно с результатом "Успех"
            this.DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void ClearCosts_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Очистить список работ и обнулить все суммы?",
                                         "Сброс расчета", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                WorksBox.Clear();
                PartsCostBox.Text = "0";
                CostBox.Text = "0";

                // Фокус на ввод названия, чтобы сразу начать заново
                ItemNameBox.Focus();

                // Если есть метод пересчета прибыли, вызываем его
                CostFields_TextChanged(null, null);
            }
        }

    }
}
