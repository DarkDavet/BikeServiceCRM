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
    /// Interaction logic for AddServiceToRepairRecordWindow.xaml
    /// </summary>
    public partial class AddServiceToRepairRecordWindow : Window
    {
        private readonly RepairManager _repairManager = new();
        public RepairItem SelectedResult { get; private set; }

        public AddServiceToRepairRecordWindow()
        {
            InitializeComponent();
            ServiceSearchBox.Focus();
        }

        // Поиск в прайс-листе по мере ввода текста
        private void ServiceSearchBox_KeyUp(object sender, KeyEventArgs e)
        {
            // Игнорируем системные клавиши
            if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Enter) return;

            string query = ServiceSearchBox.Text;
            if (query.Length < 2)
            {
                ServiceSearchBox.IsDropDownOpen = false;
                return;
            }

            // Получаем подсказки из БД (таблица ServicePriceList)
            var suggestions = _repairManager.GetServiceSuggestions(query);
            ServiceSearchBox.ItemsSource = suggestions;
            ServiceSearchBox.IsDropDownOpen = suggestions.Any();
        }

        // Если выбрали готовую услугу — подставляем её цену
        private void ServiceSearchBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ServiceSearchBox.SelectedItem is ServiceItem selected)
            {
                PriceBox.Text = selected.Price.ToString();
                PriceBox.Focus();
                PriceBox.SelectAll();
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            string name = ServiceSearchBox.Text;

            // Парсим цену с защитой от точек/запятых
            double.TryParse(PriceBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double price);

            if (string.IsNullOrWhiteSpace(name) || price <= 0)
            {
                MessageBox.Show("Укажите корректное название и цену работы!");
                return;
            }

            // ОБУЧЕНИЕ: Сохраняем или обновляем эту услугу в общем прайс-листе
            _repairManager.UpdateServicePrice(name, price);

            // Создаем объект для основной таблицы заказа
            SelectedResult = new RepairItem
            {
                ProductId = null, // NULL = работа
                Name = name,
                Price = price,
                Quantity = 1,      // Для работы всегда 1
                PurchasePrice = 0  // Услуга не имеет закупочной цены
            };

            this.DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => this.DialogResult = false;
    }
}

