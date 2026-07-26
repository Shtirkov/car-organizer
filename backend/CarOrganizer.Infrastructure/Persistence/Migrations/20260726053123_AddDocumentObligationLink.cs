using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarOrganizer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentObligationLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "VehicleObligationId",
                table: "Documents",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_VehicleObligationId",
                table: "Documents",
                column: "VehicleObligationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Obligations_VehicleObligationId",
                table: "Documents",
                column: "VehicleObligationId",
                principalTable: "Obligations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Obligations_VehicleObligationId",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_VehicleObligationId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "VehicleObligationId",
                table: "Documents");
        }
    }
}
