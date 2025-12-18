using AdminControl.DTO;

namespace AdminControl.DAL
{
    public interface IRoleDal
    {
        List<RoleDto> GetAll();
        RoleDto? GetById(int roleId);
        RoleDto Create(RoleCreateDto role);
        RoleDto Update(RoleUpdateDto role);
        bool Delete(int roleId);
        bool Exists(int roleId);
        bool NameExists(string roleName, int? excludeRoleId = null);
    }
}
