using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntranetDocumentos.Migrations
{
    /// <inheritdoc />
    public partial class RenameDownloadDateColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Renomear coluna DownloadDate para DownloadedAt
            migrationBuilder.RenameColumn(
                name: "DownloadDate",
                table: "DocumentDownloadLogs",
                newName: "DownloadedAt");

            // Adicionar novas colunas
            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "DocumentDownloadLogs",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSuccessful",
                table: "DocumentDownloadLogs",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

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

            // Atualizar UserAgent para ser maior
            migrationBuilder.AlterColumn<string>(
                name: "UserAgent",
                table: "DocumentDownloadLogs",
                type: "TEXT",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverter as mudanças
            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "DocumentDownloadLogs");

            migrationBuilder.DropColumn(
                name: "IsSuccessful",
                table: "DocumentDownloadLogs");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "DocumentDownloadLogs");

            migrationBuilder.DropColumn(
                name: "FileSizeAtDownload",
                table: "DocumentDownloadLogs");

            // Reverter UserAgent
            migrationBuilder.AlterColumn<string>(
                name: "UserAgent",
                table: "DocumentDownloadLogs",
                type: "TEXT",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500,
                oldNullable: true);

            // Renomear coluna de volta
            migrationBuilder.RenameColumn(
                name: "DownloadedAt",
                table: "DocumentDownloadLogs",
                newName: "DownloadDate");
        }
    }
}
