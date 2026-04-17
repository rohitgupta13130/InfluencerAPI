using InfluencerBackendAPI.Data;
using InfluencerBackendAPI.Dtos;
using InfluencerBackendAPI.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace InfluencerBackendAPI.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(ApplicationDbContext context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ================= LOGIN =================
        public async Task<User> AuthenticateUser(string username, string password)
        {
            _logger.LogInformation("AuthenticateUser method called for Username: {UserName}", username);

            User user = null;
            var connection = _context.Database.GetDbConnection();

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    _logger.LogInformation("Opening database connection");
                    await connection.OpenAsync();
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "influencer.AuthenticateUser";
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@UserName", username));
                    command.Parameters.Add(new SqlParameter("@Password", password));

                    _logger.LogInformation("Executing stored procedure influencer.AuthenticateUser");

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                        {
                            _logger.LogInformation("User found for Username: {UserName}", username);

                            user = new User
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                UserName = reader["UserName"]?.ToString(),
                                Email = reader["Email"]?.ToString(),
                                UserTypeName = reader["UserTypeName"]?.ToString()
                            };
                        }
                        else
                        {
                            _logger.LogWarning("No user found for Username: {UserName}", username);
                        }
                    }
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AuthenticateUser for Username: {UserName}", username);
                return null;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    await connection.CloseAsync();
                    _logger.LogInformation("Database connection closed (AuthenticateUser)");
                }
            }
        }

        // ================= REGISTER =================
        public async Task<int> RegisterUser(RegisterRequest request)
        {
            _logger.LogInformation(
                "RegisterUser method called -> Email: {Email}, Phone: {Phone}",
                request?.Email,
                request?.PhoneNumber
            );

            var connection = _context.Database.GetDbConnection();

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    _logger.LogInformation("Opening database connection");
                    await connection.OpenAsync();
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "influencer.RegisterUser";
                    command.CommandType = CommandType.StoredProcedure;

                    // 🔐 Never log password
                    command.Parameters.Add(new SqlParameter("@FullName", request.FullName ?? (object)DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Email", request.Email ?? (object)DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@PhoneNumber", request.PhoneNumber ?? (object)DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Password", request.Password));
                    command.Parameters.Add(new SqlParameter("@UserTypeId", request.UserTypeId)); // ✅ NEW

                    _logger.LogInformation("Executing stored procedure influencer.RegisterUser");

                    var result = await command.ExecuteScalarAsync();

                    if (result == null)
                    {
                        _logger.LogError("Stored procedure returned NULL result");
                        return -99;
                    }

                    var userId = Convert.ToInt32(result);

                    if (userId == -1)
                    {
                        _logger.LogWarning("User already exists -> Email: {Email}", request.Email);
                    }
                    else
                    {
                        _logger.LogInformation("User inserted successfully -> UserId: {UserId}", userId);
                    }

                    return userId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RegisterUser -> Email: {Email}", request?.Email);
                return -99;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    await connection.CloseAsync();
                    _logger.LogInformation("Database connection closed (RegisterUser)");
                }
            }
        }

        public async Task<List<User>> GetAllUsers()
        {
            _logger.LogInformation("GetAllUsers method called");

            var users = new List<User>();
            var connection = _context.Database.GetDbConnection();

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "influencer.GetAllUsers";
                    command.CommandType = CommandType.StoredProcedure;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            users.Add(new User
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                UserName = reader["UserName"]?.ToString(),
                                Email = reader["Email"]?.ToString(),
                                FullName = reader["FullName"]?.ToString(), // ✅ ADD THIS LINE
                                UserTypeName = reader["UserTypeName"]?.ToString()
                            });
                        }
                    }
                }

                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllUsers");
                return new List<User>();
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    await connection.CloseAsync();
                }
            }
        }
    }
}