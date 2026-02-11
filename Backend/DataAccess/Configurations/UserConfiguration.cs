using Core.Entities;
using Core.Enums;
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

        builder.Property(e => e.Password)
            .IsRequired()
            .HasMaxLength(256);

        // SuperAdmin seed data
        builder.HasData(new User
        {
            Id = 1,
            Username = "superadmin",
            Password = "superadmin",
            PasswordHash = "$2a$11$TjYDrGYAX3yCCQwH2yE7XuU5tWgDTl7DhwU3uWxOZxOyW5ukOeQOK",
            Role = Role.SuperAdmin,
            IsActive = true,
            CreatedAt = new DateTime(2026, 2, 4, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
