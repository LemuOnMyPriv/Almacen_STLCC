using Microsoft.AspNetCore.Mvc;
using Almacen_STLCC.Data;
using Almacen_STLCC.Models.Categorias;
using System.ComponentModel.DataAnnotations;

namespace Almacen_STLCC.Pages.Categorias
{
    public class CreateModel : SecurePageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public required InputModel Input { get; set; }

        public required string ErrorMessage { get; set; }
        public required string SuccessMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "El nombre de la categoría es obligatorio")]
            [StringLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres")]
            [Display(Name = "Nombre de la Categoría")]
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

            // Cerrar la ventana automáticamente
            return RedirectToPage("/Categorias/Index");
        }
    }
}