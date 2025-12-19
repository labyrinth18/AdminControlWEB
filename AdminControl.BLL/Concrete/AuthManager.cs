using AdminControl.BLL.Interfaces;
using AdminControl.DAL;
using AdminControl.DTO;
using System.Security.Cryptography;
using System.Text;

namespace AdminControl.BLL.Concrete
{
    public class AuthManager : IAuthManager
    {
        private readonly IUserDal _userDal;

        public AuthManager(IUserDal userDal)
        {
            _userDal = userDal;
        }

        public UserDto? Authenticate(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                throw new UnauthorizedAccessException("Невірний логін або пароль.");
            }

            string passwordHash = ComputeHash(password);
            var user = _userDal.Authenticate(login, passwordHash);

            if (user == null)
            {
                throw new UnauthorizedAccessException("Невірний логін або пароль.");
            }

            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException("Ваш обліковий запис деактивовано.");
            }

            return user;
        }

        public UserDto? GetUserById(int id)
        {
            return _userDal.GetById(id);
        }

        public UserDto? GetUserByLogin(string login)
        {
            return _userDal.GetByLogin(login);
        }

        private static string ComputeHash(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }
    }
}
