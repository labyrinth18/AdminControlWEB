using AutoMapper;
using AdminControl.BLL.Interfaces;
using AdminControl.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminControl.WebApp.Controllers
{
    // Updated to include 'Manager' based on new requirements
    [Authorize(Roles = "Admin,Manager")]
    public class RoleController : Controller
    {
        private readonly IRoleManager _manager;
        // IMapper is injected to ensure Arch consistency if we need ViewModels later
        private readonly IMapper _mapper;
        private readonly ILogger<RoleController> _logger;

        public RoleController(IRoleManager manager, IMapper mapper, ILogger<RoleController> logger)
        {
            _manager = manager;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: Role
        public ActionResult Index()
        {
            var roles = _manager.GetAllRoles();
            return View(roles);
        }

        // GET: Role/Details/5
        public ActionResult Details(int id)
        {
            var role = _manager.GetRoleById(id);
            if (role == null) return NotFound();
            return View(role);
        }

        // GET: Role/Create
        public ActionResult Create()
        {
            return View(new RoleCreateDto());
        }

        // POST: Role/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RoleCreateDto role)
        {
            if (!ModelState.IsValid) return View(role);

            try
            {
                _manager.CreateRole(role);
                _logger.LogInformation("Role '{RoleName}' created by '{User}'", role.RoleName, User.Identity?.Name);
                TempData["Success"] = "Роль успішно створено";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("RoleName", ex.Message); // Role exists error
                return View(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role");
                ModelState.AddModelError("", "Помилка створення ролі.");
                return View(role);
            }
        }

        // GET: Role/Edit/5
        public ActionResult Edit(int id)
        {
            var role = _manager.GetRoleById(id);
            if (role == null) return NotFound();

            // Mapping DTO to UpdateDTO for the view
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
        public ActionResult Edit(int id, RoleUpdateDto role)
        {
            if (id != role.RoleID) return BadRequest();
            if (!ModelState.IsValid) return View(role);

            try
            {
                _manager.UpdateRole(role);
                _logger.LogInformation("Role ID {Id} updated by '{User}'", id, User.Identity?.Name);
                TempData["Success"] = "Роль оновлено";
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException) { return NotFound(); }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("RoleName", ex.Message);
                return View(role);
            }
        }

        // GET: Role/Delete/5
        public ActionResult Delete(int id)
        {
            var role = _manager.GetRoleById(id);
            if (role == null) return NotFound();
            return View(role);
        }

        // POST: Role/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                _manager.DeleteRole(id);
                _logger.LogInformation("Role ID {Id} deleted by '{User}'", id, User.Identity?.Name);
                TempData["Success"] = "Роль видалено";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting role");
                TempData["Error"] = "Не вдалося видалити роль (можливо, вона використовується).";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}