using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessAccountantService.Data
{
    internal class DatabaseService
    {
        private const string DbName = "bike_service.db";
        public static string ConnectionString => $"Data Source={DbName}";

        public static void Initialize()
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();

                command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Repairs (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ClientId INTEGER,
                BikeInfo TEXT,
                ProblemDescription TEXT,
                WorksPerformed TEXT,
                TotalCost REAL,
                IsCompleted INTEGER DEFAULT 0,
                Status TEXT DEFAULT 'Принят', -- Добавили сюда
                DateCreated DATETIME,
                FOREIGN KEY (ClientId) REFERENCES Clients(Id)
            );";
                command.ExecuteNonQuery();

                try
                {
                    command.CommandText = "ALTER TABLE Repairs ADD COLUMN Status TEXT DEFAULT 'Принят';";
                    command.ExecuteNonQuery();
                }
                catch
                {
                    
                }
            }
        }
    }
}
