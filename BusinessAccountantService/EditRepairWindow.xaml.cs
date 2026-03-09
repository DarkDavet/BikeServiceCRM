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
    /// Interaction logic for EditRepairWindow.xaml
    /// </summary>
    public partial class EditRepairWindow : Window
    {
        public RepairRecord Repair { get; private set; }
        public bool IsDeleted { get; private set; } = false;

        public EditRepairWindow(RepairRecord repair)
        {
            InitializeComponent();
            Repair = repair;

            BikeInfoBox.Text = repair.BikeInfo;
            ProblemBox.Text = repair.ProblemDescription;
            WorksBox.Text = repair.WorksPerformed;

            // Добавьте это:
            PartsCostBox.Text = repair.PartsCost.ToString();
            TotalCostBox.Text = repair.TotalCost.ToString();

            foreach (ComboBoxItem item in StatusComboBox.Items)
            {
                if (item.Content.ToString() == repair.Status)
                {
                    StatusComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Repair.BikeInfo = BikeInfoBox.Text;
            Repair.ProblemDescription = ProblemBox.Text;
            Repair.WorksPerformed = WorksBox.Text;

            // ОБЯЗАТЕЛЬНО: Считываем цифры из текстовых полей обратно в объект
            double.TryParse(PartsCostBox.Text, out double parts);
            double.TryParse(TotalCostBox.Text, out double total);

            Repair.PartsCost = parts;
            Repair.TotalCost = total;

            if (StatusComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                Repair.Status = selectedItem.Content.ToString();
            }

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите удалить этот заказ?",
                                         "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                IsDeleted = true;
                DialogResult = true; // Закрываем окно, возвращаемся в MainWindow
            }
        }

        // Добавим автоматический фокус на поле работ при открытии окна
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WorksBox.Focus();
            // Ставим курсор в конец текста, чтобы сразу дописывать
            WorksBox.SelectionStart = WorksBox.Text.Length;
        }

        private void CostFields_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Проверка на null нужна, так как событие срабатывает при инициализации компонентов
            if (ProfitText == null || PartsCostBox == null || TotalCostBox == null) return;

            // Используем замену запятой на точку для универсальности ввода
            double.TryParse(PartsCostBox.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double parts);
            double.TryParse(TotalCostBox.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double total);

            ProfitText.Text = $"Чистая прибыль: {total - parts} руб.";
        }

        private void ClearCosts_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Очистить список работ и обнулить все суммы?",
                                         "Сброс расчета", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                WorksBox.Clear();
                PartsCostBox.Text = "0";
                TotalCostBox.Text = "0"; // В AddRepairWindow это CostBox, в Edit - TotalCostBox (проверь имя!)

                // Фокус на ввод названия, чтобы сразу начать заново
                ItemNameBox.Focus();

                // Если есть метод пересчета прибыли, вызываем его
                CostFields_TextChanged(null, null);
            }
        }


        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ItemNameBox.Text) || !double.TryParse(ItemPriceBox.Text, out double price))
            {
                MessageBox.Show("Введите название и корректную цену!");
                return;
            }

            // 1. Добавляем текст в поле выполненных работ
            string newItem = $"— {ItemNameBox.Text}: {price} руб.\n";
            WorksBox.Text += newItem;

            // 2. Спрашиваем: это расход (запчасть) или доход (работа)?
            var result = MessageBox.Show($"Это запчасть (Расход)?\n'Да' - добавит в расходы.\n'Нет' - добавит в итоговую стоимость.",
                                         "Куда прибавить сумму?", MessageBoxButton.YesNoCancel);

            if (result == MessageBoxResult.Yes) // Запчасть
            {
                double.TryParse(PartsCostBox.Text, out double currentParts);
                PartsCostBox.Text = (currentParts + price).ToString();
            }
            else if (result == MessageBoxResult.No) // Работа
            {
                double.TryParse(TotalCostBox.Text, out double currentTotal);
                TotalCostBox.Text = (currentTotal + price).ToString();
            }

            // Очищаем поля ввода
            ItemNameBox.Clear();
            ItemPriceBox.Clear();
            ItemNameBox.Focus();
        }

    }
}
