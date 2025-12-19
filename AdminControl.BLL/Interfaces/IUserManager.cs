using AdminControl.DTO;

namespace AdminControl.BLL.Interfaces
{
    public interface IUserManager
    {
        // Read operations
        List<UserDto> GetAllUsers();
        UserDto? GetUserById(int userId);
        UserDto? GetUserByLogin(string login);
        
        // Write operations
        UserDto CreateUser(UserCreateDto user);
        UserDto UpdateUser(UserUpdateDto user);
        bool DeleteUser(int userId);
        
        // Status operations
        bool ActivateUser(int userId);
        bool DeactivateUser(int userId);
        
        // Validation helpers
        bool LoginExists(string login);
        bool EmailExists(string email, int? excludeUserId = null);
        
        // Statistics for Dashboard
        int GetTotalUsersCount();
        int GetActiveUsersCount();
        int GetUsersByRoleCount(int roleId);
        Dictionary<string, int> GetUsersCountByRole();
    }
}
