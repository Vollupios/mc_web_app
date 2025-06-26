using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntranetDocumentos.Migrations
{
    /// <inheritdoc />
    public partial class AddContentTextToDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentText",
                table: "Documents",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentText",
                table: "Documents");
        }
    }
}
