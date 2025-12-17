// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using FitnessApp.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace FitnessApp.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly UserManager<AppUser> _userManager; // 1. Değişken Tanımlı (DOĞRU)

        // 2. CONSTRUCTOR (YAPICI METOT) GÜNCELLENDİ
        public LoginModel(SignInManager<AppUser> signInManager,
                          ILogger<LoginModel> logger,
                          UserManager<AppUser> userManager) // <-- BURASI EKSİKTİ, EKLENDİ!
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager; // Artık hata vermez
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

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
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Kullanıcı giriş yaptı.");

                    // --- 3. AKILLI YÖNLENDİRME KODLARI EKLENDİ ---
                    var user = await _userManager.FindByEmailAsync(Input.Email);

                    if (user != null)
                    {
                        if (await _userManager.IsInRoleAsync(user, "Admin"))
                        {
                            // Admin ise -> Yönetim Paneli
                            return RedirectToAction("Index", "Admin", new { area = "" });
                        }
                        else if (await _userManager.IsInRoleAsync(user, "Member"))
                        {
                            // Üye ise -> Randevular
                            return RedirectToAction("Index", "Appointments", new { area = "" });
                        }
                    }
                    // ---------------------------------------------

                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi.");
                    return Page();
                }
            }

            return Page();
        }
    }
}