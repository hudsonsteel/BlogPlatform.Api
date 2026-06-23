using BlogPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlogPlatform.Infrastructure.Persistence;

public sealed class BlogPlatformDbContext(DbContextOptions<BlogPlatformDbContext> options) : DbContext(options)
{
    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();

    public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BlogPlatformDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
