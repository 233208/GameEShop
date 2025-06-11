using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using User.Domain.Models.Response;

namespace User.Application.Services;

public interface IGetUserService
{
    Task<UserResponseDTO>  GetUserAsync(int userId);
}
