using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ONP.API.Migrations
{
    /// <inheritdoc />
    public partial class addLessonCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LessonCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseContentId = table.Column<int>(type: "int", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonCodes_CourseContents_CourseContentId",
                        column: x => x.CourseContentId,
                        principalTable: "CourseContents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LessonCodes_CourseContentId",
                table: "LessonCodes",
                column: "CourseContentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LessonCodes");
        }
    }
}
