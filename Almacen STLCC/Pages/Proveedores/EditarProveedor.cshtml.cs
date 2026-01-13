using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Almacen_STLCC.Data;
using Almacen_STLCC.Models.Proveedores;
using System.ComponentModel.DataAnnotations;

namespace Almacen_STLCC.Pages.Proveedores
{
    public class EditarProveedorModel(ApplicationDbContext context) : SecurePageModel
    {
        private readonly ApplicationDbContext _context = context;

        [BindProperty]
        public required InputModel Input { get; set; }

        public required string ErrorMessage { get; set; }

        public class InputModel
        {
            public int Id_Proveedor { get; set; }

            [Required(ErrorMessage = "El nombre del proveedor es obligatorio")]
            [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
            public required string Nombre_Proveedor { get; set; }

            [Required(ErrorMessage = "El RTN es obligatorio")]
            [StringLength(20, ErrorMessage = "El RTN no puede superar los 20 caracteres")]
            public required string Rtn { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var proveedor = await _context.Proveedores.FindAsync(id);

            if (proveedor == null)
            {
                return RedirectToPage("/Proveedores/Index");
            }

            Input = new InputModel
            {
                Id_Proveedor = proveedor.Id_Proveedor,
                Nombre_Proveedor = proveedor.Nombre_Proveedor,
                Rtn = proveedor.Rtn
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Por favor corrija los errores en el formulario";
                return Page();
            }

            var proveedor = await _context.Proveedores.FindAsync(Input.Id_Proveedor);

            if (proveedor == null)
            {
                ErrorMessage = "Proveedor no encontrado";
                return Page();
            }

            // Verificar si el nombre ya existe en otro proveedor
            if (await _context.Proveedores.AnyAsync(p =>
                p.Nombre_Proveedor == Input.Nombre_Proveedor &&
                p.Id_Proveedor != Input.Id_Proveedor))
            {
                ErrorMessage = "El nombre del proveedor ya existe";
                return Page();
            }

            // Verificar si el RTN ya existe en otro proveedor
            if (await _context.Proveedores.AnyAsync(p =>
                p.Rtn == Input.Rtn &&
                p.Id_Proveedor != Input.Id_Proveedor))
            {
                ErrorMessage = "El RTN ya existe";
                return Page();
            }

            // Actualizar el proveedor
            proveedor.Nombre_Proveedor = Input.Nombre_Proveedor.Trim();
            proveedor.Rtn = Input.Rtn.Trim();

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Proveedor '{Input.Nombre_Proveedor}' actualizado exitosamente";
            return RedirectToPage("/Proveedores/Index");
        }
    }
}