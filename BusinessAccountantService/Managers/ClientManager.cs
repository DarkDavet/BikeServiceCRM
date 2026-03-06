using BusinessAccountantService.Data;
using BusinessAccountantService.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BusinessAccountantService.Managers
{
    internal class ClientManager
    {
        public List<Client> GetAllClients()
        {
            var clients = new List<Client>();
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, FullName, Phone FROM Clients";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        clients.Add(new Client
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.IsDBNull(1) ? "" : reader.GetString(1),
                            Phone = reader.IsDBNull(2) ? "" : reader.GetString(2)
                        });
                    }
                }
            }
            return clients;
        }
    }
}
