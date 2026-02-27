using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Section)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Action)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.DisplayName)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(p => new { p.Section, p.Action })
            .IsUnique()
            .HasDatabaseName("IX_Permissions_Section_Action");

        builder.HasData(
            new Permission { Id = 1,  Section = "Products",   Action = "Read",        DisplayName = "Mahsulotlarni ko'rish" },
            new Permission { Id = 2,  Section = "Products",   Action = "Create",      DisplayName = "Mahsulot qo'shish" },
            new Permission { Id = 3,  Section = "Products",   Action = "Update",      DisplayName = "Mahsulotni tahrirlash" },
            new Permission { Id = 4,  Section = "Products",   Action = "Delete",      DisplayName = "Mahsulotni o'chirish" },
            new Permission { Id = 5,  Section = "Products",   Action = "AddStock",    DisplayName = "Ombor qo'shish" },
            new Permission { Id = 6,  Section = "Categories", Action = "Read",        DisplayName = "Kategoriyalarni ko'rish" },
            new Permission { Id = 7,  Section = "Categories", Action = "Create",      DisplayName = "Kategoriya qo'shish" },
            new Permission { Id = 8,  Section = "Categories", Action = "Update",      DisplayName = "Kategoriyani tahrirlash" },
            new Permission { Id = 9,  Section = "Categories", Action = "Delete",      DisplayName = "Kategoriyani o'chirish" },
            new Permission { Id = 10, Section = "Sales",      Action = "Create",      DisplayName = "Sotuv qilish" },
            new Permission { Id = 11, Section = "Sales",      Action = "ViewHistory", DisplayName = "Sotuvlar tarixini ko'rish" }
        );
    }
}
