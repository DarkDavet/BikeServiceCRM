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
                command.CommandText = @"UPDATE Repairs SET 
                                BikeInfo = $bike, 
                                ProblemDescription = $prob, 
                                WorksPerformed = $works, 
                                PartsCost = $parts,
                                TotalCost = $cost,
                                Status = $status
                                WHERE Id = $id";

                command.Parameters.AddWithValue("$bike", r.BikeInfo);
                command.Parameters.AddWithValue("$prob", r.ProblemDescription);
                command.Parameters.AddWithValue("$works", r.WorksPerformed ?? "");
                command.Parameters.AddWithValue("$parts", r.PartsCost); // Сохраняем расходы
                command.Parameters.AddWithValue("$cost", r.TotalCost);
                command.Parameters.AddWithValue("$status", r.Status);
                command.Parameters.AddWithValue("$id", r.Id);

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

        public (double rev, double parts, double prof, int count) GetStatsByMonth(DateTime date)
        {
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                string yearMonth = date.ToString("yyyy-MM");

                // Индексы: 0-TotalCost, 1-PartsCost, 2-Profit, 3-Count
                command.CommandText = @"
            SELECT SUM(TotalCost), 
                   SUM(PartsCost), 
                   SUM(TotalCost - PartsCost), 
                   COUNT(*) 
            FROM Repairs 
            WHERE Status = 'Выдан' AND strftime('%Y-%m', DateCreated) = $ym";

                command.Parameters.AddWithValue("$ym", yearMonth);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read() && !reader.IsDBNull(0))
                    {
                        return (reader.GetDouble(0), reader.GetDouble(1), reader.GetDouble(2), reader.GetInt32(3));
                    }
                }
            }
            return (0, 0, 0, 0);
        }



        public (double totalRev, double totalParts, double totalProf, int totalCount) GetGlobalStats()
        {
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();

                command.CommandText = @"
            SELECT SUM(TotalCost), 
                   SUM(PartsCost), 
                   SUM(TotalCost - PartsCost), 
                   COUNT(*) 
            FROM Repairs 
            WHERE Status = 'Выдан'";

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read() && !reader.IsDBNull(0))
                    {
                        return (reader.GetDouble(0), reader.GetDouble(1), reader.GetDouble(2), reader.GetInt32(3));
                    }
                }
            }
            return (0, 0, 0, 0);
        }


        public void UpdateStatus(int repairId, string newStatus)
        {
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "UPDATE Repairs SET Status = $status WHERE Id = $id";
                command.Parameters.AddWithValue("$status", newStatus);
                command.Parameters.AddWithValue("$id", repairId);
                command.ExecuteNonQuery();
            }
        }

        public List<(string day, double dailyRev, double dailyParts)> GetDailyStats(DateTime date)
        {
            var stats = new List<(string day, double dailyRev, double dailyParts)>();
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT strftime('%d', DateCreated) as Day, SUM(TotalCost), SUM(PartsCost)
                    FROM Repairs 
                    WHERE Status = 'Выдан' AND strftime('%Y-%m', DateCreated) = $ym
                    GROUP BY Day ORDER BY Day ASC";
                command.Parameters.AddWithValue("$ym", date.ToString("yyyy-MM"));

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                        stats.Add((reader.GetString(0), reader.GetDouble(1), reader.GetDouble(2)));
                }
            }
            return stats;
        }

        public List<(string month, double profit)> GetYearlyStats()
        {
            var stats = new List<(string month, double profit)>();
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT strftime('%m', DateCreated) as Month, SUM(TotalCost - PartsCost) 
                    FROM Repairs 
                    WHERE Status = 'Выдан' AND strftime('%Y', DateCreated) = strftime('%Y', 'now')
                    GROUP BY Month ORDER BY Month ASC";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                        stats.Add((reader.GetString(0), reader.GetDouble(1)));
                }
            }
            return stats;
        }


    }
}
