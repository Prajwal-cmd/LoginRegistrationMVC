using System.Data.SqlClient;
using LoginRegistrationMVC.Models;
using System.Configuration;

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
    }
}