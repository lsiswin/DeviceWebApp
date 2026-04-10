namespace DeviceApi.Application.Contracts;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Operator = "Operator";
    public const string Viewer = "Viewer";
}

public static class AppPermissions
{
    public const string DeviceRead = "device:read";
    public const string DeviceWrite = "device:write";
    public const string DashboardRead = "dashboard:read";
    public const string AuditRead = "audit:read";
    public const string UserManage = "user:manage";
}

public static class AppPolicies
{
    public const string DeviceRead = nameof(DeviceRead);
    public const string DeviceWrite = nameof(DeviceWrite);
    public const string DashboardRead = nameof(DashboardRead);
    public const string AuditRead = nameof(AuditRead);
    public const string UserManage = nameof(UserManage);
}
