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

        public async Task<(bool IsValid, LoginResponse? Resposne)> GenerateToken(string username, string password)
        {
            var user = await _dbContext.Users.SingleOrDefaultAsync(x => x.Username == username);
            if (user == null)
            {
                return (false, null);
            }

            var validPassword = BCrypt.Net.BCrypt.EnhancedVerify(password, user.Password);
            if (!validPassword)
            {
                return (false, null);
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(Constants.UsernameClaim, user.Username),
                new Claim(Constants.UserIdClaim, user.Id.ToString()),
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
