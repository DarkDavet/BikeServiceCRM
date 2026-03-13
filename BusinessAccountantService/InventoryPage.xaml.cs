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
        // Быстрый доступ к менеджеру из MainWindow
        private InventoryManager _inventoryManager => ((MainWindow)Application.Current.MainWindow)._inventoryManager;

        public InventoryPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            var items = _inventoryManager.GetAllItems();
            InventoryGrid.ItemsSource = items;
            // Обновляем текст статуса (он теперь внутри Page)
            StatusInfoText.Text = $"Товаров в базе: {items.Count}";
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
            if (InventoryGrid.SelectedItem is Item selectedItem)
            {
                var result = MessageBox.Show($"Списать 1 ед. товара '{selectedItem.Name}' как брак/утерю?",
                                             "Списание", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    _inventoryManager.ScrapItem(selectedItem.Id, 1);
                    LoadData();
                }
            }
            else
            {
                MessageBox.Show("Выберите товар в таблице для списания!");
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var view = CollectionViewSource.GetDefaultView(InventoryGrid.ItemsSource);
            if (view != null)
            {
                view.Filter = obj =>
                {
                    if (obj is not Item item || string.IsNullOrWhiteSpace(SearchBox.Text))
                        return true;

                    string query = SearchBox.Text.ToLower();
                    return (item.Name?.ToLower().Contains(query) ?? false) ||
                           (item.Category?.ToLower().Contains(query) ?? false);
                };
            }
        }

        private void RefillItem_Click(object sender, RoutedEventArgs e)
        {
            if (InventoryGrid.SelectedItem is Item selectedItem)
            {
                RefillItemWindow refillWin = new RefillItemWindow(selectedItem);

                if (refillWin.ShowDialog() == true)
                {
                    // Обновляем таблицу склада
                    InventoryGrid.ItemsSource = _inventoryManager.GetAllItems();
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

    }
}
