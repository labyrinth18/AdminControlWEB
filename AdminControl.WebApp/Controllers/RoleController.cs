using AdminControl.BLL.Interfaces;
using AdminControl.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminControl.WebApp.Controllers
{
    [Authorize(Policy = "CanViewRoles")]
    public class RoleController : Controller
    {
        private readonly BLL.Interfaces.IRoleManager _roleManager;
        private readonly IUserManager _userManager;
        private readonly ILogger<RoleController> _logger;

        public RoleController(
            BLL.Interfaces.IRoleManager roleManager, 
            IUserManager userManager,
            ILogger<RoleController> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Role
        public IActionResult Index()
        {
            var roles = _roleManager.GetAllRoles();
            
            // Add user count for each role
            var roleStats = roles.Select(r => new RoleWithStatsDto
            {
                RoleID = r.RoleID,
                RoleName = r.RoleName,
                UserCount = _userManager.GetUsersByRoleCount(r.RoleID)
            }).ToList();

            ViewBag.IsAdmin = User.IsInRole("Admin");
            return View(roleStats);
        }

        // GET: Role/Details/5
        public IActionResult Details(int id)
        {
            var role = _roleManager.GetRoleById(id);
            if (role == null)
            {
                TempData["Error"] = "Роль не знайдено";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.UserCount = _userManager.GetUsersByRoleCount(id);
            ViewBag.IsAdmin = User.IsInRole("Admin");
            return View(role);
        }

        // GET: Role/Create
        [Authorize(Policy = "CanManageRoles")]
        public IActionResult Create()
        {
            return View(new RoleCreateDto());
        }

        // POST: Role/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CanManageRoles")]
        public IActionResult Create(RoleCreateDto role)
        {
            if (!ModelState.IsValid)
            {
                return View(role);
            }

            try
            {
                var createdRole = _roleManager.CreateRole(role);
                _logger.LogInformation(
                    "Role '{RoleName}' (ID: {Id}) created by '{Admin}'", 
                    createdRole.RoleName, 
                    createdRole.RoleID, 
                    User.Identity?.Name);

                TempData["Success"] = $"Роль '{createdRole.RoleName}' успішно створено";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("RoleName", ex.Message);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("RoleName", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role");
                ModelState.AddModelError("", "Виникла помилка при створенні ролі");
            }

            return View(role);
        }

        // GET: Role/Edit/5
        [Authorize(Policy = "CanManageRoles")]
        public IActionResult Edit(int id)
        {
            var role = _roleManager.GetRoleById(id);
            if (role == null)
            {
                TempData["Error"] = "Роль не знайдено";
                return RedirectToAction(nameof(Index));
            }

            var editDto = new RoleUpdateDto
            {
                RoleID = role.RoleID,
                RoleName = role.RoleName
            };

            return View(editDto);
        }

        // POST: Role/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CanManageRoles")]
        public IActionResult Edit(int id, RoleUpdateDto role)
        {
            if (id != role.RoleID)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(role);
            }

            try
            {
                var updatedRole = _roleManager.UpdateRole(role);
                _logger.LogInformation(
                    "Role '{RoleName}' (ID: {Id}) updated by '{Admin}'", 
                    updatedRole.RoleName, 
                    updatedRole.RoleID, 
                    User.Identity?.Name);

                TempData["Success"] = $"Роль '{updatedRole.RoleName}' оновлено";
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                TempData["Error"] = "Роль не знайдено";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("RoleName", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role {Id}", id);
                ModelState.AddModelError("", "Виникла помилка при оновленні ролі");
            }

            return View(role);
        }

        // GET: Role/Delete/5
        [Authorize(Policy = "CanManageRoles")]
        public IActionResult Delete(int id)
        {
            var role = _roleManager.GetRoleById(id);
            if (role == null)
            {
                TempData["Error"] = "Роль не знайдено";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.UserCount = _userManager.GetUsersByRoleCount(id);
            return View(role);
        }

        // POST: Role/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CanManageRoles")]
        public IActionResult DeleteConfirmed(int id)
        {
            var role = _roleManager.GetRoleById(id);
            if (role == null)
            {
                TempData["Error"] = "Роль не знайдено";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _roleManager.DeleteRole(id);
                _logger.LogInformation(
                    "Role '{RoleName}' (ID: {Id}) deleted by '{Admin}'", 
                    role.RoleName, 
                    role.RoleID, 
                    User.Identity?.Name);

                TempData["Success"] = $"Роль '{role.RoleName}' видалено";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting role {Id}", id);
                TempData["Error"] = "Виникла помилка при видаленні ролі";
            }

            return RedirectToAction(nameof(Index));
        }
    }

    // Helper DTO for displaying role with user count
    public class RoleWithStatsDto
    {
        public int RoleID { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public int UserCount { get; set; }
    }
}
