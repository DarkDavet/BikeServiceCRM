using BusinessAccountantService.Managers;
using BusinessAccountantService.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        private ICollectionView _serviceView;

        public AddServiceToRepairRecordWindow()
        {
            InitializeComponent();

            // Загружаем список один раз
            var allServices = _repairManager.GetServiceSuggestions("");
            _serviceView = CollectionViewSource.GetDefaultView(allServices);

            // Привязываем источник один раз!
            ServiceSearchBox.ItemsSource = _serviceView;
            ServiceSearchBox.Focus();
        }

        // Поиск в прайс-листе по мере ввода текста
        private void ServiceSearchBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Enter || e.Key == Key.Escape) return;

            // Берем текст прямо из TextBox внутри ComboBox, чтобы избежать лагов
            var textBox = (TextBox)ServiceSearchBox.Template.FindName("PART_EditableTextBox", ServiceSearchBox);
            string query = textBox?.Text.ToLower() ?? ServiceSearchBox.Text.ToLower();

            _serviceView.Filter = item =>
            {
                if (string.IsNullOrEmpty(query)) return true;
                return (item as ServiceItem).Name.ToLower().Contains(query);
            };

            ServiceSearchBox.IsDropDownOpen = true;

            // Возвращаем курсор в конец, чтобы текст не затирался при автодополнении
            if (textBox != null)
            {
                textBox.SelectionStart = textBox.Text.Length;
                textBox.SelectionLength = 0;
            }
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
            decimal.TryParse(PriceBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price);

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

