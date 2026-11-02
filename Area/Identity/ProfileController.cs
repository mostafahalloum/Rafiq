using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Rafiq.DTOs.Request;
using Rafiq.DTOs.Response;
using Rafiq.Models;
using System.Threading.Tasks;

namespace Rafiq.Area.Identity
{
    [Route("api/[Area]/[controller]")]
    [ApiController]
    [Authorize]
    [Area("Identity")]
    public class ProfileController : ControllerBase
    {

        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController ( UserManager<ApplicationUser> userManager )
        {
            _userManager = userManager;
        }


        [HttpGet("me")]
        public async Task<IActionResult> GetInfo ()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null ) {
                return Unauthorized();
            }

          var userInfo = user.Adapt<ApplicationUserResponse>();

            return Ok(userInfo);
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateProfile ( ApplicationUserRequest applicationUserRequest )
        {
            var user = await _userManager.GetUserAsync(User);

            if ( user == null ) {
                return Unauthorized();
            }

            if ( !string.IsNullOrWhiteSpace(applicationUserRequest.FirstName) )
                user.FirstName = applicationUserRequest.FirstName;

            if ( !string.IsNullOrWhiteSpace(applicationUserRequest.LastName) )
                user.LastName = applicationUserRequest.LastName;

            if ( !string.IsNullOrWhiteSpace(applicationUserRequest.PhoneNumber) )
                user.PhoneNumber = applicationUserRequest.PhoneNumber;

            if ( !string.IsNullOrWhiteSpace(applicationUserRequest.Address) )
                user.Address = applicationUserRequest.Address;

            if ( !string.IsNullOrWhiteSpace(applicationUserRequest.Gender) )
                user.Gender = applicationUserRequest.Gender;

            var result = await _userManager.UpdateAsync(user);

            if ( !result.Succeeded )
                return BadRequest(result.Errors);

            return Ok(new
            {
                msg = "updated Successfully"
            });
        }

        [HttpPost("changePassword")]
        public async Task<IActionResult> ResetPassword (changePasswordProfileRequest changePasswordProfileRequest)
        {
            var user = await _userManager.GetUserAsync(User);

            if ( user == null ) {
                return Unauthorized();
            }

            if ( string.IsNullOrWhiteSpace(changePasswordProfileRequest.CurrentPassword) ||
                 string.IsNullOrWhiteSpace(changePasswordProfileRequest.NewPassword) ) {
                return BadRequest(new ErrorModelResponse
                {
                    Code = "Matching Current Password & New Password",
                    Description = "Must have a CurrentPassword & NewPassword value"
                });
            }

            var result = await _userManager.ChangePasswordAsync(user, changePasswordProfileRequest.CurrentPassword!, changePasswordProfileRequest.NewPassword!);
            
            if ( !result.Succeeded )
                return BadRequest(result.Errors);

            await _userManager.UpdateSecurityStampAsync(user);

            return Ok(new
            {
                msg = "Reset Password Successfully",
                forceLogout = true
            });
        }

        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage ( IFormFile file )
        {
            var user = await _userManager.GetUserAsync(User);

            if ( user == null )
                return Unauthorized();

            if ( file == null || file.Length == 0 )
                return BadRequest("No file uploaded");

            var uploadsFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot/personal-img"
            );

            if ( !Directory.Exists(uploadsFolder) )
                Directory.CreateDirectory(uploadsFolder);


            if ( !string.IsNullOrEmpty(user.ImageUrl) ) {
                var oldImagePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    user.ImageUrl.TrimStart('/')
                );

                if ( System.IO.File.Exists(oldImagePath) ) {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            var fileName =
                Guid.NewGuid() + Path.GetExtension(file.FileName);

            var filePath =
                Path.Combine(uploadsFolder, fileName);

            using ( var stream = new FileStream(filePath, FileMode.Create) ) {
                await file.CopyToAsync(stream);
            }

            user.ImageUrl = "/personal-img/" + fileName;

            var result = await _userManager.UpdateAsync(user);

            if ( !result.Succeeded ) {
                return BadRequest(result.Errors);
            }

            return Ok(new
            {
                imageUrl = user.ImageUrl
            });
        }
    }
}
