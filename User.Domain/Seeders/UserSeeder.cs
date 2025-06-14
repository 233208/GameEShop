using Microsoft.EntityFrameworkCore;
using User.Domain.Models.Entities;
using User.Domain.Repositories;
using User.Domain.Utils;
using System.Linq;
using System.Threading.Tasks;

namespace User.Domain.Seeders;

public class UserSeeder : IUserSeeder
{
    private readonly UserDbContext _dbContext;
    public UserSeeder(UserDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Seed()
    {
        if (!await _dbContext.Roles.AnyAsync())
        {
            var roles = new List<Role>
            {
                new Role { Name = "Admin" },
                new Role { Name = "Employee" },
                new Role { Name = "Customer" }
            };
            await _dbContext.Roles.AddRangeAsync(roles);
            await _dbContext.SaveChangesAsync();
        }

        if (!await _dbContext.Users.AnyAsync()) 
        {
            var adminRole = await _dbContext.Roles.FirstAsync(r => r.Name == "Admin");
            var employeeRole = await _dbContext.Roles.FirstAsync(r => r.Name == "Employee");
            var customerRole = await _dbContext.Roles.FirstAsync(r => r.Name == "Customer");

            var users = new List<User.Domain.Models.Entities.User>
            {
                new User.Domain.Models.Entities.User {
                    Username = "admin",
                    Email = "mateusz.wojcik2003@gmail.com",
                    PasswordHash = PasswordHasher.Hash("admin"),
                    Roles = new List<Role> { adminRole } 
                },
                new User.Domain.Models.Entities.User {
                    Username = "employee",
                    Email = "employee@test.com",
                    PasswordHash = PasswordHasher.Hash("employee"),
                    Roles = new List<Role> { employeeRole } 
                },
                new User.Domain.Models.Entities.User {
                    Username = "customer",
                    Email = "customer@test.com",
                    PasswordHash = PasswordHasher.Hash("customer"),
                    Roles = new List<Role> { customerRole } 
                }
            };

            await _dbContext.Users.AddRangeAsync(users);
            await _dbContext.SaveChangesAsync();
        }
    }
}