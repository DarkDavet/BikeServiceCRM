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
        public List<Client> GetClientsByMode(ViewMode mode)
        {
            string statusCondition = mode switch
            {
                ViewMode.Active => "WHERE r.Status != 'Выдан'",
                ViewMode.Archive => "WHERE r.Status = 'Выдан'",
                _ => "" // Для ViewMode.All условие не нужно
            };

            string query = mode == ViewMode.All
                ? "SELECT Id, FullName, Phone, Address FROM Clients"
                : $@"SELECT DISTINCT c.Id, c.FullName, c.Phone, c.Address 
             FROM Clients c 
             JOIN Repairs r ON c.Id = r.ClientId 
             {statusCondition}";

            var clients = new List<Client>();
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = query;

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        clients.Add(new Client
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.IsDBNull(1) ? "" : reader.GetString(1),
                            Phone = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            Address = reader.IsDBNull(3) ? "" : reader.GetString(3)
                        });
                    }
                }
            }
            return clients;
        }

        public void UpdateClient(Client c)
        {
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"UPDATE Clients SET 
                                FullName = $name, 
                                Phone = $phone, 
                                Address = $address 
                                WHERE Id = $id";

                command.Parameters.AddWithValue("$name", c.Name);
                command.Parameters.AddWithValue("$phone", c.Phone);
                command.Parameters.AddWithValue("$address", c.Address);
                command.Parameters.AddWithValue("$id", c.Id);

                command.ExecuteNonQuery();
            }
        }

        public void DeleteClient(Client c)
        {
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();

                // 1. ВКЛЮЧАЕМ КАСКАД (в этом конкретном соединении)
                using (var pragmaCmd = new SqliteCommand("PRAGMA foreign_keys = ON;", connection))
                {
                    pragmaCmd.ExecuteNonQuery();
                }

                // 2. УДАЛЯЕМ ТОЛЬКО КЛИЕНТА (ремонты удалятся сами)
                using (var deleteCmd = new SqliteCommand("DELETE FROM Clients WHERE Id = $id", connection))
                {
                    deleteCmd.Parameters.AddWithValue("$id", c.Id);
                    deleteCmd.ExecuteNonQuery();
                }
            }
        }
    }
}
