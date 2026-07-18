using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarOrganizer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SplitVehicleMileage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Mileage",
                table: "Vehicles",
                newName: "PurchaseMileage");

            migrationBuilder.AddColumn<int>(
                name: "CurrentMileage",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Existing rows only knew a single odometer value (now PurchaseMileage). Seed the new
            // current reading from it so the CurrentMileage >= PurchaseMileage invariant holds from
            // the start, instead of leaving them at the 0 default.
            migrationBuilder.Sql(
                """UPDATE "Vehicles" SET "CurrentMileage" = "PurchaseMileage";""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentMileage",
                table: "Vehicles");

            migrationBuilder.RenameColumn(
                name: "PurchaseMileage",
                table: "Vehicles",
                newName: "Mileage");
        }
    }
}
