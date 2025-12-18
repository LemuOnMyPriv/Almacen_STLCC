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

        public List<ActaConRequisiciones> Actas { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? FiltroRequisicion { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FiltroProveedor { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FiltroNumeroActa { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FiltroF01 { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FiltroOrdenCompra { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FechaDesde { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FechaHasta { get; set; }

        public int TotalResultados { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.Actas
                .Include(a => a.Proveedor)
                .Include(a => a.Requisiciones)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(FiltroRequisicion))
            {
                query = query.Where(a => a.Requisiciones.Any(r =>
                    r.Requisicion.Contains(FiltroRequisicion)));
            }

            if (!string.IsNullOrWhiteSpace(FiltroProveedor))
            {
                query = query.Where(a =>
                    a.Proveedor.Nombre_Proveedor.Contains(FiltroProveedor));
            }

            if (!string.IsNullOrWhiteSpace(FiltroNumeroActa))
            {
                query = query.Where(a => a.Numero_Acta.Contains(FiltroNumeroActa));
            }

            if (!string.IsNullOrWhiteSpace(FiltroOrdenCompra))
            {
                query = query.Where(predicate: a => a.Orden_Compra.Contains(FiltroOrdenCompra));
            }

            if (!string.IsNullOrWhiteSpace(FiltroF01))
            {
                query = query.Where(a => a.F01.Contains(FiltroF01));
            }

            if (FechaDesde.HasValue)
            {
                query = query.Where(a => a.Fecha >= FechaDesde.Value);
            }

            if (FechaHasta.HasValue)
            {
                query = query.Where(a => a.Fecha <= FechaHasta.Value);
            }

            var actasDb = await query
                .OrderByDescending(a => a.Fecha)
                .ToListAsync();

            Actas = actasDb.Select(a => new ActaConRequisiciones
            {
                Acta = a,
                Requisiciones = a.Requisiciones.OrderBy(r => r.Requisicion).ToList()
            }).ToList();

            TotalResultados = Actas.Count;
        }

        public class ActaConRequisiciones
        {
            public required Acta Acta { get; set; }
            public List<ActaRequisicion> Requisiciones { get; set; } = new();
        }
    }
}