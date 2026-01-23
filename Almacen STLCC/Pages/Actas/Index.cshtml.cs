using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Almacen_STLCC.Data;
using Almacen_STLCC.Models.Actas;

namespace Almacen_STLCC.Pages.Actas
{
    public class IndexModel(ApplicationDbContext context) : SecurePageModel
    {
        private readonly ApplicationDbContext _context = context;

        public List<ActaConDetalles> Actas { get; set; } = [];

        public class ActaConDetalles
        {
            public required Acta Acta { get; set; }
            public List<string> Requisiciones { get; set; } = [];
        }

        public async Task OnGetAsync()
        {
            var actasConDetalles = await _context.Actas
                .Include(a => a.Proveedor)
                .Include(a => a.DetallesActa)
                .OrderByDescending(a => a.Fecha)
                .ToListAsync();

            Actas = actasConDetalles.Select(acta => new ActaConDetalles
            {
                Acta = acta,
                Requisiciones = acta.DetallesActa
                    .Where(d => !string.IsNullOrWhiteSpace(d.Requisicion))
                    .Select(d => d.Requisicion!)
                    .Distinct()
                    .OrderBy(r => r)
                    .ToList()
            }).ToList();
        }
    }
}