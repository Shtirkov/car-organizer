using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarOrganizer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CascadeDocumentsWithTheirLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_MaintenanceRecords_MaintenanceRecordId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Obligations_VehicleObligationId",
                table: "Documents");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_MaintenanceRecords_MaintenanceRecordId",
                table: "Documents",
                column: "MaintenanceRecordId",
                principalTable: "MaintenanceRecords",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Obligations_VehicleObligationId",
                table: "Documents",
                column: "VehicleObligationId",
                principalTable: "Obligations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_MaintenanceRecords_MaintenanceRecordId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Obligations_VehicleObligationId",
                table: "Documents");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_MaintenanceRecords_MaintenanceRecordId",
                table: "Documents",
                column: "MaintenanceRecordId",
                principalTable: "MaintenanceRecords",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Obligations_VehicleObligationId",
                table: "Documents",
                column: "VehicleObligationId",
                principalTable: "Obligations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
