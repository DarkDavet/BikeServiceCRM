using BusinessAccountantService.Data;
using BusinessAccountantService.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessAccountantService.Managers
{
    internal class RepairManager
    {
        public List<RepairRecord> GetRepairsByClient(int clientId, bool onlyActive)
        {
            var list = new List<RepairRecord>();
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();

                // Базовый запрос
                string query = "SELECT Id, BikeInfo, ProblemDescription, WorksPerformed, TotalCost, Status, DateCreated FROM Repairs WHERE ClientId = $id";

                // Если флаг активен, добавляем условие
                if (onlyActive)
                {
                    query += " AND Status != 'Выдан'";
                }

                query += " ORDER BY DateCreated DESC";

                command.CommandText = query;
                command.Parameters.AddWithValue("$id", clientId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new RepairRecord
                        {
                            Id = reader.GetInt32(0),
                            BikeInfo = reader.GetString(1),
                            ProblemDescription = reader.GetString(2),
                            WorksPerformed = reader.IsDBNull(3) ? "" : reader.GetString(3),
                            TotalCost = reader.GetDouble(4),
                            Status = reader.IsDBNull(5) ? "Принят" : reader.GetString(5),
                            DateCreated = reader.IsDBNull(6) ? DateTime.Now : reader.GetDateTime(6)
                        });
                    }
                }
            }
            return list;
        }

        public int GetActiveRepairsCount()
        {
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                // Считаем все заказы, которые не выданы
                command.CommandText = "SELECT COUNT(*) FROM Repairs WHERE Status != 'Выдан'";
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public int GetAllRepairsCount()
        {
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM Repairs";
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

    }
}
