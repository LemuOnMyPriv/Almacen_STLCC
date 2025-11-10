using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Almacen_STLCC.Models;
using Almacen_STLCC.Data;

namespace Almacen_STLCC.Pages.Productos
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Producto> Productos { get; set; } = new();

        public async Task OnGetAsync()
        {

            Productos = await _context.Productos.ToListAsync();
        }
    }
}