using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class prof : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StudentModuleMarkSheets",
                columns: table => new
                {
                    StudentModuleMarkSheetId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    ModuleId = table.Column<int>(type: "int", nullable: false),
                    Test1 = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Test2 = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Test3 = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Assignment1 = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Assignment2 = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Practical = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Project = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Exam = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FinalMark = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentModuleMarkSheets", x => x.StudentModuleMarkSheetId);
                    table.ForeignKey(
                        name: "FK_StudentModuleMarkSheets_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
                        principalColumn: "ModuleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentModuleMarkSheets_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentModuleMarkSheets_ModuleId",
                table: "StudentModuleMarkSheets",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentModuleMarkSheets_StudentId_ModuleId",
                table: "StudentModuleMarkSheets",
                columns: new[] { "StudentId", "ModuleId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentModuleMarkSheets");
        }
    }
}
