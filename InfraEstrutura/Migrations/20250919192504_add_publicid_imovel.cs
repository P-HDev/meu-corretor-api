using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraEstrutura.Migrations
{
    /// <inheritdoc />
    public partial class add_publicid_imovel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Adiciona coluna como nullable primeiro (se já existir, ignoramos via bloco PL/pgSQL)
            // Em cenários normais (migration ainda não aplicada) a coluna não existe.
            migrationBuilder.AddColumn<string>(
                name: "PublicId",
                table: "Imoveis",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Imoveis_PublicId",
                table: "Imoveis",
                column: "PublicId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Imoveis_PublicId",
                table: "Imoveis");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Imoveis");
        }
    }
}
