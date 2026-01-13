using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Almacen_STLCC.Data;
using Almacen_STLCC.Models.Productos;
using Almacen_STLCC.Models.Categorias;
using System.ComponentModel.DataAnnotations;

namespace Almacen_STLCC.Pages.Productos
{
    public class EditarModel(ApplicationDbContext context) : SecurePageModel
    {
        private readonly ApplicationDbContext _context = context;

        [BindProperty]
        public required InputModel Input { get; set; }

        public required string ErrorMessage { get; set; }
        public List<Categoria> Categorias { get; set; } = new();

        public class InputModel
        {
            public int Id_Producto { get; set; }

            [Required(ErrorMessage = "El código del producto es obligatorio")]
            public int Codigo_Producto { get; set; }

            [Required(ErrorMessage = "El nombre del producto es obligatorio")]
            [StringLength(100)]
            public required string Nombre_Producto { get; set; }

            [StringLength(50)]
            public string? Marca { get; set; }

            [Required(ErrorMessage = "La categoría es obligatoria")]
            public int Id_Categoria { get; set; }

            [Required(ErrorMessage = "La unidad de medida es obligatoria")]
            [StringLength(50)]
            public required string Unidad_Medida { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.Id_Producto == id);

            if (producto == null)
            {
                return RedirectToPage("/Productos/Index");
            }

            Input = new InputModel
            {
                Id_Producto = producto.Id_Producto,
                Codigo_Producto = producto.Codigo_Producto,
                Nombre_Producto = producto.Nombre_Producto,
                Marca = producto.Marca,
                Id_Categoria = producto.Id_Categoria,
                Unidad_Medida = producto.Unidad_Medida
            };

            await CargarCategorias();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Por favor corrija los errores en el formulario";
                await CargarCategorias();
                return Page();
            }

            var producto = await _context.Productos.FindAsync(Input.Id_Producto);

            if (producto == null)
            {
                ErrorMessage = "Producto no encontrado";
                await CargarCategorias();
                return Page();
            }

            // Verificar si el código ya existe en otro producto
            if (await _context.Productos.AnyAsync(p =>
                p.Codigo_Producto == Input.Codigo_Producto &&
                p.Id_Producto != Input.Id_Producto))
            {
                ErrorMessage = "El código del producto ya existe";
                await CargarCategorias();
                return Page();
            }

            // Actualizar el producto
            producto.Codigo_Producto = Input.Codigo_Producto;
            producto.Nombre_Producto = Input.Nombre_Producto.Trim();
            producto.Marca = Input.Marca?.Trim();
            producto.Id_Categoria = Input.Id_Categoria;
            producto.Unidad_Medida = Input.Unidad_Medida.Trim();

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Producto '{Input.Nombre_Producto}' actualizado exitosamente";
            return RedirectToPage("/Productos/Index");
        }

        private async Task CargarCategorias()
        {
            Categorias = await _context.Categorias
                .OrderBy(c => c.Nombre_Categoria)
                .ToListAsync();
        }
    }
}