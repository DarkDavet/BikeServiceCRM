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
    /// Interaction logic for EditClientWindow.xaml
    /// </summary>
    public partial class EditClientWindow : Window
    {
        public  Client Client { get; private set; }
        public bool IsDeleted { get; private set; } = false;

        public EditClientWindow(Client client)
        {
            InitializeComponent();
            Client = client;

            FullNameBox.Text = client.Name;
            PhoneBox.Text = client.Phone;
            AddressBox.Text = client.Address;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Client.Name = FullNameBox.Text;
            Client.Phone = PhoneBox.Text;
            Client.Address = AddressBox.Text;

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите удалить этого клиента?",
                                         "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                IsDeleted = true;
                DialogResult = true; 
            }
        }


    }
}
