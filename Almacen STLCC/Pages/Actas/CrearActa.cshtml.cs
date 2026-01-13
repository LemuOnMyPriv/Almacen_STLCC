using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Almacen_STLCC.Data;
using Almacen_STLCC.Models.Actas;
using Almacen_STLCC.Models.Proveedores;
using Almacen_STLCC.Models.Productos;
using System.ComponentModel.DataAnnotations;

namespace Almacen_STLCC.Pages.Actas
{
    public class CrearActaModel : SecurePageModel
    {
        private readonly ApplicationDbContext _context;

        public CrearActaModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public List<Proveedor> Proveedores { get; set; } = new();
        public List<Producto> Productos { get; set; } = new();

        public string ErrorMessage { get; set; } = string.Empty;
        public string SuccessMessage { get; set; } = string.Empty;

        public class InputModel
        {
            [Required(ErrorMessage = "El Numero de Acta es obligatorio")]
            [StringLength(100)]
            public string Numero_Acta { get; set; } = string.Empty;

            [Required(ErrorMessage = "El código F01 es obligatorio")]
            [StringLength(100)]
            public string F01 { get; set; } = string.Empty;

            [StringLength(100)]
            public string? Orden_Compra { get; set; }

            [Required(ErrorMessage = "Debe seleccionar un proveedor")]
            public int Id_Proveedor { get; set; }

            [Required(ErrorMessage = "La fecha es obligatoria")]
            public DateTime Fecha { get; set; } = DateTime.Today;

            public List<DetalleInputModel> Detalles { get; set; } = new();
        }

        public class DetalleInputModel
        {
            [Required]
            public int Id_Producto { get; set; }

            [Required]
            [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
            public int Cantidad { get; set; }

            public decimal? Precio_Unitario { get; set; }
            public decimal? Precio_Con_Isv { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            Proveedores = await _context.Proveedores
                .OrderBy(p => p.Nombre_Proveedor)
                .ToListAsync();

            Productos = await _context.Productos
                .Include(p => p.Categoria)
                .OrderBy(p => p.Nombre_Producto)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Proveedores = await _context.Proveedores.OrderBy(p => p.Nombre_Proveedor).ToListAsync();
            Productos = await _context.Productos.Include(p => p.Categoria).OrderBy(p => p.Nombre_Producto).ToListAsync();

            if (!ModelState.IsValid)
            {
                ErrorMessage = "Por favor corrija los errores en el formulario";
                return Page();
            }

            if (Input.Detalles == null || !Input.Detalles.Any())
            {
                ErrorMessage = "Debe agregar al menos un producto al acta";
                return Page();
            }

            if (await _context.Actas.AnyAsync(u => u.Numero_Acta == Input.Numero_Acta))
            {
                ErrorMessage = "El Numero de Acta ya existe";
                return Page();
            }

            try
            {
                var acta = new Acta
                {
                    Numero_Acta = Input.Numero_Acta.Trim(),
                    F01 = Input.F01.Trim(),
                    Orden_Compra = Input.Orden_Compra?.Trim(),
                    Id_Proveedor = Input.Id_Proveedor,
                    Fecha = Input.Fecha,
                    Proveedor = null!
                };

                _context.Actas.Add(acta);
                await _context.SaveChangesAsync();

                foreach (var detalle in Input.Detalles)
                {
                    var detalleActa = new DetalleActa
                    {
                        Id_Acta = acta.Id_Acta,
                        Id_Producto = detalle.Id_Producto,
                        Cantidad = detalle.Cantidad,
                        Precio_Unitario = detalle.Precio_Unitario,
                        Precio_Con_Isv = detalle.Precio_Con_Isv,
                        Acta = null!,
                        Producto = null!
                    };

                    _context.DetallesActa.Add(detalleActa);
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Acta '{acta.Numero_Acta}' creada exitosamente";
                return RedirectToPage("/Actas/Index");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al crear el acta: {ex.Message}";
                return Page();
            }
        }
    }
}