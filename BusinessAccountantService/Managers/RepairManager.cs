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
    public class RepairManager
    {
        public List<RepairRecord> GetRepairsByClient(int clientId, ViewMode mode)
        {
            var list = new List<RepairRecord>();
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();

                string sql = "SELECT Id, BikeInfo, ProblemDescription, WorksPerformed, PartsCost, TotalCost, Status, DateCreated " +
                             "FROM Repairs WHERE ClientId = $id";

                if (mode == ViewMode.Active) sql += " AND Status != 'Выдан'";
                else if (mode == ViewMode.Archive) sql += " AND Status = 'Выдан'";

                sql += " ORDER BY DateCreated DESC";
                command.CommandText = sql;
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
                            PartsCost = reader.IsDBNull(4) ? 0 : reader.GetDouble(4),
                            TotalCost = reader.GetDouble(5),
                            Status = reader.IsDBNull(6) ? "Принят" : reader.GetString(6),
                            DateCreated = reader.IsDBNull(7) ? DateTime.Now : reader.GetDateTime(7)
                        });
                    }
                }
            }
            return list;
        }

        public void UpdateRepair(RepairRecord r)
        {
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();

                // Добавляем логику: если статус "Выдан", ставим текущую дату (если её еще нет)
                // Если статус не "Выдан", зануляем дату закрытия
                command.CommandText = @"UPDATE Repairs SET 
                        BikeInfo = $bike, 
                        ProblemDescription = $prob, 
                        WorksPerformed = $works, 
                        PartsCost = $parts,
                        TotalCost = $cost,
                        Status = $status,
                        DateClosed = CASE 
                            WHEN $status = 'Выдан' AND DateClosed IS NULL THEN $date 
                            WHEN $status != 'Выдан' THEN NULL 
                            ELSE DateClosed END
                        WHERE Id = $id";

                command.Parameters.AddWithValue("$bike", r.BikeInfo);
                command.Parameters.AddWithValue("$prob", r.ProblemDescription);
                command.Parameters.AddWithValue("$works", r.WorksPerformed ?? "");
                command.Parameters.AddWithValue("$parts", r.PartsCost);
                command.Parameters.AddWithValue("$cost", r.TotalCost);
                command.Parameters.AddWithValue("$status", r.Status);
                command.Parameters.AddWithValue("$id", r.Id);
                command.Parameters.AddWithValue("$date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                command.ExecuteNonQuery();
            }
        }

        public void DeleteRepair(int id)
        {
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Repairs WHERE Id = $id";
                command.Parameters.AddWithValue("$id", id);
                command.ExecuteNonQuery();
            }
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

        public void UpdateStatus(int repairId, string newStatus)
        {
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                if (newStatus == "Выдан")
                {
                    command.CommandText = "UPDATE Repairs SET Status = $status, DateClosed = $date WHERE Id = $id";
                    command.Parameters.AddWithValue("$date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                }
                else
                {
                    command.CommandText = "UPDATE Repairs SET Status = $status, DateClosed = NULL WHERE Id = $id";
                }
                command.Parameters.AddWithValue("$status", newStatus);
                command.Parameters.AddWithValue("$id", repairId);
                command.ExecuteNonQuery();
            }
        }


        // Поиск по прайсу работ
        public List<ServiceItem> GetServiceSuggestions(string query)
        {
            var list = new List<ServiceItem>();
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT ServiceName, DefaultPrice FROM ServicePriceList WHERE ServiceName LIKE $q LIMIT 10";
                command.Parameters.AddWithValue("$q", "%" + query + "%");

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                        list.Add(new ServiceItem { Name = reader.GetString(0), Price = reader.GetDouble(1) });
                }
            }
            return list;
        }

        // Сохранение/Обновление цены в прайсе
        public void UpdateServicePrice(string name, double price)
        {
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                // INSERT OR REPLACE обновит цену, если работа с таким именем уже есть
                command.CommandText = "INSERT OR REPLACE INTO ServicePriceList (ServiceName, DefaultPrice) VALUES ($name, $price)";
                command.Parameters.AddWithValue("$name", name);
                command.Parameters.AddWithValue("$price", price);
                command.ExecuteNonQuery();
            }
        }
    }
}
