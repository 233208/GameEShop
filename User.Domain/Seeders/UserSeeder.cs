using User.Domain.Repositories;
using User.Domain.Models.Entities;
using User.Domain.Utils;

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
                    PasswordHash = PasswordHasher.Hash("admin"),
                    Roles = new List<Role> { new Role { Name = "Admin" } },
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow,
                    IsActive = true
                    },
                new User.Domain.Models.Entities.User {
                    
                    Username = "employee",
                    Email = "employee@test.com",
                     PasswordHash = PasswordHasher.Hash("employee"),
                    Roles = new List<Role> { new Role { Name = "Employee" } },
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow,
                    IsActive = true
                    },
                new User.Domain.Models.Entities.User
                {
                    Username = "customer",
                    Email = "customer@test.com",
                     PasswordHash = PasswordHasher.Hash("customer"),
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