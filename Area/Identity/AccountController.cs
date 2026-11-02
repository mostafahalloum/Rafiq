
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using movieSystem.ViewModel;
using Rafiq.DTOs.Request;
using Rafiq.DTOs.Response;
using Rafiq.Models;
using Rafiq.Repositories.IRepositories;
using Rafiq.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Rafiq.Area.identity
    {
    [Route("api/[Area]/[controller]")]
    [ApiController]
    [Area("Identity")]
    public class AccountController : ControllerBase
        {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IRepository<ApplicationUserOTP> _applicationUserOTPRepository;
        private readonly ITokenService _tokenService;

        public AccountController ( UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
           IEmailSender emailSender, IRepository<ApplicationUserOTP> applicationUserOTPRepository, ITokenService tokenService )
            {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _applicationUserOTPRepository = applicationUserOTPRepository;
            _tokenService = tokenService;
            }


        [HttpPost("Register")]
        public async Task<IActionResult> Register ( [FromBody] RegisterRequest registerRequest )
            {

            var user = new ApplicationUser()
                {
                FirstName = registerRequest.FirstName,
                LastName = registerRequest.LastName,
                Email = registerRequest.Email,
                UserName = registerRequest.Email,

                };

            var result = await _userManager.CreateAsync(user, registerRequest.Password);

            if ( !result.Succeeded )
                {
                return BadRequest(result.Errors);
                }


            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action("ConfirmEmail", "Account", new { token, userId = user.Id }, Request.Scheme);

            await _emailSender.SendEmailAsync(user.Email, "Rafiq - Confirm Your Email!"
                , $"<h1>Confirm Your Email By Clicking <a href='{link}'>Here</a></h1>");
            return Ok(new
                {
                msg = "Register success"
                });
            }


        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail ( string userId, string token )
            {

            var user = await _userManager.FindByIdAsync(userId);
            if ( user is null )
                return NotFound(new ErrorModelResponse
                    {
                    Code = "Invalid Cred.",
                    Description = "Invalid User Name / Email OR Password"
                    });

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if ( !result.Succeeded )
                return BadRequest(new
                    {
                    msg = "Invalid OR Expired Token"
                    });
            else
                return Ok(new
                    {
                    msg = "Confirm Email Successfully"
                    });
            }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin ( [FromBody] GoogleLoginRequest model )
        {
            try {
  
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {

                    Audience = new List<string> { "35547670951-n2185m2r4pf7iqukgdedavh50f25r7sp.apps.googleusercontent.com" }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(model.Token, settings);

 
                var user = await _userManager.FindByEmailAsync(payload.Email);

                if ( user == null ) {
          
                    user = new ApplicationUser
                    {
                        Email = payload.Email,
                        UserName = payload.Email,
                        FirstName = payload.GivenName,
                        LastName = payload.FamilyName,
                        EmailConfirmed = true
                    };

                    var result = await _userManager.CreateAsync(user);
                    if ( !result.Succeeded )
                        return BadRequest(result.Errors);
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Email!),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

                foreach ( var role in userRoles )
                    claims.Add(new Claim(ClaimTypes.Role, role));

                var accessToken = _tokenService.GenerateAccessToken(claims);
                var refreshToken = _tokenService.GenerateRefreshToken();

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await _userManager.UpdateAsync(user);

                return Ok(new
                {
                    accessToken = accessToken,
                    refreshToken = refreshToken
                });
            }
            catch ( Exception ) {
                return BadRequest(new { Description = "Invalid Google Token" });
            }
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login ( [FromBody] loginRequest loginRequest )
            {
            var user = await _userManager.FindByEmailAsync(loginRequest.Email);
            if ( user is null )
                {
                return NotFound(new ErrorModelResponse
                    {
                    Code = "Invalid Cred.",
                    Description = "Invalid Email OR Password"
                    });
                }

            var result = await _signInManager.PasswordSignInAsync(user, loginRequest.Password, loginRequest.RememberMe, true);

            if ( !result.Succeeded )
                {
                if ( result.IsLockedOut )
                    return BadRequest(new ErrorModelResponse
                        {
                        Code = "Too many attemps",
                        Description = "Too many attemps, try again after 5 min"
                        });
                else if ( !user.EmailConfirmed )
                    return BadRequest(new ErrorModelResponse
                        {
                        Code = "Confirm Your Email",
                        Description = "Please Confirm Your Email First!!"
                        });
                else
                    return NotFound(new ErrorModelResponse
                        {
                        Code = "Invalid Cred.",
                        Description = "Invalid Email OR Password"
                        });
                }


            var userRoles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email!),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach ( var role in userRoles )
                {
                claims.Add(new Claim(ClaimTypes.Role, role));
                }

            var accessToken = _tokenService.GenerateAccessToken(claims);
            var refreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _userManager.UpdateAsync(user);

            return Ok(new
                {
                accessToken = accessToken,
                ValidTo = "15 Min",
                refreshToken = refreshToken,
                RefreshTokenExpiration = "7 day"
                });
        }


        [HttpPost("ResendEmailConfirmation")]
        public async Task<IActionResult> ResendEmailConfirmation ( [FromBody] ResendEmailConfirmationRequest resendEmailConfirmationRequest )
            {
            var user = await _userManager.FindByEmailAsync(resendEmailConfirmationRequest.Email);
            if ( user is null )
                return NotFound(new ErrorModelResponse
                    {
                    Code = "Invalid Cred.",
                    Description = "Invalid User Name / Email OR Password"
                    });

            if ( user.EmailConfirmed )
                {
                return BadRequest(new ErrorModelResponse
                    {
                    Code = "Already Confirmed!!",
                    Description = "Already Confirmed!!"
                    });
                }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action("ConfirmEmail", "Account", new { token, userId = user.Id }, Request.Scheme);

            await _emailSender.SendEmailAsync(user.Email, "Rafiq - Confirm Your Email!"
                , $"<h1>Confirm Your Email By Clicking <a href='{link}'>Here</a></h1>");


            return Ok(new ErrorModelResponse
                {
                Code = "success",
                Description = "Resend Email Confirmation success"
                });

            }

        [HttpPost("ForgetPassword")]
        public async Task<IActionResult> ForgetPassword ( [FromBody] ForgetPasswordRequest request )
            {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if ( user is null )
                return NotFound(new { msg = "If the account exists, an OTP has been sent to your email." });


            var userOTPs = await _applicationUserOTPRepository.GetAsync(e => e.ApplicationUserId == user.Id && e.IsValid && e.ValidTo > DateTime.UtcNow);
            if ( userOTPs.Count() >= 3 )
                return BadRequest(new ErrorModelResponse
                    {
                    Code = "TooManyAttempts",
                    Description = "Too many OTP requests. Try again later."
                    });


            var otp = RandomNumberGenerator.GetInt32(1000, 9999).ToString();

            await _applicationUserOTPRepository.AddAsync(new ApplicationUserOTP
                {
                Id = Guid.NewGuid().ToString(),
                OTP = otp,
                CreateAt = DateTime.UtcNow,
                ValidTo = DateTime.UtcNow.AddMinutes(5),
                IsValid = true,
                ApplicationUserId = user.Id
                });
            await _applicationUserOTPRepository.CommitAsync();

            await _emailSender.SendEmailAsync(user.Email!, "Rafiq - Reset Your Password",
                $"<h1>Use This OTP: {otp} To Reset Your Account. Don't share it.</h1>");

            return Ok(new { msg = "If the account exists, an OTP has been sent to your email." });
            }


        
            [HttpPost("ValidateOTP")]
            public async Task<IActionResult> ValidateOTP ( [FromBody] ValidateOTPRequest request )
            {

                var user = await _userManager.FindByEmailAsync(request.Email);

                if ( user is null )
                    return BadRequest(new ErrorModelResponse
                    {
                        Code = "InvalidUser",
                        Description = "User not found."
                    });

      
                var otpRecord = await _applicationUserOTPRepository.GetOneAsync(
                    e => e.ApplicationUserId == user.Id &&
                         e.OTP == request.OTP &&
                         e.IsValid &&
                         e.ValidTo > DateTime.UtcNow);

                if ( otpRecord is null )
                    return BadRequest(new ErrorModelResponse
                    {
                        Code = "InvalidOrExpired",
                        Description = "OTP is invalid or expired."
                    });

                otpRecord.IsValid = false;
                _applicationUserOTPRepository.Update(otpRecord);
                await _applicationUserOTPRepository.CommitAsync();

                return Ok(new
                {
                    msg = "OTP verified successfully",
                    userId = user.Id
                });
            }
       
        [HttpPost("NewPassword")]
        public async Task<IActionResult> NewPassword ( NewPasswordRequest newPasswordRequest )
            {
            var user = await _userManager.FindByIdAsync(newPasswordRequest.ApplicationUserId);
            if ( user == null )
                {
                return NotFound(new { msg = "If the account exists, an OTP has been sent to your email." });
                }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPasswordRequest.Password);

            if ( !result.Succeeded )
                return BadRequest(result.Errors);
            return Ok();
            }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh ( [FromBody] TokenApiRequest tokenApiRequest )
        {
            if ( tokenApiRequest?.AccessToken == null || tokenApiRequest.RefreshToken == null )
                return BadRequest("Invalid client request");

            ClaimsPrincipal principal;
            try {
                principal = _tokenService.GetPrincipalFromExpiredToken(tokenApiRequest.AccessToken);
            }
            catch {
                return BadRequest("Invalid access token");
            }
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if ( string.IsNullOrEmpty(userId) )
                return BadRequest("Invalid token claims");

            var user = await _userManager.FindByIdAsync(userId);

            if ( user == null )
                return BadRequest("User not found");

            if ( user.RefreshToken != tokenApiRequest.RefreshToken )
                return BadRequest("Refresh token mismatch");

            if ( user.RefreshTokenExpiryTime <= DateTime.UtcNow )
                return BadRequest("Refresh token expired");

            var newAccessToken = _tokenService.GenerateAccessToken(principal.Claims);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _userManager.UpdateAsync(user);

            return Ok(new
            {
                accessToken = newAccessToken,
                ValidTo = "15 Min",
                refreshToken = newRefreshToken
            });

        }
        [HttpPost, Authorize]
        [Route("revoke")]
        public async Task<IActionResult> Revoke ()
            {
            var username = User.Identity.Name;

            var user = _userManager.Users.FirstOrDefault(e => e.UserName == username);
            if ( user == null ) return BadRequest();

            user.RefreshToken = null;

            await _userManager.UpdateAsync(user);

            return NoContent();
            }
        }
    }



