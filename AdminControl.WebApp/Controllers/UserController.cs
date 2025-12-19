using AdminControl.BLL.Interfaces;
using AdminControl.DTO;
using AdminControl.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdminControl.WebApp.Controllers
{
    [Authorize(Policy = "CanViewUsers")]
    public class UserController : Controller
    {
        private readonly IUserManager _userManager;
        private readonly IRoleManager _roleManager;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IUserManager userManager,
            IRoleManager roleManager,
            ILogger<UserController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        #region Helpers

        private bool IsAdmin => User.IsInRole("Admin");
        private bool IsManager => User.IsInRole("Manager");
        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        private List<RoleDto> GetAvailableRoles()
        {
            var roles = _roleManager.GetAllRoles();

            // Manager cannot assign Admin role
            if (IsManager && !IsAdmin)
            {
                roles = roles.Where(r => !r.RoleName.Equals("Admin", StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return roles;
        }

        #endregion

        // GET: User
        public IActionResult Index(string? search, int? roleId, bool? isActive, int page = 1)
        {
            var allUsers = _userManager.GetAllUsers();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(search))
            {
                allUsers = allUsers.Where(u =>
                    u.Login.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    u.Email.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    u.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    u.LastName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    u.FullName.Contains(search, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            if (roleId.HasValue)
            {
                allUsers = allUsers.Where(u => u.RoleID == roleId.Value).ToList();
            }

            if (isActive.HasValue)
            {
                allUsers = allUsers.Where(u => u.IsActive == isActive.Value).ToList();
            }

            // Pagination
            const int pageSize = 10;
            var totalItems = allUsers.Count;
            var users = allUsers
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var viewModel = new UserListViewModel
            {
                Users = users,
                SearchTerm = search,
                RoleFilter = roleId,
                ActiveFilter = isActive,
                AvailableRoles = _roleManager.GetAllRoles(),
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            return View(viewModel);
        }

        // GET: User/Details/5
        public IActionResult Details(int id)
        {
            var user = _userManager.GetUserById(id);
            if (user == null)
            {
                TempData["Error"] = "Користувача не знайдено";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new UserDetailsViewModel
            {
                User = user,
                CanEdit = IsAdmin || (IsManager && !user.RoleName.Equals("Admin", StringComparison.OrdinalIgnoreCase)),
                CanDelete = IsAdmin,
                CanChangeStatus = IsAdmin || (IsManager && !user.RoleName.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            };

            return View(viewModel);
        }

        // GET: User/Create
        [Authorize(Policy = "CanManageUsers")]
        public IActionResult Create()
        {
            var viewModel = new UserCreateViewModel
            {
                User = new UserCreateDto { IsActive = true },
                AvailableRoles = GetAvailableRoles()
            };

            return View(viewModel);
        }

        // POST: User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CanManageUsers")]
        public IActionResult Create(UserCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableRoles = GetAvailableRoles();
                return View(model);
            }

            try
            {
                var createdUser = _userManager.CreateUser(model.User);
                _logger.LogInformation(
                    "User '{Login}' (ID: {Id}) created by '{Admin}'",
                    createdUser.Login,
                    createdUser.UserID,
                    User.Identity?.Name);

                TempData["Success"] = $"Користувача '{createdUser.FullName}' успішно створено";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                ModelState.AddModelError("", "Виникла помилка при створенні користувача");
            }

            model.AvailableRoles = GetAvailableRoles();
            return View(model);
        }

        // GET: User/Edit/5
        [Authorize(Policy = "CanEditUsers")]
        public IActionResult Edit(int id)
        {
            var user = _userManager.GetUserById(id);
            if (user == null)
            {
                TempData["Error"] = "Користувача не знайдено";
                return RedirectToAction(nameof(Index));
            }

            // Manager cannot edit Admin users
            if (IsManager && !IsAdmin && user.RoleName.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Ви не маєте прав редагувати адміністраторів";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new UserEditViewModel
            {
                User = new UserUpdateDto
                {
                    UserID = user.UserID,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address,
                    Gender = user.Gender,
                    RoleID = user.RoleID,
                    IsActive = user.IsActive
                },
                Login = user.Login,
                AvailableRoles = GetAvailableRoles(),
                CanChangeRole = IsAdmin // Only Admin can change roles
            };

            return View(viewModel);
        }

        // POST: User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CanEditUsers")]
        public IActionResult Edit(int id, UserEditViewModel model)
        {
            if (id != model.User.UserID)
            {
                return BadRequest();
            }

            var existingUser = _userManager.GetUserById(id);
            if (existingUser == null)
            {
                TempData["Error"] = "Користувача не знайдено";
                return RedirectToAction(nameof(Index));
            }

            // Manager cannot edit Admin users
            if (IsManager && !IsAdmin && existingUser.RoleName.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Ви не маєте прав редагувати адміністраторів";
                return RedirectToAction(nameof(Index));
            }

            // Manager cannot change roles
            if (IsManager && !IsAdmin)
            {
                model.User.RoleID = existingUser.RoleID;
            }

            if (!ModelState.IsValid)
            {
                model.Login = existingUser.Login;
                model.AvailableRoles = GetAvailableRoles();
                model.CanChangeRole = IsAdmin;
                return View(model);
            }

            try
            {
                var updatedUser = _userManager.UpdateUser(model.User);
                _logger.LogInformation(
                    "User '{Login}' (ID: {Id}) updated by '{Admin}'",
                    updatedUser.Login,
                    updatedUser.UserID,
                    User.Identity?.Name);

                TempData["Success"] = $"Користувача '{updatedUser.FullName}' успішно оновлено";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (KeyNotFoundException)
            {
                TempData["Error"] = "Користувача не знайдено";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {Id}", id);
                ModelState.AddModelError("", "Виникла помилка при оновленні користувача");
            }

            model.Login = existingUser.Login;
            model.AvailableRoles = GetAvailableRoles();
            model.CanChangeRole = IsAdmin;
            return View(model);
        }

        // GET: User/Delete/5
        [Authorize(Policy = "CanManageUsers")]
        public IActionResult Delete(int id)
        {
            var user = _userManager.GetUserById(id);
            if (user == null)
            {
                TempData["Error"] = "Користувача не знайдено";
                return RedirectToAction(nameof(Index));
            }

            // Prevent self-deletion
            if (user.UserID == CurrentUserId)
            {
                TempData["Error"] = "Ви не можете видалити власний обліковий запис";
                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CanManageUsers")]
        public IActionResult DeleteConfirmed(int id)
        {
            var user = _userManager.GetUserById(id);
            if (user == null)
            {
                TempData["Error"] = "Користувача не знайдено";
                return RedirectToAction(nameof(Index));
            }

            // Prevent self-deletion
            if (user.UserID == CurrentUserId)
            {
                TempData["Error"] = "Ви не можете видалити власний обліковий запис";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _userManager.DeleteUser(id);
                _logger.LogInformation(
                    "User '{Login}' (ID: {Id}) deleted by '{Admin}'",
                    user.Login,
                    user.UserID,
                    User.Identity?.Name);

                TempData["Success"] = $"Користувача '{user.FullName}' видалено";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {Id}", id);
                TempData["Error"] = "Виникла помилка при видаленні користувача";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: User/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CanEditUsers")]
        public IActionResult ToggleStatus(int id)
        {
            var user = _userManager.GetUserById(id);
            if (user == null)
            {
                TempData["Error"] = "Користувача не знайдено";
                return RedirectToAction(nameof(Index));
            }

            // Manager cannot change Admin status
            if (IsManager && !IsAdmin && user.RoleName.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Ви не маєте прав змінювати статус адміністраторів";
                return RedirectToAction(nameof(Index));
            }

            // Prevent self-deactivation
            if (user.UserID == CurrentUserId && user.IsActive)
            {
                TempData["Error"] = "Ви не можете деактивувати власний обліковий запис";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                if (user.IsActive)
                {
                    _userManager.DeactivateUser(id);
                    _logger.LogInformation("User '{Login}' deactivated by '{Admin}'", user.Login, User.Identity?.Name);
                    TempData["Success"] = $"Користувача '{user.FullName}' деактивовано";
                }
                else
                {
                    _userManager.ActivateUser(id);
                    _logger.LogInformation("User '{Login}' activated by '{Admin}'", user.Login, User.Identity?.Name);
                    TempData["Success"] = $"Користувача '{user.FullName}' активовано";
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling status for user {Id}", id);
                TempData["Error"] = "Виникла помилка при зміні статусу";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}