using BusinessAccountantService.Data;
using BusinessAccountantService.Managers;
using BusinessAccountantService.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Win32;
using QuestPDF.Fluent;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BusinessAccountantService
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Brush _defaultButtonBrush = (Brush)new BrushConverter().ConvertFrom("#34495E");

        // Менеджеры оставляем public, чтобы Page могли к ним обращаться
        public ClientManager _clientManager = new();
        public RepairManager _repairManager = new();
        public PdfExportManager _pdfmanager = new();
        public InventoryManager _inventoryManager = new();

        public MainWindow()
        {
            InitializeComponent();
            DatabaseService.Initialize();
            StatsDatePicker.SelectedDate = DateTime.Now;

            // При запуске сразу открываем страницу клиентов
            ShowAllClients_Click(null, null);
        }

        // МЕТОДЫ НАВИГАЦИИ
        private void ShowAllClients_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ClientsPage(ViewMode.All));
            HighlightButton(BtnAllClients);
        }

        private void ShowActiveOrders_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ClientsPage(ViewMode.Active));
            HighlightButton(BtnActiveOrders);
        }

        private void ShowArchive_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ClientsPage(ViewMode.Archive));
            HighlightButton(BtnArchive);
        }

        private void ShowInventory_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new InventoryPage());
            HighlightButton(BtnInventory);
        }

        // Подсветка кнопок меню
        private void HighlightButton(Button activeBtn)
        {
            BtnAllClients.Background = _defaultButtonBrush;
            BtnActiveOrders.Background = _defaultButtonBrush;
            BtnArchive.Background = _defaultButtonBrush;
            BtnInventory.Background = (Brush)new BrushConverter().ConvertFrom("#FF2BA5C0"); // Цвет склада

            if (activeBtn != null)
                activeBtn.Background = Brushes.LightGreen; // Или ваш цвет активной кнопки
        }

        // Статистика и DatePicker остаются здесь, так как они в боковом меню
        private void ShowMonthlyStats_Click(object sender, RoutedEventArgs e)
        {
            DateTime selectedDate = StatsDatePicker.SelectedDate ?? DateTime.Now;

            var monthly = _repairManager.GetStatsByMonth(selectedDate);
            var global = _repairManager.GetGlobalStats();
            double inventoryValue = _inventoryManager.GetTotalInventoryValue();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"📊 ОТЧЕТ ЗА {selectedDate:MMMM yyyy.ToUpper()}");
            sb.AppendLine($"💰 Выручка: {monthly.rev:N0} руб.");
            sb.AppendLine($"💸 Затраты на з/п: {monthly.parts:N0} руб.");
            sb.AppendLine($"📈 Чистая прибыль: {monthly.prof:N0} руб.");
            sb.AppendLine($"🚲 Выдано заказов: {monthly.count} шт.");

            sb.AppendLine("\n" + new string('─', 25) + "\n");

            sb.AppendLine("🌍 ЗА ВСЁ ВРЕМЯ РАБОТЫ:");
            sb.AppendLine($"💰 Общий оборот: {global.totalRev:N0} руб.");
            sb.AppendLine($"💸 Всего на з/п: {global.totalParts:N0} руб.");
            sb.AppendLine($"💎 Общая прибыль: {global.totalProf:N0} руб.");
            sb.AppendLine($"🚲 Всего обслужено: {global.totalCount} шт.");

            sb.AppendLine("\n📦 СКЛАД СЕЙЧАС:");
            sb.AppendLine($"💎 Стоимость остатков: {inventoryValue:N0} руб.");

            MessageBox.Show(sb.ToString(), "Финансовая аналитика", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private void StatsDatePicker_CalendarOpened(object sender, RoutedEventArgs e)
        {
            var datePicker = sender as DatePicker;
            if (datePicker == null) return;

            var popup = datePicker.Template.FindName("PART_Popup", datePicker) as System.Windows.Controls.Primitives.Popup;
            if (popup != null && popup.Child is System.Windows.Controls.Calendar calendar)
            {
                calendar.DisplayMode = CalendarMode.Year;

                calendar.DisplayModeChanged += (s, args) =>
                {
                    if (calendar.DisplayMode == CalendarMode.Month)
                    {
                        datePicker.SelectedDate = calendar.DisplayDate;
                        datePicker.IsDropDownOpen = false;
                    }
                };
            }
        }

        private void StatsDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StatsDatePicker.SelectedDate is DateTime date)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    var textBox = StatsDatePicker.Template.FindName("PART_TextBox", StatsDatePicker) as TextBox;
                    if (textBox != null)
                    {
                        textBox.Text = date.ToString("MMMM yyyy");
                    }
                }), System.Windows.Threading.DispatcherPriority.Background);
            }

        }

        private void ResetDb_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы уверены? Это удалит все данные!", "ВНИМАНИЕ", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                DatabaseService.ResetDatabase();
                MainFrame.Refresh();
            }
        }
    }
}