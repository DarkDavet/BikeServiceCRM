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
            WHERE Status = 'Выдан' AND strftime('%Y-%m', DateClosed) = $ym";

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

        public List<(string day, double dailyRev, double dailyParts, int dailyCount)> GetDailyStats(DateTime date)
        {
            var stats = new List<(string day, double dailyRev, double dailyParts, int dailyCount)>();
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();

                command.CommandText = @"
            SELECT Day, SUM(Rev), SUM(Exp), SUM(Cnt) FROM (
                -- Часть 1: Доходы из ремонтов
                SELECT strftime('%d', DateClosed) as Day, 
                       SUM(TotalCost) as Rev, 
                       0 as Exp, 
                       COUNT(*) as Cnt
                FROM Repairs 
                WHERE Status = 'Выдан' AND strftime('%Y-%m', DateClosed) = $ym
                GROUP BY Day
                
                UNION ALL
                
                -- Часть 2: Расходы из таблицы Expenses (закупки)
                SELECT strftime('%d', DateOperation) as Day, 
                       0 as Rev, 
                       SUM(Amount) as Exp, 
                       0 as Cnt
                FROM Expenses 
                WHERE strftime('%Y-%m', DateOperation) = $ym
                GROUP BY Day
            ) 
            GROUP BY Day ORDER BY Day ASC";

                command.Parameters.AddWithValue("$ym", date.ToString("yyyy-MM"));

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        stats.Add((
                            reader.GetString(0),
                            reader.GetDouble(1),
                            reader.GetDouble(2),
                            reader.GetInt32(3)
                        ));
                    }
                }
            }
            return stats;
        }




        public List<(string month, double rev, double parts, double prof, int count)> GetYearlyStats(int year)
        {
            var stats = new List<(string month, double rev, double parts, double prof, int count)>();
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();

                // Используем UNION для объединения доходов от клиентов и расходов на склад
                command.CommandText = @"
            SELECT Month, SUM(Rev), SUM(Exp), SUM(Rev - Exp), SUM(Cnt) FROM (
                -- Доходы из выданных ремонтов
                SELECT strftime('%m', DateClosed) as Month, 
                       SUM(TotalCost) as Rev, 
                       0 as Exp, 
                       COUNT(*) as Cnt
                FROM Repairs 
                WHERE Status = 'Выдан' AND strftime('%Y', DateClosed) = $year
                GROUP BY Month
                
                UNION ALL
                
                -- Расходы из закупок на склад
                SELECT strftime('%m', DateOperation) as Month, 
                       0 as Rev, 
                       SUM(Amount) as Exp, 
                       0 as Cnt
                FROM Expenses 
                WHERE strftime('%Y', DateOperation) = $year
                GROUP BY Month
            ) 
            GROUP BY Month ORDER BY Month ASC";

                command.Parameters.AddWithValue("$year", year.ToString());

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        stats.Add((
                            reader.GetString(0), // Месяц (01, 02...)
                            reader.GetDouble(1), // Выручка
                            reader.GetDouble(2), // Реальные расходы на запчасти
                            reader.GetDouble(3), // Чистая прибыль (кассовая)
                            reader.GetInt32(4)   // Количество заказов
                        ));
                    }
                }
            }
            return stats;
        }

        public List<Expense> GetExpensesByCategory(DateTime date)
        {
            var stats = new List<Expense>();
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                string ym = date.ToString("yyyy-MM");

                command.CommandText = @"SELECT Category, SUM(Amount) FROM Expenses 
                                WHERE strftime('%Y-%m', DateOperation) = $ym
                                GROUP BY Category";
                command.Parameters.AddWithValue("$ym", ym);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        stats.Add(new Expense // Создаем объект класса
                        {
                            Category = reader.IsDBNull(0) ? "Прочее" : reader.GetString(0),
                            Amount = reader.GetDouble(1)
                        });
                    }
                }
            }
            return stats;
        }

        public List<Expense> GetExpensesHistory(DateTime date)
        {
            var list = new List<Expense>();
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                string ym = date.ToString("yyyy-MM");

                command.CommandText = @"SELECT Id, Description, Amount, Category, DateOperation 
                                FROM Expenses 
                                WHERE strftime('%Y-%m', DateOperation) = $ym
                                ORDER BY DateOperation DESC";
                command.Parameters.AddWithValue("$ym", ym);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Expense
                        {
                            Id = reader.GetInt32(0),
                            Description = reader.GetString(1),
                            Amount = reader.GetDouble(2),
                            Category = reader.IsDBNull(3) ? "Прочее" : reader.GetString(3),
                            Date = reader.GetDateTime(4)
                        });
                    }
                }
            }
            return list;
        }

        public List<Expense> GetExpensesByYear(int year)
        {
            var stats = new List<Expense>();
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
            SELECT Category, SUM(Amount) 
            FROM Expenses 
            WHERE strftime('%Y', DateOperation) = $year
            GROUP BY Category 
            ORDER BY SUM(Amount) DESC";

                command.Parameters.AddWithValue("$year", year.ToString());

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        stats.Add(new Expense
                        {
                            Category = reader.IsDBNull(0) ? "Прочее" : reader.GetString(0),
                            Amount = reader.GetDouble(1)
                        });
                    }
                }
            }
            return stats;
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

        public void SaveRepairItems(int repairId, IEnumerable<RepairItem> items)
        {
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var transaction = connection.BeginTransaction(); // Используем транзакцию для скорости
                try
                {
                    // 1. Удаляем старый состав заказа
                    var deleteCmd = connection.CreateCommand();
                    deleteCmd.CommandText = "DELETE FROM RepairItems WHERE RepairId = $rid";
                    deleteCmd.Parameters.AddWithValue("$rid", repairId);
                    deleteCmd.ExecuteNonQuery();

                    // 2. Записываем новый состав
                    foreach (var item in items)
                    {
                        var insertCmd = connection.CreateCommand();
                        insertCmd.CommandText = @"
                    INSERT INTO RepairItems (RepairId, ProductId, ItemName, Quantity, Price, PurchasePrice) 
                    VALUES ($rid, $pid, $name, $qty, $price, $pPrice)";

                        insertCmd.Parameters.AddWithValue("$rid", repairId);
                        insertCmd.Parameters.AddWithValue("$pid", (object)item.ProductId ?? DBNull.Value);
                        insertCmd.Parameters.AddWithValue("$name", item.Name);
                        insertCmd.Parameters.AddWithValue("$qty", item.Quantity);
                        insertCmd.Parameters.AddWithValue("$price", item.Price);
                        insertCmd.Parameters.AddWithValue("$pPrice", item.PurchasePrice);
                        insertCmd.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
                catch { transaction.Rollback(); throw; }
            }
        }

        public List<RepairItem> GetRepairItems(int repairId)
        {
            var list = new List<RepairItem>();
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT ProductId, ItemName, Quantity, Price, PurchasePrice FROM RepairItems WHERE RepairId = $rid";
                command.Parameters.AddWithValue("$rid", repairId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new RepairItem
                        {
                            ProductId = reader.IsDBNull(0) ? (int?)null : reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Quantity = reader.GetInt32(2),
                            Price = reader.GetDouble(3),
                            PurchasePrice = reader.GetDouble(4)
                        });
                    }
                }
            }
            return list;
        }




    }
}
