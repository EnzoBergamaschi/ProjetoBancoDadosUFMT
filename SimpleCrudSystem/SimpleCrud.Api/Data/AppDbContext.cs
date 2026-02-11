using Microsoft.EntityFrameworkCore;
using SimpleCrud.Api.Models;

namespace SimpleCrud.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Item> Items { get; set; }
}
