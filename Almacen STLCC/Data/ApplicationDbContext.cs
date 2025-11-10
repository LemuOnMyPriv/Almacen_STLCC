using Almacen_STLCC.Models;
using Microsoft.EntityFrameworkCore;

namespace Almacen_STLCC.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Producto> Productos { get; set; }
        // Aquí defines tus DbSet para cada tabla
        // Ejemplo:
        // public DbSet<Producto> Productos { get; set; }
    }
}