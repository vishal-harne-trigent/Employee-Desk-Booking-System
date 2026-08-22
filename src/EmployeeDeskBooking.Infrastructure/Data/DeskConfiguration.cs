using EmployeeDeskBooking.Domain.Desks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeDeskBooking.Infrastructure.Data;

public class DeskConfiguration : IEntityTypeConfiguration<Desk>
{
    public void Configure(EntityTypeBuilder<Desk> builder)
    {
        builder.ToTable("Desks");

        builder.HasKey(desk => desk.Id);

        builder.Property(desk => desk.DeskNumber)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(desk => desk.DeskNumberNormalized)
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(desk => desk.DeskNumberNormalized)
            .IsUnique();

        builder.Property(desk => desk.Status)
            .HasConversion<byte>()
            .IsRequired();

        builder.Property(desk => desk.Location)
            .HasMaxLength(100)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(desk => desk.CreatedAt).IsRequired();
        builder.Property(desk => desk.UpdatedAt).IsRequired();
    }
}
