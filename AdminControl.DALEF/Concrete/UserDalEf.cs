using AdminControl.DAL;
using AdminControl.DALEF.Models;
using AdminControl.DTO;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace AdminControl.DALEF.Concrete
{
    public class UserDalEf : IUserDal
    {
        private readonly AdminControlContext _context;

        public UserDalEf(AdminControlContext context)
        {
            _context = context;
        }

        public List<UserDto> GetAll()
        {
            return _context.Users
                .AsNoTracking()
                .Include(u => u.Role)
                .Select(u => new UserDto
                {
                    UserID = u.UserID,
                    Login = u.Login,
                    FirstName = u.FirstName ?? string.Empty,
                    LastName = u.LastName ?? string.Empty,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Address = u.Address,
                    Gender = u.Gender,
                    RoleID = u.RoleID,
                    RoleName = u.Role != null ? u.Role.RoleName : "Немає ролі",
                    IsActive = u.IsActive
                })
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToList();
        }

        public UserDto? GetById(int userId)
        {
            var user = _context.Users
                .AsNoTracking()
                .Include(u => u.Role)
                .FirstOrDefault(u => u.UserID == userId);

            if (user == null) return null;

            return MapToDto(user);
        }

        public UserDto? GetByLogin(string login)
        {
            var user = _context.Users
                .AsNoTracking()
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Login == login);

            if (user == null) return null;

            return MapToDto(user);
        }

        public UserDto? Authenticate(string login, string passwordHash)
        {
            var user = _context.Users
                .AsNoTracking()
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Login == login && u.PasswordHash == passwordHash);

            if (user == null) return null;

            return MapToDto(user);
        }

        public UserDto Create(UserCreateDto userDto)
        {
            string passwordHash = ComputeHash(userDto.Password);

            var entity = new User
            {
                Login = userDto.Login,
                PasswordHash = passwordHash,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Email = userDto.Email,
                PhoneNumber = userDto.PhoneNumber,
                Address = userDto.Address,
                Gender = userDto.Gender,
                RoleID = userDto.RoleID,
                IsActive = userDto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(entity);
            _context.SaveChanges();

            // Reload with Role included
            var createdUser = _context.Users
                .AsNoTracking()
                .Include(u => u.Role)
                .First(u => u.UserID == entity.UserID);

            return MapToDto(createdUser);
        }

        public UserDto Update(UserUpdateDto userDto)
        {
            var entity = _context.Users.FirstOrDefault(u => u.UserID == userDto.UserID);
            if (entity == null)
            {
                throw new KeyNotFoundException($"Користувач з ID {userDto.UserID} не знайдено");
            }

            entity.FirstName = userDto.FirstName;
            entity.LastName = userDto.LastName;
            entity.Email = userDto.Email;
            entity.PhoneNumber = userDto.PhoneNumber;
            entity.Address = userDto.Address;
            entity.Gender = userDto.Gender;
            entity.RoleID = userDto.RoleID;
            entity.IsActive = userDto.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;

            _context.SaveChanges();

            // Reload with Role included
            var updatedUser = _context.Users
                .AsNoTracking()
                .Include(u => u.Role)
                .First(u => u.UserID == entity.UserID);

            return MapToDto(updatedUser);
        }

        public bool Delete(int userId)
        {
            var entity = _context.Users.FirstOrDefault(u => u.UserID == userId);
            if (entity == null) return false;

            _context.Users.Remove(entity);
            int affectedRows = _context.SaveChanges();
            return affectedRows == 1;
        }

        public bool LoginExists(string login)
        {
            return _context.Users.Any(u => u.Login == login);
        }

        public bool EmailExists(string email, int? excludeUserId = null)
        {
            if (excludeUserId.HasValue)
            {
                return _context.Users.Any(u => u.Email == email && u.UserID != excludeUserId.Value);
            }
            return _context.Users.Any(u => u.Email == email);
        }

        private static UserDto MapToDto(User user)
        {
            return new UserDto
            {
                UserID = user.UserID,
                Login = user.Login,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                Gender = user.Gender,
                RoleID = user.RoleID,
                RoleName = user.Role?.RoleName ?? string.Empty,
                IsActive = user.IsActive
            };
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
