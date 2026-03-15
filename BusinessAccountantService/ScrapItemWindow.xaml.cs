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
    /// Interaction logic for ScrapItemWindow.xaml
    /// </summary>
    public partial class ScrapItemWindow : Window
    {
        private Item _item;
        private readonly InventoryManager _invManager = new();

        public ScrapItemWindow(Item item)
        {
            InitializeComponent();
            _item = item;
            ItemNameText.Text = $"Товар: {item.Name}";
            CurrentQtyText.Text = $"В наличии: {item.Quantity} шт.";
            ScrapQtyBox.Focus();
        }

        private void ConfirmScrap_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(ScrapQtyBox.Text, out int qty) && qty > 0)
            {
                if (qty > _item.Quantity)
                {
                    MessageBox.Show("Нельзя списать больше, чем есть на складе!");
                    return;
                }

                _invManager.DecreaseQuantity(_item.Id, qty);
                this.DialogResult = true;
            }
            else { MessageBox.Show("Введите корректное число."); }
        }



        private void Cancel_Click(object sender, RoutedEventArgs e) => this.DialogResult = false;
    }
}
