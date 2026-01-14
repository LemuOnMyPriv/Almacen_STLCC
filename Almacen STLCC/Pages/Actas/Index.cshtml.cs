using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Almacen_STLCC.Data;
using Almacen_STLCC.Models.Actas;

namespace Almacen_STLCC.Pages.Actas
{
    public class IndexModel(ApplicationDbContext context) : SecurePageModel
    {
        private readonly ApplicationDbContext _context = context;

        public List<ActaViewModel> Actas { get; set; } = new();

        public class ActaViewModel
        {
            public Acta Acta { get; set; } = null!;
            public List<string> Requisiciones { get; set; } = new();
        }

        public async Task OnGetAsync()
        {
            var actas = await _context.Actas
                .Include(a => a.Proveedor)
                .Include(a => a.DetallesActa) // Incluir los detalles para extraer requisiciones
                .OrderByDescending(a => a.Fecha)
                .ToListAsync();

            Actas = actas.Select(acta => new ActaViewModel
            {
                Acta = acta,
                // Extraer requisiciones únicas de los productos del acta
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