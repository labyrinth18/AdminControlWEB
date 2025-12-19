using AdminControl.BLL.Interfaces;
using AdminControl.DAL;
using AdminControl.DTO;

namespace AdminControl.BLL.Concrete
{
    public class RoleManager : IRoleManager
    {
        private readonly IRoleDal _roleDal;

        public RoleManager(IRoleDal roleDal)
        {
            _roleDal = roleDal;
        }

        public List<RoleDto> GetAllRoles()
        {
            return _roleDal.GetAll();
        }

        public RoleDto? GetRoleById(int roleId)
        {
            return _roleDal.GetById(roleId);
        }

        public RoleDto CreateRole(RoleCreateDto role)
        {
            if (string.IsNullOrWhiteSpace(role.RoleName))
            {
                throw new ArgumentException("Назва ролі є обов'язковою");
            }

            if (_roleDal.NameExists(role.RoleName))
            {
                throw new InvalidOperationException("Роль з такою назвою вже існує");
            }

            return _roleDal.Create(role);
        }

        public RoleDto UpdateRole(RoleUpdateDto role)
        {
            if (string.IsNullOrWhiteSpace(role.RoleName))
            {
                throw new ArgumentException("Назва ролі є обов'язковою");
            }

            if (!_roleDal.Exists(role.RoleID))
            {
                throw new KeyNotFoundException("Роль не знайдено");
            }

            if (_roleDal.NameExists(role.RoleName, role.RoleID))
            {
                throw new InvalidOperationException("Роль з такою назвою вже існує");
            }

            return _roleDal.Update(role);
        }

        public bool DeleteRole(int roleId)
        {
            if (!_roleDal.Exists(roleId))
            {
                throw new KeyNotFoundException("Роль не знайдено");
            }

            return _roleDal.Delete(roleId);
        }
    }
}