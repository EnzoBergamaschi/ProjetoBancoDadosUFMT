using Microsoft.EntityFrameworkCore;
using SimpleCrud.Api.Models;

namespace SimpleCrud.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Ambulatorio> Ambulatorios { get; set; }
    public DbSet<Medico> Medicos { get; set; }
    public DbSet<Paciente> Pacientes { get; set; }
    public DbSet<Consulta> Consultas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Consulta>()
            .HasKey(c => new { c.codm, c.codp, c.data, c.hora });
    }
}
