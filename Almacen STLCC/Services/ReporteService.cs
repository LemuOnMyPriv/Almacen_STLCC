using Almacen_STLCC.Data;
using Microsoft.EntityFrameworkCore;

namespace Almacen_STLCC.Services
{
    public class ReporteService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReporteService> _logger;

        public ReporteService(ApplicationDbContext context, ILogger<ReporteService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public class ConfiguracionReporte
        {
            public List<string> TablasSeleccionadas { get; set; } = new();
            public Dictionary<string, List<string>> ColumnasSeleccionadas { get; set; } = new();
            public Dictionary<string, Dictionary<string, string>> Filtros { get; set; } = new();
        }

        public async Task<Dictionary<string, List<Dictionary<string, object>>>> ObtenerDatosReporte(ConfiguracionReporte config)
        {
            var resultado = new Dictionary<string, List<Dictionary<string, object>>>();

            foreach (var tabla in config.TablasSeleccionadas)
            {
                _logger.LogInformation("=== Procesando tabla: {Tabla} ===", tabla);
                var datos = await ObtenerDatosTabla(tabla, config);
                resultado[tabla] = datos;
                _logger.LogInformation("Tabla {Tabla}: {Count} registros después de filtros", tabla, datos.Count);
            }

            return resultado;
        }

        private async Task<List<Dictionary<string, object>>> ObtenerDatosTabla(string tabla, ConfiguracionReporte config)
        {
            var datos = new List<Dictionary<string, object>>();

            try
            {
                // Obtener filtros para esta tabla (si existen)
                var filtrosTabla = config.Filtros.GetValueOrDefault(tabla);

                if (filtrosTabla != null && filtrosTabla.Any())
                {
                    _logger.LogInformation("Filtros activos para {Tabla}:", tabla);
                    foreach (var filtro in filtrosTabla)
                    {
                        _logger.LogInformation("   - {Columna} = '{Valor}'", filtro.Key, filtro.Value);
                    }
                }
                else
                {
                    _logger.LogInformation("Sin filtros para {Tabla}", tabla);
                }

                switch (tabla.ToLower())
                {
                    case "productos":
                        var productos = await _context.Productos
                            .Include(p => p.Categoria)
                            .Include(p => p.ProductoProveedores)
                                .ThenInclude(pp => pp.Proveedor)
                            .ToListAsync();

                        _logger.LogInformation("Total productos en BD: {Count}", productos.Count);

                        foreach (var p in productos)
                        {
                            var fila = new Dictionary<string, object>();

                            if (config.ColumnasSeleccionadas["productos"].Contains("Código"))
                                fila["Código"] = p.Codigo_Producto;
                            if (config.ColumnasSeleccionadas["productos"].Contains("Nombre"))
                                fila["Nombre"] = p.Nombre_Producto;
                            if (config.ColumnasSeleccionadas["productos"].Contains("Marca"))
                                fila["Marca"] = p.Marca ?? "";
                            if (config.ColumnasSeleccionadas["productos"].Contains("Categoría"))
                                fila["Categoría"] = p.Categoria.Nombre_Categoria ?? "";
                            if (config.ColumnasSeleccionadas["productos"].Contains("Unidad de Medida"))
                                fila["Unidad de Medida"] = p.Unidad_Medida;
                            if (config.ColumnasSeleccionadas["productos"].Contains("Proveedores"))
                            {
                                fila["Proveedores"] = string.Join(", ", p.ProductoProveedores.Select(pp => pp.Proveedor.Nombre_Proveedor));
                            }

                            if (CumpleFiltros(fila, filtrosTabla))
                            {
                                datos.Add(fila);
                            }
                        }
                        break;

                    case "proveedores":
                        var proveedores = await _context.Proveedores.ToListAsync();

                        _logger.LogInformation("Total proveedores en BD: {Count}", proveedores.Count);

                        foreach (var prov in proveedores)
                        {
                            var fila = new Dictionary<string, object>();

                            if (config.ColumnasSeleccionadas["proveedores"].Contains("Nombre"))
                                fila["Nombre"] = prov.Nombre_Proveedor;
                            if (config.ColumnasSeleccionadas["proveedores"].Contains("RTN"))
                                fila["RTN"] = prov.Rtn;

                            if (CumpleFiltros(fila, filtrosTabla))
                            {
                                datos.Add(fila);
                            }
                        }
                        break;

                    case "actas":
                        var actas = await _context.Actas
                            .Include(a => a.Proveedor)
                            .Include(a => a.DetallesActa)
                                .ThenInclude(d => d.Producto)
                            .ToListAsync();

                        _logger.LogInformation("Total actas en BD: {Count}", actas.Count);

                        foreach (var acta in actas)
                        {
                            var fila = new Dictionary<string, object>();

                            if (config.ColumnasSeleccionadas["actas"].Contains("Numero de Acta"))
                                fila["Numero de Acta"] = acta.Numero_Acta;
                            if (config.ColumnasSeleccionadas["actas"].Contains("F01"))
                                fila["F01"] = acta.F01;
                            if (config.ColumnasSeleccionadas["actas"].Contains("Orden de Compra"))
                                fila["Orden de Compra"] = acta.Orden_Compra ?? "";
                            if (config.ColumnasSeleccionadas["actas"].Contains("Proveedor"))
                                fila["Proveedor"] = acta.Proveedor.Nombre_Proveedor;
                            if (config.ColumnasSeleccionadas["actas"].Contains("Fecha"))
                                fila["Fecha"] = acta.Fecha;
                            if (config.ColumnasSeleccionadas["actas"].Contains("Productos"))
                            {
                                fila["Productos"] = string.Join(", ", acta.DetallesActa.Select(d => d.Producto.Nombre_Producto));
                            }

                            if (CumpleFiltros(fila, filtrosTabla))
                            {
                                datos.Add(fila);
                            }
                        }
                        break;

                    case "movimientos":
                        var movimientos = await _context.Movimientos
                            .Include(m => m.Producto)
                            .Include(m => m.Acta)
                            .ToListAsync();

                        _logger.LogInformation("Total movimientos en BD: {Count}", movimientos.Count);

                        foreach (var mov in movimientos)
                        {
                            var fila = new Dictionary<string, object>();

                            if (config.ColumnasSeleccionadas["movimientos"].Contains("Fecha"))
                                fila["Fecha"] = mov.Fecha;
                            if (config.ColumnasSeleccionadas["movimientos"].Contains("Producto"))
                                fila["Producto"] = mov.Producto.Nombre_Producto;
                            if (config.ColumnasSeleccionadas["movimientos"].Contains("Tipo"))
                                fila["Tipo"] = mov.Tipo_Movimiento.ToUpper();
                            if (config.ColumnasSeleccionadas["movimientos"].Contains("Cantidad"))
                                fila["Cantidad"] = mov.Cantidad;
                            if (config.ColumnasSeleccionadas["movimientos"].Contains("Acta"))
                                fila["Acta"] = mov.Acta?.Numero_Acta ?? "N/A";

                            if (CumpleFiltros(fila, filtrosTabla))
                            {
                                datos.Add(fila);
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo datos de tabla {Tabla}", tabla);
            }

            return datos;
        }

        /// <summary>
        /// Verifica si una fila cumple con TODOS los filtros especificados
        /// Usa búsqueda parcial (CONTAINS) case-insensitive
        /// </summary>
        private bool CumpleFiltros(Dictionary<string, object> fila, Dictionary<string, string>? filtros)
        {
            // Si no hay filtros, incluir todos los registros
            if (filtros == null || !filtros.Any())
                return true;

            // La fila debe cumplir TODOS los filtros (AND lógico)
            foreach (var filtro in filtros)
            {
                var nombreColumna = filtro.Key;
                var valorFiltro = filtro.Value;

                // Buscar la columna en la fila (case-insensitive)
                var columnaPar = fila.FirstOrDefault(f =>
                    f.Key.Equals(nombreColumna, StringComparison.OrdinalIgnoreCase));

                // Si la columna no existe en la fila, no cumple el filtro
                if (columnaPar.Key == null)
                {
                    _logger.LogDebug("Columna '{Columna}' no encontrada en fila", nombreColumna);
                    return false;
                }

                var valorCelda = columnaPar.Value;

                // Manejar diferentes tipos de valores
                if (valorCelda == null)
                {
                    // Si el valor es null, solo cumple si el filtro está vacío
                    if (!string.IsNullOrWhiteSpace(valorFiltro))
                    {
                        _logger.LogDebug("Filtro no cumplido: {Columna} es NULL, filtro esperaba '{Valor}'",
                            nombreColumna, valorFiltro);
                        return false;
                    }
                }
                else if (valorCelda is DateTime fecha)
                {
                    // Para fechas, intentar parsear el filtro como fecha
                    if (DateTime.TryParse(valorFiltro, out DateTime fechaFiltro))
                    {
                        // Comparar solo la fecha (ignorar hora)
                        if (fecha.Date != fechaFiltro.Date)
                        {
                            _logger.LogDebug("Filtro no cumplido: {Columna} = {Valor}, filtro esperaba {Filtro}",
                                nombreColumna, fecha.Date, fechaFiltro.Date);
                            return false;
                        }
                    }
                    else
                    {
                        // Si no se puede parsear, convertir ambos a string y comparar
                        var fechaStr = fecha.ToString("dd/MM/yyyy");
                        if (!fechaStr.Contains(valorFiltro, StringComparison.OrdinalIgnoreCase))
                        {
                            _logger.LogDebug("Filtro no cumplido: {Columna} = '{Valor}', no contiene '{Filtro}'",
                                nombreColumna, fechaStr, valorFiltro);
                            return false;
                        }
                    }
                }
                else if (valorCelda is int numero)
                {
                    // Para números, convertir a string y comparar (permite búsqueda parcial)
                    var numeroStr = numero.ToString();
                    if (!numeroStr.Contains(valorFiltro, StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogDebug("Filtro no cumplido: {Columna} = {Valor}, no contiene '{Filtro}'",
                            nombreColumna, numeroStr, valorFiltro);
                        return false;
                    }
                }
                else
                {
                    // Para strings y otros tipos, convertir a string y buscar contenido parcial
                    var valorStr = valorCelda.ToString() ?? "";

                    // Búsqueda parcial (contiene), case-insensitive
                    if (!valorStr.Contains(valorFiltro, StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogDebug("Filtro no cumplido: {Columna} = '{Valor}', no contiene '{Filtro}'",
                            nombreColumna, valorStr, valorFiltro);
                        return false;
                    }
                }
            }

            // Si llegamos aquí, cumple TODOS los filtros
            return true;
        }

        public async Task<byte[]> GenerarExcel(ConfiguracionReporte config)
        {
            var datos = await ObtenerDatosReporte(config);
            return ReporteExcelGenerator.Generar(datos);
        }

        public async Task<byte[]> GenerarPDF(ConfiguracionReporte config)
        {
            var datos = await ObtenerDatosReporte(config);
            return ReportePdfGenerator.Generar(datos);
        }

        public async Task<byte[]> GenerarWord(ConfiguracionReporte config)
        {
            var datos = await ObtenerDatosReporte(config);
            return ReporteWordGenerator.Generar(datos);
        }
    }
}