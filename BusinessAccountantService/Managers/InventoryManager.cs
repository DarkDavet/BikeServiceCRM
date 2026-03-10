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
    public class InventoryManager
    {
        public List<Item> GetAllItems()
        {
            var items = new List<Item>();
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, Name, Quantity, PurchasePrice, RetailPrice, Category FROM Inventory";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(new Item
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Quantity = reader.GetInt32(2),
                            PurchasePrice = reader.GetDouble(3),
                            RetailPrice = reader.GetDouble(4),
                            Category = reader.IsDBNull(5) ? "Разное" : reader.GetString(5)
                        });
                    }
                }
            }
            return items;
        }

    }
}
