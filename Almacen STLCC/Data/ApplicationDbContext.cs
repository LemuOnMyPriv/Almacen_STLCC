using Almacen_STLCC.Models.Productos;
using Almacen_STLCC.Models.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace Almacen_STLCC.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Producto> Productos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
    }
}
