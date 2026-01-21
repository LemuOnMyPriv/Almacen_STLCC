using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Almacen_STLCC.Data;
using Almacen_STLCC.Services;
using System.Text.Json;

namespace Almacen_STLCC.Pages.Reportes
{
    [IgnoreAntiforgeryToken]
    public class CrearModel : SecurePageModel
    {
        private readonly ReporteService _reporteService;
        private readonly ApplicationDbContext _context;

        public CrearModel(ReporteService reporteService, ApplicationDbContext context)
        {
            _reporteService = reporteService;
            _context = context;
        }

        public void OnGet()
        {
        }

        // Handler para obtener datos de selects
        public async Task<IActionResult> OnGetDatosSelectsAsync()
        {
            var datos = new
            {
                categorias = await _context.Categorias
                    .OrderBy(c => c.Nombre_Categoria)
                    .Select(c => c.Nombre_Categoria)
                    .ToListAsync(),

                proveedores = await _context.Proveedores
                    .OrderBy(p => p.Nombre_Proveedor)
                    .Select(p => p.Nombre_Proveedor)
                    .ToListAsync(),

                productos = await _context.Productos
                    .OrderBy(p => p.Nombre_Producto)
                    .Select(p => p.Nombre_Producto)
                    .ToListAsync(),

                actas = await _context.Actas
                    .OrderBy(a => a.Numero_Acta)
                    .Select(a => a.Numero_Acta)
                    .ToListAsync(),

                tiposMovimiento = new[] { "ENTRADA", "SALIDA", "AJUSTE" }
            };

            return new JsonResult(datos);
        }

        // Handler para generar el reporte
        public async Task<IActionResult> OnPostGenerarAsync([FromBody] ConfiguracionReporteDto config)
        {
            try
            {
                var configuracion = new ReporteService.ConfiguracionReporte
                {
                    TablasSeleccionadas = config.Tablas,
                    ColumnasSeleccionadas = config.Columnas,
                    Filtros = config.Filtros
                };

                byte[] archivo;
                string contentType;
                string extension;

                switch (config.Formato.ToLower())
                {
                    case "excel":
                        archivo = await _reporteService.GenerarExcel(configuracion);
                        contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        extension = "xlsx";
                        break;

                    case "pdf":
                        archivo = await _reporteService.GenerarPDF(configuracion);
                        contentType = "application/pdf";
                        extension = "pdf";
                        break;

                    case "word":
                        archivo = await _reporteService.GenerarWord(configuracion);
                        contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                        extension = "docx";
                        break;

                    default:
                        return BadRequest("Formato no válido");
                }

                var nombreArchivo = $"reporte_{DateTime.Now:yyyyMMddHHmmss}.{extension}";
                return File(archivo, contentType, nombreArchivo);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al generar reporte: {ex.Message}");
            }
        }

        public class ConfiguracionReporteDto
        {
            public List<string> Tablas { get; set; } = new();
            public Dictionary<string, List<string>> Columnas { get; set; } = new();
            public Dictionary<string, Dictionary<string, string>> Filtros { get; set; } = new();
            public string Formato { get; set; } = "excel";
        }
    }
}