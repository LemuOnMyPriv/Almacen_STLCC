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
                    var headerCell = worksheet.Cell(1, i + 1);
                    headerCell.Value = columnas[i];
                    headerCell.Style.Font.Bold = true;
                    headerCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#88CFE0");
                    headerCell.Style.Font.FontColor = XLColor.White;
                    headerCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                }

                // Data
                for (int row = 0; row < tabla.Value.Count; row++)
                {
                    for (int col = 0; col < columnas.Count; col++)
                    {
                        var cell = worksheet.Cell(row + 2, col + 1);
                        var valor = tabla.Value[row][columnas[col]];

                        // Detectar el tipo de dato y asignarlo correctamente
                        if (valor == null)
                        {
                            cell.Value = "";
                        }
                        else if (valor is int valorInt)
                        {
                            cell.Value = valorInt;
                            cell.Style.NumberFormat.Format = "#,##0";
                            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        }
                        else if (valor is decimal valorDecimal)
                        {
                            cell.Value = valorDecimal;
                            cell.Style.NumberFormat.Format = "#,##0.00";
                            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        }
                        else if (valor is double valorDouble)
                        {
                            cell.Value = valorDouble;
                            cell.Style.NumberFormat.Format = "#,##0.00";
                            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        }
                        else if (valor is DateTime valorFecha)
                        {
                            cell.Value = valorFecha;
                            cell.Style.NumberFormat.Format = "dd/mm/yyyy";
                            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        }
                        else
                        {
                            cell.Value = valor.ToString();
                            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        }
                    }
                }

                // Ajustar columnas
                worksheet.Columns().AdjustToContents();

                // Agregar filtros
                var range = worksheet.RangeUsed();
                if (range != null)
                {
                    range.SetAutoFilter();
                }

                // Congelar primera fila
                worksheet.SheetView.FreezeRows(1);
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}