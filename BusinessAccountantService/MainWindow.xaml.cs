using BusinessAccountantService.Data;
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
        public MainWindow()
        {
            InitializeComponent();
            DatabaseService.Initialize();
            LoadClients();
        }

        private ICollectionView _clientsView;

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

                ExportEntryAct(selectedClient, selectedRepair);

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
                    RepairsHistoryGrid.ItemsSource = GetRepairsByClient(selectedClient.Id);
                    MessageBox.Show("Заказ успешно добавлен в базу!");
                }
            }
            else
            {
                MessageBox.Show("Сначала выберите клиента из списка выше!");
            }
        }

        private void LoadClients()
        {
            List<Client> clients = new List<Client>();

            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, FullName, Phone FROM Clients";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        clients.Add(new Client
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.IsDBNull(1) ? "" : reader.GetString(1),
                            Phone = reader.IsDBNull(2) ? "" : reader.GetString(2)
                        });
                    }
                }
            }

            _clientsView = CollectionViewSource.GetDefaultView(clients);

            _clientsView.Filter = (obj) =>
            {
                if (string.IsNullOrWhiteSpace(SearchBox.Text)) return true;

                var client = obj as Client;
                if (client == null) return false;

                string query = SearchBox.Text.ToLower();

                bool nameMatch = client.Name?.ToLower().Contains(query) ?? false;
                bool phoneMatch = client.Phone?.Contains(query) ?? false;

                return nameMatch || phoneMatch;
            };

            ClientsGrid.ItemsSource = _clientsView;

        }

        private void ExportEntryAct(Client client, RepairRecord repair)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                FileName = $"Priemka_{client.Name}_{DateTime.Now:ddMMyy}.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(50);

                        page.Header().Row(row => {
                            row.RelativeItem().Column(col => {
                                col.Item().Text("АКТ ПРИЕМКИ ВЕЛОСИПЕДА").FontSize(20).SemiBold().FontColor(Colors.Blue.B);
                                col.Item().Text($"Номер заказа: #00{repair.Id}").FontSize(10);
                            });
                            row.RelativeItem().AlignRight().Column(c => {
                                c.Item().Text(repair.Status.ToUpper()).FontSize(24).Bold().FontColor(Colors.Green.B);
                                c.Item().Text("ОПЛАЧЕНО").FontSize(10).AlignCenter();
                            });
                            row.RelativeItem().AlignRight().Text(DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
                        });

                        page.Content().PaddingVertical(20).Column(col =>
                        {
                            col.Item().PaddingBottom(5).Text("ДАННЫЕ КЛИЕНТА").Bold();
                            col.Item().Text($"ФИО: {client.Name}");
                            col.Item().Text($"Телефон: {client.Phone}");

                            col.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Gray.R);

                            col.Item().PaddingBottom(5).Text("ОБЪЕКТ ПРИЕМКИ").Bold();
                            col.Item().Text($"Велосипед: {repair.BikeInfo}");

                            col.Item().PaddingTop(15).Text("ОПИСАНИЕ НЕИСПРАВНОСТИ:").Bold();
                            col.Item().Border(0.5f).Padding(10).Background(Colors.Gray.R)
                                .Text(repair.ProblemDescription).Italic();

                            col.Item().PaddingTop(40).Row(row => {
                                row.RelativeItem().Text("Принял: __________");
                                row.RelativeItem().AlignRight().Text("Сдал: __________");
                            });
                        });

                        page.Footer().AlignCenter().Text(x => {
                            x.Span("Стр. ");
                            x.CurrentPageNumber();
                        });
                    });
                })
                .GeneratePdf(saveFileDialog.FileName);

                Process.Start(new ProcessStartInfo(saveFileDialog.FileName) { UseShellExecute = true });
            }
        }

        private List<RepairRecord> GetRepairsByClient(int clientId)
        {
            var list = new List<RepairRecord>();
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Repairs WHERE ClientId = $id ORDER BY DateCreated DESC";
                command.Parameters.AddWithValue("$id", clientId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new RepairRecord
                        {
                            Id = reader.GetInt32(0),
                            BikeInfo = reader.GetString(2),
                            ProblemDescription = reader.GetString(3),
                            WorksPerformed = reader.IsDBNull(4) ? "" : reader.GetString(4),
                            TotalCost = reader.GetDouble(5),
                            Status = reader.GetString(6)
                        });
                    }
                }
            }
            return list;
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
                var repairs = GetRepairsByClient(selectedClient.Id);

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
                        UpdateRepairStatus(repair.Id, newStatus);

                        repair.Status = newStatus;

                        if (newStatus == "Выдан")
                        {
                            MessageBox.Show("Статус обновлен. Не забудьте распечатать чек!");
                        }
                    }
                }
            }
        }

    }
}