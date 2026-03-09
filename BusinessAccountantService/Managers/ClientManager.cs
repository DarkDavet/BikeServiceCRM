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
        public List<Client> GetClientsWithActiveRepairs()
        {
            var activeClients = new List<Client>();
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                // Выбираем уникальных клиентов, у которых есть ремонт НЕ в статусе 'Выдан'
                command.CommandText = @"
            SELECT DISTINCT c.Id, c.FullName, c.Phone 
            FROM Clients c
            JOIN Repairs r ON c.Id = r.ClientId
            WHERE r.Status != 'Выдан'";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        activeClients.Add(new Client
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Phone = reader.IsDBNull(2) ? "" : reader.GetString(2)
                        });
                    }
                }
            }
            return activeClients;
        }

    }
}
