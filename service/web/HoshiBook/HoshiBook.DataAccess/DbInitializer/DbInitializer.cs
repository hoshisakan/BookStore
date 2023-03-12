
using HoshiBook.Models;
using HoshiBook.Utility;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HoshiBook.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ILogger<DbInitializer> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ApplicationDbContext _db;
        private readonly Dictionary<string, string> _adminRoleConfig;

        public DbInitializer(
            ILogger<DbInitializer> logger,
            IConfiguration _config,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ApplicationDbContext db)
        {
            _logger = logger;
            _roleManager = roleManager;
            _userManager = userManager;
            _db = db;
            _adminRoleConfig = new Dictionary<string, string>()
            {
                {"Username", _config.GetValue<string>("Roles:Admin:Username")},
                {"Email", _config.GetValue<string>("Roles:Admin:Email")},
                {"Name", _config.GetValue<string>("Roles:Admin:Name")},
                {"PhoneNumber", _config.GetValue<string>("Roles:Admin:PhoneNumber")},
                {"StreetAddress", _config.GetValue<string>("Roles:Admin:StreetAddress")},
                {"State", _config.GetValue<string>("Roles:Admin:State")},
                {"PostalCode", _config.GetValue<string>("Roles:Admin:PostalCode")},
                {"City", _config.GetValue<string>("Roles:Admin:City")},
                {"Password", _config.GetValue<string>("Roles:Admin:Password")}
            };
        }

        public void Initializer()
        {
            //TODO migrations if they are not applied
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _logger.LogInformation($"Applying migrations.");
                    _db.Database.Migrate();
                }
                else
                {
                    _logger.LogInformation("No migrations to apply.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while applying migrations: {ex.Message}");
            }

            try {
                int tempUserCount = 0;
                //TODO create roles if they are not created
                if (!_roleManager.RoleExistsAsync(SD.Role_Admin).GetAwaiter().GetResult())
                {
                    _logger.LogInformation("Admin role not found, creating it");
                    List<ApplicationRole> roles = new List<ApplicationRole>()
                    {
                        new ApplicationRole
                        {
                            Name = SD.Role_Admin,
                            RoleNumber = 1
                        },
                        new ApplicationRole
                        {
                            Name = SD.Role_Employee,
                            RoleNumber = 2
                        },
                        new ApplicationRole
                        {
                            Name = SD.Role_User_Indi,
                            RoleNumber = 3
                        },
                        new ApplicationRole
                        {
                            Name = SD.Role_User_Comp,
                            RoleNumber = 4
                        }
                    };

                    foreach (var role in roles)
                    {
                        _roleManager.CreateAsync(role).GetAwaiter().GetResult();
                    }
                
                    //TODO if roles not created, then we will create admin user as well
                    var result = _userManager.CreateAsync(new ApplicationUser
                    {
                        UserName = _adminRoleConfig["Username"],
                        Email = _adminRoleConfig["Email"],
                        Name = _adminRoleConfig["Name"],
                        PhoneNumber = _adminRoleConfig["PhoneNumber"],
                        StreetAddress = _adminRoleConfig["StreetAddress"],
                        State = _adminRoleConfig["State"],
                        PostalCode = _adminRoleConfig["PostalCode"],
                        City = _adminRoleConfig["City"]
                    }, _adminRoleConfig["Password"]);

                    if (result.Result.Succeeded)
                    {
                        ApplicationUser? user = _db.ApplicationUsers.FirstOrDefault(
                            u => u.Email == _adminRoleConfig["Email"]
                        );
                        if (user == null)
                        {
                            tempUserCount = _db.ApplicationUsers.Select(u => u.Id).Count();
                            throw new Exception($"Admin user {_adminRoleConfig["Email"]} not found, real user count: {tempUserCount}");
                        }
                        tempUserCount = _db.ApplicationUsers.Select(u => u.Id).Count();
                        _logger.LogInformation($"Admin user found, real user count: {tempUserCount}");
                        _userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
                    }
                    else
                    {
                        _logger.LogInformation("Admin user not created");
                    }
                }
                else
                {
                    tempUserCount = _db.ApplicationUsers.Select(u => u.Id).Count();
                    _logger.LogInformation($"Admin role already exists, real user count: {tempUserCount}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return;
        }
    }
}