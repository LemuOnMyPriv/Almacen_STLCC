using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Almacen_STLCC.Data;
using Almacen_STLCC.Models.Categorias;
using System.ComponentModel.DataAnnotations;

namespace Almacen_STLCC.Pages.Categorias
{
    public class EditModel(ApplicationDbContext context) : SecurePageModel
    {
        private readonly ApplicationDbContext _context = context;

        [BindProperty]
        public required InputModel Input { get; set; }

        public required string ErrorMessage { get; set; }

        public class InputModel
        {
            public int Id_Categoria { get; set; }

            [Required(ErrorMessage = "El nombre de la categoría es obligatorio")]
            [StringLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres")]
            public required string Nombre_Categoria { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);

            if (categoria == null)
            {
                return RedirectToPage("/Categorias/Index");
            }

            Input = new InputModel
            {
                Id_Categoria = categoria.Id_Categoria,
                Nombre_Categoria = categoria.Nombre_Categoria ?? ""
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

            var categoria = await _context.Categorias.FindAsync(Input.Id_Categoria);

            if (categoria == null)
            {
                ErrorMessage = "Categoría no encontrada";
                return Page();
            }

            // Verificar si el nombre ya existe en otra categoría
            if (await _context.Categorias.AnyAsync(c =>
                c.Nombre_Categoria == Input.Nombre_Categoria &&
                c.Id_Categoria != Input.Id_Categoria))
            {
                ErrorMessage = "El nombre de la categoría ya existe";
                return Page();
            }

            // Actualizar la categoría
            categoria.Nombre_Categoria = Input.Nombre_Categoria.Trim();

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Categoría '{Input.Nombre_Categoria}' actualizada exitosamente";
            return RedirectToPage("/Categorias/Index");
        }
    }
}