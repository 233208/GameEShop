using AutoMapper;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using User.Application.Settings;
using User.Domain.Models.Entities;
using User.Domain.Repositories;
using User.Domain.Utils;


namespace User.Application.Services
{
    public class RegisterService : IRegisterService
    {
        private readonly UserDbContext _dbContext;
        private readonly JwtSettings _jwtSettings;
        public RegisterService(UserDbContext dbContext, IOptions<JwtSettings> jwtSettings)
        {
            _dbContext = dbContext;
            _jwtSettings = jwtSettings.Value;
        }
        public async Task<User.Domain.Models.Entities.User> RegisterAsync(string username, string email, string password)
        {
            if (_dbContext.Users.Any(u => u.Username == username || u.Email == email))
            {
                throw new InvalidOperationException("Username or email already exists.");
            }

            string passwordHash = PasswordHasher.Hash(password);

            var user = new User.Domain.Models.Entities.User
            {
                Username = username,
                Email = email,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var defaultRole = _dbContext.Roles.FirstOrDefault(r => r.Name == "Customer");
            if (defaultRole != null)
            {
                user.Roles = new List<Role> { defaultRole };
            }

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            return user;
        }
    }
}
