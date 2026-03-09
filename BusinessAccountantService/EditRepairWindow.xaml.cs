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

            // Заполняем поля данными из заказа
            BikeInfoBox.Text = repair.BikeInfo;
            ProblemBox.Text = repair.ProblemDescription;
            WorksBox.Text = repair.WorksPerformed;
            CostBox.Text = repair.TotalCost.ToString();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Обновляем объект данными из полей
            Repair.BikeInfo = BikeInfoBox.Text;
            Repair.ProblemDescription = ProblemBox.Text;
            Repair.WorksPerformed = WorksBox.Text;

            if (double.TryParse(CostBox.Text, out double cost))
                Repair.TotalCost = cost;

            DialogResult = true; // Закрываем окно с результатом "Успех"
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
    }
}
