using Almacen_STLCC.Data;
using Almacen_STLCC.Models.Categorias;
using Almacen_STLCC.Models.Productos;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Almacen_STLCC.Pages.Categorias
{
    public class IndexModel(ApplicationDbContext context) : PageModel
    {
        private readonly ApplicationDbContext _context = context;

        public List<Categoria>? Categorias { get; set; }

        public async Task OnGetAsync()
        {
            Categorias = await _context.Categorias
                .OrderBy(c => c.Nombre_Categoria)
                .ToListAsync();
        }
    }
}
