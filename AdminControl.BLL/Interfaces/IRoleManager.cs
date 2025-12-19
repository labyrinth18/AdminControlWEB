using AdminControl.DTO;

namespace AdminControl.BLL.Interfaces
{
    public interface IRoleManager
    {
        List<RoleDto> GetAllRoles();
        RoleDto? GetRoleById(int roleId);
        RoleDto CreateRole(RoleCreateDto role);
        RoleDto UpdateRole(RoleUpdateDto role);
        bool DeleteRole(int roleId);
    }
}