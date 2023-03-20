using System.Data;
using System.Text;
using ExcelDataReader;

namespace HoshiBookWeb.Tools
{
    public class FileReadTool
    {
        public static List<List<Dictionary<string, object>>> ReadExcelFile(string fullPath, bool readAllSheets = true, int sheetIndex = 0)
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

                    if (!readAllSheets && sheetIndex >= allSheetsCount) {
                        throw new Exception("Sheet index is out of range");
                    }

                    sheetIndex = sheetIndex > 0 ? sheetIndex - 1 : sheetIndex;
                    int sheetCount = readAllSheets ? allSheetsCount : 1;

                    if (readAllSheets)
                    {
                        for (int i = 0; i < sheetCount; i++)
                        {
                            DataTable sheet = excelInfo.Tables[i];
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
                    else if (!readAllSheets && sheetIndex < allSheetsCount) {
                        DataTable sheet = excelInfo.Tables[sheetIndex];
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

        public static List<List<Dictionary<string, object>>> ReadExcelFile(string fullPath, bool readAllSheets = true)
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
                    int sheetCount = readAllSheets ? allSheetsCount : 1;

                    for (int i = 0; i < sheetCount; i++)
                    {
                        DataTable sheet = excelInfo.Tables[i];
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