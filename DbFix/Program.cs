using System;
using System.Threading.Tasks;
using Npgsql;

class Program
{
    private const string MigrationId = "20250920002139_add_corretortelefone_imovel";
    private const string ProductVersion = "9.0.9"; // mesmo do snapshot

    static async Task<int> Main()
    {
        var connString = Environment.GetEnvironmentVariable("DB_CONN") ??
                         "Host=localhost;Port=5432;Database=MeuCorretor;Username=postgres;Password=postgres";
        await using var conn = new NpgsqlConnection(connString);
        try
        {
            await conn.OpenAsync();
            Console.WriteLine($"[INFO] Conectado: {conn.Host}:{conn.Port}/{conn.Database}");

            bool tableExists = await ScalarBool(conn, "SELECT 1 FROM information_schema.tables WHERE table_name='Imoveis' AND table_schema='public' LIMIT 1");
            if (!tableExists)
            {
                Console.WriteLine("[ERRO] Tabela Imoveis não encontrada. Abortando.");
                return 2;
            }

            bool colExists = await ScalarBool(conn, "SELECT 1 FROM information_schema.columns WHERE table_name='Imoveis' AND column_name='CorretorTelefone' AND table_schema='public' LIMIT 1");
            if (!colExists)
            {
                Console.WriteLine("[INFO] Coluna CorretorTelefone ausente. Adicionando...");
                await Exec(conn, "ALTER TABLE \"Imoveis\" ADD COLUMN \"CorretorTelefone\" character varying(25) NOT NULL DEFAULT ''");
                await Exec(conn, "ALTER TABLE \"Imoveis\" ALTER COLUMN \"CorretorTelefone\" DROP DEFAULT");
                Console.WriteLine("[OK] Coluna criada.");
            }
            else
            {
                Console.WriteLine("[INFO] Coluna CorretorTelefone já existe. Validando constraints...");
                // Força NOT NULL se por acaso estiver nullable
                await Exec(conn, "ALTER TABLE \"Imoveis\" ALTER COLUMN \"CorretorTelefone\" SET NOT NULL");
                // Ajusta tamanho se diferente (>25 mantém, se menor ignora)
                await Exec(conn, "ALTER TABLE \"Imoveis\" ALTER COLUMN \"CorretorTelefone\" TYPE character varying(25)");
                Console.WriteLine("[OK] Coluna alinhada.");
            }

            bool migRowExists = await ScalarBool(conn, "SELECT 1 FROM \"__EFMigrationsHistory\" WHERE \"MigrationId\"=@id LIMIT 1", ("@id", MigrationId));
            if (!migRowExists)
            {
                Console.WriteLine("[INFO] MigrationId ausente no histórico. Registrando...");
                await Exec(conn, "INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES (@id,@pv)", ("@id", MigrationId), ("@pv", ProductVersion));
                Console.WriteLine("[OK] Migration registrada.");
            }
            else
            {
                Console.WriteLine("[INFO] MigrationId já registrada no histórico.");
            }

            Console.WriteLine("[SUCESSO] Banco alinhado com o modelo (CorretorTelefone presente e migration registrada).");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine("[FALHA] " + ex.Message);
            Console.WriteLine(ex);
            return 1;
        }
    }

    private static async Task<bool> ScalarBool(NpgsqlConnection conn, string sql, params (string name, object value)[] p)
    {
        await using var cmd = new NpgsqlCommand(sql, conn);
        foreach (var (name, value) in p) cmd.Parameters.AddWithValue(name, value);
        var obj = await cmd.ExecuteScalarAsync();
        return obj != null;
    }

    private static async Task Exec(NpgsqlConnection conn, string sql, params (string name, object value)[] p)
    {
        await using var cmd = new NpgsqlCommand(sql, conn);
        foreach (var (name, value) in p) cmd.Parameters.AddWithValue(name, value);
        await cmd.ExecuteNonQueryAsync();
    }
}

