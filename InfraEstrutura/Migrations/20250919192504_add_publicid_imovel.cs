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
                nullable: true);

            // Garante extensão pgcrypto (para gen_random_uuid)
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pgcrypto;");

            // Preenche registros existentes com valor único (32 chars hex sem hífens)
            migrationBuilder.Sql(@"UPDATE ""Imoveis"" 
SET ""PublicId"" = COALESCE(""PublicId"", REPLACE(gen_random_uuid()::text,'-','')) 
WHERE ""PublicId"" IS NULL;");

            // Deduplica caso (improvável) existam colisões antes do índice único
            migrationBuilder.Sql(@"DO $$
DECLARE v_count int; 
BEGIN
  LOOP
    SELECT COUNT(*) INTO v_count FROM (
      SELECT ""PublicId"" FROM ""Imoveis"" GROUP BY ""PublicId"" HAVING COUNT(*) > 1
    ) t; 
    EXIT WHEN v_count = 0; 
    WITH d AS (
      SELECT ctid, ""PublicId"", ROW_NUMBER() OVER (PARTITION BY ""PublicId"" ORDER BY ctid) rn
      FROM ""Imoveis""
    )
    UPDATE ""Imoveis"" i SET ""PublicId"" = REPLACE(gen_random_uuid()::text,'-','')
    FROM d WHERE i.ctid = d.ctid AND d.rn > 1; 
  END LOOP; 
END $$;");

            // Torna NOT NULL
            migrationBuilder.Sql("ALTER TABLE \"Imoveis\" ALTER COLUMN \"PublicId\" SET NOT NULL;");

            // Define default para novos registros
            migrationBuilder.Sql("ALTER TABLE \"Imoveis\" ALTER COLUMN \"PublicId\" SET DEFAULT REPLACE(gen_random_uuid()::text,'-','');");

            // Cria índice único
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
