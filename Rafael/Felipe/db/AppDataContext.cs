using Microsoft.EntityFrameworkCore;	

public class AppDataContext : DbContext
{
    public DbSet<Folha> Folhas { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=Rafael_Felipe.db");
    }
}