using BCrypt.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TasksAPI.Models;
using TasksAPI.Models.Entities;
using TasksAPI.Models.Requests;
using TasksAPI.Models.Responses;

namespace TasksAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly TasksDBContext _dbContext;
        private readonly TokenService _tokenService;

        public AuthController(TasksDBContext dbContext, TokenService tokenService)
        {
            _dbContext = dbContext;
            _tokenService = tokenService;
        }

        // POST: api/Auth/Login
        [HttpPost("[action]")]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest loginRequest)
        {
            var result = await _tokenService.GenerateToken(loginRequest.Username, loginRequest.Password);

            if(result.IsValid)
            {
                return Ok(result.Resposne);
            }

            return BadRequest("Username password combination is incorrect");
        }

        // POST: api/Auth/Signup
        [HttpPost("[action]")]
        public async Task<ActionResult<LoginResponse>> SignUp(SignUpRequest signUpRequest)
        {
            var newUser = new User
            {
                Username = signUpRequest.Username,
                Password = BCrypt.Net.BCrypt.EnhancedHashPassword(signUpRequest.Password),
                Role = signUpRequest.Role
            };

            _ = await _dbContext.Users.AddAsync(newUser);
            _ = await _dbContext.SaveChangesAsync();

            var result = await _tokenService.GenerateToken(newUser.Username, signUpRequest.Password);

            return Ok(result.Resposne);
        }
    }
}
