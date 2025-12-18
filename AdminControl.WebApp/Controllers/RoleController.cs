using AutoMapper;
using AdminControl.BLL.Interfaces;
using AdminControl.DTO;
using AdminControl.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminControl.WebApp.Controllers
{
    [Authorize(Roles = "Admin,Адміністратор")]
    public class RoleController : Controller
    {
        private readonly IRoleManager _manager;
        private readonly IMapper _mapper;
        private readonly ILogger<RoleController> _logger;

        public RoleController(IRoleManager manager, IMapper mapper, ILogger<RoleController> logger)
        {
            _manager = manager;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: RoleController
        public ActionResult Index()
        {
            _logger.LogInformation("Перегляд списку ролей користувачем {User}", User.Identity?.Name);
            var roles = _manager.GetAllRoles();
            return View(roles);
        }

        // GET: RoleController/Details/5
        public ActionResult Details(int id)
        {
            var role = _manager.GetRoleById(id);
            if (role == null)
            {
                _logger.LogWarning("Роль з ID {RoleId} не знайдено", id);
                return NotFound();
            }
            return View(role);
        }

        // GET: RoleController/Create
        public ActionResult Create()
        {
            return View(new RoleCreateDto());
        }

        // POST: RoleController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RoleCreateDto role)
        {
            if (!ModelState.IsValid)
            {
                return View(role);
            }

            try
            {
                _manager.CreateRole(role);
                _logger.LogInformation("Користувач {User} створив роль {RoleName}", User.Identity?.Name, role.RoleName);
                TempData["Success"] = "Роль успішно створено";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Помилка при створенні ролі {RoleName}", role.RoleName);
                ModelState.AddModelError("RoleName", ex.Message);
                return View(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Неочікувана помилка при створенні ролі {RoleName}", role.RoleName);
                ModelState.AddModelError("", "Виникла помилка при створенні ролі");
                return View(role);
            }
        }

        // GET: RoleController/Edit/5
        public ActionResult Edit(int id)
        {
            var role = _manager.GetRoleById(id);
            if (role == null)
            {
                return NotFound();
            }

            var editDto = new RoleUpdateDto
            {
                RoleID = role.RoleID,
                RoleName = role.RoleName
            };
            return View(editDto);
        }

        // POST: RoleController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, RoleUpdateDto role)
        {
            if (id != role.RoleID)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(role);
            }

            try
            {
                _manager.UpdateRole(role);
                _logger.LogInformation("Користувач {User} оновив роль {RoleId}", User.Identity?.Name, id);
                TempData["Success"] = "Роль успішно оновлено";
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Помилка при оновленні ролі {RoleId}", id);
                ModelState.AddModelError("RoleName", ex.Message);
                return View(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Неочікувана помилка при оновленні ролі {RoleId}", id);
                ModelState.AddModelError("", "Виникла помилка при оновленні ролі");
                return View(role);
            }
        }

        // GET: RoleController/Delete/5
        public ActionResult Delete(int id)
        {
            var role = _manager.GetRoleById(id);
            if (role == null)
            {
                return NotFound();
            }
            return View(role);
        }

        // POST: RoleController/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                _manager.DeleteRole(id);
                _logger.LogInformation("Користувач {User} видалив роль {RoleId}", User.Identity?.Name, id);
                TempData["Success"] = "Роль успішно видалено";
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при видаленні ролі {RoleId}", id);
                TempData["Error"] = "Виникла помилка при видаленні ролі";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
