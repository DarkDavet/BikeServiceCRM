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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BusinessAccountantService
{
    /// <summary>
    /// Interaction logic for InventoryPage.xaml
    /// </summary>
    public partial class InventoryPage : Page
    {
        private readonly InventoryManager _inventoryManager = new();

        public InventoryPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            var items = _inventoryManager.GetAllItems();
            InventoryGrid.ItemsSource = items;

            PartsCountText.Text = items.Where(i => i.Category == "Запчасти").Sum(i => i.Quantity) + " шт.";
            ToolsCountText.Text = items.Where(i => i.Category == "Инструменты").Sum(i => i.Quantity) + " шт.";
            ChemicalsCountText.Text = items.Where(i => i.Category == "Химия/Средства").Sum(i => i.Quantity) + " шт.";
            InventoryCountText.Text = items.Where(i => i.Category == "Инвентарь").Sum(i => i.Quantity) + " шт.";
        }

        private void FilterChanged(object sender, RoutedEventArgs e)
        {
            var view = CollectionViewSource.GetDefaultView(InventoryGrid.ItemsSource);
            if (view == null) return;

            view.Filter = obj =>
            {
                if (obj is not Item item) return false;

                // 1. Поиск по тексту
                string query = SearchBox.Text.ToLower();
                bool matchesSearch = string.IsNullOrWhiteSpace(query) ||
                                     item.Name.ToLower().Contains(query);

                // 2. Фильтр "Только запчасти"
                bool matchesPartOnly = true;
                if (OnlyPartsCheck.IsChecked == true)
                {
                    matchesPartOnly = item.Category == "Запчасти";
                }

                return matchesSearch && matchesPartOnly;
            };
        }

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            // Owner должен быть Window, а не Page
            AddItemToInventoryWindow addItemWin = new AddItemToInventoryWindow
            {
                Owner = Application.Current.MainWindow
            };

            if (addItemWin.ShowDialog() == true)
            {
                LoadData();
            }
        }

        private void ScrapItem_Click(object sender, RoutedEventArgs e)
        {
            if (InventoryGrid.SelectedItem is Item selected)
            {
                var win = new ScrapItemWindow(selected) { Owner = Window.GetWindow(this) };
                if (win.ShowDialog() == true) LoadData();
            }
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (InventoryGrid.SelectedItem is Item selected)
            {
                if (MessageBox.Show($"Удалить ПОЛНОСТЬЮ '{selected.Name}' из списка?", "Внимание",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    _inventoryManager.DeleteItemPermanently(selected.Id);
                    LoadData();
                }
            }
        }


        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var view = CollectionViewSource.GetDefaultView(InventoryGrid.ItemsSource);
            if (view == null) return;

            view.Filter = obj =>
            {
                if (obj is not Item item) return false;

                string query = SearchBox.Text.ToLower();
                bool matchesSearch = string.IsNullOrWhiteSpace(query) ||
                                     (item.Name?.ToLower().Contains(query) ?? false);

                bool matchesOnlyParts = OnlyPartsCheck.IsChecked == false ||
                                        (item.Category?.Trim().Equals("Запчасти", StringComparison.OrdinalIgnoreCase) ?? false);

                return matchesSearch && matchesOnlyParts;
            };
        }

        private void RefillItem_Click(object sender, RoutedEventArgs e)
        {
            if (InventoryGrid.SelectedItem is Item selectedItem)
            {
                RefillItemWindow refillWin = new RefillItemWindow(selectedItem);

                if (refillWin.ShowDialog() == true)
                {
                    LoadData();
                }
            }
            else
            {
                MessageBox.Show("Сначала выберите товар из списка!");
            }
        }

        private void InventoryGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (InventoryGrid.SelectedItem is Item selectedItem)
            {
                var editWin = new EditItemWindow(selectedItem);
                if (editWin.ShowDialog() == true)
                {
                    if (editWin.IsDeleted)
                        _inventoryManager.DeleteItem(selectedItem.Id); 
                    else
                        _inventoryManager.UpdateItem(selectedItem); 

                    InventoryGrid.ItemsSource = _inventoryManager.GetAllItems();
                }
            }
        }

        private void AddManualExpense_Click(object sender, RoutedEventArgs e)
        {
            var expenseWin = new AddManualExpenseWindow();

            expenseWin.Owner = Window.GetWindow(this);

            expenseWin.ShowDialog();
        }

    }
}
