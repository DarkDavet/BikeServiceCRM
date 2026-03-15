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
                            PurchasePrice = reader.GetDecimal(3),
                            RetailPrice = reader.GetDecimal(4),
                            Category = reader.IsDBNull(5) ? "Разное" : reader.GetString(5)
                        });
                    }
                }
            }
            return items;
        }

        public void DecreaseQuantity(int id, int qty)
        {
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "UPDATE Inventory SET Quantity = Quantity - $qty WHERE Id = $id";
                command.Parameters.AddWithValue("$qty", qty);
                command.Parameters.AddWithValue("$id", id);
                command.ExecuteNonQuery();
            }
        }

        public void IncreaseQuantity(int id, int qty)
        {
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "UPDATE Inventory SET Quantity = Quantity + $qty WHERE Id = $id";
                command.Parameters.AddWithValue("$qty", qty);
                command.Parameters.AddWithValue("$id", id);
                command.ExecuteNonQuery();
            }
        }

        public void DeleteItemPermanently(int id)
        {
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                // Удаляем саму позицию из склада
                command.CommandText = "DELETE FROM Inventory WHERE Id = $id";
                command.Parameters.AddWithValue("$id", id);
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

        public void RefillItem(int itemId, int addQty, decimal newPrice)
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

        public double GetTotalInventoryValue()
        {
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT SUM(Quantity * PurchasePrice) FROM Inventory";

                var result = command.ExecuteScalar();
                return result != DBNull.Value ? Convert.ToDouble(result) : 0;
            }
        }

        public void AddExpense(string desc, decimal totalAmount, string category)
        {
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"INSERT INTO Expenses (Description, Amount, Category, DateOperation) 
                               VALUES ($desc, $amount, $cat, $date)";

                command.Parameters.AddWithValue("$desc", desc);
                command.Parameters.AddWithValue("$amount", totalAmount);
                command.Parameters.AddWithValue("$cat", category);
                command.Parameters.AddWithValue("$date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                command.ExecuteNonQuery();
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
                            Price = reader.GetDecimal(3),
                            PurchasePrice = reader.GetDecimal(4)
                        });
                    }
                }
            }
            return list;
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
    }
}
