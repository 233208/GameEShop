using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using User.Domain.Models.Response;
using User.Domain.Repositories;

namespace User.Application.Services;

public class UserService : IUserService
{
    private readonly UserDbContext _dbContext;
    private readonly IMapper _mapper;

    public UserService(UserDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }
    public async Task<UserResponseDTO> GetUserAsync(int userId)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
            throw new Exception("Record not found");

        return _mapper.Map<UserResponseDTO>(user);
    }
}
