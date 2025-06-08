using User.Domain.Repositories;
using User.Domain.Models.Entities;

namespace User.Domain.Seeders;

public class UserSeeder(UserDbContext dbContext) : IUserSeeder
{
    public async Task Seed()
    {
        if (!dbContext.Users.Any())
        {
            var Users = new List<User.Domain.Models.Entities.User>
            {
                new User.Domain.Models.Entities.User {
                    
                    Username = "admin",
                    Email = "mateusz.wojcik2003@gmail.com",
                    PasswordHash = "admin",
                    Roles = new List<Role> { new Role { Name = "Admin" } },
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow,
                    IsActive = true
                    },
                new User.Domain.Models.Entities.User {
                    
                    Username = "employee",
                    Email = "employee@test.com",
                    PasswordHash = "employee",
                    Roles = new List<Role> { new Role { Name = "Employee" } },
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow,
                    IsActive = true
                    },
                new User.Domain.Models.Entities.User
                {
                    Username = "customer",
                    Email = "customer@test.com",
                    PasswordHash = "customer",
                    Roles = new List<Role> { new Role { Name = "Customer" } },
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow,
                    IsActive = true
                }

            };
            dbContext.Users.AddRange(Users);
            dbContext.SaveChanges();
        }
    }
}