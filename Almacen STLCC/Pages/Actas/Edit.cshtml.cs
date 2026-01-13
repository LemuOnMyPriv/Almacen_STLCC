using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Almacen_STLCC.Data;
using Almacen_STLCC.Models.Actas;
using Almacen_STLCC.Models.Proveedores;
using Almacen_STLCC.Models.Productos;
using System.ComponentModel.DataAnnotations;

namespace Almacen_STLCC.Pages.Actas
{
    public class EditModel(ApplicationDbContext context) : SecurePageModel
    {
        private readonly ApplicationDbContext _context = context;

        [BindProperty]
        public required InputModel Input { get; set; }

        public required string ErrorMessage { get; set; }
        public List<Proveedor> Proveedores { get; set; } = new();
        public List<Producto> Productos { get; set; } = new();

        public class InputModel
        {
            public int Id_Acta { get; set; }

            [Required(ErrorMessage = "El número de acta es obligatorio")]
            [StringLength(100)]
            public required string Numero_Acta { get; set; }

            [StringLength(100)]
            public string? Orden_Compra { get; set; }

            [Required(ErrorMessage = "El código F01 es obligatorio")]
            [StringLength(100)]
            public required string F01 { get; set; }

            [Required(ErrorMessage = "El proveedor es obligatorio")]
            public int Id_Proveedor { get; set; }

            [Required(ErrorMessage = "La fecha es obligatoria")]
            public DateTime Fecha { get; set; }

            public List<DetalleInputModel> Detalles { get; set; } = new();
        }

        public class DetalleInputModel
        {
            public int Id_Detalle { get; set; }

            [Required(ErrorMessage = "El producto es obligatorio")]
            public int Id_Producto { get; set; }

            [Required(ErrorMessage = "La cantidad es obligatoria")]
            [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
            public int Cantidad { get; set; }

            public decimal? Precio_Unitario { get; set; }

            public decimal? Precio_Con_Isv { get; set; }

            [StringLength(100)]
            public string? Requisicion { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var acta = await _context.Actas
                .Include(a => a.DetallesActa)
                .FirstOrDefaultAsync(a => a.Id_Acta == id);

            if (acta == null)
            {
                return RedirectToPage("/Actas/Index");
            }

            Input = new InputModel
            {
                Id_Acta = acta.Id_Acta,
                Numero_Acta = acta.Numero_Acta,
                Orden_Compra = acta.Orden_Compra,
                F01 = acta.F01,
                Id_Proveedor = acta.Id_Proveedor,
                Fecha = acta.Fecha,
                Detalles = acta.DetallesActa.Select(d => new DetalleInputModel
                {
                    Id_Detalle = d.Id_Detalle,
                    Id_Producto = d.Id_Producto,
                    Cantidad = d.Cantidad,
                    Precio_Unitario = d.Precio_Unitario,
                    Precio_Con_Isv = d.Precio_Con_Isv,
                    Requisicion = d.Requisicion
                }).ToList()
            };

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

            if (Input.Detalles == null || Input.Detalles.Count == 0)
            {
                ErrorMessage = "Debe agregar al menos un producto al acta";
                await CargarDatos();
                return Page();
            }

            var acta = await _context.Actas
                .Include(a => a.DetallesActa)
                .FirstOrDefaultAsync(a => a.Id_Acta == Input.Id_Acta);

            if (acta == null)
            {
                ErrorMessage = "Acta no encontrada";
                await CargarDatos();
                return Page();
            }

            // Verificar si el número ya existe en otra acta
            if (await _context.Actas.AnyAsync(a =>
                a.Numero_Acta == Input.Numero_Acta &&
                a.Id_Acta != Input.Id_Acta))
            {
                ErrorMessage = "Ya existe un acta con ese número";
                await CargarDatos();
                return Page();
            }

            // Actualizar datos del acta
            acta.Numero_Acta = Input.Numero_Acta.Trim();
            acta.Orden_Compra = Input.Orden_Compra?.Trim();
            acta.F01 = Input.F01.Trim();
            acta.Id_Proveedor = Input.Id_Proveedor;
            acta.Fecha = Input.Fecha;

            // Eliminar detalles anteriores
            _context.DetallesActa.RemoveRange(acta.DetallesActa);

            // Agregar nuevos detalles
            foreach (var detalle in Input.Detalles)
            {
                var nuevoDetalle = new DetalleActa
                {
                    Id_Acta = acta.Id_Acta,
                    Id_Producto = detalle.Id_Producto,
                    Cantidad = detalle.Cantidad,
                    Precio_Unitario = detalle.Precio_Unitario,
                    Precio_Con_Isv = detalle.Precio_Con_Isv,
                    Requisicion = detalle.Requisicion?.Trim(),
                    Acta = acta,
                    Producto = (await _context.Productos.FindAsync(detalle.Id_Producto))!
                };

                _context.DetallesActa.Add(nuevoDetalle);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Acta '{Input.Numero_Acta}' actualizada exitosamente";
            return RedirectToPage("/Actas/Index");
        }

        private async Task CargarDatos()
        {
            Proveedores = await _context.Proveedores
                .OrderBy(p => p.Nombre_Proveedor)
                .ToListAsync();

            Productos = await _context.Productos
                .Include(p => p.Categoria)
                .OrderBy(p => p.Nombre_Producto)
                .ToListAsync();
        }
    }
}