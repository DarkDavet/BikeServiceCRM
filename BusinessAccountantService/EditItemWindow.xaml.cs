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
            PurchaseBox.Text = item.PurchasePrice.ToString();
            RetailBox.Text = item.RetailPrice.ToString();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            CurrentItem.Name = ItemNameBox.Text;
            CurrentItem.Category = CategoryBox.Text;

            decimal.TryParse(PurchaseBox.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal p);
            decimal.TryParse(RetailBox.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal r);

            CurrentItem.PurchasePrice = p;
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
