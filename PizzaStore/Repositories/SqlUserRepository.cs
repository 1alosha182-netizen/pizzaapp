using System;
using Microsoft.Data.SqlClient;
using PizzaStore.Models;

namespace PizzaStore.Repositories
{
    // Класс отвечает ТОЛЬКО за взаимодействие с таблицей Users в базе данных (SRP)
    public class SqlUserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public SqlUserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Register(User user)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = @"
                INSERT INTO Users (FullName, Email, PasswordHash, Role) 
                VALUES (@FullName, @Email, @PasswordHash, @Role);
                SELECT SCOPE_IDENTITY();";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@FullName", user.FullName);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            command.Parameters.AddWithValue("@Role", user.Role);

            // Получаем ID только что созданного пользователя
            user.Id = Convert.ToInt32(command.ExecuteScalar());
        }

        public User? GetByEmail(string email)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = "SELECT Id, FullName, Email, PasswordHash, Role FROM Users WHERE Email = @Email;";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Email", email);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new User
                {
                    Id = reader.GetInt32(0),
                    FullName = reader.GetString(1),
                    Email = reader.GetString(2),
                    PasswordHash = reader.GetString(3),
                    Role = reader.GetString(4)
                };
            }

            return null; // Если пользователь с таким email не найден
        }
    }
}