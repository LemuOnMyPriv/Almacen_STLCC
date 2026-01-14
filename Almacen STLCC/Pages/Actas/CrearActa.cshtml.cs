using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Almacen_STLCC.Data;
using Almacen_STLCC.Models.Actas;
using Almacen_STLCC.Models.Proveedores;
using Almacen_STLCC.Models.Productos;
using System.ComponentModel.DataAnnotations;

namespace Almacen_STLCC.Pages.Actas
{
    public class CrearActaModel(ApplicationDbContext context) : SecurePageModel
    {
        private readonly ApplicationDbContext _context = context;

        [BindProperty]
        public required InputModel Input { get; set; }

        public required string ErrorMessage { get; set; }
        public required string SuccessMessage { get; set; }

        public List<Proveedor> Proveedores { get; set; } = new();
        public List<Producto> Productos { get; set; } = new();

        public class InputModel
        {
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
            public DateTime Fecha { get; set; } = DateTime.Now;

            public List<DetalleInputModel> Detalles { get; set; } = new();
        }

        public class DetalleInputModel
        {
            [Required(ErrorMessage = "El producto es obligatorio")]
            public int Id_Producto { get; set; }

            [Required(ErrorMessage = "La cantidad es obligatoria")]
            [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
            public int Cantidad { get; set; }

            public decimal? Precio_Unitario { get; set; }

            public decimal? Precio_Con_Isv { get; set; }

            // NUEVA PROPIEDAD
            [StringLength(100, ErrorMessage = "La requisición no puede superar los 100 caracteres")]
            public string? Requisicion { get; set; }
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

            if (Input.Detalles == null || Input.Detalles.Count == 0)
            {
                ErrorMessage = "Debe agregar al menos un producto al acta";
                await CargarDatos();
                return Page();
            }

            // Verificar que no exista un acta con el mismo número
            if (await _context.Actas.AnyAsync(a => a.Numero_Acta == Input.Numero_Acta))
            {
                ErrorMessage = "Ya existe un acta con ese número";
                await CargarDatos();
                return Page();
            }

            // Crear el acta
            var acta = new Acta
            {
                Numero_Acta = Input.Numero_Acta.Trim(),
                Orden_Compra = Input.Orden_Compra?.Trim(),
                F01 = Input.F01.Trim(),
                Id_Proveedor = Input.Id_Proveedor,
                Fecha = Input.Fecha,
                Proveedor = (await _context.Proveedores.FindAsync(Input.Id_Proveedor))!
            };

            _context.Actas.Add(acta);
            await _context.SaveChangesAsync();

            // Agregar los detalles
            foreach (var detalle in Input.Detalles)
            {
                var detalleActa = new DetalleActa
                {
                    Id_Acta = acta.Id_Acta,
                    Id_Producto = detalle.Id_Producto,
                    Cantidad = detalle.Cantidad,
                    Precio_Unitario = detalle.Precio_Unitario,
                    Precio_Con_Isv = detalle.Precio_Con_Isv,
                    Requisicion = detalle.Requisicion?.Trim(), // GUARDAR REQUISICIÓN
                    Acta = acta,
                    Producto = (await _context.Productos.FindAsync(detalle.Id_Producto))!
                };

                _context.DetallesActa.Add(detalleActa);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Acta '{Input.Numero_Acta}' creada exitosamente";
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