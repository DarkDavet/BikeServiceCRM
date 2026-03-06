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
        public List<RepairRecord> GetRepairsByClient(int clientId)
        {
            var list = new List<RepairRecord>();
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, BikeInfo, ProblemDescription, WorksPerformed, TotalCost, Status, DateCreated FROM Repairs WHERE ClientId = $id ORDER BY DateCreated DESC";
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
    }
}
