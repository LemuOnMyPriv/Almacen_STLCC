using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Almacen_STLCC.Models.Productos;
using Almacen_STLCC.Data;

namespace Almacen_STLCC.Pages.Productos
{
    public class IndexModel(ApplicationDbContext context) : SecurePageModel
    {
        private readonly ApplicationDbContext _context = context;

        public List<Producto> Productos { get; set; } = [];
        public Dictionary<int, int> Inventarios { get; set; } = [];
        public Dictionary<int, List<string>> ProveedoresPorProducto { get; set; } = [];

        public async Task OnGetAsync()
        {
            Productos = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.ProductoProveedores)
                    .ThenInclude(pp => pp.Proveedor)
                .OrderBy(p => p.Nombre_Producto)
                .ToListAsync();

            var movimientos = await _context.Movimientos
                .GroupBy(m => m.Id_Producto)
                .Select(g => new
                {
                    IdProducto = g.Key,
                    InventarioActual = g.Sum(m =>
                        m.Tipo_Movimiento == "entrada" ? m.Cantidad :
                        m.Tipo_Movimiento == "salida" ? -m.Cantidad : 0)
                })
                .ToListAsync();

            Inventarios = movimientos.ToDictionary(m => m.IdProducto, m => m.InventarioActual);

            foreach (var producto in Productos)
            {
                var proveedores = producto.ProductoProveedores
                    .OrderByDescending(pp => pp.Proveedor.Nombre_Proveedor)
                    .Select(pp => pp.Proveedor.Nombre_Proveedor)
                    .ToList();

                ProveedoresPorProducto[producto.Id_Producto] = proveedores;
            }
        }
    }
}