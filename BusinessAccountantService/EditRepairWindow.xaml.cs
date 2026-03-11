using BusinessAccountantService.Managers;
using BusinessAccountantService.Models;
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
    /// Interaction logic for EditRepairWindow.xaml
    /// </summary>
    public partial class EditRepairWindow : Window
    {
        private readonly InventoryManager _inventoryManager = new();
        public RepairRecord Repair { get; private set; }
        public bool IsDeleted { get; private set; } = false;


        public EditRepairWindow(RepairRecord repair)
        {
            InitializeComponent();
            Repair = repair;

            BikeInfoBox.Text = repair.BikeInfo;
            ProblemBox.Text = repair.ProblemDescription;
            WorksBox.Text = repair.WorksPerformed;
            PartsCostBox.Text = repair.PartsCost.ToString();
            TotalCostBox.Text = repair.TotalCost.ToString();

            RefreshSearchList();

            foreach (ComboBoxItem item in StatusComboBox.Items)
            {
                if (item.Content?.ToString() == repair.Status)
                {
                    StatusComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Repair.BikeInfo = BikeInfoBox.Text;
            Repair.ProblemDescription = ProblemBox.Text;
            Repair.WorksPerformed = WorksBox.Text;

            double.TryParse(PartsCostBox.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double parts);
            double.TryParse(TotalCostBox.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double total);

            Repair.PartsCost = parts;
            Repair.TotalCost = total;

            if (StatusComboBox.SelectedItem is ComboBoxItem selectedItem)
                Repair.Status = selectedItem.Content.ToString();

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите удалить этот заказ?",
                                         "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                IsDeleted = true;
                DialogResult = true; 
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WorksBox.Focus();
            WorksBox.SelectionStart = WorksBox.Text.Length;
        }

        private void CostFields_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ProfitText == null || PartsCostBox == null || TotalCostBox == null) return;

            double.TryParse(PartsCostBox.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double parts);
            double.TryParse(TotalCostBox.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double total);

            ProfitText.Text = $"Чистая прибыль: {total - parts} руб.";
        }

        private void ClearCosts_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Очистить список и обнулить суммы?\nВнимание: списанные со склада товары не вернутся автоматически!",
                                 "Сброс расчета", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                WorksBox.Clear();
                PartsCostBox.Text = "0";

                if (TotalCostBox != null) TotalCostBox.Text = "0";

                ItemSearchBox.SelectedIndex = -1;
                ItemSearchBox.Text = "";
                ItemSearchBox.Focus();

                CostFields_TextChanged(null, null);
            }
        }


        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            if (ItemSearchBox.SelectedItem is Item selectedItem)
            {
                if (selectedItem.Quantity <= 0)
                {
                    MessageBox.Show("Товар закончился на складе!", "Склад", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _inventoryManager.DecreaseQuantity(selectedItem.Id, 1);

                double.TryParse(PartsCostBox.Text, out double currentParts);
                PartsCostBox.Text = (currentParts + selectedItem.PurchasePrice).ToString();

                double.TryParse(TotalCostBox.Text, out double currentTotal);
                TotalCostBox.Text = (currentTotal + selectedItem.RetailPrice).ToString();

                WorksBox.Text += $"— {selectedItem.Name}: {selectedItem.RetailPrice} руб.\n";

                ItemSearchBox.SelectedIndex = -1;
                ItemSearchBox.Text = "";
            }
            else if (!string.IsNullOrWhiteSpace(ItemSearchBox.Text)) 
            {
                string manualName = ItemSearchBox.Text;
                if (!double.TryParse(ItemPriceBox.Text.Replace(",", "."), out double price))
                {
                    MessageBox.Show("Введите корректную цену для ручного ввода!");
                    return;
                }

                var result = MessageBox.Show($"Это запчасть (Расход)?\n'Да' — добавит в расходы.\n'Нет' — добавит в итоговую стоимость.",
                                             "Тип записи", MessageBoxButton.YesNoCancel);

                if (result == MessageBoxResult.Yes) 
                {
                    double.TryParse(PartsCostBox.Text, out double currentParts);
                    PartsCostBox.Text = (currentParts + price).ToString();

                    double.TryParse(TotalCostBox.Text, out double currentTotal);
                    TotalCostBox.Text = (currentTotal + price).ToString();

                    WorksBox.Text += $"— {manualName}: {price} руб.\n";
                }
                else if (result == MessageBoxResult.No) 
                {
                    double.TryParse(TotalCostBox.Text, out double currentTotal);
                    TotalCostBox.Text = (currentTotal + price).ToString();

                    WorksBox.Text += $"— {manualName}: {price} руб.\n";
                }

                ItemSearchBox.Text = "";
                ItemPriceBox.Clear();
            }

        }

        private void ItemSearchBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ItemSearchBox.SelectedItem is Item selectedItem)
            {
                ItemPriceBox.Text = selectedItem.RetailPrice.ToString();
            }
        }

        private void ItemSearchBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Enter) return;

            string query = ItemSearchBox.Text;

            if (string.IsNullOrWhiteSpace(query))
            {
                RefreshSearchList();
                ItemSearchBox.IsDropDownOpen = true;
                return;
            }

            RefreshSearchList(query);

            ItemSearchBox.IsDropDownOpen = ItemSearchBox.HasItems;

            var textBox = ItemSearchBox.Template.FindName("PART_TextBox", ItemSearchBox) as TextBox;
            if (textBox != null)
            {
                textBox.SelectionStart = query.Length;
            }
        }

        private void RefreshSearchList(string query = "")
        {
            var allItems = _inventoryManager.GetAllItems();

            var availableItems = allItems.Where(i => i.Quantity > 0);

            if (!string.IsNullOrWhiteSpace(query))
            {
                availableItems = availableItems.Where(i => i.Name.ToLower().Contains(query.ToLower()));
            }

            ItemSearchBox.ItemsSource = availableItems.ToList();
        }




    }
}
