using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Odonto.Infrastructure.Context;

#nullable disable

namespace Odonto.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260821180000_ModelagemFuncionario")]
public sealed class ModelagemFuncionario : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "FuncionarioId",
            table: "AspNetUsers",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "Funcionarios",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                NomeCompleto = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                NomeExibicao = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Telefone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                FotoKey = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Funcionarios", x => x.Id);
            });

        migrationBuilder.Sql(
            """
            IF EXISTS (
                SELECT 1
                FROM AspNetUsers
                WHERE LEN(LTRIM(RTRIM(Nome))) > 100
            )
                THROW 51000, 'Existem nomes de usuários com mais de 100 caracteres. Corrija-os antes de aplicar esta migration.', 1;

            UPDATE AspNetUsers
            SET FuncionarioId = NEWID()
            WHERE FuncionarioId IS NULL;

            INSERT INTO Funcionarios (Id, NomeCompleto, NomeExibicao, Telefone, FotoKey)
            SELECT
                FuncionarioId,
                CASE WHEN NULLIF(LTRIM(RTRIM(Nome)), '') IS NULL THEN N'Funcionário' ELSE LTRIM(RTRIM(Nome)) END,
                CASE WHEN NULLIF(LTRIM(RTRIM(Nome)), '') IS NULL THEN N'Funcionário' ELSE LTRIM(RTRIM(Nome)) END,
                NULL,
                NULL
            FROM AspNetUsers;
            """);

        migrationBuilder.AlterColumn<Guid>(
            name: "FuncionarioId",
            table: "AspNetUsers",
            type: "uniqueidentifier",
            nullable: false,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.DropColumn(
            name: "Nome",
            table: "AspNetUsers");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUsers_FuncionarioId",
            table: "AspNetUsers",
            column: "FuncionarioId",
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "FK_AspNetUsers_Funcionarios_FuncionarioId",
            table: "AspNetUsers",
            column: "FuncionarioId",
            principalTable: "Funcionarios",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Nome",
            table: "AspNetUsers",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.Sql(
            """
            UPDATE usuarios
            SET usuarios.Nome = funcionarios.NomeCompleto
            FROM AspNetUsers AS usuarios
            INNER JOIN Funcionarios AS funcionarios
                ON funcionarios.Id = usuarios.FuncionarioId;
            """);

        migrationBuilder.AlterColumn<string>(
            name: "Nome",
            table: "AspNetUsers",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)",
            oldNullable: true);

        migrationBuilder.DropForeignKey(
            name: "FK_AspNetUsers_Funcionarios_FuncionarioId",
            table: "AspNetUsers");

        migrationBuilder.DropIndex(
            name: "IX_AspNetUsers_FuncionarioId",
            table: "AspNetUsers");

        migrationBuilder.DropColumn(
            name: "FuncionarioId",
            table: "AspNetUsers");

        migrationBuilder.DropTable(
            name: "Funcionarios");
    }
}
