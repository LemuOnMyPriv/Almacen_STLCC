using ClosedXML.Excel;

namespace Almacen_STLCC.Services
{
    public static class ReporteExcelGenerator
    {
        public static byte[] Generar(Dictionary<string, List<Dictionary<string, object>>> datos)
        {
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
                    worksheet.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
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
    }
}