using System.Data;
using System.Text;
using ExcelDataReader;

namespace HoshiBookWeb.Tools
{
    public class FileReadTool
    {
        public static List<List<Dictionary<string, object>>> ReadExcelFile(string fullPath)
        {
            List<List<Dictionary<string, object>>> result = new();
            System.Text.Encoding.RegisterProvider(
                System.Text.CodePagesEncodingProvider.Instance
            );
            Encoding srcEncoding = Encoding.GetEncoding(1251);

            using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    DataSet excelInfo = reader.AsDataSet();
                    int allSheetsCount = excelInfo.Tables.Count;
                    Console.WriteLine($"allSheetsCount: {allSheetsCount}");

                    foreach (DataTable sheet in excelInfo.Tables) {
                        List<Dictionary<string, object>> sheetData = new();
                        DataRowCollection rows = sheet.Rows;
                        rows.RemoveAt(0);

                        foreach (DataRow row in sheet.Rows) {
                            Dictionary<string, object> rowData = new();
                            foreach (DataColumn column in sheet.Columns) {
                                rowData.Add(column.ColumnName, row[column]);
                            }
                            sheetData.Add(rowData);
                        }
                        result.Add(sheetData);
                    }
                }
            }
            return result;
        }
    }
}