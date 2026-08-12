using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MindLink.Infrastructure.Persistence;

public sealed class MindLinkDbContextFactory : IDesignTimeDbContextFactory<MindLinkDbContext>
{
    public MindLinkDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<MindLinkDbContext>()
            .UseSqlite("Data Source=mindlink.design.db")
            .Options;
        return new MindLinkDbContext(options);
    }
}
