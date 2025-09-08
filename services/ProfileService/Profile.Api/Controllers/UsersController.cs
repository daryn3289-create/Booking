using Booking.Shared.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Profile.Infrastructure;

namespace Profile.Api.Controllers;

public class UsersController : BaseController
{
    private readonly ProfileDbContext _context;

    public UsersController(ProfileDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers(int page = 1, int pageSize = 10)
    {
        var users = await _context.Users
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return Ok(users);
    }
}