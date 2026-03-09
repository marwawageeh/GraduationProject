using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduation_Project.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerToEquipment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "Equipments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Equipments_OwnerId",
                table: "Equipments",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Equipments_users_OwnerId",
                table: "Equipments",
                column: "OwnerId",
                principalTable: "users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Equipments_users_OwnerId",
                table: "Equipments");

            migrationBuilder.DropIndex(
                name: "IX_Equipments_OwnerId",
                table: "Equipments");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Equipments");
        }
    }
}
