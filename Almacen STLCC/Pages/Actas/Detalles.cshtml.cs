using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Almacen_STLCC.Data;
using Almacen_STLCC.Models.Actas;

namespace Almacen_STLCC.Pages.Actas
{
    public class DetallesModel(ApplicationDbContext context) : SecurePageModel
    {
        private readonly ApplicationDbContext _context = context;

        public Acta? Acta { get; set; } = null!;
        public List<DetalleActa> Detalles { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Acta = await _context.Actas
                .Include(a => a.Proveedor)
                .FirstOrDefaultAsync(a => a.Id_Acta == id);

            if (Acta == null)
            {
                return RedirectToPage("/Actas/Index");
            }

            Detalles = await _context.DetallesActa
                .Include(d => d.Producto)
                .Where(d => d.Id_Acta == id)
                .ToListAsync();

            return Page();
        }
    }
}