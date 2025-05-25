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
                    Id = 1,
                    Username = "admin",
                    Email = "mateusz.wojcik2003@gmail.com",
                    PasswordHash = "admin",
                    Roles = new List<Role> { new Role { Id = 1, Name = "Admin" } },
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow,
                    IsActive = true
                    },
                new Users {
                    Id = 2,
                    Username = "employee",
                    Email = "employee@test.com",
                    PasswordHash = "employee",
                    Roles = new List<Role> { new Role { Id = 2, Name = "Employee" } },
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow,
                    IsActive = true
                    },
                new Users
                {
                    Id = 3,
                    Username = "customer",
                    Email = "customer@test.com",
                    PasswordHash = "customer",
                    Roles = new List<Role> { new Role { Id = 3, Name = "Customer" } },
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