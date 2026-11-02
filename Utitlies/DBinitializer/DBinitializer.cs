using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rafiq.Data_Access;
using Rafiq.Models;

namespace Rafiq.Utitlies.DBinitializer
{
    public class DBinitializer : IDBinitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DBinitializer> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public DBinitializer(ApplicationDbContext context, ILogger<DBinitializer> logger,
            RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _logger = logger;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public void initialize()
        {
            try
            {
                if (_context.Database.GetPendingMigrations().Any())
                {
                    _context.Database.Migrate();
                }

                if ( !_roleManager.RoleExistsAsync(SD.SUPER_ADMIN_ROLE).GetAwaiter().GetResult() ) {
                    _roleManager.CreateAsync(new IdentityRole(SD.SUPER_ADMIN_ROLE)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new IdentityRole(SD.ADMIN_ROLE)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new IdentityRole(SD.EMPLOYEE_ROLE)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new IdentityRole(SD.CUSTOMER_ROLE)).GetAwaiter().GetResult();
                }

                var user = _userManager.FindByNameAsync("superadmin")
                          .GetAwaiter().GetResult();

                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        Email = "mostafasami@movie.com",
                        UserName = "superadmin",
                        EmailConfirmed = true,
                        FirstName = "super",
                        LastName = "admin"
                    };

                    _userManager.CreateAsync(user, "Admin123$")
                        .GetAwaiter().GetResult();
                }

                _userManager.AddToRoleAsync(user, SD.SUPER_ADMIN_ROLE)
                    .GetAwaiter().GetResult();

                _userManager.AddToRoleAsync(user, SD.ADMIN_ROLE)
                    .GetAwaiter().GetResult();

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error : {ex.Message}");
            }
            
        }
    }
}
