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
        private InventoryManager _inventoryManager => ((MainWindow)Application.Current.MainWindow)._inventoryManager;
        private RepairManager _repairManager => ((MainWindow) Application.Current.MainWindow)._repairManager;

        private List<RepairItem> _sessionAddedParts = new(); 
        private List<RepairItem> _sessionRemovedParts = new();

        // Коллекция, которая "живет" в таблице
        private ObservableCollection<RepairItem> _orderItems = new();

        public EditRepairWindow(RepairRecord repair)
        {
            InitializeComponent();
            _currentRepair = repair;

            // Заполняем поля данными из заказа
            OrderDateText.Text = $"Заказ от {repair.DateCreated:dd.MM.yyyy}";
            BikeInfoBox.Text = repair.BikeInfo;
            ProblemBox.Text = repair.ProblemDescription;
            StatusComboBox.Text = repair.Status;

            // Загружаем состав заказа из БД
            var itemsFromDb = _inventoryManager.GetRepairItems(repair.Id);
            _orderItems = new ObservableCollection<RepairItem>(itemsFromDb);

            // Привязываем коллекцию к таблице
            OrderItemsGrid.ItemsSource = _orderItems;

            // Подписываемся на изменения в таблице для автопересчета итогов
            _orderItems.CollectionChanged += (s, e) => UpdateTotals();
            UpdateTotals();
        }

        private void UpdateTotals()
        {
            double total = _orderItems.Sum(x => x.Total);

            // Прибыль = (Цена работы) + (Маржа запчасти: Розница - Закупка)
            double profit = _orderItems.Sum(x => x.ProductId.HasValue
                ? (x.Price - x.PurchasePrice) * x.Quantity
                : x.Total);

            TotalCostText.Text = $"{total:N0} ₽";
            ProfitText.Text = $"Чистая прибыль: {profit:N0} руб.";
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
            RollbackChanges();
            this.DialogResult = false;
        }

        // Вызываем это и при закрытии окна через X (событие Closing)
        private void RollbackChanges()
        {
            // 1. Возвращаем на склад то, что добавили в этом сеансе
            foreach (var item in _sessionAddedParts)
            {
                _inventoryManager.IncreaseQuantity(item.ProductId.Value, item.Quantity);
            }

            // 2. Снова списываем то, что пытались удалить
            foreach (var item in _sessionRemovedParts)
            {
                _inventoryManager.DecreaseQuantity(item.ProductId.Value, item.Quantity);
            }

            // Очищаем списки, чтобы не сработало дважды
            _sessionAddedParts.Clear();
            _sessionRemovedParts.Clear();
        }


    }
}
