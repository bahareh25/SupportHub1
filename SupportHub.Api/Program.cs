
using Scalar.AspNetCore;
using SupportHub.Application.Contracts;
using SupportHub.Application.Services;
using SupportHub.Infrastructure;
namespace SupportHub.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        // RFC 9457 Problem Details: structured, machine-readable error bodies.
        builder.Services.AddProblemDetails();
        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddScoped<ICustomerService, CustomerService>();
        builder.Services.AddScoped<ITicketService, TicketService>();
        builder.Services.AddScoped<ITicketCommentService, TicketCommentService>();
        builder.Services.AddScoped<ITagService, TagService>();
        
        var app = builder.Build();
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();

            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SupportHubDbContext>();
            await SeedData.EnsureSeadedAsync(db);
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        //Not mandatory but a good practice to have a health check endpoint for liveness and readiness probes.
        app.MapGet("/health/live", () => Results.Ok(new { status = "live" }))
            .ExcludeFromDescription();

        await app.RunAsync();
    }
}
