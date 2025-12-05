using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Almacen_STLCC.Data;
using Almacen_STLCC.Models.Auditoria;
using System.Text;

namespace Almacen_STLCC.Pages.Auditoria
{
    public class IndexModel : SecurePageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ApplicationDbContext context, ILogger<IndexModel> logger)
        {
            _context = context;
            _logger = logger;
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

        public async Task<IActionResult> OnPostExportarYLimpiarAsync()
        {
            var rol = HttpContext.Session.GetString("Rol");

            if (rol != "ADMINISTRADOR")
            {
                TempData["ErrorMessage"] = "No tienes permisos para esta acción";
                return RedirectToPage();
            }

            try
            {
                var auditorias = await _context.Auditorias
                    .OrderBy(a => a.Fecha_Hora)
                    .ToListAsync();

                if (!auditorias.Any())
                {
                    TempData["ErrorMessage"] = "No hay auditorías para exportar";
                    return RedirectToPage();
                }

                var sb = new StringBuilder();
                sb.AppendLine("=================================================================");
                sb.AppendLine($"AUDITORÍA DEL SISTEMA - EXPORTADO EL {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                sb.AppendLine($"POR: {CurrentDisplayName} ({CurrentUsername})");
                sb.AppendLine($"TOTAL DE REGISTROS: {auditorias.Count}");
                sb.AppendLine("=================================================================");
                sb.AppendLine();

                foreach (var audit in auditorias)
                {
                    sb.AppendLine($"[{audit.Fecha_Hora:dd/MM/yyyy HH:mm:ss}] {audit.Accion} en {audit.Tabla}");
                    sb.AppendLine($"  Usuario: {audit.Usuario}");
                    sb.AppendLine($"  ID Registro: {audit.Id_Registro}");
                    sb.AppendLine($"  Descripción: {audit.Descripcion}");
                    sb.AppendLine($"  IP: {audit.Ip_Address ?? "N/A"}");
                    sb.AppendLine();
                }

                var nombreArchivo = $"auditoria_backup_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                var rutaArchivo = Path.Combine("Logs", "Auditorias", nombreArchivo);

                Directory.CreateDirectory(Path.Combine("Logs", "Auditorias"));

                await System.IO.File.WriteAllTextAsync(rutaArchivo, sb.ToString(), Encoding.UTF8);

                _context.Auditorias.RemoveRange(auditorias);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Auditorías exportadas y limpiadas por {User}. Total: {Count}",
                    CurrentUsername, auditorias.Count);

                TempData["SuccessMessage"] = $"Se exportaron {auditorias.Count} registros a '{nombreArchivo}' y se limpiaron de la base de datos";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al exportar y limpiar auditorías");
                TempData["ErrorMessage"] = $"Error al exportar: {ex.Message}";
                return RedirectToPage();
            }
        }
    }
}