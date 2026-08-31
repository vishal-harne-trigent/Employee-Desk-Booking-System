using EmployeeDeskBooking.Domain.Bookings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeDeskBooking.Infrastructure.Data;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");

        builder.HasKey(booking => booking.Id);

        builder.Property(booking => booking.BookingDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(booking => booking.Status)
            .HasConversion<byte>()
            .IsRequired();

        builder.Property(booking => booking.CreatedAt).IsRequired();
        builder.Property(booking => booking.UpdatedAt).IsRequired();

        builder.HasIndex(booking => new { booking.UserId, booking.BookingDate })
            .IsUnique()
            .HasFilter("Status = 0");

        builder.HasIndex(booking => new { booking.DeskId, booking.BookingDate })
            .IsUnique()
            .HasFilter("Status = 0");

        builder.HasIndex(booking => new { booking.BookingDate, booking.Status });
        builder.HasIndex(booking => new { booking.UserId, booking.BookingDate });
    }
}
