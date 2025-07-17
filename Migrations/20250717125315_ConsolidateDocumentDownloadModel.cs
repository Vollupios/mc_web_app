using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntranetDocumentos.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidateDocumentDownloadModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DownloadDate",
                table: "DocumentDownloadLogs",
                newName: "DownloadedAt");

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "DocumentDownloadLogs",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FileSizeAtDownload",
                table: "DocumentDownloadLogs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSuccessful",
                table: "DocumentDownloadLogs",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "DocumentDownloadLogs",
                type: "TEXT",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "DocumentDownloadLogs");

            migrationBuilder.DropColumn(
                name: "FileSizeAtDownload",
                table: "DocumentDownloadLogs");

            migrationBuilder.DropColumn(
                name: "IsSuccessful",
                table: "DocumentDownloadLogs");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "DocumentDownloadLogs");

            migrationBuilder.RenameColumn(
                name: "DownloadedAt",
                table: "DocumentDownloadLogs",
                newName: "DownloadDate");
        }
    }
}
