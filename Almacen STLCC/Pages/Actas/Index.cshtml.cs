using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Almacen_STLCC.Data;
using Almacen_STLCC.Models.Actas;

namespace Almacen_STLCC.Pages.Actas
{
    public class IndexModel : SecurePageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Acta> Actas { get; set; } = new();

        public async Task OnGetAsync()
        {
            Actas = await _context.Actas
                .Include(a => a.Proveedor)
                .OrderByDescending(a => a.Fecha)
                .ToListAsync();
        }
    }
}