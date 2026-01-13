using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Almacen_STLCC.Data;
using Almacen_STLCC.Models.Productos;
using Almacen_STLCC.Models.Categorias;
using System.ComponentModel.DataAnnotations;

namespace Almacen_STLCC.Pages.Productos
{
    public class CrearPModel(ApplicationDbContext context) : SecurePageModel
    {
        private readonly ApplicationDbContext _context = context;

        [BindProperty]
        public required InputModel Input { get; set; }

        public required string ErrorMessage { get; set; }
        public required string SuccessMessage { get; set; }

        public List<Categoria> Categorias { get; set; } = [];

        public class InputModel
        {
            [Required(ErrorMessage = "El código del producto es obligatorio")]
            [Display(Name = "Código del Producto")]
            public int Codigo_Producto { get; set; }

            [Required(ErrorMessage = "El nombre del producto es obligatorio")]
            [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
            [Display(Name = "Nombre del Producto")]
            public required string Nombre_Producto { get; set; }

            [StringLength(50, ErrorMessage = "La marca no puede superar los 50 caracteres")]
            [Display(Name = "Marca")]
            public string? Marca { get; set; }

            [Required(ErrorMessage = "La categoría es obligatoria")]
            [Display(Name = "Categoría")]
            public int Id_Categoria { get; set; }

            [Required(ErrorMessage = "La unidad de medida es obligatoria")]
            [StringLength(50, ErrorMessage = "La unidad de medida no puede superar los 50 caracteres")]
            [Display(Name = "Unidad de Medida")]
            public required string Unidad_Medida { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await CargarDatos();
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

            var categoria = await _context.Categorias.FindAsync(Input.Id_Categoria);
            if (categoria == null)
            {
                ErrorMessage = "La categoría seleccionada no existe";
                await CargarDatos();
                return Page();
            }

            var producto = new Producto
            {
                Codigo_Producto = Input.Codigo_Producto,
                Nombre_Producto = Input.Nombre_Producto.Trim(),
                Marca = Input.Marca?.Trim(),
                Id_Categoria = Input.Id_Categoria,
                Unidad_Medida = Input.Unidad_Medida.Trim(),
                Categoria = categoria
            };

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Producto '{producto.Nombre_Producto}' creado exitosamente";
            return RedirectToPage("/Productos/Index");
        }

        private async Task CargarDatos()
        {
            Categorias = await _context.Categorias
                .OrderBy(c => c.Nombre_Categoria)
                .ToListAsync();
        }
    }
}