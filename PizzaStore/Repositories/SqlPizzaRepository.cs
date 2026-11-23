using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using PizzaStore.Models;

namespace PizzaStore.Repositories
{
    // Этот класс реализует интерфейс и отвечает ТОЛЬКО за общение с базой данных (SRP).
    public class SqlPizzaRepository : IPizzaRepository
    {
        private readonly string _connectionString;

        // Строка подключения передается через конструктор
        public SqlPizzaRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IEnumerable<Pizza> GetAllPizzas()
        {
            var pizzas = new List<Pizza>();

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = "SELECT Id, Name, Description, Price, ImageUrl FROM Pizzas;";
            using var command = new SqlCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                pizzas.Add(new Pizza
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.GetString(2),
                    Price = reader.GetDecimal(3),
                    // Обрабатываем возможные значения NULL для картинок
                    ImageUrl = reader.IsDBNull(4) ? string.Empty : reader.GetString(4)
                });
            }

            return pizzas;
        }
    }
}