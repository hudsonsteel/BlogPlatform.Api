using BlogPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogPlatform.Infrastructure.Persistence.Configurations;

internal sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("Comments");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Author)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Content)
            .IsRequired()
            .HasMaxLength(2_000);

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.HasIndex(c => c.BlogPostId);
    }
}
