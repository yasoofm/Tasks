using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TasksAPI.Models.Responses;

namespace TasksAPI.Models
{
    public class TokenService
    {
        private readonly IConfiguration _configuration;
        private readonly TasksDBContext _dbContext;

        public TokenService(IConfiguration configuration, TasksDBContext tasksDBContext)
        {
            _configuration = configuration;
            _dbContext = tasksDBContext;
        }

        public (bool IsValid, LoginResponse? Resposne) GenerateToken(string username, string password)
        {
            var user = _dbContext.Users.SingleOrDefault(x => x.Username == username);
            if (user == null)
            {
                return (false, null);
            }

            var validPassword = BCrypt.Net.BCrypt.Verify(password, user.Password);
            if (!validPassword)
            {
                return (false, null);
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("username", user.Username),
                new Claim("userId", user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role),
            };

            var token = new JwtSecurityToken(
                issuer: "Yousef Mandani",
                audience: "Lyla AlKandari",
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials);

            var generatedToken = new JwtSecurityTokenHandler().WriteToken(token);
            return (true, new LoginResponse { Token = generatedToken});
        }
    }
}
