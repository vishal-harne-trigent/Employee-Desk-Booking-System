using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeDeskBooking.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDeskLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Desks",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Location",
                table: "Desks");
        }
    }
}
