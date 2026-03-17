using BusinessAccountantService.Managers;
using BusinessAccountantService.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for EditRepairWindow.xaml
    /// </summary>
    public partial class EditRepairWindow : Window
    {
        private RepairRecord _currentRepair;

        private InventoryManager _inventoryManager = new();
        private RepairManager _repairManager = new();
        private ClientManager _clientManager = new();

        private List<RepairItem> _sessionAddedParts = new(); 
        private List<RepairItem> _sessionRemovedParts = new();

        // Коллекция, которая "живет" в таблице
        private ObservableCollection<RepairItem> _orderItems = new();

        public EditRepairWindow(RepairRecord repair)
        {
            InitializeComponent();
            _currentRepair = repair;
            this.Closing += EditRepairWindow_Closing;

            // 1. Сначала ставим номер и дату (они работают)
            OrderIdText.Text = repair.Id.ToString("D4");
            OrderDateText.Text = $"Заказ от {repair.DateCreated:dd.MM.yyyy}";

            // 2. ЗАГРУЗКА КЛИЕНТА (С проверкой!)
            var client = _clientManager.GetClientById(repair.ClientId);
            if (client != null)
            {
                ClientNameText.Text = $"Клиент: {client.Name}";
            }
            else
            {
                // Если увидите это в окне — значит в repair.ClientId пришел 0 или не тот ID
                ClientNameText.Text = $"Клиент не найден (ID: {repair.ClientId})";
            }

            // 3. Остальные поля
            BikeInfoBox.Text = repair.BikeInfo;
            ProblemBox.Text = repair.ProblemDescription;

            // ВАЖНО: Установка текста в ComboBox
            StatusComboBox.Text = repair.Status;

            // 4. Загрузка таблицы
            var itemsFromDb = _inventoryManager.GetRepairItems(repair.Id);
            _orderItems = new ObservableCollection<RepairItem>(itemsFromDb);
            OrderItemsGrid.ItemsSource = _orderItems;

            _orderItems.CollectionChanged += (s, e) => UpdateTotals();
            UpdateTotals();

            if (repair.Status == "Выдан")
            {
                LockWindowForEditing();
            }
        }

        private void UpdateTotals()
        {
            decimal total = _orderItems.Sum(x => x.Total);

            // Прибыль = (Цена работы) + (Маржа запчасти: Розница - Закупка)
            decimal profit = _orderItems.Sum(x => x.ProductId.HasValue
                ? (x.Price - x.PurchasePrice) * x.Quantity
                : x.Total);

            TotalCostText.Text = $"{total:N2} ₽";
            ProfitText.Text = $"Чистая прибыль: {profit:N2} руб.";
        }

        // Кнопка + ЗАПЧАСТЬ
        private void OpenPartSearch_Click(object sender, RoutedEventArgs e)
        {
            var win = new AddPartToRepairRecordWindow { Owner = this };
            if (win.ShowDialog() == true)
            {
                var newItem = win.SelectedResult;
                _inventoryManager.DecreaseQuantity(newItem.ProductId.Value, newItem.Quantity);
                _orderItems.Add(newItem);

                // Запоминаем, что мы это добавили
                _sessionAddedParts.Add(newItem);
                UpdateTotals();
            }
        }

        // Кнопка + РАБОТА
        private void OpenServiceSearch_Click(object sender, RoutedEventArgs e)
        {
            var win = new AddServiceToRepairRecordWindow { Owner = this };
            if (win.ShowDialog() == true)
            {
                _orderItems.Add(win.SelectedResult);
                UpdateTotals();
            }
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (OrderItemsGrid.SelectedItem is RepairItem selected)
            {
                // 1. Если это была запчасть — возвращаем на склад
                if (selected.ProductId.HasValue)
                {
                    _inventoryManager.IncreaseQuantity(selected.ProductId.Value, selected.Quantity);

                    // Запоминаем, что мы это вернули на склад
                    _sessionRemovedParts.Add(selected);
                }

                // 2. Удаляем из таблицы
                _orderItems.Remove(selected);
                UpdateTotals();
            }
        }


        private void Save_Click(object sender, RoutedEventArgs e)
        {
            OrderItemsGrid.CommitEdit(DataGridEditingUnit.Row, true);

            string selectedStatus = StatusComboBox.Text;

            if (selectedStatus == "Выдан" && _currentRepair.Status != "Выдан")
            {
                var result = MessageBox.Show(
                    "Установка статуса 'Выдан' заблокирует редактирование заказа навсегда. Вы уверены?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.No) return; 
            }

            _currentRepair.BikeInfo = BikeInfoBox.Text;
            _currentRepair.ProblemDescription = ProblemBox.Text;
            _currentRepair.Status = StatusComboBox.Text;
            _currentRepair.TotalCost = _orderItems.Sum(x => x.Total);
            _currentRepair.PartsCost = _orderItems.Where(x => x.ProductId.HasValue).Sum(x => x.PurchasePrice * x.Quantity);

            try
            {
                // 1. Обновляем основную карточку заказа
                _repairManager.UpdateRepair(_currentRepair);

                // 2. Сохраняем состав заказа (удаляем старые строки и пишем новые)
                _inventoryManager.SaveRepairItems(_currentRepair.Id, _orderItems);

                _sessionAddedParts.Clear();
                _sessionRemovedParts.Clear();

                if(_currentRepair.Status == "Выдан")
                {
                    LockWindowForEditing();
                }

                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}");
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Удалить этот заказ навсегда?", "Удаление", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _repairManager.DeleteRepair(_currentRepair.Id);
                this.DialogResult = true;
            }
        }



        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void EditRepairWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.DialogResult != true)
            {
                RollbackChanges();
            }
        }

        private void RollbackChanges()
        {
            foreach (var item in _sessionAddedParts)
            {
                _inventoryManager.IncreaseQuantity(item.ProductId.Value, item.Quantity);
            }

            foreach (var item in _sessionRemovedParts)
            {
                _inventoryManager.DecreaseQuantity(item.ProductId.Value, item.Quantity);
            }

            _sessionAddedParts.Clear();
            _sessionRemovedParts.Clear();
        }

        private void LockWindowForEditing()
        {
            BikeInfoBox.IsReadOnly = true;
            ProblemBox.IsReadOnly = true;

            OrderItemsGrid.IsReadOnly = true;
            BtnOpenPartSearch.IsEnabled = false;
            BtnOpenServiceSearch.IsEnabled = false;

           if (DeleteColumn != null) DeleteColumn.Visibility = Visibility.Collapsed;

            StatusComboBox.IsEnabled = false;

            BtnSave.Visibility = Visibility.Collapsed;
            BtnDelete.Visibility = Visibility.Collapsed;

            this.Title = $"ПРОСМОТР ЗАКАЗА №{_currentRepair.Id} (ВЫДАН)";
        }

    }
}
