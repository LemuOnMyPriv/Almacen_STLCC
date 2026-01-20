using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Almacen_STLCC.Services;
using System.Text.Json;

namespace Almacen_STLCC.Pages.Reportes
{
    [IgnoreAntiforgeryToken]
    public class CrearModel : SecurePageModel
    {
        private readonly ReporteService _reporteService;
        private readonly ILogger<CrearModel> _logger;

        public CrearModel(
            ReporteService reporteService,
            ILogger<CrearModel> logger)
        {
            _reporteService = reporteService;
            _logger = logger;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostGenerarAsync()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();

                if (string.IsNullOrWhiteSpace(body))
                {
                    _logger.LogWarning("El body del request llegó vacío");
                    return BadRequest("Request vacío");
                }

                var request = JsonSerializer.Deserialize<ReporteRequest>(
                    body,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (request == null)
                {
                    _logger.LogWarning("No se pudo deserializar el request");
                    return BadRequest("Datos inválidos");
                }

                _logger.LogInformation(
                    "Generando reporte. Tablas: {Tablas} | Formato: {Formato}",
                    string.Join(", ", request.Tablas),
                    request.Formato
                );

                // Convertir filtros al formato correcto
                var filtrosConvertidos = new Dictionary<string, Dictionary<string, string>>();

                foreach (var tabla in request.Filtros)
                {
                    filtrosConvertidos[tabla.Key] = tabla.Value;
                }

                var config = new ReporteService.ConfiguracionReporte
                {
                    TablasSeleccionadas = request.Tablas,
                    ColumnasSeleccionadas = request.Columnas,
                    Filtros = filtrosConvertidos
                };

                byte[] archivo;
                string contentType;
                string extension;

                switch (request.Formato?.ToLower())
                {
                    case "excel":
                        archivo = await _reporteService.GenerarExcel(config);
                        contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        extension = "xlsx";
                        break;

                    case "pdf":
                        archivo = await _reporteService.GenerarPDF(config);
                        contentType = "application/pdf";
                        extension = "pdf";
                        break;

                    case "word":
                        archivo = await _reporteService.GenerarWord(config);
                        contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                        extension = "docx";
                        break;

                    default:
                        _logger.LogWarning("Formato no válido: {Formato}", request.Formato);
                        return BadRequest("Formato no válido");
                }

                var nombreArchivo = $"reporte_{DateTime.Now:yyyyMMddHHmmss}.{extension}";

                _logger.LogInformation("Reporte generado correctamente: {Archivo}", nombreArchivo);

                return File(archivo, contentType, nombreArchivo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar el reporte");
                return StatusCode(500, ex.ToString());
            }
        }

        public class ReporteRequest
        {
            public List<string> Tablas { get; set; } = new();
            public Dictionary<string, List<string>> Columnas { get; set; } = new();
            public Dictionary<string, Dictionary<string, string>> Filtros { get; set; } = new();
            public string Formato { get; set; } = "excel";
        }
    }
}