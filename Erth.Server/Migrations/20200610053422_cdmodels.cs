using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Erth.Server.Migrations
{
    public partial class cdmodels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CdLabels",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Label = table.Column<string>(nullable: true),
                    TypeErth = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CdLabels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RegisteredLabels",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserPcSN = table.Column<string>(nullable: true),
                    FullName = table.Column<string>(nullable: true),
                    United = table.Column<string>(nullable: true),
                    City = table.Column<string>(nullable: true),
                    Shobeh = table.Column<string>(nullable: true),
                    DateOf = table.Column<DateTime>(nullable: false),
                    CdLabelId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegisteredLabels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegisteredLabels_CdLabels_CdLabelId",
                        column: x => x.CdLabelId,
                        principalTable: "CdLabels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegisteredLabels_CdLabelId",
                table: "RegisteredLabels",
                column: "CdLabelId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegisteredLabels");

            migrationBuilder.DropTable(
                name: "CdLabels");
        }
    }
}
