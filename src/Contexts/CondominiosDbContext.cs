using AuxiAPI.src.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuxiAPI.src.Contexts
{
    public class CondominiosDbContext(DbContextOptions<CondominiosDbContext> options) : DbContext(options)
    {
        public DbSet<Condominio> Condominios { get; set; }
        public DbSet<CacheEntry> Cache { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CondominiosDbContext).Assembly);

            modelBuilder
                .HasDbFunction(typeof(PostgresDbFunctions)
                    .GetMethod(nameof(PostgresDbFunctions.Unaccent), [typeof(string)])!)
                .HasName("unaccent")
                .HasSchema("public");
        }
    }
}