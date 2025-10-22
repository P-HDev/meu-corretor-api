using Dominio;
using Microsoft.EntityFrameworkCore;

namespace InfraEstrutura.ContextoBancoPsql;

public class ContextoDb(DbContextOptions<ContextoDb> options) : DbContext(options)
{
    public DbSet<Imovel> Imoveis { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Imovel>()
            .HasIndex(i => i.PublicId)
            .IsUnique();

        modelBuilder.Entity<Imovel>()
            .Property(i => i.CorretorTelefone)
            .HasMaxLength(25);

        modelBuilder.Entity<Imovel>()
            .Property(i => i.ImagensUrls)
            .HasConversion(
                v => string.Join(';', v),
                v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList())
            .HasColumnName("ImagensUrls");

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
        
        modelBuilder.Entity<User>()
            .Property(u => u.Nome).HasMaxLength(150);
        
        modelBuilder.Entity<User>()
            .Property(u => u.Email).HasMaxLength(180);
        
        modelBuilder.Entity<User>()
            .Property(u => u.Telefone).HasMaxLength(25);

        modelBuilder.Entity<Imovel>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Imovel>()
            .HasIndex(i => i.UserId);
    }
}