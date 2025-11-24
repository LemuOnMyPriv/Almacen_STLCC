using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Almacen_STLCC.Models.Productos;
using Almacen_STLCC.Data;

namespace Almacen_STLCC.Pages.Productos
{
    public class IndexModel : SecurePageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Producto> Productos { get; set; } = new();
        public Dictionary<int, int> Inventarios { get; set; } = new();

        public async Task OnGetAsync()
        {
            Productos = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Proveedor)
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
        }
    }
}