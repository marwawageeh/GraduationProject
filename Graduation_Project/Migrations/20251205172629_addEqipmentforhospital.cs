using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduation_Project.Migrations
{
    /// <inheritdoc />
    public partial class addEqipmentforhospital : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HospitalEquipment",
                columns: table => new
                {
                    HospitalEquipmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HospitalEquipment", x => x.HospitalEquipmentId);
                });

            migrationBuilder.CreateTable(
                name: "HospitalHospitalEquipment",
                columns: table => new
                {
                    EquipmentsHospitalEquipmentId = table.Column<int>(type: "int", nullable: false),
                    HospitalsHospitalId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HospitalHospitalEquipment", x => new { x.EquipmentsHospitalEquipmentId, x.HospitalsHospitalId });
                    table.ForeignKey(
                        name: "FK_HospitalHospitalEquipment_HospitalEquipment_EquipmentsHospitalEquipmentId",
                        column: x => x.EquipmentsHospitalEquipmentId,
                        principalTable: "HospitalEquipment",
                        principalColumn: "HospitalEquipmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HospitalHospitalEquipment_Hospitals_HospitalsHospitalId",
                        column: x => x.HospitalsHospitalId,
                        principalTable: "Hospitals",
                        principalColumn: "HospitalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HospitalHospitalEquipment_HospitalsHospitalId",
                table: "HospitalHospitalEquipment",
                column: "HospitalsHospitalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HospitalHospitalEquipment");

            migrationBuilder.DropTable(
                name: "HospitalEquipment");
        }
    }
}
