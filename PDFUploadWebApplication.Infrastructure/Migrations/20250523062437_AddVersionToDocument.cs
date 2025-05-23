using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PDFUploadWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVersionToDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Documents",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                table: "Documents");
        }
    }
}
