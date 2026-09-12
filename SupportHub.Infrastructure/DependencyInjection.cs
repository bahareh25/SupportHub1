using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using SupportHub.Application.Contracts;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace SupportHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register your infrastructure services here
        var connectionString = configuration.GetConnectionString("DefaultConnection");
       
        services.AddDbContext<SupportHubDbContext>(Options => Options.UseSqlServer(connectionString));
        services.AddScoped<IApplicationDbContext>(sp=>sp.GetRequiredService<SupportHubDbContext>());
        return services;
    }


}
