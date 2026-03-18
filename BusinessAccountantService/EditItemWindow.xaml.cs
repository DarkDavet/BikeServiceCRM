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
    /// Interaction logic for EditItemWindow.xaml
    /// </summary>
    public partial class EditItemWindow : Window
    {
        public Item CurrentItem { get; private set; }
        public bool IsDeleted { get; private set; } = false;

        public EditItemWindow(Item item)
        {
            InitializeComponent();
            CurrentItem = item;

            ItemNameBox.Text = item.Name;
            CategoryBox.Text = item.Category;
            RetailBox.Text = item.RetailPrice.ToString();

            UpdateVisibility(item.Category);
        }

        private void UpdateVisibility(string category)
        {
            if (RetailPriceBlock == null) return;

            if (!string.IsNullOrEmpty(category) && category.Trim().Equals("Запчасти", StringComparison.OrdinalIgnoreCase))
            {
                RetailPriceBlock.Visibility = Visibility.Visible;
            }
            else
            {
                RetailPriceBlock.Visibility = Visibility.Collapsed;
                RetailBox.Text = "0";
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            CurrentItem.Name = ItemNameBox.Text;

            decimal.TryParse(RetailBox.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal r);

            CurrentItem.RetailPrice = r;

            DialogResult = true;
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Удалить карточку товара навсегда?", "Удаление", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                IsDeleted = true;
                DialogResult = true;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
