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
        public StatsManager _statsManager = new();
        public ExpensesManager _expensesManager = new();

        public MainWindow()
        {
           // DatabaseService.ResetDatabase();
            InitializeComponent();
            DatabaseService.Initialize();
            

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

        private void HighlightButton(Button activeBtn)
        {
            // 1. Сбрасываем кнопки навигации в дефолт (прозрачные)
            BtnAllClients.ClearValue(Button.BackgroundProperty);
            BtnActiveOrders.ClearValue(Button.BackgroundProperty);
            BtnArchive.ClearValue(Button.BackgroundProperty);

            // Сбрасываем текст (ClearValue заставит кнопку взять цвет из стиля)
            BtnAllClients.ClearValue(Button.ForegroundProperty);
            BtnActiveOrders.ClearValue(Button.ForegroundProperty);
            BtnArchive.ClearValue(Button.ForegroundProperty);

            // 2. Активируем выбранную кнопку
            if (activeBtn != null)
            {
                if (activeBtn == BtnInventory)
                {
                    activeBtn.Background = (Brush)new BrushConverter().ConvertFrom("#FF2BA5C0");
                    activeBtn.Foreground = Brushes.White;
                }
                else
                {
                    // Красим в LightGreen — XAML увидит этот цвет и сам сделает текст ТЕМНЫМ
                    activeBtn.Background = Brushes.LightGreen;
                }
            }
        }




        private void ShowMonthlyStats_Click(object sender, RoutedEventArgs e)
        {
            AnalyticsWindow analyticsWin = new AnalyticsWindow();
            analyticsWin.Owner = this; 
            analyticsWin.ShowDialog();
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