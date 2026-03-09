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
        private ViewMode _currentMode = ViewMode.All;

        private readonly ClientManager _clientManager = new();
        private readonly RepairManager _repairManager = new();
        private readonly PdfExportManager _pdfmanager = new();
        public MainWindow()
        {
            InitializeComponent();
            //DatabaseService.ResetDatabase();
            DatabaseService.Initialize();
            LoadClients();
            UpdateStatusInfo();
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

        private void SwitchViewMode(ViewMode newMode, Button activeBtn)
        {
            _currentMode = newMode;

            // Сбрасываем цвета всех кнопок
            BtnAllClients.Background = _defaultButtonBrush;
            BtnActiveOrders.Background = _defaultButtonBrush;
            BtnArchive.Background = _defaultButtonBrush;

            // Подсвечиваем нажатую
            activeBtn.Background = Brushes.LightGreen;

            // Загружаем данные
            var clients = _clientManager.GetClientsByMode(_currentMode);
            _clientsView = CollectionViewSource.GetDefaultView(clients);
            _clientsView.Filter = ClientFilterPredicate;
            ClientsGrid.ItemsSource = _clientsView;

            RepairsHistoryGrid.ItemsSource = null;
            UpdateStatusInfo();
        }

        private void ShowAllClients_Click(object sender, RoutedEventArgs e) => SwitchViewMode(ViewMode.All, BtnAllClients);
        private void ShowActiveOrders_Click(object sender, RoutedEventArgs e) => SwitchViewMode(ViewMode.Active, BtnActiveOrders);
        private void ShowArchive_Click(object sender, RoutedEventArgs e) => SwitchViewMode(ViewMode.Archive, BtnArchive);

        private bool ClientFilterPredicate(object obj)
        {
            // Если в строке поиска пусто — показываем всех
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
                return true;

            var client = obj as Client;
            if (client == null) return false;

            string query = SearchBox.Text.ToLower();

            // Проверяем совпадение в имени или телефоне
            // Важно: используйте те свойства, которые есть в вашем классе Client (Name или FullName)
            return (client.Name != null && client.Name.ToLower().Contains(query)) ||
                   (client.Phone != null && client.Phone.Contains(query));
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
                    RepairsHistoryGrid.ItemsSource = _repairManager.GetRepairsByClient(selectedClient.Id, _currentMode);
                    UpdateStatusInfo();
                    MessageBox.Show("Заказ успешно добавлен в базу!");
                }
            }
            else
            {
                MessageBox.Show("Сначала выберите клиента из списка выше!");
            }
        }

        private void DeleteClient_Click(object sender, RoutedEventArgs e)
        {
            if (ClientsGrid.SelectedItem is Client selectedClient)
            {
                if (MessageBox.Show($"Удалить {selectedClient.Name} и его историю?", "Удаление",
                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
                    {
                        connection.Open();

                        // 1. ВКЛЮЧАЕМ КАСКАД (в этом конкретном соединении)
                        using (var pragmaCmd = new SqliteCommand("PRAGMA foreign_keys = ON;", connection))
                        {
                            pragmaCmd.ExecuteNonQuery();
                        }

                        // 2. УДАЛЯЕМ ТОЛЬКО КЛИЕНТА (ремонты удалятся сами)
                        using (var deleteCmd = new SqliteCommand("DELETE FROM Clients WHERE Id = $id", connection))
                        {
                            deleteCmd.Parameters.AddWithValue("$id", selectedClient.Id);
                            deleteCmd.ExecuteNonQuery();
                        }
                    }
                    UpdateStatusInfo();
                    LoadClients(); // Обновляем список на экране
                    RepairsHistoryGrid.ItemsSource = null; // Очищаем таблицу ремонтов
                }
            }
        }

        private void DeleteRepair_Click(object sender, RoutedEventArgs e)
        {
            if (RepairsHistoryGrid.SelectedItem is RepairRecord selectedRepair)
            {
                if (MessageBox.Show("Удалить этот заказ?", "Удаление заказа", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    using (var conn = new SqliteConnection(DatabaseService.ConnectionString))
                    {
                        conn.Open();
                        var cmd = new SqliteCommand("DELETE FROM Repairs WHERE Id = $id", conn);
                        cmd.Parameters.AddWithValue("$id", selectedRepair.Id);
                        cmd.ExecuteNonQuery();
                    }
                    // Обновляем только нижнюю таблицу
                    if (ClientsGrid.SelectedItem is Client c)
                        RepairsHistoryGrid.ItemsSource = _repairManager.GetRepairsByClient(c.Id, _currentMode);
                    UpdateStatusInfo();
                }
            }
        }



        private void ResetDb_Click(object sender, RoutedEventArgs e)
        {
            DatabaseService.ResetDatabase();
        }

        private void RepairsHistoryGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                DeleteRepair_Click(sender, e);
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
                var repairs = _repairManager.GetRepairsByClient(selectedClient.Id, _currentMode);

                RepairsHistoryGrid.ItemsSource = repairs;
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _clientsView?.Refresh();
            UpdateStatusInfo();
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
            BtnAllClients.Background = Brushes.LightGreen;
            BtnActiveOrders.Background = _defaultButtonBrush;

            List<Client> clients = _clientManager.GetClientsByMode(_currentMode); // Ваш метод загрузки из SQLite

            _clientsView = CollectionViewSource.GetDefaultView(clients);

            // Привязываем наш метод-фильтр
            _clientsView.Filter = ClientFilterPredicate;

            ClientsGrid.ItemsSource = _clientsView;
        }

        private void UpdateStatusInfo()
        {
            if (_clientsView == null) return;

            int visibleClientsCount = 0;
            int visibleRepairsCount = 0;

            // Проходим по клиентам, которые видны в списке прямо сейчас (с учетом поиска и вкладок)
            foreach (var item in _clientsView)
            {
                if (item is Client client)
                {
                    visibleClientsCount++;

                    // Считаем заказы только для этого клиента, 
                    // передавая текущий режим _currentMode (Active, Archive или All)
                    var repairs = _repairManager.GetRepairsByClient(client.Id, _currentMode);
                    visibleRepairsCount += repairs.Count;
                }
            }

            // Меняем текст и цвет в зависимости от режима
            if (_currentMode == ViewMode.Archive)
            {
                StatusInfoText.Text = $"Клиентов в архиве: {visibleClientsCount} | Выдано заказов: {visibleRepairsCount}";
                StatusInfoText.Foreground = Brushes.RoyalBlue; // Синий для архива
            }
            else if (_currentMode == ViewMode.Active)
            {
                StatusInfoText.Text = $"Активных клиентов: {visibleClientsCount} | Заказов в работе: {visibleRepairsCount}";
                StatusInfoText.Foreground = Brushes.SeaGreen; // Зеленый для работы
            }
            else
            {
                StatusInfoText.Text = $"Всего клиентов: {visibleClientsCount} | Всего заказов: {visibleRepairsCount}";
                StatusInfoText.Foreground = Brushes.Gray; // Нейтральный для общего списка
            }
        }

        // Добавьте событие MouseDoubleClick в XAML для RepairsHistoryGrid
        private void RepairsHistoryGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // 1. Проверяем, что в таблице действительно выбран заказ
            if (RepairsHistoryGrid.SelectedItem is RepairRecord selectedRepair)
            {
                // 2. Открываем окно редактирования и передаем в него выбранный заказ
                EditRepairWindow editWin = new EditRepairWindow(selectedRepair);
                editWin.Owner = this; // Чтобы окно открывалось по центру главного

                // 3. Ждем закрытия окна. Если нажали "Сохранить" или "Удалить" (DialogResult = true)
                if (editWin.ShowDialog() == true)
                {
                    if (editWin.IsDeleted)
                    {
                        // Если в окне нажали кнопку "Удалить"
                        _repairManager.DeleteRepair(selectedRepair.Id);
                        MessageBox.Show("Заказ удален.");
                    }
                    else
                    {
                        // Если нажали "Сохранить"
                        // (Объект selectedRepair уже содержит новые данные, т.к. мы правили его в окне)
                        _repairManager.UpdateRepair(selectedRepair);
                        MessageBox.Show("Изменения сохранены.");
                    }

                    // 4. ОБЯЗАТЕЛЬНО обновляем данные на экране, чтобы увидеть изменения
                    if (ClientsGrid.SelectedItem is Client selectedClient)
                    {
                        // Перезагружаем список заказов для текущего клиента
                        RepairsHistoryGrid.ItemsSource = _repairManager.GetRepairsByClient(selectedClient.Id, _currentMode);
                    }

                    // Обновляем зеленый счетчик (вдруг мы удалили заказ или изменили его статус)
                    UpdateStatusInfo();
                }
            }
        }


    }
}