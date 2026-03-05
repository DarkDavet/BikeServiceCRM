using BusinessAccountantService.Data;
using BusinessAccountantService.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Win32;
using QuestPDF.Fluent;
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

        private void AddClient_Click(object sender, RoutedEventArgs e)
        {
            AddClientWindow addWindow = new AddClientWindow();
            addWindow.Owner = this;

            if (addWindow.ShowDialog() == true)
            {
                LoadClients();
            }
        }

        private void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            if (ClientsGrid.SelectedItem is Client selectedClient)
            {
                ExportToPdf(selectedClient);
            }
            else
            {
                MessageBox.Show("Сначала выберите клиента в списке!");
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
                            Name = reader.GetString(1),
                            Phone = reader.GetInt32(2)
                        });
                    }
                }
            }
            ClientsGrid.ItemsSource = clients;

        }

        private void ExportToPdf(Client client)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                FileName = $"Report_{client.Name}.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(50);
                        page.Header().Text("ОТЧЕТ ВЕЛОМАСТЕРСКОЙ").FontSize(20).SemiBold().FontColor(Colors.Blue.A);

                        page.Content().PaddingVertical(20).Column(col =>
                        {
                            col.Item().Text($"Клиент: {client.Name}").FontSize(14);
                            col.Item().Text($"Телефон: {client.Phone}").FontSize(14);
                            col.Item().PaddingTop(10).LineHorizontal(1);
                            col.Item().PaddingTop(10).Text("Список выполненных работ:").FontSize(12).Italic();

                        });

                        page.Footer().AlignCenter().Text(x =>
                        {
                            x.Span("Дата формирования: ");
                            x.CurrentPageNumber();
                        });
                    });
                })
                .GeneratePdf(saveFileDialog.FileName);

                MessageBox.Show("PDF отчет успешно создан!");
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
                            IsCompleted = reader.GetInt32(6) == 1
                        });
                    }
                }
            }
            return list;
        }

        private void ClientsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClientsGrid.SelectedItem is Client selectedClient)
            {
                var repairs = GetRepairsByClient(selectedClient.Id);

               // RepairsGrid.ItemsSource = repairs;
            }
        }


    }
}