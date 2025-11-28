using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Almacen_STLCC.Data;
using Almacen_STLCC.Models.Productos;
using Almacen_STLCC.Models.Categorias;
using Almacen_STLCC.Models.Proveedores;
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
        public List<Proveedor> Proveedores { get; set; } = [];

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
            [Display(Name = "Marca del Producto")]
            public string? Marca { get; set; }

            [Required(ErrorMessage = "La categoría es obligatoria")]
            [Display(Name = "Categoría")]
            public int Id_Categoria { get; set; }

            [Required(ErrorMessage = "La unidad de medida es obligatoria")]
            [StringLength(50, ErrorMessage = "La unidad de medida no puede superar los 50 caracteres")]
            [Display(Name = "Unidad de Medida")]
            public required string Unidad_Medida { get; set; }

            [Display(Name = "Proveedor")]
            public int? Id_Proveedor { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await CargarDatosAsync();
            Input = new InputModel
            {
                Nombre_Producto = string.Empty,
                Marca = "S/M",
                Id_Categoria = Categorias.FirstOrDefault(c => c.Nombre_Categoria == "S/C")?.Id_Categoria ?? 0,
                Unidad_Medida = string.Empty
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Por favor corrija los errores en el formulario";
                await CargarDatosAsync();
                return Page();
            }

            var categoria = await _context.Categorias
                .FirstOrDefaultAsync(c => c.Id_Categoria == Input.Id_Categoria);

            if (categoria == null)
            {
                ErrorMessage = "La categoría seleccionada no existe.";
                await CargarDatosAsync();
                return Page();
            }

            var producto = new Producto
            {
                Codigo_Producto = Input.Codigo_Producto,
                Nombre_Producto = Input.Nombre_Producto.Trim(),
                Marca = Input.Marca?.Trim() ?? "S/M",
                Id_Categoria = Input.Id_Categoria,
                Unidad_Medida = Input.Unidad_Medida.Trim(),
                Id_Proveedor = Input.Id_Proveedor,
                Categoria = categoria // Inicializa el miembro requerido
            };

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Producto '{producto.Nombre_Producto}' creado exitosamente";
            return RedirectToPage("/Productos/Index");
        }

        private async Task CargarDatosAsync()
        {
            Categorias = await _context.Categorias
                .OrderBy(c => c.Nombre_Categoria)
                .ToListAsync();

            Proveedores = await _context.Proveedores
                .OrderBy(p => p.Nombre_Proveedor)
                .ToListAsync();
        }
    }
}