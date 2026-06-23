using BlogPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogPlatform.Infrastructure.Persistence.Configurations;

internal sealed class BlogPostConfiguration : IEntityTypeConfiguration<BlogPost>
{
    public void Configure(EntityTypeBuilder<BlogPost> builder)
    {
        builder.ToTable("BlogPosts");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Content)
            .IsRequired()
            .HasMaxLength(10_000);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.HasMany(p => p.Comments)
            .WithOne()
            .HasForeignKey(c => c.BlogPostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(BlogPost.Comments))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
