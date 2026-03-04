using BusinessAccountantService.Models;
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
        public MainWindow()
        {
            InitializeComponent();

            // Создаем тестовый список
            List<RepairRecord> repairs = new List<RepairRecord>
            {
                new RepairRecord { Id = 1, ClientName = "Иван Иванов", BikeModel = "Giant Talon 2", Status = "В работе", Cost = 1500 },
                new RepairRecord { Id = 2, ClientName = "Анна Петрова", BikeModel = "Specialized Sirrus", Status = "Готов", Cost = 3200 }
            };

            // Привязываем список к таблице в XAML (которую мы назвали RepairsGrid)
            RepairsGrid.ItemsSource = repairs;
        }
    }
}