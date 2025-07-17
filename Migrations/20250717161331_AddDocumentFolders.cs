using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntranetDocumentos.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentFolders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FolderId",
                table: "Documents",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DocumentFolders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ParentFolderId = table.Column<int>(type: "INTEGER", nullable: true),
                    DepartmentId = table.Column<int>(type: "INTEGER", nullable: true),
                    Color = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false),
                    Icon = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    Path = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    IsSystemFolder = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedById = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentFolders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentFolders_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentFolders_AspNetUsers_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DocumentFolders_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DocumentFolders_DocumentFolders_ParentFolderId",
                        column: x => x.ParentFolderId,
                        principalTable: "DocumentFolders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_FolderId",
                table: "Documents",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentFolders_CreatedById",
                table: "DocumentFolders",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentFolders_DepartmentId",
                table: "DocumentFolders",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentFolders_ParentFolderId_Name",
                table: "DocumentFolders",
                columns: new[] { "ParentFolderId", "Name" },
                unique: true,
                filter: "[ParentFolderId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentFolders_Path",
                table: "DocumentFolders",
                column: "Path");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentFolders_UpdatedById",
                table: "DocumentFolders",
                column: "UpdatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_DocumentFolders_FolderId",
                table: "Documents",
                column: "FolderId",
                principalTable: "DocumentFolders",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_DocumentFolders_FolderId",
                table: "Documents");

            migrationBuilder.DropTable(
                name: "DocumentFolders");

            migrationBuilder.DropIndex(
                name: "IX_Documents_FolderId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "FolderId",
                table: "Documents");
        }
    }
}
