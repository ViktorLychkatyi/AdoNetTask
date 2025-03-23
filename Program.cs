using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.Xml;

namespace AdoNetTask
{
    class Program
    {
        static void Main()
        {
            IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

            string connectionString = config.GetConnectionString("DefaultConnection");
            ShowMenu(connectionString);

            //using (var connection = new SqlConnection(connectionString))
            //{
            //    try
            //    {
            //        connection.Open();
            //        var tablesQuery = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";
            //        var tables = new List<string>();

            //        using (var tableCommand = new SqlCommand(tablesQuery, connection))
            //        using (var reader = tableCommand.ExecuteReader())
            //        {
            //            Console.WriteLine("Таблицы в базе данных:");
            //            while (reader.Read())
            //            {
            //                string tableName = reader.GetString(0);
            //                tables.Add(tableName);
            //                Console.WriteLine($"- {tableName}");
            //            }
            //        }

            //        Console.WriteLine("\nДетали полей в таблицах:\n");
            //        foreach (var table in tables)
            //        {
            //            Console.WriteLine($"Таблица: {table}");

            //            string columnsQuery = $@"
            //            SELECT COLUMN_NAME, DATA_TYPE 
            //            FROM INFORMATION_SCHEMA.COLUMNS 
            //            WHERE TABLE_NAME = @TableName";

            //            using (var columnCommand = new SqlCommand(columnsQuery, connection))
            //            {
            //                columnCommand.Parameters.AddWithValue("@TableName", table);

            //                using (var columnReader = columnCommand.ExecuteReader())
            //                {
            //                    while (columnReader.Read())
            //                    {
            //                        string columnName = columnReader.GetString(0);
            //                        string dataType = columnReader.GetString(1);
            //                        Console.WriteLine($"  - {columnName} ({dataType})");
            //                    }
            //                }
            //            }
            //            Console.WriteLine();
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine("Ошибка: " + ex.Message);
            //    }
            //}

            static void ShowMenu(string connectionString)
            {
                while (true)
                {
                    Console.WriteLine("Меню");
                    Console.WriteLine("1. Показать структуру таблицы");
                    Console.WriteLine("2. Выбрать все данные");
                    Console.WriteLine("3. Вставить в строку");
                    Console.WriteLine("4. Обновить строку");
                    Console.WriteLine("5. Удалить строку");
                    Console.WriteLine("6. Вернуться к списку таблиц");
                    Console.WriteLine("7. Выход");
                    Console.Write("Выберите действие: ");
                    ActionMenu(connectionString);
                }
            }

            static void ActionMenu(string connectionString)
            {
                string choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        StructureTable(connectionString);
                        break;
                    case "2":
                        SelectAll(connectionString);
                        break;
                    case "3":
                        InsertRow(connectionString);
                        break;
                    case "4":
                        UpdateRow(connectionString);
                        break;
                    case "5":
                        DeleteRow(connectionString);
                        break;
                    case "6":
                        ShowTables(connectionString);
                        break;
                    case "7":
                        Console.WriteLine("Вы вышли из приложения");
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("Неверный ввод");
                        break;
                }
            }

            static void StructureTable(string connectionString)
            {
                Console.Write("Введите имя таблицы: ");
                string tableName = Console.ReadLine();
                string query = $@"
                            SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH 
                            FROM INFORMATION_SCHEMA.COLUMNS 
                            WHERE TABLE_NAME = @TableName";

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var command = new SqlCommand(query, connection);

                    command.Parameters.AddWithValue("@TableName", tableName);
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        Console.WriteLine($"{reader["COLUMN_NAME"]} - {reader["DATA_TYPE"]}({reader["CHARACTER_MAXIMUM_LENGTH"]})");
                    }
                }
            }

            static void SelectAll(string connectionString)
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";

                    var command = new SqlCommand(query, connection);
                    var reader = command.ExecuteReader();

                    List<string> tableNames = new List<string>();
                    while (reader.Read())
                    {
                        tableNames.Add(reader.GetString(0));
                    }
                    reader.Close();

                    foreach (var tableName in tableNames)
                    {
                        Console.WriteLine($"\nДанные таблицы: {tableName}");
                        var selectCommand = new SqlCommand($"SELECT * FROM {tableName}", connection);
                        var selectReader = selectCommand.ExecuteReader();

                        while (selectReader.Read())
                        {
                            for (int i = 0; i < selectReader.FieldCount; i++)
                            {
                                Console.Write(selectReader[i] + " ");
                            }
                            Console.WriteLine();
                        }
                        selectReader.Close();
                    }
                }
            }

            static void InsertRow(string connectionString)
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    Console.Write("Введите название: ");
                    string name = Console.ReadLine();

                    Console.Write("Введите цену: ");
                    decimal price = Convert.ToDecimal(Console.ReadLine());

                    string query = "INSERT INTO Product (Name, Price) VALUES (@Name, @Price); SELECT SCOPE_IDENTITY();";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Name", name);
                        command.Parameters.AddWithValue("@Price", price);

                        int newId = Convert.ToInt32(command.ExecuteScalar());
                        Console.WriteLine($"Данные добавлены! ID: {newId}, Имя продукта: {name}, Цена: {price}");
                    }
                }
            }

            static void UpdateRow(string connectionString)
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    Console.Write("Введите ID продукта: ");
                    int id = Convert.ToInt32(Console.ReadLine());

                    Console.Write("Введите новое название: ");
                    string name = Console.ReadLine();

                    Console.Write("Введите новую цену: ");
                    decimal price = Convert.ToDecimal(Console.ReadLine());

                    string query = "UPDATE Product SET Name = @Name, Price = @Price WHERE Id = @Id";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        command.Parameters.AddWithValue("@Name", name);
                        command.Parameters.AddWithValue("@Price", price);

                        int rowsAffected = command.ExecuteNonQuery();
                        Console.WriteLine(rowsAffected > 0 ? $"Продукт с ID {id} обновлен!" : "Продукт с таким ID не найден.");
                    }
                }
            }


            static void DeleteRow(string connectionString)
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    Console.Write("Введите ID продукта для удаления: ");
                    int id = Convert.ToInt32(Console.ReadLine());

                    string query = "DELETE FROM Product WHERE Id = @Id";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);

                        int rowsAffected = command.ExecuteNonQuery();
                        Console.WriteLine(rowsAffected > 0 ? $"Продукт с ID {id} удален!" : "Продукт с таким ID не найден.");
                    }
                }
            }

            static void ShowTables(string connectionString)
            {
                Console.Clear();
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";

                    using (var command = new SqlCommand(query, connection))
                    {
                        var reader = command.ExecuteReader();
                        Console.WriteLine("Список таблиц в базе данных:\n");

                        while (reader.Read())
                        {
                            Console.WriteLine(reader["TABLE_NAME"]);
                        }
                    }
                }
            }
        }
    }
}