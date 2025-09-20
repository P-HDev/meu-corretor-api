using System;
using System.Linq;
using InfraEstrutura.ContextoBancoPsql;
using Microsoft.EntityFrameworkCore;
using Dominio.Builders;
using System.Threading.Tasks;

internal class Program
{
    private static async Task<int> Main()
    {
        var conn = Environment.GetEnvironmentVariable("DB_CONN") ?? "Host=localhost;Port=5432;Database=MeuCorretor;Username=postgres;Password=postgres";
        Console.WriteLine("[INFO] Usando connection string: " + conn);

        var opts = new DbContextOptionsBuilder<ContextoDb>()
            .UseNpgsql(conn)
            .EnableSensitiveDataLogging(false)
            .Options;

        try
        {
            await using var ctx = new ContextoDb(opts);
            Console.WriteLine("[INFO] Checando migrations pendentes...");
            var pending = (await ctx.Database.GetPendingMigrationsAsync()).ToList();
            if (pending.Any())
            {
                Console.WriteLine("[INFO] Migrations pendentes: " + string.Join(", ", pending));
                await ctx.Database.MigrateAsync();
                Console.WriteLine("[OK] Migrations aplicadas.");
            }
            else
            {
                Console.WriteLine("[OK] Nenhuma migration pendente.");
            }

            // Verifica existência da coluna CorretorTelefone via metadata do modelo e via banco
            Console.WriteLine("[INFO] Verificando coluna CorretorTelefone no snapshot do modelo...");
            var modelHasProperty = ctx.Model.FindEntityType(typeof(Dominio.Imovel))!
                .GetProperties().Any(p => p.Name == "CorretorTelefone");
            Console.WriteLine(" - Modelo contém propriedade: " + modelHasProperty);

            var colunaExiste = await ctx.Database.ExecuteSqlRawAsync(
                "SELECT 1 FROM information_schema.columns WHERE table_name='Imoveis' AND column_name='CorretorTelefone' AND table_schema='public' LIMIT 1") == -1; // ExecuteSqlRawAsync retorna -1 para comandos SELECT
            // Ajuste: como acima sempre retorna -1 para SELECT, vamos fazer leitura manual
            bool colunaBanco;
            await using (var cmd = ctx.Database.GetDbConnection().CreateCommand())
            {
                cmd.CommandText = "SELECT 1 FROM information_schema.columns WHERE table_name='Imoveis' AND column_name='CorretorTelefone' AND table_schema='public' LIMIT 1";
                if (cmd.Connection!.State != System.Data.ConnectionState.Open)
                    await cmd.Connection.OpenAsync();
                var r = await cmd.ExecuteScalarAsync();
                colunaBanco = r != null;            
            }
            Console.WriteLine(" - Coluna existe no banco: " + colunaBanco);

            if (!(modelHasProperty && colunaBanco))
            {
                Console.WriteLine("[ERRO] Modelo e/ou banco não alinhados para CorretorTelefone.");
                return 2;
            }

            // Teste de persistência: cria e remove um imóvel com telefone
            Console.WriteLine("[INFO] Testando persistência de telefone...");
            var telefoneTeste = "+5511998877665";
            var imovelTeste = ImovelBuilder.Novo()
                .ComTitulo("IMOVEL_TESTE_MIGRATION")
                .ComEndereco("Rua X 123")
                .ComDescricao("Teste telefone corretor")
                .ComStatus("À Venda")
                .ComPreco(123456)
                .ComArea(80)
                .ComQuartos(2)
                .ComBanheiros(1)
                .ComSuites(1)
                .ComVagas(1)
                .ComCorretorTelefone(telefoneTeste)
                .Build();

            await ctx.Imoveis.AddAsync(imovelTeste);
            await ctx.SaveChangesAsync();
            var id = imovelTeste.Id;
            var carregado = await ctx.Imoveis.FirstOrDefaultAsync(i => i.Id == id);
            var ok = carregado?.CorretorTelefone == telefoneTeste;
            Console.WriteLine(" - Persistência OK: " + ok);
            if (!ok)
            {
                Console.WriteLine("[ERRO] Telefone não persistiu corretamente.");
                return 3;
            }
            ctx.Imoveis.Remove(carregado!);
            await ctx.SaveChangesAsync();
            Console.WriteLine("[OK] Teste de criação/remoção concluído.");

            Console.WriteLine("[SUCESSO] Banco e migrations alinhados.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine("[FALHA] " + ex.Message);
            Console.WriteLine(ex);
            return 1;
        }
    }
}

