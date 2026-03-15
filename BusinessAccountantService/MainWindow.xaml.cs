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