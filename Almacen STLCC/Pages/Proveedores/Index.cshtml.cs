using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Almacen_STLCC.Models.Proveedores;
using Almacen_STLCC.Data;

namespace Almacen_STLCC.Pages.Proveedores
{
    public class IndexModel : SecurePageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Proveedor> Proveedores { get; set; } = new();

        public async Task OnGetAsync()
        {
            Proveedores = await _context.Proveedores
                .ToListAsync();
        }
    }
}