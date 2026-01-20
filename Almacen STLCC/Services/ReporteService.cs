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
            public DateTime? FechaDesde { get; set; }
            public DateTime? FechaHasta { get; set; }
        }

        public async Task<Dictionary<string, List<Dictionary<string, object>>>> ObtenerDatosReporte(ConfiguracionReporte config)
        {
            var resultado = new Dictionary<string, List<Dictionary<string, object>>>();

            foreach (var tabla in config.TablasSeleccionadas)
            {
                var datos = await ObtenerDatosTabla(tabla, config);
                resultado[tabla] = datos;
            }

            return resultado;
        }

        private async Task<List<Dictionary<string, object>>> ObtenerDatosTabla(string tabla, ConfiguracionReporte config)
        {
            var datos = new List<Dictionary<string, object>>();

            try
            {
                switch (tabla.ToLower())
                {
                    case "productos":
                        var productos = await _context.Productos
                            .Include(p => p.Categoria)
                            .Include(p => p.ProductoProveedores)
                                .ThenInclude(pp => pp.Proveedor)
                            .ToListAsync();

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

                            if (AplicarFiltros(fila, config.Filtros.GetValueOrDefault("productos")))
                            {
                                datos.Add(fila);
                            }
                        }
                        break;

                    case "proveedores":
                        var proveedores = await _context.Proveedores.ToListAsync();
                        foreach (var prov in proveedores)
                        {
                            var fila = new Dictionary<string, object>();
                            if (config.ColumnasSeleccionadas["proveedores"].Contains("Nombre"))
                                fila["Nombre"] = prov.Nombre_Proveedor;
                            if (config.ColumnasSeleccionadas["proveedores"].Contains("RTN"))
                                fila["RTN"] = prov.Rtn;

                            if (AplicarFiltros(fila, config.Filtros.GetValueOrDefault("proveedores")))
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
                                fila["Fecha"] = acta.Fecha.ToString("dd/MM/yyyy");
                            if (config.ColumnasSeleccionadas["actas"].Contains("Productos"))
                            {
                                fila["Productos"] = string.Join(", ", acta.DetallesActa.Select(d => d.Producto.Nombre_Producto));
                            }

                            if (AplicarFiltros(fila, config.Filtros.GetValueOrDefault("actas")))
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

                        foreach (var mov in movimientos)
                        {
                            var fila = new Dictionary<string, object>();
                            if (config.ColumnasSeleccionadas["movimientos"].Contains("Fecha"))
                                fila["Fecha"] = mov.Fecha.ToString("dd/MM/yyyy");
                            if (config.ColumnasSeleccionadas["movimientos"].Contains("Producto"))
                                fila["Producto"] = mov.Producto.Nombre_Producto;
                            if (config.ColumnasSeleccionadas["movimientos"].Contains("Tipo"))
                                fila["Tipo"] = mov.Tipo_Movimiento.ToUpper();
                            if (config.ColumnasSeleccionadas["movimientos"].Contains("Cantidad"))
                                fila["Cantidad"] = mov.Cantidad;
                            if (config.ColumnasSeleccionadas["movimientos"].Contains("Acta"))
                                fila["Acta"] = mov.Acta?.Numero_Acta ?? "N/A";

                            if (AplicarFiltros(fila, config.Filtros.GetValueOrDefault("movimientos")))
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

        private bool AplicarFiltros(Dictionary<string, object> fila, Dictionary<string, string>? filtros)
        {
            if (filtros == null || !filtros.Any())
                return true;

            foreach (var filtro in filtros)
            {
                if (fila.ContainsKey(filtro.Key))
                {
                    var valor = fila[filtro.Key]?.ToString()?.ToLower() ?? "";
                    var filtroValor = filtro.Value.ToLower();

                    if (!valor.Contains(filtroValor))
                        return false;
                }
            }

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