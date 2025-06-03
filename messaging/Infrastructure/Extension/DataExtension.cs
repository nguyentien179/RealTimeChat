using System;
using Microsoft.EntityFrameworkCore;

namespace messaging.Infrastructure.Extension;

public static class DataExtension
{
    public static void MigrateDb(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.MigrateAsync().GetAwaiter().GetResult();
    }
}
