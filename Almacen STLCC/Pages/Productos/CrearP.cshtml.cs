using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Almacen_STLCC.Data;
using Almacen_STLCC.Models.Productos;
using Almacen_STLCC.Models.Categorias;
using Almacen_STLCC.Models.Proveedores;
using System.ComponentModel.DataAnnotations;

namespace Almacen_STLCC.Pages.Productos
{
    public class CrearPModel : SecurePageModel
    {
        private readonly ApplicationDbContext _context;

        public CrearPModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public List<Categoria> Categorias { get; set; } = new();
        public List<Proveedor> Proveedores { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "El código del producto es obligatorio")]
            [Display(Name = "Código del Producto")]
            public int Codigo_Producto { get; set; }

            [Required(ErrorMessage = "El nombre del producto es obligatorio")]
            [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
            [Display(Name = "Nombre del Producto")]
            public string Nombre_Producto { get; set; }

            [StringLength(50, ErrorMessage = "La marca no puede superar los 50 caracteres")]
            [Display(Name = "Marca del Producto")]
            public string Marca { get; set; }

            [Required(ErrorMessage = "La categoría es obligatoria")]
            [Display(Name = "Categoría")]
            public int Id_Categoria { get; set; }

            [Required(ErrorMessage = "La unidad de medida es obligatoria")]
            [StringLength(50, ErrorMessage = "La unidad de medida no puede superar los 50 caracteres")]
            [Display(Name = "Unidad de Medida")]
            public string Unidad_Medida { get; set; }

            [Display(Name = "Proveedor")]
            public int? Id_Proveedor { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await CargarDatosAsync();
            Input = new InputModel
            {
                Marca = "Sin Marca",
                Id_Categoria = Categorias.FirstOrDefault(c => c.Nombre_Categoria == "Sin Categoría")?.Id_Categoria ?? 0
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

            var producto = new Producto
            {
                Codigo_Producto = Input.Codigo_Producto,
                Nombre_Producto = Input.Nombre_Producto.Trim(),
                Marca = Input.Marca.Trim(),
                Id_Categoria = Input.Id_Categoria,
                Unidad_Medida = Input.Unidad_Medida.Trim(),
                Id_Proveedor = Input.Id_Proveedor
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