using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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

                command.CommandText = "PRAGMA foreign_keys = ON;";
                command.ExecuteNonQuery();

                command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Clients (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                FullName TEXT NOT NULL,
                Phone TEXT,
                Address TEXT
            );

            CREATE TABLE IF NOT EXISTS Repairs (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ClientId INTEGER,
                BikeInfo TEXT,
                ProblemDescription TEXT,
                WorksPerformed TEXT,
                PartsCost REAL DEFAULT 0,
                TotalCost REAL,
                Status TEXT DEFAULT 'Принят',
                DateCreated DATETIME,
                FOREIGN KEY (ClientId) REFERENCES Clients(Id) ON DELETE CASCADE
            );";
                command.ExecuteNonQuery();
            }
        }
        public static void ResetDatabase()
        {
            if (File.Exists(DbName))
            {
                SqliteConnection.ClearAllPools();
                File.Delete(DbName);
                MessageBox.Show("База данных удалена. Перезапустите программу для создания новой структуры.");
                Application.Current.Shutdown();
            }
        }
    }
}
