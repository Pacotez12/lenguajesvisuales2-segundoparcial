using LenguajesVisualesII.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LenguajesVisualesII.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<ArchivoCliente> ArchivosCliente => Set<ArchivoCliente>();
    public DbSet<LogApi> LogApis => Set<LogApi>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Cliente>()
            .HasMany(c => c.Archivos)
            .WithOne(a => a.Cliente!)
            .HasForeignKey(a => a.CICliente)
            .HasPrincipalKey(c => c.CI);

        modelBuilder.Entity<Cliente>().Property(c => c.FotoCasa1).HasColumnType("varbinary(max)");
        modelBuilder.Entity<Cliente>().Property(c => c.FotoCasa2).HasColumnType("varbinary(max)");
        modelBuilder.Entity<Cliente>().Property(c => c.FotoCasa3).HasColumnType("varbinary(max)");
    }
}
