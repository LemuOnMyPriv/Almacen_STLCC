using Almacen_STLCC.Models.Usuarios;
using Almacen_STLCC.Models.Categorias;
using Almacen_STLCC.Models.Proveedores;
using Almacen_STLCC.Models.Productos;
using Almacen_STLCC.Models.Actas;
using Almacen_STLCC.Models.Movimientos;
using Almacen_STLCC.Models.Auditoria;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;

namespace Almacen_STLCC.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly IHttpContextAccessor? _httpContextAccessor;
        private readonly ILogger<ApplicationDbContext>? _logger;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            IHttpContextAccessor? httpContextAccessor = null,
            ILogger<ApplicationDbContext>? logger = null) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<ProductoProveedor> ProductoProveedores { get; set; }
        public DbSet<Acta> Actas { get; set; }
        public DbSet<DetalleActa> DetallesActa { get; set; }
        public DbSet<Movimiento> Movimientos { get; set; }
        public DbSet<Anexo> Anexos { get; set; }
        public DbSet<Auditoria> Auditorias { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Anexo>()
                .HasOne(a => a.Acta)
                .WithMany(ac => ac.Anexos)
                .HasForeignKey(a => a.Id_Acta)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductoProveedor>()
                .HasIndex(pp => new { pp.Id_Producto, pp.Id_Proveedor })
                .IsUnique();
        }

        public override int SaveChanges()
        {
            RegistrarAuditoria();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            RegistrarAuditoria();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void RegistrarAuditoria()
        {
            if (_httpContextAccessor?.HttpContext == null)
            {
                _logger?.LogWarning("HttpContext no disponible para auditoría. Probablemente ejecutándose en background o migración.");
                return;
            }
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added ||
                           e.State == EntityState.Modified ||
                           e.State == EntityState.Deleted)
                .Where(e => e.Entity.GetType() != typeof(Auditoria))
                .ToList();

            if (entries.Count == 0)
                return;

            var usuario = _httpContextAccessor?.HttpContext?.Session.GetString("DisplayName")
                       ?? _httpContextAccessor?.HttpContext?.Session.GetString("Username")
                       ?? "Sistema";

            _logger?.LogDebug("Registrando {Count} cambios en auditoría por usuario {Usuario}", entries.Count, usuario);

            foreach (var entry in entries)
            {
                try
                {
                    var auditoria = new Auditoria
                    {
                        Usuario = usuario,
                        Accion = ObtenerAccion(entry.State),
                        Tabla = ObtenerNombreTabla(entry.Entity),
                        Id_Registro = ObtenerIdRegistro(entry),
                        Descripcion = GenerarDescripcion(entry),
                        Fecha_Hora = DateTime.Now,
                        Ip_Address = _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString()
                    };

                    Auditorias.Add(auditoria);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error al crear registro de auditoría para {Entity}", entry.Entity.GetType().Name);
                }
            }
        }

        private static string ObtenerAccion(EntityState state)
        {
            return state switch
            {
                EntityState.Added => "CREAR",
                EntityState.Modified => "EDITAR",
                EntityState.Deleted => "ELIMINAR",
                _ => "DESCONOCIDO"
            };
        }

        private static string ObtenerNombreTabla(object entity)
        {
            return entity.GetType().Name.ToLower();
        }

        private static int ObtenerIdRegistro(EntityEntry entry)
        {
            if (entry.State == EntityState.Added)
            {
                return 0;
            }

            var keyProperty = entry.Properties
                .FirstOrDefault(p => p.Metadata.IsPrimaryKey());

            if (keyProperty?.CurrentValue != null)
            {
                return (int)Convert.ToInt32(keyProperty.CurrentValue);
            }

            return 0;
        }

        private static string GenerarDescripcion(EntityEntry entry)
        {
            var entityName = entry.Entity.GetType().Name;

            switch (entry.State)
            {
                case EntityState.Added:
                    return $"Creó {entityName}: {ObtenerNombreEntidad(entry)}";

                case EntityState.Modified:
                    var cambios = ObtenerCambios(entry);
                    return $"Editó {entityName}: {ObtenerNombreEntidad(entry)}. Cambios: {cambios}";

                case EntityState.Deleted:
                    return $"Eliminó {entityName}: {ObtenerNombreEntidad(entry)}";

                default:
                    return $"Acción en {entityName}";
            }
        }

        private static string ObtenerNombreEntidad(EntityEntry entry)
        {
            var entity = entry.Entity;

            var nombreProp = entity.GetType().GetProperty("Nombre_Producto") ??
                           entity.GetType().GetProperty("Nombre_Proveedor") ??
                           entity.GetType().GetProperty("Nombre_Categoria") ??
                           entity.GetType().GetProperty("NombreUsuario") ??
                           entity.GetType().GetProperty("Requisicion") ??
                           entity.GetType().GetProperty("Nombre_Archivo") ??
                           entity.GetType().GetProperty("Id_Producto_Proveedor") ??
                           entity.GetType().GetProperty("Numero_Acta");

            if (nombreProp != null)
            {
                var valor = nombreProp.GetValue(entity);
                return valor?.ToString() ?? "Sin nombre";
            }

            return "ID: " + ObtenerIdRegistro(entry);
        }

        private static string ObtenerCambios(EntityEntry entry)
        {
            var cambios = new List<string>();

            foreach (var property in entry.Properties)
            {
                if (property.IsModified &&
                    !property.Metadata.IsPrimaryKey() &&
                    property.Metadata.Name != "Fecha_Hora")
                {
                    if (property.Metadata.Name.ToLower().Contains("contraseña") ||
                        property.Metadata.Name.ToLower().Contains("password"))
                    {
                        cambios.Add("Contraseña: modificada");
                        continue;
                    }

                    var valorAnterior = property.OriginalValue?.ToString() ?? "vacío";
                    var valorNuevo = property.CurrentValue?.ToString() ?? "vacío";

                    if (valorAnterior != valorNuevo)
                    {
                        cambios.Add($"{property.Metadata.Name}: '{valorAnterior}' cambió a '{valorNuevo}'");
                    }
                }
            }

            return cambios.Count != 0 ? string.Join(", ", cambios) : "Sin cambios detectados";
        }
    }
}