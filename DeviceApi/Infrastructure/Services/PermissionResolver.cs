using DeviceApi.Application.Contracts;

namespace DeviceApi.Infrastructure.Services;

public static class PermissionResolver
{
    public static IReadOnlyCollection<string> ResolvePermissions(IEnumerable<string> roles)
    {
        var permissionSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var role in roles)
        {
            if (role.Equals(AppRoles.Admin, StringComparison.OrdinalIgnoreCase))
            {
                permissionSet.Add(AppPermissions.DeviceRead);
                permissionSet.Add(AppPermissions.DeviceWrite);
                permissionSet.Add(AppPermissions.DashboardRead);
                permissionSet.Add(AppPermissions.AuditRead);
                permissionSet.Add(AppPermissions.UserManage);
            }
            else if (role.Equals(AppRoles.Operator, StringComparison.OrdinalIgnoreCase))
            {
                permissionSet.Add(AppPermissions.DeviceRead);
                permissionSet.Add(AppPermissions.DeviceWrite);
                permissionSet.Add(AppPermissions.DashboardRead);
                permissionSet.Add(AppPermissions.AuditRead);
            }
            else if (role.Equals(AppRoles.Viewer, StringComparison.OrdinalIgnoreCase))
            {
                permissionSet.Add(AppPermissions.DeviceRead);
                permissionSet.Add(AppPermissions.DashboardRead);
            }
        }

        return permissionSet.ToArray();
    }
}
