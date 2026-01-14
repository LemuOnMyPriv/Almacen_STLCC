using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Almacen_STLCC.Data;
using Almacen_STLCC.Models.Actas;

namespace Almacen_STLCC.Pages.Actas
{
    public class DetallesModel(ApplicationDbContext context) : SecurePageModel
    {
        private readonly ApplicationDbContext _context = context;

        public required Acta Acta { get; set; }
        public List<DetalleActa> Detalles { get; set; } = [];
        public int CantidadRequisiciones { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var acta = await _context.Actas
                .Include(a => a.Proveedor)
                .Include(a => a.Requisiciones)
                .FirstOrDefaultAsync(a => a.Id_Acta == id);

            if (acta == null)
            {
                return RedirectToPage("/Actas/Index");
            }

            Acta = acta;

            Detalles = await _context.DetallesActa
                .Include(d => d.Producto)
                .Where(d => d.Id_Acta == id)
                .OrderBy(d => d.Producto.Nombre_Producto)
                .ToListAsync();

            CantidadRequisiciones = acta.Requisiciones.Count;

            return Page();
        }
    }
}