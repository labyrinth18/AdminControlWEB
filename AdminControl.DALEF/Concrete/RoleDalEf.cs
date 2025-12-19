using AdminControl.DAL;
using AdminControl.DALEF.Models;
using AdminControl.DTO;
using Microsoft.EntityFrameworkCore;

namespace AdminControl.DALEF.Concrete
{
    public class RoleDalEf : IRoleDal
    {
        private readonly AdminControlContext _context;

        public RoleDalEf(AdminControlContext context)
        {
            _context = context;
        }

        public List<RoleDto> GetAll()
        {
            return _context.Roles
                .AsNoTracking()
                .Select(r => new RoleDto
                {
                    RoleID = r.RoleID,
                    RoleName = r.RoleName
                })
                .OrderBy(r => r.RoleName)
                .ToList();
        }

        public RoleDto? GetById(int roleId)
        {
            var role = _context.Roles
                .AsNoTracking()
                .FirstOrDefault(r => r.RoleID == roleId);

            if (role == null) return null;

            return new RoleDto
            {
                RoleID = role.RoleID,
                RoleName = role.RoleName
            };
        }

        public RoleDto Create(RoleCreateDto roleDto)
        {
            var entity = new Role
            {
                RoleName = roleDto.RoleName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Roles.Add(entity);
            _context.SaveChanges();

            return new RoleDto
            {
                RoleID = entity.RoleID,
                RoleName = entity.RoleName
            };
        }

        public RoleDto Update(RoleUpdateDto roleDto)
        {
            var entity = _context.Roles.FirstOrDefault(r => r.RoleID == roleDto.RoleID);
            if (entity == null)
            {
                throw new KeyNotFoundException($"Роль з ID {roleDto.RoleID} не знайдено");
            }

            entity.RoleName = roleDto.RoleName;
            entity.UpdatedAt = DateTime.UtcNow;

            _context.SaveChanges();

            return new RoleDto
            {
                RoleID = entity.RoleID,
                RoleName = entity.RoleName
            };
        }

        public bool Delete(int roleId)
        {
            var entity = _context.Roles.FirstOrDefault(r => r.RoleID == roleId);
            if (entity == null) return false;

            // Check if role is used by any users
            var usersWithRole = _context.Users.Any(u => u.RoleID == roleId);
            if (usersWithRole)
            {
                throw new InvalidOperationException("Неможливо видалити роль, яка використовується користувачами");
            }

            _context.Roles.Remove(entity);
            int affectedRows = _context.SaveChanges();
            return affectedRows == 1;
        }

        public bool Exists(int roleId)
        {
            return _context.Roles.Any(r => r.RoleID == roleId);
        }

        public bool NameExists(string roleName, int? excludeRoleId = null)
        {
            if (excludeRoleId.HasValue)
            {
                return _context.Roles.Any(r => r.RoleName == roleName && r.RoleID != excludeRoleId.Value);
            }
            return _context.Roles.Any(r => r.RoleName == roleName);
        }
    }
}
