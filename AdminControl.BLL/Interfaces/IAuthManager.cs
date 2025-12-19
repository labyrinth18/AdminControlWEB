using AdminControl.DTO;

namespace AdminControl.BLL.Interfaces
{
    public interface IAuthManager
    {
        UserDto? Authenticate(string login, string password);
        UserDto? GetUserById(int id);
        UserDto? GetUserByLogin(string login);
    }
}
