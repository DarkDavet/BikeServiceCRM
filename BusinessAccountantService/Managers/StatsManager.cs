using BusinessAccountantService.Data;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessAccountantService.Managers
{
    public class StatsManager
    {
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

        public List<(string day, decimal dailyRev, decimal dailyParts, int dailyCount)> GetDailyStats(DateTime date)
        {
            var stats = new List<(string day, decimal dailyRev, decimal dailyParts, int dailyCount)>();
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
                            reader.GetDecimal(1),
                            reader.GetDecimal(2),
                            reader.GetInt32(3)
                        ));
                    }
                }
            }
            return stats;
        }

        public List<(string month, decimal rev, decimal parts, decimal prof, int count)> GetYearlyStats(int year)
        {
            var stats = new List<(string month, decimal rev, decimal parts, decimal prof, int count)>();
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
                            reader.GetDecimal(1), // Выручка
                            reader.GetDecimal(2), // Реальные расходы на запчасти
                            reader.GetDecimal(3), // Чистая прибыль (кассовая)
                            reader.GetInt32(4)   // Количество заказов
                        ));
                    }
                }
            }
            return stats;
        }
    }
}
