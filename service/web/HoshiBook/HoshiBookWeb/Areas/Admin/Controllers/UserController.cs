using HoshiBook.DataAccess.Repository.IRepository;
using HoshiBookWeb.Tools;
using HoshiBook.Utility;
using HoshiBook.Models.ViewModels;
using HoshiBookWeb.Tools.CommonTool;
using HoshiBook.Models;


using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace HoshiBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
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

        public IActionResult Index()
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

                UserEditVM userEditVM = new()
                {
                    ApplicationUser = new(),
                    CompanyList = _unitOfWork.Company.GetAll().Select(
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
                    string? UID = obj.ApplicationUser.Id;
                    string? Email = obj.ApplicationUser.Email;
                    string? Name = obj.ApplicationUser.Name;
                    string? PhoneNumber = obj.ApplicationUser.PhoneNumber;
                    string? StreetAddress = obj.ApplicationUser.StreetAddress;
                    string? City = obj.ApplicationUser.City;
                    string? State = obj.ApplicationUser.State;
                    string? PostalCode = obj.ApplicationUser.PostalCode;
                    // string RoleId = obj.RoleId ?? "";
                    // string CompanyId = obj.CompanyId ?? "";
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
                    // oldUser.CompanyId = CompanyId;

                    // oldUser = new ApplicationUser {
                    //     Email = Email,
                    //     Name = Name,
                    //     PhoneNumber = PhoneNumber,
                    //     StreetAddress = StreetAddress,
                    //     City = City,
                    //     State = State,
                    //     PostalCode = PostalCode,
                    //     CompanyId = CompanyId,
                    //     PasswordHash = oldUser2.PasswordHash,
                    // };
                    var userInfoUpdateResult = await _userManager.UpdateAsync(oldUser);
                    _logger.LogInformation("userInfoUpdateResult: {0}", userInfoUpdateResult);

                    TempData["success"] = "User updated successfully!";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                TempData["error"] = "User updated failed.";
            }
            return View(obj);
        }

        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userList = _unitOfWork.ApplicationUser.GetAll();
            var userCompany = _unitOfWork.Company.GetAll();
            List<UserDetailsVM> userDetails = new();

            var userInfoFilter = (
                from user in userList
                join company in userCompany
                on user.CompanyId equals company.Id
                into groupjoin from b in groupjoin.DefaultIfEmpty()
                select new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    user.PhoneNumber,
                    user.StreetAddress,
                    user.City,
                    user.State,
                    user.PostalCode,
                    CompanyName = b == null ? "" : b.Name
                }
            );

            foreach (var user in userInfoFilter)
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
                    CompanyName = user.CompanyName
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

        //POST
        //TODO Add ValidateAntiForgeryToken to avoid CORS attack
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var obj = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);

            if (obj == null)
            {
                return Json(
                    new {success = false, message = "Error while deleting"}
                );
            }

            string? oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, obj.ImageUrl.TrimStart('\\'));
            // TODO Check product image URL does exists.
            if (obj.ImageUrl != null)
            {
                // TODO If does exists, then obtain full storage path.
                FileTool.CheckFileExistsAndRemove(oldImagePath);
            }
            
            _unitOfWork.Product.Remove(obj);
            _unitOfWork.Save();

            return Json(
                new {success = true, message = "Delete Successful"}
            );
        }
        #endregion
    }
}