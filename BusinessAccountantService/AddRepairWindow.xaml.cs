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
    /// Interaction logic for AddRepairWindow.xaml
    /// </summary>
    public partial class AddRepairWindow : Window
    {
        private int _clientId;

        public AddRepairWindow(int clientId)
        {
            InitializeComponent();
            _clientId = clientId; // Запоминаем, для кого ремонт
        }

        private void SaveRepair_Click(object sender, RoutedEventArgs e)
        {
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Repairs (ClientId, BikeInfo, ProblemDescription, TotalCost, DateCreated, IsCompleted) 
                    VALUES ($cid, $bike, $prob, $cost, $date, 0)";

                command.Parameters.AddWithValue("$cid", _clientId);
                command.Parameters.AddWithValue("$bike", BikeInfoBox.Text);
                command.Parameters.AddWithValue("$prob", ProblemBox.Text);
                command.Parameters.AddWithValue("$cost", double.TryParse(CostBox.Text, out var c) ? c : 0);
                command.Parameters.AddWithValue("$date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                command.ExecuteNonQuery();
            }
            this.DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => this.DialogResult = false;
    }
}
