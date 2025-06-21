using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntranetDocumentos.Migrations
{
    /// <inheritdoc />
    public partial class AddReunioes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cargo",
                table: "Ramais");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Ramais");

            migrationBuilder.AddColumn<int>(
                name: "TipoFuncionario",
                table: "Ramais",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Reunioes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Data = table.Column<DateTime>(type: "TEXT", nullable: false),
                    HoraInicio = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    HoraFim = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    TipoReuniao = table.Column<int>(type: "INTEGER", nullable: false),
                    SalaReuniao = table.Column<int>(type: "INTEGER", nullable: true),
                    VeiculoReuniao = table.Column<int>(type: "INTEGER", nullable: true),
                    LinkReuniao = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Empresa = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Comentarios = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    DataCadastro = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ResponsavelCadastro = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reunioes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReuniaoParticipantes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReuniaoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Ramal = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Setor = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReuniaoParticipantes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReuniaoParticipantes_Reunioes_ReuniaoId",
                        column: x => x.ReuniaoId,
                        principalTable: "Reunioes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReuniaoParticipantes_ReuniaoId",
                table: "ReuniaoParticipantes",
                column: "ReuniaoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReuniaoParticipantes");

            migrationBuilder.DropTable(
                name: "Reunioes");

            migrationBuilder.DropColumn(
                name: "TipoFuncionario",
                table: "Ramais");

            migrationBuilder.AddColumn<string>(
                name: "Cargo",
                table: "Ramais",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Ramais",
                type: "TEXT",
                maxLength: 150,
                nullable: true);
        }
    }
}
