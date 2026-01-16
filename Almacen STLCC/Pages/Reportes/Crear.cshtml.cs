using Microsoft.AspNetCore.Mvc;
using Almacen_STLCC.Services;
using System.Text.Json;

namespace Almacen_STLCC.Pages.Reportes
{
    public class CrearModel : SecurePageModel
    {
        private readonly ReporteService _reporteService;

        public CrearModel(ReporteService reporteService)
        {
            _reporteService = reporteService;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostGenerarAsync([FromBody] ReporteRequest request)
        {
            try
            {
                var config = new ReporteService.ConfiguracionReporte
                {
                    TablasSeleccionadas = request.Tablas,
                    ColumnasSeleccionadas = request.Columnas,
                    Filtros = request.Filtros ?? new(),
                    FechaDesde = request.FechaDesde,
                    FechaHasta = request.FechaHasta
                };

                byte[] archivo;
                string contentType;
                string nombreArchivo;

                switch (request.Formato.ToLower())
                {
                    case "excel":
                        archivo = await _reporteService.GenerarExcel(config);
                        contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        nombreArchivo = $"reporte_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                        break;

                    case "pdf":
                        archivo = await _reporteService.GenerarPDF(config);
                        contentType = "application/pdf";
                        nombreArchivo = $"reporte_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                        break;

                    case "word":
                        archivo = await _reporteService.GenerarWord(config);
                        contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                        nombreArchivo = $"reporte_{DateTime.Now:yyyyMMdd_HHmmss}.docx";
                        break;

                    default:
                        return BadRequest("Formato no válido");
                }

                return File(archivo, contentType, nombreArchivo);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public class ReporteRequest
        {
            public List<string> Tablas { get; set; } = new();
            public Dictionary<string, List<string>> Columnas { get; set; } = new();
            public Dictionary<string, string>? Filtros { get; set; }
            public DateTime? FechaDesde { get; set; }
            public DateTime? FechaHasta { get; set; }
            public string Formato { get; set; } = "excel";
        }
    }
}