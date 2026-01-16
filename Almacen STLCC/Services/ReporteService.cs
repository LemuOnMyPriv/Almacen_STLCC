using Almacen_STLCC.Data;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using iText.Kernel.Pdf;
using iText.Layout;
using PdfDocument = iText.Kernel.Pdf.PdfDocument;
using PdfLayoutDocument = iText.Layout.Document;
using PdfParagraph = iText.Layout.Element.Paragraph;
using PdfTable = iText.Layout.Element.Table;
using PdfCell = iText.Layout.Element.Cell;
using PdfText = iText.Layout.Element.Text;
using PdfTextAlignment = iText.Layout.Properties.TextAlignment;
using DocumentFormat.OpenXml.Packaging;
using WordDocument = DocumentFormat.OpenXml.Wordprocessing.Document;
using WordParagraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using WordRun = DocumentFormat.OpenXml.Wordprocessing.Run;
using WordText = DocumentFormat.OpenXml.Wordprocessing.Text;
using WordTable = DocumentFormat.OpenXml.Wordprocessing.Table;
using WordTableRow = DocumentFormat.OpenXml.Wordprocessing.TableRow;
using WordTableCell = DocumentFormat.OpenXml.Wordprocessing.TableCell;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;

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
            public Dictionary<string, string> Filtros { get; set; } = new();
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
                            if (config.ColumnasSeleccionadas["productos"].Contains("Código")) fila["Código"] = p.Codigo_Producto;
                            if (config.ColumnasSeleccionadas["productos"].Contains("Nombre")) fila["Nombre"] = p.Nombre_Producto;
                            if (config.ColumnasSeleccionadas["productos"].Contains("Marca")) fila["Marca"] = p.Marca ?? "";
                            if (config.ColumnasSeleccionadas["productos"].Contains("Categoría")) fila["Categoría"] = p.Categoria.Nombre_Categoria ?? "";
                            if (config.ColumnasSeleccionadas["productos"].Contains("Unidad de Medida")) fila["Unidad de Medida"] = p.Unidad_Medida;
                            if (config.ColumnasSeleccionadas["productos"].Contains("Proveedores"))
                            {
                                fila["Proveedores"] = string.Join(", ", p.ProductoProveedores.Select(pp => pp.Proveedor.Nombre_Proveedor));
                            }
                            datos.Add(fila);
                        }
                        break;

                    case "proveedores":
                        var proveedores = await _context.Proveedores.ToListAsync();
                        foreach (var prov in proveedores)
                        {
                            var fila = new Dictionary<string, object>();
                            if (config.ColumnasSeleccionadas["proveedores"].Contains("Nombre")) fila["Nombre"] = prov.Nombre_Proveedor;
                            if (config.ColumnasSeleccionadas["proveedores"].Contains("RTN")) fila["RTN"] = prov.Rtn;
                            datos.Add(fila);
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
                            if (config.ColumnasSeleccionadas["actas"].Contains("Numero de Acta")) fila["Numero de Acta"] = acta.Numero_Acta;
                            if (config.ColumnasSeleccionadas["actas"].Contains("F01")) fila["F01"] = acta.F01;
                            if (config.ColumnasSeleccionadas["actas"].Contains("Orden de Compra")) fila["Orden de Compra"] = acta.Orden_Compra ?? "";
                            if (config.ColumnasSeleccionadas["actas"].Contains("Proveedor")) fila["Proveedor"] = acta.Proveedor.Nombre_Proveedor;
                            if (config.ColumnasSeleccionadas["actas"].Contains("Fecha")) fila["Fecha"] = acta.Fecha.ToString("dd/MM/yyyy");
                            if (config.ColumnasSeleccionadas["actas"].Contains("Productos"))
                            {
                                fila["Productos"] = string.Join(", ", acta.DetallesActa.Select(d => d.Producto.Nombre_Producto));
                            }
                            datos.Add(fila);
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
                            if (config.ColumnasSeleccionadas["movimientos"].Contains("Fecha")) fila["Fecha"] = mov.Fecha.ToString("dd/MM/yyyy");
                            if (config.ColumnasSeleccionadas["movimientos"].Contains("Producto")) fila["Producto"] = mov.Producto.Nombre_Producto;
                            if (config.ColumnasSeleccionadas["movimientos"].Contains("Tipo")) fila["Tipo"] = mov.Tipo_Movimiento.ToUpper();
                            if (config.ColumnasSeleccionadas["movimientos"].Contains("Cantidad")) fila["Cantidad"] = mov.Cantidad;
                            if (config.ColumnasSeleccionadas["movimientos"].Contains("Acta")) fila["Acta"] = mov.Acta?.Numero_Acta ?? "N/A";
                            datos.Add(fila);
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

        public async Task<byte[]> GenerarExcel(ConfiguracionReporte config)
        {
            var datos = await ObtenerDatosReporte(config);

            using var workbook = new XLWorkbook();

            foreach (var tabla in datos)
            {
                var worksheet = workbook.Worksheets.Add(tabla.Key);

                if (tabla.Value.Count == 0) continue;

                // Headers
                var columnas = tabla.Value[0].Keys.ToList();
                for (int i = 0; i < columnas.Count; i++)
                {
                    worksheet.Cell(1, i + 1).Value = columnas[i];
                    worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#88CFE0");
                }

                // Data
                for (int row = 0; row < tabla.Value.Count; row++)
                {
                    for (int col = 0; col < columnas.Count; col++)
                    {
                        var valor = tabla.Value[row][columnas[col]]?.ToString() ?? "";
                        worksheet.Cell(row + 2, col + 1).Value = valor;
                    }
                }

                worksheet.Columns().AdjustToContents();
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> GenerarPDF(ConfiguracionReporte config)
        {
            var datos = await ObtenerDatosReporte(config);

            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            using var pdf = new PdfDocument(writer);
            using var document = new PdfLayoutDocument(pdf);

            document.Add(new PdfParagraph("REPORTE DEL SISTEMA - ALMACÉN STLCC")
                .SetFontSize(18)
                .SetBold()
                .SetTextAlignment(PdfTextAlignment.CENTER));

            document.Add(new PdfParagraph($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm:ss}")
                .SetFontSize(10)
                .SetTextAlignment(PdfTextAlignment.CENTER));

            document.Add(new PdfParagraph("\n"));

            foreach (var tabla in datos)
            {
                document.Add(new PdfParagraph(tabla.Key.ToUpper())
                    .SetFontSize(14)
                    .SetBold());

                if (tabla.Value.Count == 0)
                {
                    document.Add(new PdfParagraph("No hay datos para mostrar"));
                    continue;
                }

                var columnas = tabla.Value[0].Keys.ToList();
                var pdfTable = new PdfTable(columnas.Count);

                // Headers
                foreach (var columna in columnas)
                {
                    pdfTable.AddHeaderCell(new PdfCell()
                        .Add(new PdfParagraph(columna).SetBold())
                        .SetBackgroundColor(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY));
                }

                // Data
                foreach (var fila in tabla.Value)
                {
                    foreach (var columna in columnas)
                    {
                        pdfTable.AddCell(fila[columna]?.ToString() ?? "");
                    }
                }

                document.Add(pdfTable);
                document.Add(new PdfParagraph("\n"));
            }

            document.Close();
            return stream.ToArray();
        }

        public async Task<byte[]> GenerarWord(ConfiguracionReporte config)
        {
            var datos = await ObtenerDatosReporte(config);

            using var stream = new MemoryStream();
            using (var wordDoc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document))
            {
                var mainPart = wordDoc.AddMainDocumentPart();
                mainPart.Document = new WordDocument();
                var body = mainPart.Document.AppendChild(new Body());

                // Título
                var titlePara = body.AppendChild(new WordParagraph());
                var titleRun = titlePara.AppendChild(new WordRun());
                titleRun.AppendChild(new WordText("REPORTE DEL SISTEMA - ALMACÉN STLCC"));
                var titleProps = titleRun.AppendChild(new RunProperties());
                titleProps.AppendChild(new Bold());
                titleProps.AppendChild(new FontSize { Val = "32" });

                // Fecha
                var datePara = body.AppendChild(new WordParagraph());
                datePara.AppendChild(new WordRun(new WordText($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm:ss}")));

                foreach (var tabla in datos)
                {
                    // Título de tabla
                    var tableTitlePara = body.AppendChild(new WordParagraph());
                    var tableTitleRun = tableTitlePara.AppendChild(new WordRun());
                    tableTitleRun.AppendChild(new WordText(tabla.Key.ToUpper()));
                    var tableTitleProps = tableTitleRun.AppendChild(new RunProperties());
                    tableTitleProps.AppendChild(new Bold());
                    tableTitleProps.AppendChild(new FontSize { Val = "28" });

                    if (tabla.Value.Count == 0)
                    {
                        body.AppendChild(new WordParagraph(new WordRun(new WordText("No hay datos para mostrar"))));
                        continue;
                    }

                    var columnas = tabla.Value[0].Keys.ToList();
                    var wordTable = new WordTable();

                    // Headers
                    var headerRow = new WordTableRow();
                    foreach (var columna in columnas)
                    {
                        var cell = new WordTableCell();
                        cell.Append(new WordParagraph(new WordRun(new WordText(columna))));
                        headerRow.Append(cell);
                    }
                    wordTable.Append(headerRow);

                    // Data
                    foreach (var fila in tabla.Value)
                    {
                        var dataRow = new WordTableRow();
                        foreach (var columna in columnas)
                        {
                            var cell = new WordTableCell();
                            cell.Append(new WordParagraph(new WordRun(new WordText(fila[columna]?.ToString() ?? ""))));
                            dataRow.Append(cell);
                        }
                        wordTable.Append(dataRow);
                    }

                    body.Append(wordTable);
                    body.AppendChild(new WordParagraph()); // Espacio
                }

                mainPart.Document.Save();
            } // Aquí se cierra automáticamente el WordprocessingDocument

            return stream.ToArray();
        }
    }
}