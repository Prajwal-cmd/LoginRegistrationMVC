using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using LoginRegistrationMVC.Models;

namespace LoginRegistrationMVC.Data
{
    public class UserRepository
    {
        private readonly string _connectionString;

        public UserRepository()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        public void AddUser(User user)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("INSERT INTO Users (Email, HashedPassword) VALUES (@Email, @HashedPassword)", connection);
                command.Parameters.AddWithValue("@Email", user.Email);
                command.Parameters.AddWithValue("@HashedPassword", user.HashedPassword);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public User GetUserByEmail(string email)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("SELECT Id, Email, HashedPassword FROM Users WHERE Email = @Email", connection);
                command.Parameters.AddWithValue("@Email", email);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new User
                        {
                            Id = reader.GetInt32(0),
                            Email = reader.GetString(1),
                            HashedPassword = reader.GetString(2)
                        };
                    }
                    return null;
                }
            }
        }

        public List<User> GetAllUsers(string searchTerm = null, string filter = null)
        {
            var users = new List<User>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT Id, Email, HashedPassword FROM Users";
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    searchTerm = searchTerm?.ToLower(); 
                    if (filter == "id")
                    {
                        query += " WHERE Id = @SearchTerm";
                    }
                    else if (filter == "email" || string.IsNullOrEmpty(filter))
                    {
                        query += " WHERE LOWER(Email) LIKE @SearchTerm";
                    }
                }
                using (var command = new SqlCommand(query, connection))
                {
                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
                    }
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new User
                            {
                                Id = reader.GetInt32(0),
                                Email = reader.GetString(1),
                                HashedPassword = reader.GetString(2)
                            });
                        }
                    }
                }
            }
            return users;
        }
    }
}