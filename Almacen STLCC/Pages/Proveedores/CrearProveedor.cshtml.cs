using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Almacen_STLCC.Data;
using Almacen_STLCC.Models.Proveedores;
using System.ComponentModel.DataAnnotations;

namespace Almacen_STLCC.Pages.Proveedores
{
    public class CrearProveedorModel(ApplicationDbContext context) : SecurePageModel
    {
        private readonly ApplicationDbContext _context = context;

        [BindProperty]
        public required InputModel Input { get; set; }

        public required string ErrorMessage { get; set; }
        public required string SuccessMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "El nombre del proveedor es obligatorio")]
            [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
            [Display(Name = "Nombre del Proveedor")]
            public required string Nombre_Proveedor { get; set; }

            [Required(ErrorMessage = "El RTN es obligatorio")]
            [StringLength(20, ErrorMessage = "El RTN no puede superar los 20 caracteres")]
            [Display(Name = "RTN")]
            public required string Rtn { get; set; }
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Por favor corrija los errores en el formulario";
                return Page();
            }

            if (await _context.Proveedores.AnyAsync(p => p.Nombre_Proveedor == Input.Nombre_Proveedor && p.Rtn == Input.Rtn))
            {
                ErrorMessage = "El nombre del proveedor y el RTN ya existen";
                return Page();
            }

            if (await _context.Proveedores.AnyAsync(p => p.Nombre_Proveedor == Input.Nombre_Proveedor))
            {
                ErrorMessage = "El nombre del proveedor ya existe";
                return Page();
            }

            if (await _context.Proveedores.AnyAsync(p => p.Rtn == Input.Rtn))
            {
                ErrorMessage = "El RTN ya existe";
                return Page();
            }

            var proveedor = new Proveedor
            {
                Nombre_Proveedor = Input.Nombre_Proveedor.Trim(),
                Rtn = Input.Rtn.Trim(),
            };

            _context.Proveedores.Add(proveedor);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Proveedor '{proveedor.Nombre_Proveedor}' creado exitosamente";
            return RedirectToPage("/Proveedores/Index");
        }
    }
}