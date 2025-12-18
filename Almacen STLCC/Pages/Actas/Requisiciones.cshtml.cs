using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Almacen_STLCC.Data;
using Almacen_STLCC.Models.Actas;
using System.ComponentModel.DataAnnotations;

namespace Almacen_STLCC.Pages.Actas
{
    public class RequisicionesModel(ApplicationDbContext context) : SecurePageModel
    {
        private readonly ApplicationDbContext _context = context;

        public Acta? Acta { get; set; } = null!;
        public List<ActaRequisicion> Requisiciones { get; set; } = new();

        [BindProperty]
        public required InputModel Input { get; set; }

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "La requisición es obligatoria")]
            [StringLength(100, ErrorMessage = "La requisición no puede superar los 100 caracteres")]
            public required string Requisicion { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Acta = await _context.Actas
                .Include(a => a.Proveedor)
                .FirstOrDefaultAsync(a => a.Id_Acta == id);

            if (Acta == null)
            {
                return RedirectToPage("/Actas/Index");
            }

            await CargarRequisicionesAsync(id);

            return Page();
        }

        public async Task<IActionResult> OnPostAgregarAsync(int id)
        {
            Acta = await _context.Actas
                .Include(a => a.Proveedor)
                .FirstOrDefaultAsync(a => a.Id_Acta == id);

            if (Acta == null)
            {
                return RedirectToPage("/Actas/Index");
            }

            if (!ModelState.IsValid)
            {
                await CargarRequisicionesAsync(id);
                ErrorMessage = "Por favor corrija los errores en el formulario";
                return Page();
            }

            var existe = await _context.ActasRequisiciones
                .AnyAsync(r => r.Id_Acta == id && r.Requisicion == Input.Requisicion.Trim());

            if (existe)
            {
                await CargarRequisicionesAsync(id);
                ErrorMessage = "Esta requisición ya está registrada para esta acta";
                return Page();
            }

            var nuevaRequisicion = new ActaRequisicion
            {
                Id_Acta = id,
                Requisicion = Input.Requisicion.Trim(),
                Acta = null!
            };

            _context.ActasRequisiciones.Add(nuevaRequisicion);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Requisición '{Input.Requisicion}' agregada exitosamente";
            return RedirectToPage(new { id });
        }

        private async Task CargarRequisicionesAsync(int idActa)
        {
            Requisiciones = await _context.ActasRequisiciones
                .Where(r => r.Id_Acta == idActa)
                .OrderBy(r => r.Requisicion)
                .ToListAsync();
        }
    }
}