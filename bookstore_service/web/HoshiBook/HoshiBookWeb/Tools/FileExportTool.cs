using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Data;
using System.Globalization;
using System.IO;

namespace HoshiBookWeb.Tools
{
    public class FileExportTool
    {
        private const string noRecordsToDisplay = "No records to display"; // Excel 無資料時顯示的訊息

        public FileExportTool() { }

        // 設定欄位寬度
        private static Columns SetDefaultColumnWidth()
        {
            Columns columns = new Columns();
            columns.Append(new Column() { Min = 1, Max = 1, Width = 25, CustomWidth = true }); // 第1欄
            columns.Append(new Column() { Min = 2, Max = 2, Width = 50, CustomWidth = true }); // 第2欄
            columns.Append(new Column() { Min = 3, Max = 400, Width = 10, CustomWidth = true }); // 第3到400欄
            return columns;
        }

        // 加入樣式設定
        private static void AddNewPartStyle(WorkbookPart workbookpart)
        {
            WorkbookStylesPart stylePart = workbookpart.AddNewPart<WorkbookStylesPart>();
            stylePart.Stylesheet = GenerateStylesheet();
            stylePart.Stylesheet.Save();
        }

        // 建立 Sheets 容器與初始 Sheet ID
        private static void AddSheet(SpreadsheetDocument spreadsheetDocument, out Sheets sheets, out uint currentSheetID)
        {
            sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild(new Sheets());
            currentSheetID = 1;
        }

        // 加入 WorkbookPart 並初始化 Workbook
        private static WorkbookPart AddWorkbookPart(SpreadsheetDocument spreadsheetDocument)
        {
            WorkbookPart workbookpart = spreadsheetDocument.AddWorkbookPart();
            workbookpart.Workbook = new Workbook();
            return workbookpart;
        }

        // 無資料時的預設訊息列
        private static void CreateDefaultWithMessage(int rowIndexCount, SheetData sheetData)
        {
            Row Sheetrow = new Row() { RowIndex = Convert.ToUInt32(rowIndexCount) };
            Cell cellHeader = new Cell() { CellReference = "A1", CellValue = new CellValue(noRecordsToDisplay), DataType = CellValues.String };
            cellHeader.StyleIndex = 1;
            Sheetrow.Append(cellHeader);
            sheetData.Append(Sheetrow);
        }

