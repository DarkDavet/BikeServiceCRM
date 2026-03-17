using BusinessAccountantService.Data;
using BusinessAccountantService.Managers;
using BusinessAccountantService.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BusinessAccountantService
{
    /// <summary>
    /// Interaction logic for ClientsPage.xaml
    /// </summary>
    public partial class ClientsPage : Page
    {
        private ViewMode _currentMode;
        private ICollectionView _clientsView;

        private ClientManager _clientManager = new();
        private RepairManager _repairManager = new();
        private PdfExportManager _pdfManager = new();
        private InventoryManager _inventoryManager = new();

        public ClientsPage(ViewMode mode)
        {
            InitializeComponent();
            _currentMode = mode;
            LoadClients();
        }

        private void LoadClients()
        {
            var clients = _clientManager.GetClientsByMode(_currentMode);
            _clientsView = CollectionViewSource.GetDefaultView(clients);
            _clientsView.Filter = ClientFilterPredicate;
            ClientsGrid.ItemsSource = _clientsView;
            UpdateStatusInfo();
        }

        private void AddClient_Click(object sender, RoutedEventArgs e)
        {
            AddClientWindow addWindow = new AddClientWindow { Owner = Application.Current.MainWindow };
            if (addWindow.ShowDialog() == true) LoadClients();
        }

        private void AddRepair_Click(object sender, RoutedEventArgs e)
        {
            if (ClientsGrid.SelectedItem is Client selectedClient)
            {
                AddRepairWindow repairWin = new AddRepairWindow(selectedClient.Id)
                {
                    Owner = Application.Current.MainWindow
                };

                if (repairWin.ShowDialog() == true)
                {
                    RepairsHistoryGrid.ItemsSource = _repairManager.GetRepairsByClient(selectedClient.Id, _currentMode);
                    UpdateStatusInfo();
                    MessageBox.Show("Заказ добавлен!");
                }
            }
            else
            {
                MessageBox.Show("Выберите клиента!");
            }
        }

        private void DeleteRepair_Click(object sender, RoutedEventArgs e)
        {
            if (RepairsHistoryGrid.SelectedItem is RepairRecord selectedRepair)
            {
                if (MessageBox.Show("Удалить этот заказ?", "Удаление", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    _repairManager.DeleteRepair(selectedRepair.Id);

                    if (ClientsGrid.SelectedItem is Client c)
                        RepairsHistoryGrid.ItemsSource = _repairManager.GetRepairsByClient(c.Id, _currentMode);

                    UpdateStatusInfo();
                }
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _clientsView?.Refresh();
            UpdateStatusInfo();
        }

        private bool ClientFilterPredicate(object obj)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text)) return true;
            var client = obj as Client;
            if (client == null) return false;

            string query = SearchBox.Text.ToLower();
            return (client.Name?.ToLower().Contains(query) ?? false) ||
                   (client.Phone?.Contains(query) ?? false);
        }

        // PDF акты
        private void EntryAct_Click(object sender, RoutedEventArgs e)
        {
            if (RepairsHistoryGrid.SelectedItem is RepairRecord selectedRepair &&
                ClientsGrid.SelectedItem is Client selectedClient)
            {
                _pdfManager.ExportEntryAct(selectedClient, selectedRepair);
                RepairsHistoryGrid.Items.Refresh();
            }
            else
            {
                MessageBox.Show("Выберите клиента и заказ для печати акта приемки!");
            }
        }

        private void FinalAct_Click(object sender, RoutedEventArgs e)
        {
            if (RepairsHistoryGrid.SelectedItem is RepairRecord selectedRepair &&
                ClientsGrid.SelectedItem is Client selectedClient)
            {
                if (selectedRepair.Status != "Выдан" && selectedRepair.Status != "Готов")
                {
                    MessageBox.Show("Сначала завершите ремонт и выдайте заказ через окно редактирования (двойной клик), чтобы зафиксировать запчасти и итоговую сумму!",
                                    "Печать невозможна", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var items = _inventoryManager.GetRepairItems(selectedRepair.Id);

                _pdfManager.ExportFinalAct(selectedClient, selectedRepair, items);
            }
            else
            {
                MessageBox.Show("Выберите клиента и заказ!");
            }
        }

        private void UpdateStatusInfo()
        {
            int clientCount = ClientsGrid.Items.Count;

            int activeOrders = _repairManager.GetActiveRepairsCount();
            int totalOrders = _repairManager.GetAllRepairsCount();
            int archivedOrders = totalOrders - activeOrders;

            if (_currentMode == ViewMode.Active)
            {
                StatusInfoText.Text = $"Активных клиентов: {clientCount} | Заказов в работе: {activeOrders}";
                StatusInfoText.Foreground = Brushes.SeaGreen; 
                MainTitleText.Text = "В РАБОТЕ";
            }
            else if (_currentMode == ViewMode.Archive)
            {
                StatusInfoText.Text = $"Спящих клиентов: {clientCount} | Выполненных заказов: {archivedOrders}";
                StatusInfoText.Foreground = Brushes.RoyalBlue;
                MainTitleText.Text = "АРХИВ";
            }
            else 
            {
                StatusInfoText.Text = $"Всего клиентов: {clientCount} | Всего заказов: {totalOrders}";
                StatusInfoText.Foreground = Brushes.DimGray;
                MainTitleText.Text = "БАЗА КЛИЕНТОВ И ЗАКАЗОВ";
            }
        }


        private void ClientsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ClientsGrid.SelectedItem is Client selectedClient)
            {
                EditClientWindow editWin = new EditClientWindow(selectedClient)
                {
                    Owner = Application.Current.MainWindow
                };

                if (editWin.ShowDialog() == true)
                {
                    if (editWin.IsDeleted)
                    {
                        _clientManager.DeleteClient(selectedClient);
                        LoadClients();
                        RepairsHistoryGrid.ItemsSource = null;
                    }
                    else
                    {
                        _clientManager.UpdateClient(selectedClient);
                        LoadClients();
                    }
                    UpdateStatusInfo();
                }
            }
        }

        private void RepairsHistoryGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (RepairsHistoryGrid.SelectedItem is RepairRecord selectedRepair)
            {
                EditRepairWindow editWin = new EditRepairWindow(selectedRepair)
                {
                    Owner = Application.Current.MainWindow
                };

                if (editWin.ShowDialog() == true)
                {
                 
                    _repairManager.UpdateRepair(selectedRepair);

                    if (ClientsGrid.SelectedItem is Client selectedClient)
                        RepairsHistoryGrid.ItemsSource = _repairManager.GetRepairsByClient(selectedClient.Id, _currentMode);

                    UpdateStatusInfo();
                }
            }
        }

        private void ClientsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClientsGrid.SelectedItem is Client selectedClient)
            {
                RepairsHistoryGrid.ItemsSource = _repairManager.GetRepairsByClient(selectedClient.Id, _currentMode);
            }
            else
            {
                RepairsHistoryGrid.ItemsSource = null;
            }
        }
    }
}
