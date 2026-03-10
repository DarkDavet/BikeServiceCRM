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
                // Используем те же названия колонок, что и в таблице
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

        // Метод для удаления заказа из базы
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

        public (double rev, double prof, int count) GetStatsByMonth(DateTime date)
        {
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();

                // Формируем строку года и месяца из выбранной даты (например "2023-10")
                string yearMonth = date.ToString("yyyy-MM");

                command.CommandText = @"
            SELECT SUM(TotalCost), SUM(TotalCost - PartsCost), COUNT(*) 
            FROM Repairs 
            WHERE Status = 'Выдан' 
            AND strftime('%Y-%m', DateCreated) = $ym";

                command.Parameters.AddWithValue("$ym", yearMonth);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read() && !reader.IsDBNull(0))
                    {
                        return (reader.GetDouble(0), reader.GetDouble(1), reader.GetInt32(2));
                    }
                }
            }
            return (0, 0, 0);
        }


    }
}
