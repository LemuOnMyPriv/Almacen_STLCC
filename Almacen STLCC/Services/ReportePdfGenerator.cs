using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.IO.Font.Constants;

namespace Almacen_STLCC.Services
{
    public static class ReportePdfGenerator
    {
        public static byte[] Generar(Dictionary<string, List<Dictionary<string, object>>> datos)
        {
            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            // Fuentes
            var fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var fontNormal = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            // Título
            var titulo = new Paragraph("REPORTE DEL SISTEMA - ALMACÉN STLCC")
                .SetFont(fontBold)
                .SetFontSize(18)
                .SetTextAlignment(TextAlignment.CENTER);
            document.Add(titulo);

            var fecha = new Paragraph($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm:ss}")
                .SetFont(fontNormal)
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER);
            document.Add(fecha);

            document.Add(new Paragraph("\n"));

            foreach (var tabla in datos)
            {
                var tituloTabla = new Paragraph(tabla.Key.ToUpper())
                    .SetFont(fontBold)
                    .SetFontSize(14);
                document.Add(tituloTabla);

                if (tabla.Value.Count == 0)
                {
                    document.Add(new Paragraph("No hay datos para mostrar")
                        .SetFont(fontNormal));
                    continue;
                }

                var columnas = tabla.Value[0].Keys.ToList();
                var table = new Table(columnas.Count);
                table.SetWidth(UnitValue.CreatePercentValue(100));

                // Headers
                foreach (var columna in columnas)
                {
                    var cell = new Cell()
                        .Add(new Paragraph(columna).SetFont(fontBold))
                        .SetBackgroundColor(ColorConstants.LIGHT_GRAY);
                    table.AddHeaderCell(cell);
                }

                // Data
                foreach (var fila in tabla.Value)
                {
                    foreach (var columna in columnas)
                    {
                        var cellText = fila[columna]?.ToString() ?? "";
                        var cell = new Cell()
                            .Add(new Paragraph(cellText).SetFont(fontNormal));
                        table.AddCell(cell);
                    }
                }

                document.Add(table);
                document.Add(new Paragraph("\n"));
            }

            document.Close();
            return stream.ToArray();
        }
    }
}