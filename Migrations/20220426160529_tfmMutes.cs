using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkyChat.Migrations
{
    public partial class tfmMutes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "Mute",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Muter",
                table: "Mute",
                type: "varchar(32)",
                maxLength: 32,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Mute",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UnMuteClientId",
                table: "Mute",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UnMuter",
                table: "Mute",
                type: "varchar(32)",
                maxLength: 32,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Mute_Uuid_Expires_Status",
                table: "Mute",
                columns: new[] { "Uuid", "Expires", "Status" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Mute_Uuid_Expires_Status",
                table: "Mute");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Mute");

            migrationBuilder.DropColumn(
                name: "Muter",
                table: "Mute");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Mute");

            migrationBuilder.DropColumn(
                name: "UnMuteClientId",
                table: "Mute");

            migrationBuilder.DropColumn(
                name: "UnMuter",
                table: "Mute");
        }
    }
}
