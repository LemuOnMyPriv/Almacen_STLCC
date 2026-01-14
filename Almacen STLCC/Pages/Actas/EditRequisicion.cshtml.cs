using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Almacen_STLCC.Data;
using System.ComponentModel.DataAnnotations;

namespace Almacen_STLCC.Pages.Actas
{
    public class EditRequisicionModel(ApplicationDbContext context) : SecurePageModel
    {
        private readonly ApplicationDbContext _context = context;

        [BindProperty]
        public required InputModel Input { get; set; }

        public required string NombreProducto { get; set; }
        public required string NumeroActa { get; set; }
        public required string ErrorMessage { get; set; }

        public class InputModel
        {
            public int Id_Detalle { get; set; }
            public int Id_Acta { get; set; }

            [StringLength(100, ErrorMessage = "La requisición no puede superar los 100 caracteres")]
            public string? Requisicion { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var detalle = await _context.DetallesActa
                .Include(d => d.Producto)
                .Include(d => d.Acta)
                .FirstOrDefaultAsync(d => d.Id_Detalle == id);

            if (detalle == null)
            {
                return RedirectToPage("/Actas/Index");
            }

            Input = new InputModel
            {
                Id_Detalle = detalle.Id_Detalle,
                Id_Acta = detalle.Id_Acta,
                Requisicion = detalle.Requisicion
            };

            NombreProducto = detalle.Producto.Nombre_Producto;
            NumeroActa = detalle.Acta.Numero_Acta;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Por favor corrija los errores en el formulario";
                await CargarDatos();
                return Page();
            }

            var detalle = await _context.DetallesActa
                .Include(d => d.Producto)
                .Include(d => d.Acta)
                .FirstOrDefaultAsync(d => d.Id_Detalle == Input.Id_Detalle);

            if (detalle == null)
            {
                ErrorMessage = "Detalle no encontrado";
                await CargarDatos();
                return Page();
            }

            // Actualizar requisición (puede ser null para removerla)
            detalle.Requisicion = string.IsNullOrWhiteSpace(Input.Requisicion)
                ? null
                : Input.Requisicion.Trim();

            await _context.SaveChangesAsync();

            var mensaje = string.IsNullOrWhiteSpace(Input.Requisicion)
                ? "Requisición removida exitosamente"
                : "Requisición actualizada exitosamente";

            TempData["SuccessMessage"] = mensaje;
            return RedirectToPage("/Actas/Detalles", new { id = Input.Id_Acta });
        }

        private async Task CargarDatos()
        {
            var detalle = await _context.DetallesActa
                .Include(d => d.Producto)
                .Include(d => d.Acta)
                .FirstOrDefaultAsync(d => d.Id_Detalle == Input.Id_Detalle);

            if (detalle != null)
            {
                NombreProducto = detalle.Producto.Nombre_Producto;
                NumeroActa = detalle.Acta.Numero_Acta;
            }
        }
    }
}