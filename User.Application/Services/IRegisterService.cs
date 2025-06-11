using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace User.Application.Services;

public interface IRegisterService
{
    Task<User.Domain.Models.Entities.User> RegisterAsync(string username, string password, string email);
}
