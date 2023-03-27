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
using System.Data;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Identity.UI.Services;

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
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;


        public UserController(
            ILogger<UserController> logger, IUnitOfWork unitOfWork,
            IWebHostEnvironment hostEnvironment, IConfiguration config,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender
        )
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
            _config = config;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _userStore = userStore;
            _emailStore = GetEmailStore();
        }

        public ActionResult Index()
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
                    oldUser.ModifiedAt = DateTime.Now;

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
                        user.ModifiedAt = DateTime.Now;
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

        public async Task<List<UserDetailsVM>> GetAllUsersDetails(string status)
        {
            List<UserDetailsVM> userDetails = new();
            List<UserLockStatusVM> userLockStatusVMs = new();
        
            userLockStatusVMs = _unitOfWork.ApplicationUser.GetUsersLockStatus(status);

            _logger.LogInformation("userLockStatusVMs.Count: {0}", userLockStatusVMs.Count);

            foreach (var user in userLockStatusVMs)
            {
                var currentUser = await _userManager.FindByIdAsync(user.Id);
                var currentUserRole = await _userManager.GetRolesAsync(currentUser);
                string _currentUserRoleName = currentUserRole.FirstOrDefault() ?? "";
                int _currentUserRoleNumber = string.IsNullOrEmpty(_currentUserRoleName) ? -1 : (
                    from role in _roleManager.Roles
                    where role.Name == _currentUserRoleName
                    select role.RoleNumber
                ).SingleOrDefault();

                _logger.LogInformation("_currentUserRoleNumber: {0}", _currentUserRoleNumber);

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
                    RoleName = _currentUserRoleName,
                    CompanyName = user.CompanyName,
                    IsLockedOut = user.IsLockedOut ? "Locked" : "Unlocked",
                    CreatedAt = user.CreatedAt,
                    ModifiedAt = user.ModifiedAt,
                    LastLoginTime = user.LastLoginTime,
                    LoginIPv4Address = user.LoginIPv4Address,
                    RoleNumber = _currentUserRoleNumber
                });
            }
            return userDetails;
        }

        public async Task<List<UserImortFormatVM>> GetUserImportFormat(string status)
        {
            List<UserImortFormatVM> userDetails = new();
            List<UserLockStatusVM> userLockStatusVMs = new();
        
            userLockStatusVMs = _unitOfWork.ApplicationUser.GetUsersLockStatus(status);

            _logger.LogInformation("userLockStatusVMs.Count: {0}", userLockStatusVMs.Count);

            foreach (var user in userLockStatusVMs)
            {
                var currentUser = await _userManager.FindByIdAsync(user.Id);
                var currentUserRole = await _userManager.GetRolesAsync(currentUser);
                
                userDetails.Add(new UserImortFormatVM
                {
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
            }
            return userDetails;
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }

        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAll(string status)
        {
            List<UserDetailsVM> userDetails = new();

            _logger.LogInformation("status: {0}", status);
            userDetails = await GetAllUsersDetails(status);
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

            int checkUserOrderCount = _unitOfWork.ApplicationUser.GetExistsOrderHeadersUsersCount(id);
            if (checkUserOrderCount > 0)
            {
                return Json(new { success = false, message = $"Error while deleting, uid {id} account has been used to place orders." });
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

            oldUser.ModifiedAt = DateTime.Now;
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

            oldUser.ModifiedAt = DateTime.Now;
            oldUser.IsLockedOut = false;

            var userInfoUpdateResult = await _userManager.UpdateAsync(oldUser);
            _logger.LogInformation("userInfoUpdateResult: {0}", userInfoUpdateResult);

            return Json(
                new {success = true, message = "Unlock Account Successful"}
            );
        }

        //POST
        //TODO Add ValidateAntiForgeryToken to avoid CORS attack
        [HttpPost]
        public async Task<IActionResult> BulkCreate(IFormFile uploadFile)
        {
            try {
                if (uploadFile == null)
                {
                    throw new Exception("Please select a file to upload.");
                }

                var _common = new Common(_config);
                string? uploads = "";
                uploads = _common.GetUploadFilesStoragePath();
                _logger.LogInformation("Document upload path: {0}", uploads);
                string newFileName = Guid.NewGuid().ToString();
                string oldFileName = Path.GetFileName(uploadFile.FileName);
                string fileExtension = '.' + oldFileName.Split('.').Last();
                string? extension = Path.GetExtension(uploadFile.FileName);
                int _userCreatedCount = 0;

                _logger.LogInformation("Received Document File extension: {0}", fileExtension);
                bool _IsContainsExtension = FileUploadTool.IsContainsExtension(fileExtension, "import");

                if (!_IsContainsExtension)
                {
                    throw new Exception("Upload file failed, because file extension is not allowed.");
                }

                // TODO If storage path does not exist, then create it.
                FileTool.CheckAndCreateDirectory(uploads);

                //TODO Storage user upload file to server
                bool _IsUploadSuccess = FileUploadTool.UploadImage(uploadFile, newFileName, extension, uploads);

                if (!_IsUploadSuccess)
                {
                    throw new Exception("Upload file failed, because save file to server local failed.");
                }

                List<List<Dictionary<string, object>>> Results = new List<List<Dictionary<string, object>>>();

                string? filePath = Path.Combine(uploads, newFileName + extension);

                if (filePath != null)
                {
                    Results = FileReadTool.ReadExcelFile(filePath, false, 1);

                    if (Results.Count == 0)
                    {
                        throw new Exception("Upload file failed, because read file content failed.");
                    }

                    foreach (var sheet in Results)
                    {
                        foreach (var rows in sheet)
                        {
                            bool _allowCreateUser = true;
                            var user = CreateUser();
                            
                            string _Email = rows["Column0"].ToString() ?? "";
                            string _Name = rows["Column1"].ToString() ?? "";
                            string _PhoneNumber = rows["Column2"].ToString() ?? "";
                            string _StreetAddress = rows["Column3"].ToString() ?? "";
                            string _City = rows["Column4"].ToString() ?? "";
                            string _State = rows["Column5"].ToString() ?? "";
                            string _PostalCode = rows["Column6"].ToString() ?? "";
                            string _Password = rows["Column7"].ToString() ?? "";
                            string _RoleName = rows["Column8"].ToString() ?? "";
                            string _ComanyName = rows["Column9"].ToString() ?? "";
                            int _RoleNumber = 0;

                            if (string.IsNullOrEmpty(_Email))
                            {
                                throw new Exception("Email is required.");
                            }
                            else
                            {
                                user.Email = _Email;
                            }

                            if (string.IsNullOrEmpty(_Name))
                            {
                                throw new Exception("Name is required.");
                            }
                            else
                            {
                                user.Name = _Name;
                            }

                            if (string.IsNullOrEmpty(_PhoneNumber))
                            {
                                throw new Exception("PhoneNumber is required.");
                            }
                            else
                            {
                                user.PhoneNumber = _PhoneNumber;
                            }

                            if (string.IsNullOrEmpty(_StreetAddress))
                            {
                                throw new Exception("StreetAddress is required.");
                            }
                            else
                            {
                                user.StreetAddress = _StreetAddress;
                            }

                            if (string.IsNullOrEmpty(_City))
                            {
                                throw new Exception("City is required.");
                            }
                            else
                            {
                                user.City = _City;
                            }

                            if (string.IsNullOrEmpty(_State))
                            {
                                throw new Exception("State is required.");
                            }
                            else
                            {
                                user.State = _State;
                            }

                            if (string.IsNullOrEmpty(_PostalCode))
                            {
                                throw new Exception("PostalCode is required.");
                            }
                            else
                            {
                                user.PostalCode = _PostalCode;
                            }

                            if (string.IsNullOrEmpty(_Password))
                            {
                                throw new Exception("Password is required.");
                            }

                            if (string.IsNullOrEmpty(_RoleName))
                            {
                                throw new Exception("RoleName is required.");
                            }

                            bool _EmailIsExists = _unitOfWork.ApplicationUser.IsExists(
                                includeProperties: "Email", user.Email
                            );
                            bool _NameIsExists = _unitOfWork.ApplicationUser.IsExists(
                                includeProperties: "Name", user.Name
                            );
                            bool _PhoneNumberIsExists = _unitOfWork.ApplicationUser.IsExists(
                                includeProperties: "PhoneNumber", user.PhoneNumber
                            );
                            bool _RoleNameIsExists = _unitOfWork.ApplicationRole.IsExists(
                                includeProperties: "Name", _RoleName
                            );

                            if (!_RoleNameIsExists)
                            {
                                throw new Exception("RoleName is not exists.");
                            }
                            else
                            {
                                _RoleNumber = _roleManager.Roles.SingleOrDefault(r => r.Name == _RoleName).RoleNumber;
                            }

                            if (_RoleNumber == 0)
                            {
                                throw new Exception("RoleName is not exists.");
                            }

                            if (_RoleNumber == 4 && string.IsNullOrEmpty(_ComanyName))
                            {
                                throw new Exception("ComanyName is required.");
                            }
                            else if (_RoleNumber == 4 && !string.IsNullOrEmpty(_ComanyName))
                            {
                                bool _ComanyNameIsExists = _unitOfWork.Company.IsExists(
                                    includeProperties: "Name", _ComanyName
                                );
                                if (!_ComanyNameIsExists)
                                {
                                    throw new Exception("ComanyName is not exists.");
                                }
                                user.CompanyId = _unitOfWork.Company.GetFirstOrDefault(u => u.Name == _ComanyName).Id;
                            }

                            if (_EmailIsExists)
                            {
                                // throw new Exception("Email is exists.");
                                _logger.LogInformation($"Email {_Email} is exists.");
                                _allowCreateUser = false;
                            }

                            if (_NameIsExists)
                            {
                                // throw new Exception("Name is exists.");
                                _logger.LogInformation($"Name {_Name} is exists.");
                                _allowCreateUser = false;
                            }

                            if (_PhoneNumberIsExists)
                            {
                                // throw new Exception("PhoneNumber is exists.");
                                _logger.LogInformation($"PhoneNumber {_PhoneNumber} is exists.");
                                _allowCreateUser = false;
                            }

                            if (_allowCreateUser)
                            {
                                user.CreatedAt = DateTime.Now;
                                await _userStore.SetUserNameAsync(user, user.Email, CancellationToken.None);
                                await _emailStore.SetEmailAsync(user, user.Email, CancellationToken.None);

                                var userCreatedResult = await _userManager.CreateAsync(user, _Password);

                                if (userCreatedResult.Succeeded)
                                {
                                    var userGivenRoleResult = await _userManager.AddToRoleAsync(user, _RoleName);
                                    if (!userGivenRoleResult.Succeeded)
                                    {
                                        throw new Exception($"Given user '{user.Name}' role failed.");
                                    }
                                    _logger.LogInformation($"User '{user.Name}' created a new account with password.");
                                    _userCreatedCount++;
                                }
                                else
                                {
                                    throw new Exception($"Create user '{user.Name}' failed.");
                                }

                                _logger.LogInformation(
                                    "Name: {0}, Email: {1}, PhoneNumber: {2}, StreetAddress: {3}, City: {4}, State: {5}, PostalCode: {6}, RoleName: {7}, ComanyName: {8}, CompanyId: {9}",
                                    user.Name, user.Email, user.PhoneNumber, user.StreetAddress, user.City, user.State, user.PostalCode, _RoleName, _ComanyName, user.CompanyId
                                );
                            }
                            else
                            {
                                user = new();
                                _logger.LogInformation($"User '{_Email}' or '{_Name}' or '{_PhoneNumber}' is exists.");
                            }
                        }
                    }
                }

                if (_userCreatedCount > 0)
                {
                    _logger.LogInformation("UserController.BulkCreate: {0}", "Bulk create successful!");
                    return Json(
                        new {success = true, message = "Bulk create successful!"}
                    );
                }
                else
                {
                    _logger.LogInformation("UserController.BulkCreate: {0}", "No user created.");
                    return Json(
                        new {success = false, message = "No user created."}
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("UserController.BulkCreate Message: {0}", ex.Message);
                _logger.LogError("UserController.BulkCreate StackTrace: {0}", ex.StackTrace);
                return Json(
                    new {success = false, message = ex.Message}
                );
            }
        }

        [HttpGet]
        public async Task<IActionResult> ImportFormat()
        {
            try
            {
                List<UserImortFormatVM> userImportFormat = new();

                userImportFormat = await GetUserImportFormat("all");

                if (userImportFormat.Count == 0)
                {
                    throw new Exception("No data to export.");
                }

                DataSet ds = new DataSet();
                ds = _unitOfWork.ApplicationUser.ConvertToDataSet(userImportFormat, includeProperty: "RoleName");

                string fileName = DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss") + "_UsersImportFormat.xlsx";

                return File(
                    FileExportTool.ExportToExcelDownload(ds),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName
                );
            }
            catch (Exception ex)
            {
                _logger.LogError("CategoryController.ExportDetails: {0}", ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportDetails()
        {
            try
            {
                List<UserDetailsVM> userDetails = new();

                userDetails = await GetAllUsersDetails("all");

                if (userDetails.Count == 0)
                {
                    throw new Exception("No data to export.");
                }

                DataSet ds = new DataSet();
                ds = _unitOfWork.ApplicationUser.ConvertToDataSet(userDetails, includeProperty: "RoleNumber");

                string fileName = DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss") + "_UsersDetails.xlsx";

                return File(
                    FileExportTool.ExportToExcelDownload(ds),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName
                );
            }
            catch (Exception ex)
            {
                _logger.LogError("CategoryController.ExportDetails: {0}", ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }
        #endregion
    }
}