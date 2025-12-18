using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Almacen_STLCC.Data;
using Almacen_STLCC.Models.Productos;
using Almacen_STLCC.Models.Proveedores;

namespace Almacen_STLCC.Pages.Productos
{
    public class GestionarProveedoresModel : SecurePageModel
    {
        private readonly ApplicationDbContext _context;

        public GestionarProveedoresModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Producto? Producto { get; set; }
        public List<ProductoProveedor> ProveedoresProducto { get; set; } = new();
        public List<Proveedor> ProveedoresDisponibles { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Producto = await _context.Productos
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.Id_Producto == id);

            if (Producto == null)
            {
                return RedirectToPage("/Productos/Index");
            }

            await CargarDatos(id);
            return Page();
        }

        public async Task<IActionResult> OnPostAgregarAsync(int IdProducto, int IdProveedor, bool EsPrincipal)
        {
            if (IdProveedor == 0)
            {
                TempData["ErrorMessage"] = "Debe seleccionar un proveedor";
                return RedirectToPage(new { id = IdProducto });
            }

            var existe = await _context.ProductoProveedores
                .AnyAsync(pp => pp.Id_Producto == IdProducto && pp.Id_Proveedor == IdProveedor);

            if (existe)
            {
                TempData["ErrorMessage"] = "Este proveedor ya está asociado al producto";
                return RedirectToPage(new { id = IdProducto });
            }

            var producto = await _context.Productos.FindAsync(IdProducto);
            var proveedor = await _context.Proveedores.FindAsync(IdProveedor);

            var nuevoProductoProveedor = new ProductoProveedor
            {
                Id_Producto = IdProducto,
                Id_Proveedor = IdProveedor,
                Producto = producto!,
                Proveedor = proveedor!
            };

            _context.ProductoProveedores.Add(nuevoProductoProveedor);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Proveedor agregado exitosamente";
            return RedirectToPage(new { id = IdProducto });
        }

        public async Task<IActionResult> OnPostEliminarAsync(int IdProductoProveedor)
        {
            var productoProveedor = await _context.ProductoProveedores
                .FirstOrDefaultAsync(pp => pp.Id_Producto_Proveedor == IdProductoProveedor);

            if (productoProveedor == null)
            {
                TempData["ErrorMessage"] = "Registro no encontrado";
                return RedirectToPage();
            }

            var idProducto = productoProveedor.Id_Producto;

            _context.ProductoProveedores.Remove(productoProveedor);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Proveedor eliminado exitosamente";
            return RedirectToPage(new { id = idProducto });
        }

        private async Task CargarDatos(int idProducto)
        {
            ProveedoresProducto = await _context.ProductoProveedores
                .Include(pp => pp.Proveedor)
                .Where(pp => pp.Id_Producto == idProducto)
                .OrderByDescending(pp => pp.Proveedor.Nombre_Proveedor)
                .ToListAsync();

            var proveedoresAsociados = ProveedoresProducto.Select(pp => pp.Id_Proveedor).ToList();

            ProveedoresDisponibles = await _context.Proveedores
                .Where(p => !proveedoresAsociados.Contains(p.Id_Proveedor))
                .OrderBy(p => p.Nombre_Proveedor)
                .ToListAsync();
        }
    }
}