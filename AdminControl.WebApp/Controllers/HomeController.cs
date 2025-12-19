using AdminControl.BLL.Interfaces;
using AdminControl.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AdminControl.WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUserManager _userManager;
        private readonly BLL.Interfaces.IRoleManager _roleManager;

        public HomeController(
            ILogger<HomeController> logger,
            IUserManager userManager,
            BLL.Interfaces.IRoleManager roleManager)
        {
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            // If not authenticated, redirect to login
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Login", "Account");
            }

            // Check if user has access to dashboard
            if (!User.IsInRole("Admin") && !User.IsInRole("Manager"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            // Build Dashboard ViewModel
            var viewModel = new DashboardViewModel();

            try
            {
                viewModel.TotalUsers = _userManager.GetTotalUsersCount();
                viewModel.ActiveUsers = _userManager.GetActiveUsersCount();
                viewModel.TotalRoles = _roleManager.GetAllRoles().Count;
                viewModel.UsersByRole = _userManager.GetUsersCountByRole();

                // Mock recent activity (in real app, this would come from AdminActionLog)
                viewModel.RecentActivity =
                [
                    new RecentActivityItem
                    {
                        Action = "Увійшли в систему",
                        User = User.Identity?.Name ?? "Unknown",
                        Date = DateTime.Now,
                        Icon = "bi-box-arrow-in-right",
                        ColorClass = "text-success"
                    }
                ];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");
                TempData["Error"] = "Помилка завантаження даних dashboard";
            }

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel 
            { 
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier 
            });
        }
    }
}
