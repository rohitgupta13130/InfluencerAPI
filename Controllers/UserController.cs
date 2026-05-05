using InfluencerAPI.Dtos;
using InfluencerBackendAPI.Dtos;
using InfluencerBackendAPI.Models;
using InfluencerBackendAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InfluencerBackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IUserRepository _repository;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IConfiguration config,
            IUserRepository repository,
            ILogger<UserController> logger)
        {
            _config = config;
            _repository = repository;
            _logger = logger;
        }

        // ================= LOGIN =================
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            _logger.LogInformation("Login API called");

            try
            {
                if (request == null)
                {
                    _logger.LogWarning("Login request is null");
                    return BadRequest("Invalid request");
                }

                _logger.LogInformation("Login attempt for Username: {UserName}", request.UserName);

                var user = await _repository.AuthenticateUser(request.UserName, request.Password);

                if (user == null)
                {
                    _logger.LogWarning("Invalid login attempt for Username: {UserName}", request.UserName);
                    return Unauthorized("Invalid username or password");
                }

                await _repository.UpdateUserStatus(user.Id, true);
                _logger.LogInformation("User authenticated successfully. UserId: {UserId}", user.Id);

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, _config["Jwt:Subject"] ?? "default"),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("UserId", user.Id.ToString()),
                    new Claim("UserName", user.UserName),
                    new Claim("UserType", user.UserTypeName ?? ""),
                    new Claim("FullName", user.FullName ?? "")
                };

                var keyString = _config["Jwt:Key"];

                if (string.IsNullOrEmpty(keyString))
                {
                    _logger.LogError("JWT Key is missing in configuration");
                    return StatusCode(500, "JWT configuration error");
                }

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));

                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var expiryMinutes = Convert.ToDouble(_config["Jwt:DurationInMinutes"]);

                var token = new JwtSecurityToken(
                    issuer: _config["Jwt:Issuer"],
                    audience: _config["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                    signingCredentials: creds
                );

                //var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

                //_logger.LogInformation("JWT token generated successfully for UserId: {UserId}", user.Id);

                //return Ok(new
                //{
                //    token = tokenValue,
                //    userId = user.Id,
                //    userName = user.UserName,
                //    userType = user.UserTypeName
                //});

                var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

                // 🔥 STORE TOKEN IN COOKIE
                Response.Cookies.Append("AuthToken", tokenValue, new CookieOptions
                {
                    HttpOnly = true,
                    //Secure = false, // true in production (HTTPS)
                    Secure = true,
                    //SameSite = SameSiteMode.Strict,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddMinutes(expiryMinutes)
                });

                return Ok(new
                {
                    message = "Login successful",
                    token = tokenValue,
                    userId = user.Id,
                    userName = user.UserName,
                    userType = user.UserTypeName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during login for Username: {UserName}", request?.UserName);
                return StatusCode(500, "Internal server error");
            }
        }

        // ================= REGISTER =================
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterRequest request)
        {
            _logger.LogInformation("Register API called");

            try
            {
                // ✅ NULL CHECK
                if (request == null)
                {
                    _logger.LogWarning("Register request is null");
                    return BadRequest("Invalid request data");
                }

                // ✅ BASIC VALIDATION
                if (string.IsNullOrWhiteSpace(request.Email) ||
                    string.IsNullOrWhiteSpace(request.Password))
                {
                    _logger.LogWarning("Missing required fields -> Email: {Email}", request.Email);
                    return BadRequest("Email and Password are required");
                }

                _logger.LogInformation(
                    "Registration attempt -> Email: {Email}, Phone: {Phone}",
                    request.Email,
                    request.PhoneNumber
                );

                // ✅ CALL REPOSITORY
                var result = await _repository.RegisterUser(request);

                // ✅ HANDLE RESPONSES
                return result switch
                {
                    -1 => BadRequest("User already exists"),

                    -2 => BadRequest("Only JPG, JPEG, PNG images are allowed"),

                    -3 => BadRequest("Image size must be less than 2MB"),

                    -99 => StatusCode(500, "Something went wrong"),

                    _ => Ok(new
                    {
                        message = "Registration successful",
                        userId = result
                    })
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error occurred during registration for Email: {Email}",
                    request?.Email
                );

                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            _logger.LogInformation("GetAllUsers API called");

            try
            {
                var users = await _repository.GetAllUsers();

                if (users == null || users.Count == 0)
                {
                    return NotFound("No users found");
                }

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllUsers API");
                return StatusCode(500, "Internal server error");
            }
        }



        [Authorize]
        [HttpGet("Profile")]
        public async Task<IActionResult> GetProfile()
        {
            _logger.LogInformation("GetProfile API called");

            try
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;

                if (string.IsNullOrEmpty(userIdClaim))
                {
                    _logger.LogWarning("UserId claim not found in token");
                    return Unauthorized("Invalid token");
                }

                int userId = int.Parse(userIdClaim);

                _logger.LogInformation("Fetching profile for UserId: {UserId}", userId);

                var user = await _repository.GetUserById(userId);

                if (user == null)
                {
                    _logger.LogWarning("User not found for UserId: {UserId}", userId);
                    return NotFound("User not found");
                }

                var result = new UserProfileRequest
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    UserName = user.UserName,
                    Email = user.Email,
                    UserTypeName = user.UserTypeName,
                    LastSeen = user.LastSeen,
                    IsOnline = user.IsOnline
                };

                _logger.LogInformation("Profile fetched successfully for UserId: {UserId}", userId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in GetProfile API");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var user = await _repository.GetUserById(id);

                if (user == null)
                {
                    return NotFound("User not found");
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUserById");
                return StatusCode(500, "Internal server error");
            }
        }



        [HttpPost("Logout")]
        public async Task<IActionResult> Logout([FromQuery] int userId)
        {
            await _repository.UpdateUserStatus(userId, false);
            Response.Cookies.Delete("AuthToken");
            return Ok("Logged out successfully");
        }
    }
}