using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;

namespace Almacen_STLCC.Services
{
    public static class ReporteWordGenerator
    {
        public static byte[] Generar(Dictionary<string, List<Dictionary<string, object>>> datos)
        {
            using var stream = new MemoryStream();
            using (var wordDoc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document))
            {
                var mainPart = wordDoc.AddMainDocumentPart();
                mainPart.Document = new Document();
                var body = mainPart.Document.AppendChild(new Body());

                // Título
                var titlePara = body.AppendChild(new Paragraph());
                var titleRun = titlePara.AppendChild(new Run());
                titleRun.AppendChild(new Text("REPORTE DEL SISTEMA - ALMACÉN STLCC"));
                var titleProps = titleRun.AppendChild(new RunProperties());
                titleProps.AppendChild(new Bold());
                titleProps.AppendChild(new FontSize { Val = "32" });

                // Fecha
                var datePara = body.AppendChild(new Paragraph());
                datePara.AppendChild(new Run(new Text($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm:ss}")));

                foreach (var tabla in datos)
                {
                    // Título de tabla
                    var tableTitlePara = body.AppendChild(new Paragraph());
                    var tableTitleRun = tableTitlePara.AppendChild(new Run());
                    tableTitleRun.AppendChild(new Text(tabla.Key.ToUpper()));
                    var tableTitleProps = tableTitleRun.AppendChild(new RunProperties());
                    tableTitleProps.AppendChild(new Bold());
                    tableTitleProps.AppendChild(new FontSize { Val = "28" });

                    if (tabla.Value.Count == 0)
                    {
                        body.AppendChild(new Paragraph(new Run(new Text("No hay datos para mostrar"))));
                        continue;
                    }

                    var columnas = tabla.Value[0].Keys.ToList();
                    var wordTable = new Table();

                    // Headers
                    var headerRow = new TableRow();
                    foreach (var columna in columnas)
                    {
                        var cell = new TableCell();
                        cell.Append(new Paragraph(new Run(new Text(columna))));
                        headerRow.Append(cell);
                    }
                    wordTable.Append(headerRow);

                    // Data
                    foreach (var fila in tabla.Value)
                    {
                        var dataRow = new TableRow();
                        foreach (var columna in columnas)
                        {
                            var cell = new TableCell();
                            cell.Append(new Paragraph(new Run(new Text(fila[columna]?.ToString() ?? ""))));
                            dataRow.Append(cell);
                        }
                        wordTable.Append(dataRow);
                    }

                    body.Append(wordTable);
                    body.AppendChild(new Paragraph()); // Espacio
                }

                mainPart.Document.Save();
            }

            return stream.ToArray();
        }
    }
}