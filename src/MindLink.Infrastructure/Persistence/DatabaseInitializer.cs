using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using MindLink.Domain.Entities;

namespace MindLink.Infrastructure.Persistence;

public sealed class DatabaseInitializer(MindLinkDbContext database)
{
    public async Task InitializeAsync()
    {
        await database.Database.MigrateAsync();
        var demoUser = await database.Users
            .Where(user => user.Email == "jonny@mindlink.local")
            .FirstOrDefaultAsync();

        demoUser ??= await database.Users
            .Where(user => user.Email == "valeria@mindlink.local")
            .FirstOrDefaultAsync();
        if (demoUser is not null)
        {
            demoUser.FullName = "Jonny";
            demoUser.Email = "jonny@mindlink.local";
            demoUser.UpdatedAt = DateTime.Now;
            await database.SaveChangesAsync();
            return;
        }

        if (await database.Users.AnyAsync()) return;

        var now = DateTime.Now;
        database.Users.Add(new LocalUser
        {
            FullName = "Jonny", Email = "jonny@mindlink.local",
            PasswordHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("MindLink2026!"))),
            CreatedAt = now, UpdatedAt = now
        });
        database.ActivityLogs.AddRange(
            new ActivityLog { Title = "Mapa actualizado", Detail = "Conectaste 4 ideas sobre carga cognitiva.", Category = "Conexión", OccurredAt = now.AddMinutes(-18) },
            new ActivityLog { Title = "Nueva fuente vinculada", Detail = "Añadiste el estudio de Sweller a Marco teórico.", Category = "Fuente", OccurredAt = now.AddHours(-2) },
            new ActivityLog { Title = "Borrador guardado", Detail = "Introducción · versión 6 creada correctamente.", Category = "Redacción", OccurredAt = now.AddHours(-5) },
            new ActivityLog { Title = "Objetivo revisado", Detail = "Ajustaste el alcance del proyecto de investigación.", Category = "Proyecto", OccurredAt = now.AddDays(-1) });
        await database.SaveChangesAsync();
    }
}
