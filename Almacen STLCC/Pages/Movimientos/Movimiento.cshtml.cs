using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Almacen_STLCC.Data;
using Almacen_STLCC.Models.Movimientos;
using Almacen_STLCC.Models.Productos;
using Almacen_STLCC.Models.Actas;
using System.ComponentModel.DataAnnotations;

namespace Almacen_STLCC.Pages.Movimientos
{
    public class MovimientoModel(ApplicationDbContext context) : SecurePageModel
    {
        private readonly ApplicationDbContext _context = context;

        [BindProperty]
        public required InputModel Input { get; set; }

        public List<Producto> Productos { get; set; } = [];
        public List<Acta> Actas { get; set; } = [];

        public required string ErrorMessage { get; set; }
        public required string SuccessMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "El producto es obligatorio")]
            public int Id_Producto { get; set; }

            [Required(ErrorMessage = "El tipo de movimiento es obligatorio")]
            public required string Tipo_Movimiento { get; set; }

            [Required(ErrorMessage = "La cantidad es obligatoria")]
            [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
            public int Cantidad { get; set; }

            [Required(ErrorMessage = "La fecha es obligatoria")]
            public DateTime Fecha { get; set; }

            public int? Id_Acta { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            Productos = await _context.Productos
                .OrderBy(p => p.Nombre_Producto)
                .ToListAsync();

            Actas = await _context.Actas
                .Include(a => a.Proveedor)
                .OrderByDescending(a => a.Fecha)
                .ToListAsync();

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

            // Validar que el tipo de movimiento sea válido
            if (Input.Tipo_Movimiento != "entrada" &&
                Input.Tipo_Movimiento != "salida" &&
                Input.Tipo_Movimiento != "ajuste")
            {
                ErrorMessage = "Tipo de movimiento inválido";
                await CargarDatos();
                return Page();
            }

            // Obtener el producto
            var producto = await _context.Productos.FindAsync(Input.Id_Producto);
            if (producto == null)
            {
                ErrorMessage = "Producto no encontrado";
                await CargarDatos();
                return Page();
            }

            // Verificar inventario disponible para salidas
            if (Input.Tipo_Movimiento == "salida")
            {
                var inventarioActual = await _context.Movimientos
                    .Where(m => m.Id_Producto == Input.Id_Producto)
                    .SumAsync(m => m.Tipo_Movimiento == "entrada" ? m.Cantidad :
                                  m.Tipo_Movimiento == "salida" ? -m.Cantidad : 0);

                if (inventarioActual < Input.Cantidad)
                {
                    ErrorMessage = $"Inventario insuficiente. Disponible: {inventarioActual}";
                    await CargarDatos();
                    return Page();
                }
            }

            // Crear el movimiento
            var movimiento = new Movimiento
            {
                Id_Producto = Input.Id_Producto,
                Tipo_Movimiento = Input.Tipo_Movimiento,
                Cantidad = Input.Cantidad,
                Fecha = Input.Fecha,
                Id_Acta = Input.Id_Acta,
                Producto = producto
            };

            _context.Movimientos.Add(movimiento);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Movimiento de {Input.Tipo_Movimiento} registrado exitosamente";
            return RedirectToPage("/Movimientos/Index");
        }

        private async Task CargarDatos()
        {
            Productos = await _context.Productos
                .OrderBy(p => p.Nombre_Producto)
                .ToListAsync();

            Actas = await _context.Actas
                .Include(a => a.Proveedor)
                .OrderByDescending(a => a.Fecha)
                .ToListAsync();
        }
    }
}