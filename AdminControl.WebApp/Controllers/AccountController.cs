using AdminControl.BLL.Interfaces;
using AdminControl.WebApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdminControl.WebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthManager _authManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAuthManager authManager, ILogger<AccountController> logger)
        {
            _authManager = authManager;
            _logger = logger;
        }

        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // 1. Authenticate Credentials
                var user = _authManager.Authenticate(model.Login, model.Password);

                if (user == null)
                {
                    _logger.LogWarning("Login failed: Invalid credentials for user '{Login}'", model.Login);
                    ModelState.AddModelError("", "Невірний логін або пароль");
                    return View(model);
                }

                // 2. Role-Based Restriction (Admin or Manager ONLY for Admin Panel)
                var allowedRoles = new[] { "Admin", "Manager" };
                if (!allowedRoles.Contains(user.RoleName, StringComparer.OrdinalIgnoreCase))
                {
                    _logger.LogWarning(
                        "Security Alert: User '{Login}' (ID: {Id}) with role '{Role}' attempted admin panel access.",
                        user.Login, user.UserID, user.RoleName);

                    ModelState.AddModelError("", "Доступ заборонено. Тільки для адміністраторів та менеджерів.");
                    return View(model);
                }

                // 3. Create Identity
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                    new Claim(ClaimTypes.Name, user.Login),
                    new Claim(ClaimTypes.GivenName, user.FullName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.RoleName),
                    new Claim("RoleID", user.RoleID.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(model.RememberMe ? 24 * 7 : 2)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                _logger.LogInformation("User '{Login}' logged in successfully as '{Role}'.", model.Login, user.RoleName);

                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }

                return RedirectToAction("Index", "Home");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Login failed for '{Login}': {Message}", model.Login, ex.Message);
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "System Error during login for '{Login}'", model.Login);
                ModelState.AddModelError("", "Виникла системна помилка. Спробуйте пізніше.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var userName = User.Identity?.Name ?? "Unknown";
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation("User '{Login}' logged out.", userName);
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied(string? returnUrl = null)
        {
            _logger.LogWarning(
                "Access Denied: User '{User}' attempted to access '{Url}'",
                User.Identity?.Name ?? "Anonymous", 
                returnUrl);

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
    }
}
