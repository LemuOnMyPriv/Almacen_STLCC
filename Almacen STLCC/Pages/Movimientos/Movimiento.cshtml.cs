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
            [Range(0, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor o igual a 0")]
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

        // Handler para obtener inventario (llamado por AJAX)
        public async Task<IActionResult> OnGetInventarioAsync(int id)
        {
            var inventarioActual = await _context.Movimientos
                .Where(m => m.Id_Producto == id)
                .SumAsync(m => m.Cantidad);

            return new JsonResult(new { cantidad = inventarioActual });
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

            // Calcular inventario actual
            var inventarioActual = await _context.Movimientos
                .Where(m => m.Id_Producto == Input.Id_Producto)
                .SumAsync(m => m.Cantidad);

            // Validar y procesar según el tipo de movimiento
            int cantidadMovimiento = 0;

            switch (Input.Tipo_Movimiento)
            {
                case "entrada":
                    // Entrada: suma directamente la cantidad
                    cantidadMovimiento = Input.Cantidad;
                    break;

                case "salida":
                    // Salida: verifica inventario y resta
                    if (inventarioActual < Input.Cantidad)
                    {
                        ErrorMessage = $"Inventario insuficiente. Disponible: {inventarioActual}";
                        await CargarDatos();
                        return Page();
                    }
                    cantidadMovimiento = -Input.Cantidad;
                    break;

                case "ajuste":
                    // Ajuste: calcula la diferencia entre inventario actual y nueva cantidad
                    cantidadMovimiento = Input.Cantidad - inventarioActual;
                    break;
            }

            // Crear el movimiento
            var movimiento = new Movimiento
            {
                Id_Producto = Input.Id_Producto,
                Tipo_Movimiento = Input.Tipo_Movimiento,
                Cantidad = cantidadMovimiento, // Esta es la cantidad que se guardará
                Fecha = Input.Fecha,
                Id_Acta = Input.Id_Acta,
                Producto = producto
            };

            _context.Movimientos.Add(movimiento);
            await _context.SaveChangesAsync();

            var mensaje = Input.Tipo_Movimiento switch
            {
                "entrada" => $"Entrada de {Input.Cantidad} unidades registrada",
                "salida" => $"Salida de {Input.Cantidad} unidades registrada",
                "ajuste" => $"Ajuste registrado: Antes había: {inventarioActual}, ahora hay: {Input.Cantidad}",
                _ => "Movimiento registrado"
            };

            TempData["SuccessMessage"] = mensaje;
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