        // 寫入資料列
        private static int CreateBody(int rowIndexCount, DataTable dt, SheetData sheetData, string[] excelColumnNames)
        {
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                Row Sheetrow = new Row() { RowIndex = Convert.ToUInt32(rowIndexCount) };
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    Sheetrow.Append(GetCellWithDataType(excelColumnNames[j] + rowIndexCount, dt.Rows[i][j], dt.Columns[j].DataType));
                }
                sheetData.Append(Sheetrow);
                ++rowIndexCount;
            }
            return rowIndexCount;
        }

        // 建立標題列
        private static Row CreateHeader(int rowIndexCount, DataTable dt, int numberOfColumns, string[] excelColumnNames)
        {
            Row SheetrowHeader = new Row() { RowIndex = Convert.ToUInt32(rowIndexCount) };
            for (int n = 0; n < numberOfColumns; n++)
            {
                excelColumnNames[n] = GetExcelColumnName(n);
                Cell cellHeader = new Cell() { CellReference = excelColumnNames[n] + rowIndexCount, CellValue = new CellValue(dt.Columns[n].ColumnName), DataType = CellValues.String };
                cellHeader.StyleIndex = 2;
                SheetrowHeader.Append(cellHeader);
            }
            return SheetrowHeader;
        }

        // 將欄位索引轉為 Excel 標籤（例如 A, B... AA）
        private static string GetExcelColumnName(int columnIndex)
        {
            if (columnIndex < 26)
            {
                return ((char)('A' + columnIndex)).ToString();
            }
            char firstChar = (char)('A' + (columnIndex / 26) - 1);
            char secondChar = (char)('A' + (columnIndex % 26));
            return string.Format(CultureInfo.CurrentCulture, "{0}{1}", firstChar, secondChar);
        }

        // 建立樣式表
        private static Stylesheet GenerateStylesheet()
        {
            Fonts fonts = GenerateFonts();
            Fills fills = GenerateFills();
            Borders borders = GenerateBorders();
            CellFormats cellFormats = GenerateCellFormats();
            Column column = GenerateColumnProperty();
            Stylesheet styleSheet = new Stylesheet(fonts, fills, borders, cellFormats, column);
            return styleSheet;
        }

        private static Column GenerateColumnProperty()
        {
            return new Column { Width = 100, CustomWidth = true };
        }

        // 建立 Cell 樣式格式定義
        private static CellFormats GenerateCellFormats()
        {
            CellFormats cellFormats = new CellFormats(
                new CellFormat(new Alignment() { WrapText = true, Vertical = VerticalAlignmentValues.Top }), // default
                new CellFormat(new Alignment() { WrapText = true, Vertical = VerticalAlignmentValues.Top }) { FontId = 0, FillId = 0, BorderId = 1, ApplyBorder = true }, // default2
                new CellFormat(new Alignment() { WrapText = true, Vertical = VerticalAlignmentValues.Top }) { FontId = 1, FillId = 0, BorderId = 1, ApplyFill = true }, // header
                new CellFormat(new Alignment() { Vertical = VerticalAlignmentValues.Top }) { FontId = 0, FillId = 0, BorderId = 1, ApplyBorder = true, NumberFormatId = 15, ApplyNumberFormat = true }, // datetime
                new CellFormat(new Alignment() { WrapText = true, Vertical = VerticalAlignmentValues.Top }) { FontId = 0, FillId = 0, BorderId = 1, ApplyBorder = true, NumberFormatId = 1 }, // int
                new CellFormat(new Alignment() { WrapText = true, Vertical = VerticalAlignmentValues.Top }) { FontId = 0, FillId = 0, BorderId = 1, ApplyBorder = true, NumberFormatId = 2 } // decimal
            );
            return cellFormats;
        }

        private static Borders GenerateBorders()
        {
            // 建立 Borders 物件（儲存所有邊框樣式的集合）
            Borders borders = new Borders(

                // 預設邊框（無邊框），通常是索引 0
                new Border(),

                // 自訂邊框樣式（索引 1）
                new Border(
                    // 左邊框：設定為自動顏色 + 細線
                    new LeftBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },

                    // 右邊框：設定為自動顏色 + 細線
                    new RightBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },

                    // 上邊框：設定為自動顏色 + 細線
                    new TopBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },

                    // 下邊框：設定為自動顏色 + 細線
                    new BottomBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },

                    // 對角線邊框：預設空白
                    new DiagonalBorder()
                )
            );

            // 回傳邊框集合，可用於 CellFormat.StyleIndex 的樣式設定中
            return borders;
        }

        private static Fills GenerateFills()
        {
            // 建立 Fills 集合（儲存所有儲存格填滿樣式）
            Fills fills = new Fills(

                // 索引 0：無填滿（預設）
                new Fill(new PatternFill() { PatternType = PatternValues.None }),

                // 索引 1：灰色 125 樣式（Excel 預設用於儲存格背景偵測）
                new Fill(new PatternFill() { PatternType = PatternValues.Gray125 }),

                // 索引 2：實心填滿，前景色為灰色（ARGB = 0x66 66 66 66）
                new Fill(
                    new PatternFill(
                        new ForegroundColor
                        {
                            Rgb = new HexBinaryValue() { Value = "66666666" } // ARGB 格式，66 為透明度（不完全不透明）
                        }
                    )
                    {
                        PatternType = PatternValues.Solid // 指定為實心填滿
                    }
                )
            );

            // 回傳填滿樣式集合
            return fills;
        }


        private static Fonts GenerateFonts()
        {
            // 建立 Fonts 集合，儲存所有字型樣式
            Fonts fonts = new Fonts(

                // 索引 0：預設字型樣式
                new Font(
                    new FontSize() { Val = 12 },          // 字體大小為 12pt
                    new FontName() { Val = "Arial Unicode" } // 字體名稱為 Arial Unicode（可顯示多語系）
                ),

                // 索引 1：加粗樣式字型
                new Font(
                    new FontSize() { Val = 12 }, // 字體大小為 12pt
                    new Bold()                   // 套用加粗效果
                )
            );

            // 回傳字型集合，可供 CellFormat 使用
            return fonts;
        }

        // 根據欄位型別產生 Cell
        private static Cell GetCellWithDataType(string cellRef, object value, Type type)
        {
            // 判斷型別是否為 DateTime
            if (type == typeof(DateTime))
            {
                Cell cell = new Cell()
                {
                    DataType = new EnumValue<CellValues>(CellValues.Number), // 日期在 Excel 中是以數字儲存
                    StyleIndex = 3 // 使用預設樣式索引 3（通常會是日期格式樣式）
                };

                // 若值不是 DBNull，則轉換成 OADate 格式（Excel 可接受的數字日期）
                if (value != DBNull.Value)
                {
                    CultureInfo cultureinfo = new CultureInfo("en-US");
                    DateTime valueDate = (DateTime)value;
                    string valueString = valueDate.ToOADate().ToString(cultureinfo); // 轉為 OLE Automation 日期格式字串
                    cell.Append(new CellValue(valueString)); // 將字串加入儲存格
                }

                return cell; // 回傳處理後的 Cell
            }

            // 判斷是否為整數型（int, long, short）
            if (type == typeof(long) || type == typeof(int) || type == typeof(short))
            {
                return new Cell()
                {
                    CellReference = cellRef,
                    CellValue = new CellValue(value.ToString()), // 數值直接轉字串
                    DataType = CellValues.Number, // Excel 中的數字類型
                    StyleIndex = 4 // 使用數字樣式（例如靠右、無小數點）
                };
            }

            // 若為 decimal（小數）類型
            if (type == typeof(decimal))
            {
                return new Cell()
                {
                    CellReference = cellRef,
                    CellValue = new CellValue(value.ToString()), // 小數轉字串
                    DataType = CellValues.Number,
                    StyleIndex = 5 // 使用有小數點格式的樣式
                };
            }

            // 其他型別皆當作文字處理
            return new Cell()
            {
                CellReference = cellRef,
                CellValue = new CellValue(value.ToString()), // 一般轉為字串
                DataType = CellValues.String, // Excel 中的文字欄位
                StyleIndex = 1 // 套用預設文字樣式
            };
        }


        // 將 DataSet 轉為 Excel byte array
        public static byte[] ExportToExcelDownload(DataSet dataSet)
        {
            // 初始化輸出結果為 null
            byte[] byteResult = null;

            // 如果傳入的 DataSet 為 null，直接回傳 null
            if (dataSet == null) return byteResult;

            // 確保 DataSet 中至少有一個 DataTable
            if (dataSet.Tables.Count > 0)
            {
                // 建立記憶體串流作為 Excel 的輸出媒介
                using (MemoryStream stream = new MemoryStream())
                {
                    // 建立 Excel 文件（使用 OpenXML）
                    using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
                    {
                        // 加入 WorkbookPart（Excel 的主要部分）
                        WorkbookPart workbookpart = AddWorkbookPart(spreadsheetDocument);

                        // 加入 Sheets 元素，並初始化工作表 ID
                        AddSheet(spreadsheetDocument, out Sheets sheets, out uint currentSheetID);

                        // 加入樣式設定（如字型、格式等）
                        AddNewPartStyle(workbookpart);

                        // 初始化資料列索引（從第 1 列開始）
                        int rowIndexCount = 1;

                        // 逐一處理 DataSet 中的每一個 DataTable
                        foreach (DataTable dt in dataSet.Tables)
                        {
                            // 新增一個 WorksheetPart，表示一個工作表
                            WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
                            worksheetPart.Worksheet = new Worksheet();

                            // 設定欄位寬度
                            Columns columns = SetDefaultColumnWidth();
                            worksheetPart.Worksheet.Append(columns);

                            // 建立 SheetData（儲存資料列資料）
                            SheetData sheetData = new SheetData();
                            worksheetPart.Worksheet.AppendChild(sheetData);

                            // 建立 Sheet 實例，並加入到 sheets 中
                            Sheet sheet = new Sheet()
                            {
                                Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart), // 取得 worksheetPart 的 ID
                                SheetId = currentSheetID,                                           // 設定工作表的 ID
                                Name = string.IsNullOrWhiteSpace(dt.TableName)                     // 設定工作表名稱（如果沒指定就自動命名）
                                    ? "Sheet" + currentSheetID
                                    : dt.TableName
                            };

                            // 如果該 DataTable 沒有資料列
                            if (dt.Rows.Count == 0)
                            {
                                // 建立一列預設訊息（如 "No Data" 顯示）
                                CreateDefaultWithMessage(rowIndexCount, sheetData);
                            }
                            else
                            {
                                // 計算欄位數量
                                int numberOfColumns = dt.Columns.Count;

                                // 建立對應 Excel 欄位名稱的陣列（例如 A, B, C...）
                                string[] excelColumnNames = new string[numberOfColumns];

                                // 建立欄位標題列（Header Row）
                                Row SheetrowHeader = CreateHeader(rowIndexCount, dt, numberOfColumns, excelColumnNames);
                                sheetData.Append(SheetrowHeader); // 加入至 SheetData

                                ++rowIndexCount; // 前往下一列

                                // 建立資料列內容（Body），並回傳目前的列索引
                                rowIndexCount = CreateBody(rowIndexCount, dt, sheetData, excelColumnNames);
                            }

                            // 將 Sheet 加入 Sheets 集合中
                            sheets.Append(sheet);

                            // 準備下一個工作表的 ID
                            ++currentSheetID;

                            // 每個新工作表從第 1 列重新開始
                            rowIndexCount = 1;
                        }

                        // 儲存 Workbook 的內容
                        workbookpart.Workbook.Save();
                    }

                    // 將記憶體串流的資料轉為 byte 陣列作為輸出
                    stream.Flush();
                    stream.Position = 0; // 回到記憶體開頭
                    byteResult = new byte[stream.Length];
                    stream.Read(byteResult, 0, byteResult.Length); // 讀入 byte 陣列
                }
            }

            // 回傳建立好的 Excel 檔案內容（byte 陣列格式，可作為下載）
            return byteResult;
        }
    }
}
