using BlogPlatform.Application.Common.Queries;
using BlogPlatform.Domain.Repositories;
using BlogPlatform.Infrastructure.Persistence;
using BlogPlatform.Infrastructure.Queries;
using BlogPlatform.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BlogPlatform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? "Data Source=blogplatform.db";

        services.AddDbContext<BlogPlatformDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IBlogPostRepository, BlogPostRepository>();
        services.AddScoped<IBlogPostQueries, BlogPostQueries>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
