using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Rafiq.DTOs.Request;
using Rafiq.Models;
using Rafiq.Services;

namespace Rafiq.Controllers
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Area("Customer")]
    public class AIController : ControllerBase
    {
        private readonly IDeepSeekService _deepSeekService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AIController (IDeepSeekService deepSeekService, UserManager<ApplicationUser> userManager )
        {
            _deepSeekService = deepSeekService;
            _userManager = userManager;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask ( [FromBody] ChatRequest request )
        {
            var user = await _userManager.GetUserAsync(User);

            if ( user == null ) {
                return Unauthorized();
            }

            if ( user.AiRequestsDate.Date != DateTime.UtcNow.Date ) {
                user.AiRequestsDate = DateTime.UtcNow.Date;
                user.AiRequestsCount = 0;
            }

            if ( user.AiRequestsCount >= 10 ) {
                return BadRequest(new
                {
                    message = "لقد وصلت للحد اليومي"
                });
            }

            user.AiRequestsCount++;

            await _userManager.UpdateAsync(user);

            var response = await _deepSeekService.AskAI(request.Message);

            return Ok(new
            {
                response,
                remaining = 10 - user.AiRequestsCount
            });
        }
    }
}