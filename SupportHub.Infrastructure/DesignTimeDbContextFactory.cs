using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupportHub.Infrastructure;
/// <summary>
/// Enables EF Core design-time tooling (migrations) to construct the context
/// without running the host. The connection string here is only used to build
/// the model/migrations, never at runtime.
/// </summary>
public class DesignTimeDbContextFactory: IDesignTimeDbContextFactory<SupportHubDbContext>
{
    public SupportHubDbContext CreateDbContext(string[] args)
    {
        var optionBuilder= new DbContextOptionsBuilder<SupportHubDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("SUPPORTHUB_MIGRATION_CONNECTION")
            ?? "Server=.; Initial Catalog=SupportHubCore; User Id = sa; Password=1qaz!QAZ";
        optionBuilder.UseSqlServer(connectionString);
        return new SupportHubDbContext(optionBuilder.Options);
    }

}
