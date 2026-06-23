using BlogPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogPlatform.Infrastructure.Persistence.Configurations;

internal sealed class BlogPostConfiguration : EntityConfiguration<BlogPost>
{
    protected override void ConfigureEntity(EntityTypeBuilder<BlogPost> builder)
    {
        builder.ToTable("BlogPosts");

        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Content)
            .IsRequired()
            .HasMaxLength(10_000);

        builder.HasMany(p => p.Comments)
            .WithOne()
            .HasForeignKey(c => c.BlogPostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(BlogPost.Comments))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
