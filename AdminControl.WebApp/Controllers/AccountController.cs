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

        // GET: Account/Login
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        // POST: Account/Login
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
                var user = _authManager.Authenticate(model.Login, model.Password);

                if (user == null)
                {
                    _logger.LogWarning("Невдала спроба входу для користувача {Login}", model.Login);
                    ModelState.AddModelError("", "Невірний логін або пароль");
                    return View(model);
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("Спроба входу деактивованого користувача {Login}", model.Login);
                    ModelState.AddModelError("", "Ваш обліковий запис деактивовано. Зверніться до адміністратора.");
                    return View(model);
                }

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

                _logger.LogInformation("Користувач {Login} успішно увійшов у систему", model.Login);

                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }

                return RedirectToAction("Index", "Home");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Невдала спроба входу для користувача {Login}: {Message}", model.Login, ex.Message);
                ModelState.AddModelError("", "Невірний логін або пароль");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка автентифікації для користувача {Login}", model.Login);
                ModelState.AddModelError("", "Виникла помилка при вході в систему");
                return View(model);
            }
        }

        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var userName = User.Identity?.Name ?? "Unknown";
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation("Користувач {Login} вийшов із системи", userName);
            return RedirectToAction("Login");
        }

        // GET: Account/AccessDenied
        [AllowAnonymous]
        public IActionResult AccessDenied(string? returnUrl = null)
        {
            _logger.LogWarning("Відмовлено в доступі користувачу {User} до ресурсу {Url}", 
                User.Identity?.Name ?? "Anonymous", returnUrl);
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
    }
}
