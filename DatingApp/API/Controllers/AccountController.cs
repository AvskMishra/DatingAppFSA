using API.Data;
using API.DTOs;
using System.Text;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using API.Interfaces;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController(DataContext context, ITokenServices tokenServices) : BaseApiController
    {
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await EmailExist(registerDto.Email)) return BadRequest("Email already Taken");

            var hmac = new HMACSHA512();
            var user = new AppUser
            {
                Email = registerDto.Email,
                UserName = registerDto.DisplayName,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return new UserDto
            {
                Id = user.Id.ToString(),
                DisplayName = user.UserName,
                Email = user.Email!,
                Token = tokenServices.CreateToken(user)
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await context.Users.SingleOrDefaultAsync(x => x.Email == loginDto.Email);
            if (user is null) return Unauthorized("Invalid Email");
            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computeHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
            for (var i = 0; i < computeHash.Length; i++)
            {
                if (computeHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid Password");
            }
            return new UserDto
            {
                Id = user.Id.ToString(),
                DisplayName = user.UserName,
                Email = user.Email!,
                Token = tokenServices.CreateToken(user)
            };
        }

        private async Task<bool> EmailExist(string email)
        {
            return await context.Users.AnyAsync(x => x.Email.ToLower() == email.ToLower());
        }

    }
}
