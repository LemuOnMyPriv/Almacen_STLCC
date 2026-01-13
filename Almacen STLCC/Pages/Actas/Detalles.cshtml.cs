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
        public int CantidadRequisiciones { get; set; } // AGREGAR ESTO

        public async Task<IActionResult> OnGetAsync(int id)
        {
            await CargarDatos(id);
            return Page();
        }

        public async Task<IActionResult> OnPostActualizarRequisicionAsync(int idDetalle, string? requisicion)
        {
            var detalle = await _context.DetallesActa.FindAsync(idDetalle);

            if (detalle == null)
            {
                TempData["ErrorMessage"] = "No se encontró el producto";
                return RedirectToPage();
            }

            // Actualizar la requisición (puede ser null o vacío para removerla)
            detalle.Requisicion = string.IsNullOrWhiteSpace(requisicion) ? null : requisicion.Trim();

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Requisición actualizada exitosamente";
            return RedirectToPage(new { id = detalle.Id_Acta });
        }

        private async Task CargarDatos(int id)
        {
            Acta = await _context.Actas
                .Include(a => a.Proveedor)
                .FirstOrDefaultAsync(a => a.Id_Acta == id);

            if (Acta == null)
            {
                return;
            }

            Detalles = await _context.DetallesActa
                .Include(d => d.Producto)
                .Where(d => d.Id_Acta == id)
                .ToListAsync();

<<<<<<< HEAD
            // Extraer requisiciones únicas
            RequisicionesUnicas = Detalles
                .Where(d => !string.IsNullOrWhiteSpace(d.Requisicion))
                .Select(d => d.Requisicion!)
                .Distinct()
                .OrderBy(r => r)
                .ToList();

            // Agrupar productos por requisición para mostrar en la vista
            ProductosPorRequisicion = Detalles
                .Where(d => !string.IsNullOrWhiteSpace(d.Requisicion))
                .GroupBy(d => d.Requisicion!)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToList()
                );
=======
            CantidadRequisiciones = await _context.ActasRequisiciones
                .Where(r => r.Id_Acta == id)
                .CountAsync();

            return Page();
>>>>>>> parent of 7a13c5a (Cambios 09/01/2026)
        }
    }
}