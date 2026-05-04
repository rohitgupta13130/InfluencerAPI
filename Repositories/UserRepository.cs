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
                                UserTypeName = reader["UserTypeName"]?.ToString(),
                                FullName = reader["FullName"]?.ToString()
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
                "RegisterUser called -> Email: {Email}, Phone: {Phone}",
                request?.Email,
                request?.PhoneNumber
            );

            string imagePath = null;

            try
            {
                // ✅ HANDLE IMAGE
                if (request.ProfileImage != null && request.ProfileImage.Length > 0)
                {
                    // ✅ FILE SIZE VALIDATION (2MB)
                    if (request.ProfileImage.Length > 2 * 1024 * 1024)
                    {
                        _logger.LogWarning("File too large");
                        return -3; // file too large
                    }

                    var extension = Path.GetExtension(request.ProfileImage.FileName).ToLower();

                    // ✅ FORMAT VALIDATION
                    if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
                    {
                        _logger.LogWarning("Invalid image format");
                        return -2; // invalid format
                    }

                    // ✅ CREATE FOLDER
                    var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/profile");

                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    // ✅ UNIQUE FILE NAME
                    var fileName = Guid.NewGuid().ToString() + extension;
                    var filePath = Path.Combine(folderPath, fileName);

                    // ✅ SAVE FILE
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await request.ProfileImage.CopyToAsync(stream);
                    }

                    imagePath = "/profile/" + fileName;
                }

                var connection = _context.Database.GetDbConnection();

                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "influencer.RegisterUser";
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@FullName", request.FullName ?? (object)DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Email", request.Email ?? (object)DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@PhoneNumber", request.PhoneNumber ?? (object)DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Password", request.Password));
                    command.Parameters.Add(new SqlParameter("@UserTypeId", request.UserTypeId));

                    // ✅ IMAGE PATH
                    command.Parameters.Add(new SqlParameter("@ProfileImage", imagePath ?? (object)DBNull.Value));

                    var result = await command.ExecuteScalarAsync();

                    return result == null ? -99 : Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RegisterUser");
                return -99;
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
                                UserTypeName = reader["UserTypeName"]?.ToString(),
                                IsOnline = Convert.ToBoolean(reader["IsOnline"]),   // ✅ ADD
                                LastSeen = reader["LastSeen"] as DateTime?          // ✅ ADD
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


        public async Task UpdateUserStatus(int userId, bool isOnline)
        {
            var connection = _context.Database.GetDbConnection();

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "influencer.UpdateUserStatus";
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@UserId", userId));
                    command.Parameters.Add(new SqlParameter("@IsOnline", isOnline));

                    await command.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user status");
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    await connection.CloseAsync();
                }
            }
        }



        public async Task<User> GetUserById(int userId)
        {
            User user = null;
            var connection = _context.Database.GetDbConnection();

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "influencer.GetUserById";
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@UserId", userId));

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                        {
                            user = new User
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                UserName = reader["UserName"]?.ToString(),
                                Email = reader["Email"]?.ToString(),
                                FullName = reader["FullName"]?.ToString(),
                                UserTypeName = reader["UserTypeName"]?.ToString(),
                                IsOnline = Convert.ToBoolean(reader["IsOnline"]),
                                LastSeen = reader["LastSeen"] as DateTime?
                            };
                        }
                    }
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUserById");
                return null;
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