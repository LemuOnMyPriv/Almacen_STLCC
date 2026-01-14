using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Almacen_STLCC.Data;
using Almacen_STLCC.Models.Movimientos;

namespace Almacen_STLCC.Pages.Movimientos
{
    public class IndexModel(ApplicationDbContext context) : SecurePageModel
    {
        private readonly ApplicationDbContext _context = context;

        public List<MovimientoConDetalles> Movimientos { get; set; } = [];

        public class MovimientoConDetalles
        {
            public required Movimiento Movimiento { get; set; }
            public string NombreProducto { get; set; } = string.Empty;
            public string NumeroActa { get; set; } = string.Empty;
        }

        public async Task OnGetAsync()
        {
            var movimientos = await _context.Movimientos
                .Include(m => m.Producto)
                .Include(m => m.Acta)
                .OrderByDescending(m => m.Fecha)
                .ToListAsync();

            Movimientos = movimientos.Select(m => new MovimientoConDetalles
            {
                Movimiento = m,
                NombreProducto = m.Producto.Nombre_Producto,
                NumeroActa = m.Acta?.Numero_Acta ?? "N/A"
            }).ToList();
        }
    }
}