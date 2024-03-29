// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using HoshiBook.Models;


using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.Ocsp;

namespace HoshiBookWeb.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager,
            UserManager<ApplicationUser> userManager,
            ILogger<LoginModel> logger
        )
        {
            _signInManager = signInManager;
            _roleManager = roleManager;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
 
            if (ModelState.IsValid)
            {
                var loginUser = await _userManager.FindByEmailAsync(Input.Email);

                if (loginUser == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }

                _logger.LogInformation($"User: {loginUser.UserName} - {loginUser.Email} - {loginUser.Id}- {loginUser.IsLockedOut}");
                bool _IsLockedOut = loginUser.IsLockedOut;

                string remoteIPv4Address = HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                if (Request.Headers.ContainsKey("X-Forwarded-For"))
                {
                    remoteIPv4Address = Request.Headers["X-Forwarded-For"];
                }

                _logger.LogInformation($"Normal Login remoteIPv4Address: {remoteIPv4Address}");

                loginUser.LoginIPv4Address = remoteIPv4Address;
                loginUser.LastTryLoginTime = DateTime.Now;

                var updateLoginInfoResult = await _userManager.UpdateAsync(loginUser);
                bool _IsUpdateLoginInfoResult = updateLoginInfoResult.Succeeded;
                _logger.LogInformation($"Normal Login User LoginIPv4Address And LastLoginTime Update Result: {_IsUpdateLoginInfoResult}");

                if (_IsLockedOut)
                {
                    _logger.LogWarning("User account locked out, because the account has been manager locked.");
                    return RedirectToPage("./UserLockout");
                }

                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);
                var info = await _signInManager.GetExternalLoginInfoAsync();

                if (result.Succeeded)
                {
                    // _logger.LogInformation($"RememberMe: {Input.RememberMe}");
                    ExternalLogins = new List<AuthenticationScheme>();
                    var hasExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).Any();
                    // _logger.LogInformation($"hasExternalLogins: {hasExternalLogins}");
                    // _logger.LogInformation($"Normal Logins: {ExternalLogins.Count}");
                    loginUser.LoginIPv4Address = remoteIPv4Address;
                    loginUser.LastLoginTime = DateTime.Now;
                    updateLoginInfoResult = await _userManager.UpdateAsync(loginUser);
                    _IsUpdateLoginInfoResult = updateLoginInfoResult.Succeeded;
                    _logger.LogInformation($"Normal Login User LoginIPv4Address And LastLoginTime Update Result: {_IsUpdateLoginInfoResult}");
                    return LocalRedirect(returnUrl);
                }
                else
                {
                    loginUser.LastTryLoginTime = DateTime.Now;
                    _logger.LogInformation($"AccessFailedCount: {loginUser.AccessFailedCount}");
                    updateLoginInfoResult = await _userManager.UpdateAsync(loginUser);
                    _IsUpdateLoginInfoResult = updateLoginInfoResult.Succeeded;
                    _logger.LogInformation($"Normal Login User LastTryLoginTime Update Result: {_IsUpdateLoginInfoResult}");
                }

                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out, because login error times limit exceeded.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}