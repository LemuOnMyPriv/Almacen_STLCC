using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Almacen_STLCC.Data;
using Almacen_STLCC.Models.Proveedores;
using System.ComponentModel.DataAnnotations;

namespace Almacen_STLCC.Pages.Proveedores
{
    public class CrearProveedorModel : SecurePageModel
    {
        private readonly ApplicationDbContext _context;

        public CrearProveedorModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "El nombre del proveedor es obligatorio")]
            [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
            [Display(Name = "Nombre del Proveedor")]
            public string Nombre_Proveedor { get; set; }

            [Required(ErrorMessage = "El RTN es obligatorio")]
            [StringLength(20, ErrorMessage = "El RTN no puede superar los 20 caracteres")]
            [Display(Name = "RTN")]
            public string Rtn { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
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