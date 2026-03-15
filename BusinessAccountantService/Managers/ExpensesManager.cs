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
    public class ExpensesManager
    {
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
    }
}
