using AdminControl.DTO;

namespace AdminControl.DAL
{
    public interface IUserDal
    {
        List<UserDto> GetAll();
        UserDto? GetById(int userId);
        UserDto? GetByLogin(string login);
        UserDto Create(UserCreateDto user);
        UserDto Update(UserUpdateDto user);
        bool Delete(int userId);
        UserDto? Authenticate(string login, string passwordHash);
        bool LoginExists(string login);
        bool EmailExists(string email, int? excludeUserId = null);
    }
}
