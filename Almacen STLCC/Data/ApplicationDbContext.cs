using Almacen_STLCC.Models.Usuarios;
using Almacen_STLCC.Models.Categorias;
using Almacen_STLCC.Models.Proveedores;
using Almacen_STLCC.Models.Productos;
using Almacen_STLCC.Models.Actas;
using Almacen_STLCC.Models.Movimientos;
using Microsoft.EntityFrameworkCore;

namespace Almacen_STLCC.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Acta> Actas { get; set; }
        public DbSet<DetalleActa> DetallesActa { get; set; }
        public DbSet<Movimiento> Movimientos { get; set; }
        public DbSet<Anexo> Anexos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar DELETE CASCADE en Anexos
            modelBuilder.Entity<Anexo>()
                .HasOne(a => a.Acta)
                .WithMany(ac => ac.Anexos)
                .HasForeignKey(a => a.Id_Acta)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}