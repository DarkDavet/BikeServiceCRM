using BusinessAccountantService.Data;
using Microsoft.Data.Sqlite;
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
    /// Interaction logic for AddClientWindow.xaml
    /// </summary>
    public partial class AddClientWindow : Window
    {
        public AddClientWindow()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string name = FullNameBox.Text;
            string phone = PhoneBox.Text;
            string address = AddressBox.Text;

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Имя обязательно!");
                return;
            }

            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO Clients (FullName, Phone, Address) VALUES ($name, $phone, $address)";
                command.Parameters.AddWithValue("$name", name);
                command.Parameters.AddWithValue("$phone", phone);
                command.Parameters.AddWithValue("$address", address);

                command.ExecuteNonQuery();
            }

            this.DialogResult = true; 
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
