using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Almacen_STLCC.Data;
using Almacen_STLCC.Models.Auditoria;

namespace Almacen_STLCC.Pages.Auditoria
{
    public class IndexModel : SecurePageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Models.Auditoria.Auditoria> Auditorias { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? FiltroUsuario { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FiltroTabla { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FiltroAccion { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FechaDesde { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FechaHasta { get; set; }

        public int TotalRegistros { get; set; }
        public int PaginaActual { get; set; } = 1;
        public int RegistrosPorPagina { get; set; } = 50;
        public int TotalPaginas { get; set; }

        public async Task<IActionResult> OnGetAsync(int pagina = 1)
        {
            var rol = HttpContext.Session.GetString("Rol");

            if (rol != "ADMINISTRADOR")
            {
                return RedirectToPage("/Index");
            }

            PaginaActual = pagina;

            var query = _context.Auditorias.AsQueryable();

            if (!string.IsNullOrEmpty(FiltroUsuario))
            {
                query = query.Where(a => a.Usuario.Contains(FiltroUsuario));
            }

            if (!string.IsNullOrEmpty(FiltroTabla))
            {
                query = query.Where(a => a.Tabla.Contains(FiltroTabla));
            }

            if (!string.IsNullOrEmpty(FiltroAccion))
            {
                query = query.Where(a => a.Accion == FiltroAccion);
            }

            if (FechaDesde.HasValue)
            {
                query = query.Where(a => a.Fecha_Hora >= FechaDesde.Value);
            }

            if (FechaHasta.HasValue)
            {
                var fechaHastaFin = FechaHasta.Value.Date.AddDays(1).AddSeconds(-1);
                query = query.Where(a => a.Fecha_Hora <= fechaHastaFin);
            }

            TotalRegistros = await query.CountAsync();
            TotalPaginas = (int)Math.Ceiling(TotalRegistros / (double)RegistrosPorPagina);

            Auditorias = await query
                .OrderByDescending(a => a.Fecha_Hora)
                .Skip((PaginaActual - 1) * RegistrosPorPagina)
                .Take(RegistrosPorPagina)
                .ToListAsync();

            return Page();
        }
    }
}