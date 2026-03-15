using BusinessAccountantService.Data;
using BusinessAccountantService.Managers;
using BusinessAccountantService.Models;
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
        private readonly int _clientId;
        private Client _currentClient;

        private readonly RepairManager _repairManager = new();
        private readonly ClientManager _clientManager = new(); 
        private readonly PdfExportManager _pdfExportManager = new();

        public AddRepairWindow(int clientId)
        {
            InitializeComponent();
            _clientId = clientId;

            // Загружаем имя клиента при открытии окна
            LoadClientInfo();

            BikeInfoBox.Focus();
        }

        private void LoadClientInfo()
        {
            _currentClient = _clientManager.GetClientById(_clientId);
            if (_currentClient != null)
            {
                ClientNameText.Text = _currentClient.Name;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(BikeInfoBox.Text))
            {
                MessageBox.Show("Заполните данные о велосипеде!");
                return;
            }

            var newRepair = new RepairRecord
            {
                ClientId = _clientId,
                BikeInfo = BikeInfoBox.Text.Trim(),
                ProblemDescription = ProblemBox.Text.Trim(),
                Status = "Принят",
                DateCreated = DateTime.Now,
                TotalCost = 0,
                PartsCost = 0
            };

            try
            {
                int newId = _repairManager.AddRepair(newRepair);
                newRepair.Id = newId; 

                var result = MessageBox.Show("Заказ оформлен. Распечатать Акт приемки?",
                                             "Печать", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _pdfExportManager.ExportEntryAct(_currentClient, newRepair);
                }

                this.DialogResult = true;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => this.DialogResult = false;

    }
}
