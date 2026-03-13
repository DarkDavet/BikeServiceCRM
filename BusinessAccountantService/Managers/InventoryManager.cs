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

        public void DecreaseQuantity(int itemId, int count)
        {
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();

                // UPDATE уменьшает количество на указанное число
                // Условие AND Quantity >= $count страхует от ухода в минус
                command.CommandText = "UPDATE Inventory SET Quantity = Quantity - $count WHERE Id = $id AND Quantity >= $count";

                command.Parameters.AddWithValue("$count", count);
                command.Parameters.AddWithValue("$id", itemId);

                command.ExecuteNonQuery();
            }
        }

        public void ScrapItem(int itemId, int count)
        {
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                // Уменьшаем остаток
                command.CommandText = "UPDATE Inventory SET Quantity = Quantity - $count WHERE Id = $id AND Quantity >= $count";
                command.Parameters.AddWithValue("$count", count);
                command.Parameters.AddWithValue("$id", itemId);
                command.ExecuteNonQuery();
            }
        }

        public void UpdateRepairStatus(int repairId, string newStatus)
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

        public void RefillItem(int itemId, int addQty, double newPrice)
        {
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                // Увеличиваем количество и обновляем цену закупки на актуальную
                command.CommandText = @"UPDATE Inventory SET 
                                Quantity = Quantity + $qty, 
                                PurchasePrice = $price 
                                WHERE Id = $id";

                command.Parameters.AddWithValue("$qty", addQty);
                command.Parameters.AddWithValue("$price", newPrice);
                command.Parameters.AddWithValue("$id", itemId);
                command.ExecuteNonQuery();
            }
        }

        // Обновление всей карточки товара
        public void UpdateItem(Item item)
        {
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"UPDATE Inventory SET 
                                Name = $name, 
                                Quantity = $qty, 
                                PurchasePrice = $pPrice, 
                                RetailPrice = $rPrice, 
                                Category = $cat 
                                WHERE Id = $id";

                command.Parameters.AddWithValue("$name", item.Name);
                command.Parameters.AddWithValue("$qty", item.Quantity);
                command.Parameters.AddWithValue("$pPrice", item.PurchasePrice);
                command.Parameters.AddWithValue("$rPrice", item.RetailPrice);
                command.Parameters.AddWithValue("$cat", item.Category ?? "Разное");
                command.Parameters.AddWithValue("$id", item.Id);

                command.ExecuteNonQuery();
            }
        }

        public void DeleteItem(int id)
        {
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Inventory WHERE Id = $id";
                command.Parameters.AddWithValue("$id", id);
                command.ExecuteNonQuery();
            }
        }

    }
}
