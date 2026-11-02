using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Rafiq.DTOs.Response;
using Rafiq.Models;
using Rafiq.Repositories.IRepositories;
using Rafiq.Utitlies;
using System.Threading;

namespace Rafiq.Area.Admin
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    [Area("Admin")]
    public class UsersController : Controller
    {
        public UserManager<ApplicationUser> _UserManager { get; }


        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _UserManager = userManager;
        }

        [HttpGet]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public IActionResult GetAll ( int page = 1, int pageSize = 7 )
        {
            var query = _UserManager.Users;

            var totalCount = query.Count();

            var users = query
                .Skip(( page - 1 ) * pageSize)
                .Take(pageSize)
                .Adapt<IEnumerable<UserResponse>>();

            return Ok(new
            {
                Data = users,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> LockUnLock(string id)
        {
            var user = await _UserManager.FindByIdAsync(id);

            if (user is null)
                return NotFound();

            if (await _UserManager.IsInRoleAsync(user, SD.SUPER_ADMIN_ROLE))
            {
                return BadRequest(new ErrorModelResponse
                {
                    Code = "Error",
                    Description = "You can not block super admin account"
                });
            }


            if(user.LockoutEnd is not null && user.LockoutEnd > DateTimeOffset.UtcNow)
                user.LockoutEnd = null;
            else
                user.LockoutEnd = DateTimeOffset.UtcNow.AddDays(30);

            user.LockoutEnabled = true;

            await _UserManager.UpdateAsync(user);

          return NoContent();

        }
    }
}
