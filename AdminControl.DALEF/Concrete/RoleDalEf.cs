using AdminControl.DAL;
using AdminControl.DALEF.Models;
using AdminControl.DTO;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AdminControl.DALEF.Concrete
{
    public class RoleDalEf : IRoleDal
    {
        private readonly string _connStr;
        private readonly IMapper _mapper;

        public RoleDalEf(string connStr, IMapper mapper)
        {
            _connStr = connStr;
            _mapper = mapper;
        }

        public List<RoleDto> GetAll()
        {
            using (var context = new AdminControlContext(_connStr))
            {
                return context.Roles.Select(r => new RoleDto
                {
                    RoleID = r.RoleID,
                    RoleName = r.RoleName
                }).ToList();
            }
        }

        public RoleDto? GetById(int roleId)
        {
            using (var context = new AdminControlContext(_connStr))
            {
                var role = context.Roles.FirstOrDefault(r => r.RoleID == roleId);
                if (role == null) return null;

                return new RoleDto
                {
                    RoleID = role.RoleID,
                    RoleName = role.RoleName
                };
            }
        }

        public RoleDto Create(RoleCreateDto roleDto)
        {
            using (var context = new AdminControlContext(_connStr))
            {
                var entity = new Role
                {
                    RoleName = roleDto.RoleName,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                context.Roles.Add(entity);
                context.SaveChanges();

                return new RoleDto
                {
                    RoleID = entity.RoleID,
                    RoleName = entity.RoleName
                };
            }
        }

        public RoleDto Update(RoleUpdateDto roleDto)
        {
            using (var context = new AdminControlContext(_connStr))
            {
                var entity = context.Roles.FirstOrDefault(r => r.RoleID == roleDto.RoleID);
                if (entity == null)
                {
                    throw new KeyNotFoundException($"Роль з ID {roleDto.RoleID} не знайдено");
                }

                entity.RoleName = roleDto.RoleName;
                entity.UpdatedAt = DateTime.UtcNow;

                context.SaveChanges();

                return new RoleDto
                {
                    RoleID = entity.RoleID,
                    RoleName = entity.RoleName
                };
            }
        }

        public bool Delete(int roleId)
        {
            using (var context = new AdminControlContext(_connStr))
            {
                var entity = context.Roles.FirstOrDefault(r => r.RoleID == roleId);
                if (entity == null) return false;

                context.Roles.Remove(entity);
                int affectedRows = context.SaveChanges();
                return affectedRows == 1;
            }
        }

        public bool Exists(int roleId)
        {
            using (var context = new AdminControlContext(_connStr))
            {
                return context.Roles.Any(r => r.RoleID == roleId);
            }
        }

        public bool NameExists(string roleName, int? excludeRoleId = null)
        {
            using (var context = new AdminControlContext(_connStr))
            {
                if (excludeRoleId.HasValue)
                {
                    return context.Roles.Any(r => r.RoleName == roleName && r.RoleID != excludeRoleId.Value);
                }
                return context.Roles.Any(r => r.RoleName == roleName);
            }
        }
    }
}
