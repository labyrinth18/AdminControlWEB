using System.ComponentModel.DataAnnotations;
using AdminControl.DTO;

namespace AdminControl.WebApp.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public string? Message { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Логін є обов'язковим")]
        [Display(Name = "Логін")]
        public string Login { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль є обов'язковим")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Запам'ятати мене")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }

    public class DashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers => TotalUsers - ActiveUsers;
        public int TotalRoles { get; set; }
        public Dictionary<string, int> UsersByRole { get; set; } = [];
        public List<RecentActivityItem> RecentActivity { get; set; } = [];
        
        // Statistics Cards
        public decimal ActiveUsersPercentage => TotalUsers > 0 
            ? Math.Round((decimal)ActiveUsers / TotalUsers * 100, 1) 
            : 0;
    }

    public class RecentActivityItem
    {
        public string Action { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Icon { get; set; } = "bi-clock";
        public string ColorClass { get; set; } = "text-secondary";
    }

    public class UserListViewModel
    {
        public List<UserDto> Users { get; set; } = [];
        public string? SearchTerm { get; set; }
        public int? RoleFilter { get; set; }
        public bool? ActiveFilter { get; set; }
        public List<RoleDto> AvailableRoles { get; set; } = [];
        
        // Pagination
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    }

    public class UserCreateViewModel
    {
        public UserCreateDto User { get; set; } = new();
        public List<RoleDto> AvailableRoles { get; set; } = [];
    }

    public class UserEditViewModel
    {
        public UserUpdateDto User { get; set; } = new();
        public string Login { get; set; } = string.Empty;
        public List<RoleDto> AvailableRoles { get; set; } = [];
        public bool CanChangeRole { get; set; } = true;
    }

    public class UserDetailsViewModel
    {
        public UserDto User { get; set; } = new();
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanChangeStatus { get; set; }
    }
}
