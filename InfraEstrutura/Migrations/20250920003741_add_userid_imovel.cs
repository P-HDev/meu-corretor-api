using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraEstrutura.Migrations
{
    /// <inheritdoc />
    public partial class add_userid_imovel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Imoveis",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Imoveis_UserId",
                table: "Imoveis",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Imoveis_Users_UserId",
                table: "Imoveis",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Imoveis_Users_UserId",
                table: "Imoveis");

            migrationBuilder.DropIndex(
                name: "IX_Imoveis_UserId",
                table: "Imoveis");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Imoveis");
        }
    }
}
