using Almacen_STLCC.Data;
using Almacen_STLCC.Models.Categorias;
using Almacen_STLCC.Models.Productos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Almacen_STLCC.Pages.Categorias
{
    public class CrearCategoriaModel(ApplicationDbContext context) : SecurePageModel
    {
        private readonly ApplicationDbContext _context = context;

        [BindProperty]
        public required InputModel Input { get; set; }

        public required string ErrorMessage { get; set; }
        public required string SuccessMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "El nombre de la categoría es obligatorio")]
            [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
            [Display(Name = "Nombre de Categoría")]
            public required string Nombre_Categoria { get; set; }
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

            var categoria = new Categoria
            {
                Nombre_Categoria = Input.Nombre_Categoria.Trim()
            };

            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Categoría '{categoria.Nombre_Categoria}' creada exitosamente";
            return RedirectToPage("/Categorias/Index");
        }
    }
}
