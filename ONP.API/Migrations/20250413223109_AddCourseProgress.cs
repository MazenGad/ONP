using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ONP.API.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CourseProgress",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    ContentId = table.Column<int>(type: "int", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseProgress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseProgress_AspNetUsers_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseProgress_CourseContents_ContentId",
                        column: x => x.ContentId,
                        principalTable: "CourseContents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CourseProgress_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseProgress_ContentId",
                table: "CourseProgress",
                column: "ContentId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseProgress_CourseId",
                table: "CourseProgress",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseProgress_StudentId_ContentId",
                table: "CourseProgress",
                columns: new[] { "StudentId", "ContentId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseProgress");
        }
    }
}
