using DeviceApi.Application.Contracts;
using DeviceApi.Domain.Entities;
using DeviceApi.Domain.Enums;
using DeviceApi.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DeviceApi.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        await dbContext.Database.EnsureCreatedAsync();

        foreach (var role in new[] { AppRoles.Admin, AppRoles.Operator, AppRoles.Viewer })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var adminUser = await userManager.FindByNameAsync("admin");
        if (adminUser is null)
        {
            var user = new AppUser
            {
                UserName = "admin",
                Email = "admin@device.local"
            };
            var createResult = await userManager.CreateAsync(user, "Admin@123456");
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(user, AppRoles.Admin);
            }
        }

        var operatorUser = await userManager.FindByNameAsync("operator");
        if (operatorUser is null)
        {
            var user = new AppUser
            {
                UserName = "operator",
                Email = "operator@device.local"
            };
            var createResult = await userManager.CreateAsync(user, "Operator@123456");
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(user, AppRoles.Operator);
            }
        }

        var viewerUser = await userManager.FindByNameAsync("viewer");
        if (viewerUser is null)
        {
            var user = new AppUser
            {
                UserName = "viewer",
                Email = "viewer@device.local"
            };
            var createResult = await userManager.CreateAsync(user, "Viewer@123456");
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(user, AppRoles.Viewer);
            }
        }

        if (await dbContext.Devices.AnyAsync())
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var sensor = new Device
        {
            Id = Guid.NewGuid(),
            Name = "车间温湿度传感器",
            Type = DeviceType.Sensor,
            Status = "在线",
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        var plc = new Device
        {
            Id = Guid.NewGuid(),
            Name = "包装线 PLC",
            Type = DeviceType.Plc,
            Status = "在线",
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        dbContext.Devices.AddRange(sensor, plc);
        dbContext.DataPoints.AddRange(
            new DataPoint
            {
                Id = Guid.NewGuid(),
                DeviceId = sensor.Id,
                Key = "sensor.temperature",
                Name = "温度",
                DataType = "double",
                Value = "25.3",
                UpdatedAtUtc = now
            },
            new DataPoint
            {
                Id = Guid.NewGuid(),
                DeviceId = sensor.Id,
                Key = "sensor.humidity",
                Name = "湿度",
                DataType = "double",
                Value = "57.6",
                UpdatedAtUtc = now
            },
            new DataPoint
            {
                Id = Guid.NewGuid(),
                DeviceId = plc.Id,
                Key = "plc.motor.speed",
                Name = "电机转速",
                DataType = "int",
                Value = "1260",
                UpdatedAtUtc = now
            },
            new DataPoint
            {
                Id = Guid.NewGuid(),
                DeviceId = plc.Id,
                Key = "plc.alarm.code",
                Name = "报警代码",
                DataType = "int",
                Value = "0",
                UpdatedAtUtc = now
            }
        );

        await dbContext.SaveChangesAsync();
    }
}
