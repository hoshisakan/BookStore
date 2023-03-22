using HoshiBook.DataAccess.Repository.IRepository;
using HoshiBookWeb.Tools;
using HoshiBook.Utility;
using HoshiBook.Models.ViewModels.User;
using HoshiBookWeb.Tools.CommonTool;
using HoshiBook.Models;


using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;


namespace HoshiBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IConfiguration _config;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public UserController(
            ILogger<UserController> logger, IUnitOfWork unitOfWork,
            IWebHostEnvironment hostEnvironment, IConfiguration config,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager
        )
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
            _config = config;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Lock()
        {
            return View();
        }

        public ActionResult Lockout()
        {
            return View();
        }

        //GET
        public async Task<IActionResult> Edit(string? uid)
        {
            if (string.IsNullOrEmpty(uid)) {
                return NotFound();
            }
            else
            {
                var editUser = await _userManager.FindByIdAsync(uid);

                bool checkUserlocked = editUser.IsLockedOut;

                if (checkUserlocked)
                {
                    TempData["error"] = "This user is locked";
                    return RedirectToAction(nameof(Index));
                }
            
                var editUserHasRole = await _userManager.GetRolesAsync(editUser);
                var editUserHasRoleName = editUserHasRole.FirstOrDefault();
                var editUserHasRoleId = (
                    from role in _roleManager.Roles
                    where role.Name == editUserHasRoleName
                    select role.Id
                ).SingleOrDefault();

                _logger.LogInformation("uid: {0}", uid);
                _logger.LogInformation("editUserHasRoleId: {0}", editUserHasRoleId);

                var editUserHasRoleNumber = (
                    from role in _roleManager.Roles
                    where role.Id == editUserHasRoleId
                    select role.RoleNumber
                ).SingleOrDefault();

                _logger.LogInformation("editUserHasRoleNumber: {0}", editUserHasRoleNumber);
                _logger.LogInformation("editUser.CompanyId: {0}", editUser.CompanyId);

                int? editUserHasCompanyId = editUser.CompanyId ?? 0;

                _logger.LogInformation("editUserHasCompanyId: {0}", editUser.CompanyId);

                List<Company> companyList = _unitOfWork.Company.GetAll();

                UserEditVM userEditVM = new()
                {
                    ApplicationUser = new(),
                    CompanyList = companyList.Select(
                        u => new SelectListItem
                        {
                            Text = u.Name,
                            Value = u.Id.ToString(),
                            Selected = editUserHasCompanyId == u.Id
                        }
                    ),
                    RoleList = _roleManager.Roles.ToList().Select(i => new SelectListItem
                    {
                        Text = i.Name,
                        Value = i.RoleNumber.ToString(),
                        Selected = editUserHasRoleNumber == i.RoleNumber
                    }),
                };
                _logger.LogInformation("uid: {0}", uid);
                // update user
                userEditVM.ApplicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == uid);
                return View(userEditVM);
            }
        }

        //POST
        //TODO Add ValidateAntiForgeryToken to avoid CORS attack
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserEditVM obj)
        {
            try {
                if (ModelState.IsValid)
                {
                    _logger.LogInformation("obj.ApplicationUser.Id: {0}", obj.ApplicationUser.Id);
                    string? UID = obj.ApplicationUser.Id;
                    string? Email = obj.ApplicationUser.Email;
                    string? Name = obj.ApplicationUser.Name;
                    string? PhoneNumber = obj.ApplicationUser.PhoneNumber;
                    string? StreetAddress = obj.ApplicationUser.StreetAddress;
                    string? City = obj.ApplicationUser.City;
                    string? State = obj.ApplicationUser.State;
                    string? PostalCode = obj.ApplicationUser.PostalCode;
                    int RoleNumber = obj.ApplicationRole.RoleNumber;
                    int? CompanyId = obj.ApplicationUser.CompanyId ?? 0;

                    _logger.LogInformation("UID: {0}", UID);
                    _logger.LogInformation("Email: {0}", Email);
                    _logger.LogInformation("Name: {0}", Name);
                    _logger.LogInformation("PhoneNumber: {0}", PhoneNumber);
                    _logger.LogInformation("StreetAddress: {0}", StreetAddress);
                    _logger.LogInformation("City: {0}", City);
                    _logger.LogInformation("State: {0}", State);
                    _logger.LogInformation("PostalCode: {0}", PostalCode);
                    _logger.LogInformation("RoleNumber: {0}", RoleNumber);
                    _logger.LogInformation("CompanyId: {0}", CompanyId);

                    if (RoleNumber == 0)
                    {
                        TempData["error"] = "Please select a role";
                        return View(obj);
                    }

                    if (RoleNumber == 4 && CompanyId == 0)
                    {
                        TempData["error"] = "Please select a company";
                        return View(obj);
                    }

                    var oldUser = _userManager.FindByIdAsync(UID).Result;

                    var oldUserHasRoleName = _userManager.GetRolesAsync(oldUser).Result.FirstOrDefault();
                    var oldUserHasRoleId = (
                        from role in _roleManager.Roles
                        where role.Name == oldUserHasRoleName
                        select role.Id
                    ).SingleOrDefault();

                    _logger.LogInformation("oldUserHasRoleName: {0}", oldUserHasRoleName);
                    _logger.LogInformation("oldUserHasRoleId: {0}", oldUserHasRoleId);

                    var newRoleId = (
                        from role in _roleManager.Roles
                        where role.RoleNumber == RoleNumber
                        select role.Id
                    ).SingleOrDefault();

                    var newRoleName = (
                        from role in _roleManager.Roles
                        where role.RoleNumber == RoleNumber
                        select role.Name
                    ).SingleOrDefault();

                    _logger.LogInformation("newRoleId: {0}", newRoleId);
                    _logger.LogInformation("newRoleName: {0}", newRoleName);

                    if (oldUserHasRoleId != newRoleId)
                    {
                        var removeRoleResult = await _userManager.RemoveFromRoleAsync(oldUser, oldUserHasRoleName);
                        _logger.LogInformation("removeRoleResult: {0}", removeRoleResult);
                        var updateRoleResult = await _userManager.AddToRoleAsync(oldUser, newRoleName);
                        _logger.LogInformation("updateRoleResult: {0}", updateRoleResult);
                    }

                    oldUser.Id = UID;
                    oldUser.Email = Email;
                    oldUser.Name = Name;
                    oldUser.PhoneNumber = PhoneNumber;
                    oldUser.StreetAddress = StreetAddress;
                    oldUser.City = City;
                    oldUser.State = State;
                    oldUser.PostalCode = PostalCode;

                    if (RoleNumber == 4 && CompanyId != 0)
                    {
                        oldUser.CompanyId = CompanyId;
                    }
                    else
                    {
                        oldUser.CompanyId = null;
                    }

                    var userInfoUpdateResult = await _userManager.UpdateAsync(oldUser);
                    _logger.LogInformation("userInfoUpdateResult: {0}", userInfoUpdateResult);

                    TempData["success"] = "User updated successfully!";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["error"] = "User details updated failed.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                TempData["error"] = "User details updated failed.";
            }
            return View(obj);
        }

        public async Task<IActionResult> ResetPassword(string? uid)
        {
            if (uid == null)
            {
                return NotFound();
            }
            else
            {
                var user = await _userManager.FindByIdAsync(uid);
                
                if (user == null)
                {
                    return NotFound();
                }
                else
                {
                    bool checkUserlocked = user.IsLockedOut;
                    if (checkUserlocked)
                    {
                        TempData["error"] = "This user is locked";
                        return RedirectToAction(nameof(Index));
                    }

                    UserResetPasswordVM userResetPasswordVM = new();
                    userResetPasswordVM.UID = uid;
                    var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    userResetPasswordVM.Code = code;

                    return View(userResetPasswordVM);
                }
            }
        }

        //POST
        //TODO Add ValidateAntiForgeryToken to avoid CORS attack
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(UserResetPasswordVM obj)
        {
            try {
                if (ModelState.IsValid)
                {
                    string UID = obj.UID;
                    string Code = obj.Code;
                    string Password = obj.Password;
                    string ConfirmPassword = obj.ConfirmPassword;

                    _logger.LogInformation("UID: {0}", UID);
                    _logger.LogInformation("Code: {0}", Code);
                    _logger.LogInformation("Password: {0}", Password);
                    _logger.LogInformation("ConfirmPassword: {0}", ConfirmPassword);

                    if (Code == null)
                    {
                        TempData["error"] = "Code is null.";
                        return RedirectToAction("Index");
                    }

                    Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Code));

                    var user = await _userManager.FindByIdAsync(UID);
                    if (user == null)
                    {
                        TempData["error"] = "User not found.";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        var result = await _userManager.ResetPasswordAsync(user, Code, Password);
                        if (result.Succeeded)
                        {
                            TempData["success"] = "User password updated successfully!";
                            return RedirectToAction("Index");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                TempData["error"] = "User password updated failed.";
            }
            return View(obj);
        }

        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            List<UserDetailsVM> userDetails = new();
            List<UserLockStatusVM> userLockStatusVMs = new();

            userLockStatusVMs = _unitOfWork.ApplicationUser.GetUsersLockStatus(false);

            foreach (var user in userLockStatusVMs)
            {
                var currentUser = await _userManager.FindByIdAsync(user.Id);
                var currentUserRole = await _userManager.GetRolesAsync(currentUser);
                
                userDetails.Add(new UserDetailsVM
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    StreetAddress = user.StreetAddress,
                    City = user.City,
                    State = user.State,
                    PostalCode = user.PostalCode,
                    RoleName = currentUserRole.FirstOrDefault() ?? "",
                    CompanyName = user.CompanyName,
                    IsLockedOut = user.IsLockedOut ? "Locked" : "Unlocked"
                });

                // _logger.LogInformation("user.Id: {0}", user.Id);
                // _logger.LogInformation("user.Name: {0}", user.Name);
                // _logger.LogInformation("user.Email: {0}", user.Email);
                // _logger.LogInformation("user.PhoneNumber: {0}", user.PhoneNumber);
                // _logger.LogInformation("user.StreetAddress: {0}", user.StreetAddress);
                // _logger.LogInformation("user.City: {0}", user.City);
                // _logger.LogInformation("user.State: {0}", user.State);
                // _logger.LogInformation("user.PostalCode: {0}", user.PostalCode);
                // _logger.LogInformation("user.RoleName: {0}", currentUserRole.FirstOrDefault() ?? "Nan");
                // _logger.LogInformation("user.CompanyName: {0}", user.CompanyName);
            }
            return Json(new { data = userDetails });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLocked()
        {
            List<UserDetailsVM> userDetails = new();
            List<UserLockStatusVM> userLockStatusVMs = new();

            userLockStatusVMs = _unitOfWork.ApplicationUser.GetUsersLockStatus(true);

            foreach (var user in userLockStatusVMs)
            {
                var currentUser = await _userManager.FindByIdAsync(user.Id);
                var currentUserRole = await _userManager.GetRolesAsync(currentUser);
                
                userDetails.Add(new UserDetailsVM
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    StreetAddress = user.StreetAddress,
                    City = user.City,
                    State = user.State,
                    PostalCode = user.PostalCode,
                    RoleName = currentUserRole.FirstOrDefault() ?? "",
                    CompanyName = user.CompanyName,
                    IsLockedOut = user.IsLockedOut ? "Locked" : "Unlocked"
                });

                // _logger.LogInformation("user.Id: {0}", user.Id);
                // _logger.LogInformation("user.Name: {0}", user.Name);
                // _logger.LogInformation("user.Email: {0}", user.Email);
                // _logger.LogInformation("user.PhoneNumber: {0}", user.PhoneNumber);
                // _logger.LogInformation("user.StreetAddress: {0}", user.StreetAddress);
                // _logger.LogInformation("user.City: {0}", user.City);
                // _logger.LogInformation("user.State: {0}", user.State);
                // _logger.LogInformation("user.PostalCode: {0}", user.PostalCode);
                // _logger.LogInformation("user.RoleName: {0}", currentUserRole.FirstOrDefault() ?? "Nan");
                // _logger.LogInformation("user.CompanyName: {0}", user.CompanyName);
            }
            return Json(new { data = userDetails });
        }

        //DELETE
        //TODO Add ValidateAntiForgeryToken to avoid CORS attack
        [HttpDelete]
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null)
            {
                return Json(new { success = false, message = "Error while deleting, uid cannot be empty." });
            }
            _logger.LogInformation("Delete account uid: {0}", id);

            var oldUser = _userManager.FindByIdAsync(id).Result;

            if (oldUser == null)
            {
                return Json(new { success = false, message = $"Error while deleting, unknown uid {id}." });
            }

            bool checkUserlocked = oldUser.IsLockedOut;
            if (checkUserlocked)
            {
                return Json(new { success = false, message = $"Error while deleting, uid {id} account has been locked." });
            }

            var userInfoRemoveResult = await _userManager.DeleteAsync(oldUser);
            _logger.LogInformation("userInfoRemoveResult: {0}", userInfoRemoveResult);

            return Json(
                new {success = true, message = "Delete Account Successful"}
            );
        }

        //POST
        //TODO Add ValidateAntiForgeryToken to avoid CORS attack
        [HttpPost]
        public async Task<IActionResult> Lock(string? id)
        {
            if (id == null)
            {
                return Json(new { success = false, message = "Error while locking, uid cannot be empty." });
            }
            _logger.LogInformation("Lock account uid: {0}", id);

            var oldUser = _userManager.FindByIdAsync(id).Result;

            if (oldUser == null)
            {
                return Json(new { success = false, message = $"Error while locking, unknown uid {id}." });
            }

            oldUser.IsLockedOut = true;

            var userInfoUpdateResult = await _userManager.UpdateAsync(oldUser);
            _logger.LogInformation("userInfoUpdateResult: {0}", userInfoUpdateResult);

            return Json(
                new {success = true, message = "Lock Account Successful"}
            );
        }

        //POST
        //TODO Add ValidateAntiForgeryToken to avoid CORS attack
        [HttpPost]
        public async Task<IActionResult> Unlock(string? id)
        {
            if (id == null)
            {
                return Json(new { success = false, message = "Error while unlocking, uid cannot be empty." });
            }
            _logger.LogInformation("Unlock account uid: {0}", id);

            var oldUser = _userManager.FindByIdAsync(id).Result;

            if (oldUser == null)
            {
                return Json(new { success = false, message = $"Error while unlocking, unknown uid {id}." });
            }

            oldUser.IsLockedOut = false;

            var userInfoUpdateResult = await _userManager.UpdateAsync(oldUser);
            _logger.LogInformation("userInfoUpdateResult: {0}", userInfoUpdateResult);

            return Json(
                new {success = true, message = "Unlock Account Successful"}
            );
        }
        #endregion
    }
}