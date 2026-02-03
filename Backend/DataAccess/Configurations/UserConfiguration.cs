using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Username)
            .IsRequired()
            .HasMaxLength(100);

        // Username unique constraint
        builder.HasIndex(e => e.Username)
            .IsUnique()
            .HasDatabaseName("IX_Users_Username_Unique");

        builder.Property(e => e.PasswordHash)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.Role)
            .IsRequired();

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}
