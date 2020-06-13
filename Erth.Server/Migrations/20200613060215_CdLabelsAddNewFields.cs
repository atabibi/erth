using Microsoft.EntityFrameworkCore.Migrations;

namespace Erth.Server.Migrations
{
    public partial class CdLabelsAddNewFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "CdLabels",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerUnited",
                table: "CdLabels",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "CdLabels");

            migrationBuilder.DropColumn(
                name: "CustomerUnited",
                table: "CdLabels");
        }
    }
}
