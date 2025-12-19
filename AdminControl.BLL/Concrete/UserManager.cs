using AdminControl.BLL.Interfaces;
using AdminControl.DAL;
using AdminControl.DTO;

namespace AdminControl.BLL.Concrete
{
    public class UserManager : IUserManager
    {
        private readonly IUserDal _userDal;
        private readonly IRoleDal _roleDal;

        public UserManager(IUserDal userDal, IRoleDal roleDal)
        {
            _userDal = userDal;
            _roleDal = roleDal;
        }

        #region Read Operations

        public List<UserDto> GetAllUsers()
        {
            return _userDal.GetAll();
        }

        public UserDto? GetUserById(int userId)
        {
            return _userDal.GetById(userId);
        }

        public UserDto? GetUserByLogin(string login)
        {
            return _userDal.GetByLogin(login);
        }

        #endregion

        #region Write Operations

        public UserDto CreateUser(UserCreateDto user)
        {
            // Validation
            ValidateUserCreate(user);

            return _userDal.Create(user);
        }

        public UserDto UpdateUser(UserUpdateDto user)
        {
            // Validation
            ValidateUserUpdate(user);

            return _userDal.Update(user);
        }

        public bool DeleteUser(int userId)
        {
            var existingUser = _userDal.GetById(userId);
            if (existingUser == null)
            {
                throw new KeyNotFoundException("Користувача не знайдено");
            }

            // Prevent deleting the last admin
            if (existingUser.RoleName.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                var adminCount = GetUsersByRoleCount(existingUser.RoleID);
                if (adminCount <= 1)
                {
                    throw new InvalidOperationException("Неможливо видалити останнього адміністратора системи");
                }
            }

            return _userDal.Delete(userId);
        }

        #endregion

        #region Status Operations

        public bool ActivateUser(int userId)
        {
            var user = _userDal.GetById(userId);
            if (user == null)
            {
                throw new KeyNotFoundException("Користувача не знайдено");
            }

            if (user.IsActive)
            {
                return true; // Already active
            }

            var updateDto = new UserUpdateDto
            {
                UserID = user.UserID,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                Gender = user.Gender,
                RoleID = user.RoleID,
                IsActive = true
            };

            _userDal.Update(updateDto);
            return true;
        }

        public bool DeactivateUser(int userId)
        {
            var user = _userDal.GetById(userId);
            if (user == null)
            {
                throw new KeyNotFoundException("Користувача не знайдено");
            }

            // Prevent deactivating the last admin
            if (user.RoleName.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                var activeAdmins = GetAllUsers()
                    .Count(u => u.RoleName.Equals("Admin", StringComparison.OrdinalIgnoreCase) && u.IsActive);
                
                if (activeAdmins <= 1)
                {
                    throw new InvalidOperationException("Неможливо деактивувати останнього активного адміністратора");
                }
            }

            if (!user.IsActive)
            {
                return true; // Already inactive
            }

            var updateDto = new UserUpdateDto
            {
                UserID = user.UserID,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                Gender = user.Gender,
                RoleID = user.RoleID,
                IsActive = false
            };

            _userDal.Update(updateDto);
            return true;
        }

        #endregion

        #region Validation Helpers

        public bool LoginExists(string login)
        {
            return _userDal.LoginExists(login);
        }

        public bool EmailExists(string email, int? excludeUserId = null)
        {
            return _userDal.EmailExists(email, excludeUserId);
        }

        private void ValidateUserCreate(UserCreateDto user)
        {
            if (string.IsNullOrWhiteSpace(user.Login))
            {
                throw new ArgumentException("Логін є обов'язковим");
            }

            if (string.IsNullOrWhiteSpace(user.Password))
            {
                throw new ArgumentException("Пароль є обов'язковим");
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                throw new ArgumentException("Email є обов'язковим");
            }

            if (string.IsNullOrWhiteSpace(user.FirstName))
            {
                throw new ArgumentException("Ім'я є обов'язковим");
            }

            if (string.IsNullOrWhiteSpace(user.LastName))
            {
                throw new ArgumentException("Прізвище є обов'язковим");
            }

            if (_userDal.LoginExists(user.Login))
            {
                throw new InvalidOperationException("Користувач з таким логіном вже існує");
            }

            if (_userDal.EmailExists(user.Email))
            {
                throw new InvalidOperationException("Користувач з таким Email вже існує");
            }

            if (!_roleDal.Exists(user.RoleID))
            {
                throw new InvalidOperationException("Вказана роль не існує");
            }
        }

        private void ValidateUserUpdate(UserUpdateDto user)
        {
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                throw new ArgumentException("Email є обов'язковим");
            }

            if (string.IsNullOrWhiteSpace(user.FirstName))
            {
                throw new ArgumentException("Ім'я є обов'язковим");
            }

            if (string.IsNullOrWhiteSpace(user.LastName))
            {
                throw new ArgumentException("Прізвище є обов'язковим");
            }

            var existingUser = _userDal.GetById(user.UserID);
            if (existingUser == null)
            {
                throw new KeyNotFoundException("Користувача не знайдено");
            }

            if (_userDal.EmailExists(user.Email, user.UserID))
            {
                throw new InvalidOperationException("Користувач з таким Email вже існує");
            }

            if (!_roleDal.Exists(user.RoleID))
            {
                throw new InvalidOperationException("Вказана роль не існує");
            }

            // Prevent removing admin role from the last admin
            if (existingUser.RoleName.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                var newRole = _roleDal.GetById(user.RoleID);
                if (newRole != null && !newRole.RoleName.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    var adminCount = GetUsersByRoleCount(existingUser.RoleID);
                    if (adminCount <= 1)
                    {
                        throw new InvalidOperationException("Неможливо змінити роль останнього адміністратора");
                    }
                }
            }
        }

        #endregion

        #region Statistics

        public int GetTotalUsersCount()
        {
            return _userDal.GetAll().Count;
        }

        public int GetActiveUsersCount()
        {
            return _userDal.GetAll().Count(u => u.IsActive);
        }

        public int GetUsersByRoleCount(int roleId)
        {
            return _userDal.GetAll().Count(u => u.RoleID == roleId);
        }

        public Dictionary<string, int> GetUsersCountByRole()
        {
            var users = _userDal.GetAll();
            return users
                .GroupBy(u => u.RoleName)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        #endregion
    }
}
