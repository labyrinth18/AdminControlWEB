using AdminControl.DAL;
using AdminControl.DALEF.Models;
using AdminControl.DTO;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace AdminControl.DALEF.Concrete
{
    public class UserDalEf : IUserDal
    {
        private readonly string _connStr;
        private readonly IMapper _mapper;

        public UserDalEf(string connStr, IMapper mapper)
        {
            _connStr = connStr;
            _mapper = mapper;
        }

        public List<UserDto> GetAll()
        {
            using (var context = new AdminControlContext(_connStr))
            {
                return context.Users
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
                    }).ToList();
            }
        }

        public UserDto? GetById(int userId)
        {
            using (var context = new AdminControlContext(_connStr))
            {
                var user = context.Users
                    .Include(u => u.Role)
                    .FirstOrDefault(u => u.UserID == userId);

                if (user == null) return null;

                return MapToDto(user);
            }
        }

        public UserDto? GetByLogin(string login)
        {
            using (var context = new AdminControlContext(_connStr))
            {
                var user = context.Users
                    .Include(u => u.Role)
                    .FirstOrDefault(u => u.Login == login);

                if (user == null) return null;

                return MapToDto(user);
            }
        }

        public UserDto? Authenticate(string login, string passwordHash)
        {
            using (var context = new AdminControlContext(_connStr))
            {
                var user = context.Users
                    .Include(u => u.Role)
                    .FirstOrDefault(u => u.Login == login && u.PasswordHash == passwordHash);

                if (user == null) return null;

                return MapToDto(user);
            }
        }

        public UserDto Create(UserCreateDto userDto)
        {
            using (var context = new AdminControlContext(_connStr))
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

                context.Users.Add(entity);
                context.SaveChanges();

                var createdUser = context.Users
                    .Include(u => u.Role)
                    .First(u => u.UserID == entity.UserID);

                return MapToDto(createdUser);
            }
        }

        public UserDto Update(UserUpdateDto userDto)
        {
            using (var context = new AdminControlContext(_connStr))
            {
                var entity = context.Users.FirstOrDefault(u => u.UserID == userDto.UserID);
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

                context.SaveChanges();

                var updatedUser = context.Users
                    .Include(u => u.Role)
                    .First(u => u.UserID == entity.UserID);

                return MapToDto(updatedUser);
            }
        }

        public bool Delete(int userId)
        {
            using (var context = new AdminControlContext(_connStr))
            {
                var entity = context.Users.FirstOrDefault(u => u.UserID == userId);
                if (entity == null) return false;

                context.Users.Remove(entity);
                int affectedRows = context.SaveChanges();
                return affectedRows == 1;
            }
        }

        public bool LoginExists(string login)
        {
            using (var context = new AdminControlContext(_connStr))
            {
                return context.Users.Any(u => u.Login == login);
            }
        }

        public bool EmailExists(string email, int? excludeUserId = null)
        {
            using (var context = new AdminControlContext(_connStr))
            {
                if (excludeUserId.HasValue)
                {
                    return context.Users.Any(u => u.Email == email && u.UserID != excludeUserId.Value);
                }
                return context.Users.Any(u => u.Email == email);
            }
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
