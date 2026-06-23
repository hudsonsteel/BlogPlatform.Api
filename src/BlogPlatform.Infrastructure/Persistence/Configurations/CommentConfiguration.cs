using BlogPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogPlatform.Infrastructure.Persistence.Configurations;

internal sealed class CommentConfiguration : EntityConfiguration<Comment>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("Comments");

        builder.Property(c => c.Author)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Content)
            .IsRequired()
            .HasMaxLength(2_000);

        builder.HasIndex(c => c.BlogPostId);
    }
}
