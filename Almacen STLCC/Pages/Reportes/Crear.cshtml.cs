using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Almacen_STLCC.Services;
using Microsoft.EntityFrameworkCore;
using Almacen_STLCC.Data;

namespace Almacen_STLCC.Pages.Reportes
{
    [IgnoreAntiforgeryToken]
    public class CrearModel : SecurePageModel
    {
        private readonly ReporteService _reporteService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CrearModel> _logger;

        public CrearModel(ReporteService reporteService, ApplicationDbContext context, ILogger<CrearModel> logger)
        {
            _reporteService = reporteService;
            _context = context;
            _logger = logger;
        }

        public void OnGet()
        {
        }

        public class PayloadReporte
        {
            public List<string> Tablas { get; set; } = new();
            public Dictionary<string, List<string>> Columnas { get; set; } = new();
            public Dictionary<string, Dictionary<string, string>> Filtros { get; set; } = new();
            public string Formato { get; set; } = "excel";
        }

        public async Task<IActionResult> OnPostGenerarAsync([FromBody] PayloadReporte payload)
        {
            try
            {
                _logger.LogInformation("=== INICIO GENERACIÓN DE REPORTE ===");
                _logger.LogInformation("Tablas seleccionadas: {Tablas}", string.Join(", ", payload.Tablas));

                // Log de columnas
                foreach (var tabla in payload.Columnas)
                {
                    _logger.LogInformation("Columnas de {Tabla}: {Columnas}",
                        tabla.Key, string.Join(", ", tabla.Value));
                }

                // Log de filtros
                if (payload.Filtros.Any())
                {
                    _logger.LogInformation("Filtros recibidos:");
                    foreach (var tabla in payload.Filtros)
                    {
                        _logger.LogInformation("Tabla: {Tabla}", tabla.Key);
                        foreach (var filtro in tabla.Value)
                        {
                            _logger.LogInformation("{Columna} = '{Valor}'", filtro.Key, filtro.Value);
                        }
                    }
                }
                else
                {
                    _logger.LogInformation("Sin filtros aplicados");
                }

                var config = new ReporteService.ConfiguracionReporte
                {
                    TablasSeleccionadas = payload.Tablas,
                    ColumnasSeleccionadas = payload.Columnas,
                    Filtros = payload.Filtros
                };

                byte[] archivo;
                string contentType;
                string extension;

                switch (payload.Formato.ToLower())
                {
                    case "excel":
                        _logger.LogInformation("Generando reporte en formato Excel");
                        archivo = await _reporteService.GenerarExcel(config);
                        contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        extension = "xlsx";
                        break;
                    case "pdf":
                        _logger.LogInformation("Generando reporte en formato PDF");
                        archivo = await _reporteService.GenerarPDF(config);
                        contentType = "application/pdf";
                        extension = "pdf";
                        break;
                    case "word":
                        _logger.LogInformation("Generando reporte en formato Word");
                        archivo = await _reporteService.GenerarWord(config);
                        contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                        extension = "docx";
                        break;
                    default:
                        _logger.LogWarning("Formato no válido: {Formato}", payload.Formato);
                        return BadRequest("Formato no válido");
                }

                _logger.LogInformation("Reporte generado exitosamente. Tamaño: {Size} bytes", archivo.Length);
                return File(archivo, contentType, $"reporte_{DateTime.Now:yyyyMMdd_HHmmss}.{extension}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando reporte");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        public async Task<IActionResult> OnGetDatosSelectsAsync()
        {
            try
            {
                var categorias = await _context.Categorias
                    .OrderBy(c => c.Nombre_Categoria)
                    .Select(c => c.Nombre_Categoria ?? "")
                    .ToListAsync();

                var proveedores = await _context.Proveedores
                    .OrderBy(p => p.Nombre_Proveedor)
                    .Select(p => p.Nombre_Proveedor)
                    .ToListAsync();

                var productos = await _context.Productos
                    .OrderBy(p => p.Nombre_Producto)
                    .Select(p => p.Nombre_Producto)
                    .ToListAsync();

                var actas = await _context.Actas
                    .OrderBy(a => a.Numero_Acta)
                    .Select(a => a.Numero_Acta)
                    .ToListAsync();

                var datos = new
                {
                    categorias,
                    proveedores,
                    productos,
                    actas,
                    tiposMovimiento = new[] { "ENTRADA", "SALIDA", "AJUSTE" }
                };

                return new JsonResult(datos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo datos para selects");
                return StatusCode(500, "Error obteniendo datos");
            }
        }
    }
}