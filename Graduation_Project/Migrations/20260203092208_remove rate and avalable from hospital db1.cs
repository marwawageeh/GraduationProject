using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduation_Project.Migrations
{
    /// <inheritdoc />
    public partial class removerateandavalablefromhospitaldb1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Hospitals");

            migrationBuilder.DropColumn(
                name: "Avaliable",
                table: "Doctors");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Rating",
                table: "Hospitals",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Avaliable",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
