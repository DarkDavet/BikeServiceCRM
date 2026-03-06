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
        private readonly ClientManager _clientManager = new();
        private readonly RepairManager _repairManager = new();
        private readonly PdfExportManager _pdfmanager = new();
        public MainWindow()
        {
            InitializeComponent();
            DatabaseService.Initialize();
            LoadClients();
        }


        private void AddClient_Click(object sender, RoutedEventArgs e)
        {
            AddClientWindow addWindow = new AddClientWindow();
            addWindow.Owner = this;

            if (addWindow.ShowDialog() == true)
            {
                LoadClients();
            }
        }

        private void EntryAct_Click(object sender, RoutedEventArgs e)
        {
            if (RepairsHistoryGrid.SelectedItem is RepairRecord selectedRepair &&
                ClientsGrid.SelectedItem is Client selectedClient) 
            {
                UpdateRepairStatus(selectedRepair.Id, "Выдан");
                selectedRepair.Status = "Выдан";

                _pdfmanager.ExportEntryAct(selectedClient, selectedRepair);

                RepairsHistoryGrid.Items.Refresh();
            }
            else
            {
                MessageBox.Show("Выберите и клиента, и заказ!");
            }
        }

        private void FinalAct_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void AddRepair_Click(object sender, RoutedEventArgs e)
        {
            if (ClientsGrid.SelectedItem is Client selectedClient)
            {
                AddRepairWindow repairWin = new AddRepairWindow(selectedClient.Id);
                repairWin.Owner = this;

                if (repairWin.ShowDialog() == true)
                {
                    RepairsHistoryGrid.ItemsSource = _repairManager.GetRepairsByClient(selectedClient.Id);
                    MessageBox.Show("Заказ успешно добавлен в базу!");
                }
            }
            else
            {
                MessageBox.Show("Сначала выберите клиента из списка выше!");
            }
        }


        private void UpdateRepairStatus(int repairId, string newStatus)
        {
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "UPDATE Repairs SET Status = $status WHERE Id = $id";
                command.Parameters.AddWithValue("$status", newStatus);
                command.Parameters.AddWithValue("$id", repairId);
                command.ExecuteNonQuery();
            }
        }

        private void ClientsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClientsGrid.SelectedItem is Client selectedClient)
            {
                var repairs = _repairManager.GetRepairsByClient(selectedClient.Id);

                RepairsHistoryGrid.ItemsSource = repairs;
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _clientsView?.Refresh();
        }

        private void RepairsHistoryGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Column.Header.ToString() == "Статус" && e.EditAction == DataGridEditAction.Commit)
            {
                var repair = e.Row.Item as RepairRecord;
                var cb = e.EditingElement as ComboBox;

                if (repair != null && cb != null)
                {
                    string newStatus = cb.SelectedItem?.ToString() ?? cb.Text;

                    if (!string.IsNullOrEmpty(newStatus))
                    {
                        repair.Status = newStatus;

                        UpdateRepairStatus(repair.Id, newStatus);

                        if (newStatus == "Выдан")
                        {
                            MessageBox.Show("Статус обновлен. Не забудьте распечатать чек!");
                        }
                    }
                }
            }
        }

        private ICollectionView _clientsView;

        private void LoadClients()
        {
            var clients = _clientManager.GetAllClients();

            _clientsView = CollectionViewSource.GetDefaultView(clients);

            _clientsView.Filter = (obj) =>
            {
                string searchText = SearchBox.Text; 
                if (string.IsNullOrWhiteSpace(searchText)) return true;

                var client = obj as Client;
                string query = searchText.ToLower();

                return (client.Name?.ToLower().Contains(query) ?? false) ||
                       (client.Phone?.Contains(query) ?? false);
            };

            ClientsGrid.ItemsSource = _clientsView;
        }

    }
}