namespace User.Domain.Seeders;
using User.Domain.Repositories;

public class UserSeeder(UserDbContext dbContext) : IUserSeeder
{
    public async Task Seed()
    {
        if (!dbContext.Users.Any())
        {
            var Users = new List<Users>
            {
                new Users {
                    
                    Username = "admin",
                    Email = "mateusz.wojcik2003@gmail.com",
                    PasswordHash = "admin",
                    Roles = new List<Role> { new Role { Name = "Admin" } },
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow,
                    IsActive = true
                    },
                new Users {
                    
                    Username = "employee",
                    Email = "employee@test.com",
                    PasswordHash = "employee",
                    Roles = new List<Role> { new Role { Name = "Employee" } },
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow,
                    IsActive = true
                    },
                new Users
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