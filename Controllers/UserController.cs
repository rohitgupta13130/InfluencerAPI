using InfluencerBackendAPI.Dtos;
using InfluencerBackendAPI.Repositories;
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

                _logger.LogInformation("User authenticated successfully. UserId: {UserId}", user.Id);

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, _config["Jwt:Subject"] ?? "default"),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("UserId", user.Id.ToString()),
                    new Claim("UserName", user.UserName),
                    new Claim("UserType", user.UserTypeName ?? "")
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

                var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

                _logger.LogInformation("JWT token generated successfully for UserId: {UserId}", user.Id);

                return Ok(new
                {
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
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            _logger.LogInformation("Register API called");

            try
            {
                if (request == null)
                {
                    _logger.LogWarning("Register request is null");
                    return BadRequest("Invalid data");
                }

                _logger.LogInformation(
                    "Registration attempt -> Email: {Email}, Phone: {Phone}",
                    request.Email,
                    request.PhoneNumber
                );

                if (string.IsNullOrEmpty(request.Password))
                {
                    _logger.LogWarning("Password is empty for Email: {Email}", request.Email);
                    return BadRequest("Password is required");
                }

                var result = await _repository.RegisterUser(request);

                if (result == -1)
                {
                    _logger.LogWarning("User already exists with Email: {Email}", request.Email);
                    return BadRequest("User already exists");
                }

                if (result == -99)
                {
                    _logger.LogError("Repository error while registering Email: {Email}", request.Email);
                    return StatusCode(500, "Something went wrong");
                }

                _logger.LogInformation("User registered successfully. UserId: {UserId}", result);

                return Ok(new
                {
                    message = "Registration successful",
                    userId = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during registration for Email: {Email}", request?.Email);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}