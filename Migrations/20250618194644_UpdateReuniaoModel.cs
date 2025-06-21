using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntranetDocumentos.Migrations
{
    /// <inheritdoc />
    public partial class UpdateReuniaoModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResponsavelCadastro",
                table: "Reunioes");

            migrationBuilder.DropColumn(
                name: "Ramal",
                table: "ReuniaoParticipantes");

            migrationBuilder.DropColumn(
                name: "Setor",
                table: "ReuniaoParticipantes");

            migrationBuilder.RenameColumn(
                name: "VeiculoReuniao",
                table: "Reunioes",
                newName: "Veiculo");

            migrationBuilder.RenameColumn(
                name: "SalaReuniao",
                table: "Reunioes",
                newName: "Sala");

            migrationBuilder.RenameColumn(
                name: "LinkReuniao",
                table: "Reunioes",
                newName: "LinkOnline");

            migrationBuilder.RenameColumn(
                name: "DataCadastro",
                table: "Reunioes",
                newName: "ResponsavelUserId");

            migrationBuilder.RenameColumn(
                name: "Comentarios",
                table: "Reunioes",
                newName: "Observacoes");

            migrationBuilder.AlterColumn<string>(
                name: "Empresa",
                table: "Reunioes",
                type: "TEXT",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataCriacao",
                table: "Reunioes",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Titulo",
                table: "Reunioes",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DepartamentoId",
                table: "ReuniaoParticipantes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RamalId",
                table: "ReuniaoParticipantes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nome",
                table: "Departments",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                column: "Nome",
                value: "");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                column: "Nome",
                value: "");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                column: "Nome",
                value: "");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                column: "Nome",
                value: "");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 5,
                column: "Nome",
                value: "");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 6,
                column: "Nome",
                value: "");

            migrationBuilder.CreateIndex(
                name: "IX_Reunioes_ResponsavelUserId",
                table: "Reunioes",
                column: "ResponsavelUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReuniaoParticipantes_DepartamentoId",
                table: "ReuniaoParticipantes",
                column: "DepartamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_ReuniaoParticipantes_RamalId",
                table: "ReuniaoParticipantes",
                column: "RamalId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReuniaoParticipantes_Departments_DepartamentoId",
                table: "ReuniaoParticipantes",
                column: "DepartamentoId",
                principalTable: "Departments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReuniaoParticipantes_Ramais_RamalId",
                table: "ReuniaoParticipantes",
                column: "RamalId",
                principalTable: "Ramais",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reunioes_AspNetUsers_ResponsavelUserId",
                table: "Reunioes",
                column: "ResponsavelUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReuniaoParticipantes_Departments_DepartamentoId",
                table: "ReuniaoParticipantes");

            migrationBuilder.DropForeignKey(
                name: "FK_ReuniaoParticipantes_Ramais_RamalId",
                table: "ReuniaoParticipantes");

            migrationBuilder.DropForeignKey(
                name: "FK_Reunioes_AspNetUsers_ResponsavelUserId",
                table: "Reunioes");

            migrationBuilder.DropIndex(
                name: "IX_Reunioes_ResponsavelUserId",
                table: "Reunioes");

            migrationBuilder.DropIndex(
                name: "IX_ReuniaoParticipantes_DepartamentoId",
                table: "ReuniaoParticipantes");

            migrationBuilder.DropIndex(
                name: "IX_ReuniaoParticipantes_RamalId",
                table: "ReuniaoParticipantes");

            migrationBuilder.DropColumn(
                name: "DataCriacao",
                table: "Reunioes");

            migrationBuilder.DropColumn(
                name: "Titulo",
                table: "Reunioes");

            migrationBuilder.DropColumn(
                name: "DepartamentoId",
                table: "ReuniaoParticipantes");

            migrationBuilder.DropColumn(
                name: "RamalId",
                table: "ReuniaoParticipantes");

            migrationBuilder.DropColumn(
                name: "Nome",
                table: "Departments");

            migrationBuilder.RenameColumn(
                name: "Veiculo",
                table: "Reunioes",
                newName: "VeiculoReuniao");

            migrationBuilder.RenameColumn(
                name: "Sala",
                table: "Reunioes",
                newName: "SalaReuniao");

            migrationBuilder.RenameColumn(
                name: "ResponsavelUserId",
                table: "Reunioes",
                newName: "DataCadastro");

            migrationBuilder.RenameColumn(
                name: "Observacoes",
                table: "Reunioes",
                newName: "Comentarios");

            migrationBuilder.RenameColumn(
                name: "LinkOnline",
                table: "Reunioes",
                newName: "LinkReuniao");

            migrationBuilder.AlterColumn<string>(
                name: "Empresa",
                table: "Reunioes",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsavelCadastro",
                table: "Reunioes",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Ramal",
                table: "ReuniaoParticipantes",
                type: "TEXT",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Setor",
                table: "ReuniaoParticipantes",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
