using OfficeOpenXml;
using System.Data;
using WinFormsApp1.Interfaces;

namespace WinFormsApp1.DataLoaders
{
    /// <summary>
    /// Excel file data loader implementation using EPPlus
    /// </summary>
    public class ExcelDataLoader : BaseDataLoader
    {
        public ExcelDataLoader(bool removeEmptyRows = true) : base(removeEmptyRows)
        {
        }

        public override string[] SupportedExtensions => new[] { ".xlsx", ".xlsm" };

        public override string FileTypeDescription => "Excel Files";

        public override async Task<DataTable> LoadAsync(string path, IProgress<int>? progress = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("File path cannot be null or empty", nameof(path));

            if (!File.Exists(path))
                throw new FileNotFoundException($"File not found: {path}");

            try
            {
                return await Task.Run(() =>
                {
                    using var package = new ExcelPackage(new FileInfo(path));
                    
                    if (package.Workbook.Worksheets.Count == 0)
                        throw new InvalidDataException("The Excel file contains no worksheets.");
                    
                    var worksheet = package.Workbook.Worksheets[0]; // Load first worksheet

                    if (worksheet.Dimension == null)
                        throw new InvalidDataException("The Excel worksheet is empty.");

                    var dataTable = new DataTable(worksheet.Name);

                    progress?.Report(5);

                    int startRow = worksheet.Dimension.Start.Row;
                    int startCol = worksheet.Dimension.Start.Column;
                    int endRow = worksheet.Dimension.End.Row;
                    int endCol = worksheet.Dimension.End.Column;

                    // Read headers from first row
                    for (int col = startCol; col <= endCol; col++)
                    {
                        var cellValue = worksheet.Cells[startRow, col].Value;
                        string columnName = cellValue?.ToString() ?? $"Column{col}";
                        dataTable.Columns.Add(columnName);
                    }

                    progress?.Report(10);

                    int totalRows = endRow - startRow;
                    int rowsProcessed = 0;

                    // Read data rows
                    for (int row = startRow + 1; row <= endRow; row++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var dataRow = dataTable.NewRow();
                        for (int col = startCol; col <= endCol; col++)
                        {
                            var cellValue = worksheet.Cells[row, col].Value;
                            dataRow[col - startCol] = cellValue ?? DBNull.Value;
                        }
                        dataTable.Rows.Add(dataRow);

                        rowsProcessed++;

                        // Report progress
                        if (progress != null && rowsProcessed % Math.Max(1, totalRows / 85) == 0)
                        {
                            int percentage = 10 + (rowsProcessed * 85 / totalRows);
                            progress.Report(Math.Min(percentage, 95));
                        }
                    }

                    progress?.Report(95);

                    if (_removeEmptyRows)
                        RemoveEmptyRows(dataTable);

                    progress?.Report(100);

                    return dataTable;
                }, cancellationToken);
            }
            catch (System.IO.IOException ex)
            {
                throw new InvalidDataException(
                    "Failed to read Excel file. The file may be corrupted, password-protected, or opened in another program.", 
                    ex);
            }
            catch (InvalidDataException)
            {
                throw;
            }
            catch (Exception ex) when (ex is not ArgumentException && ex is not FileNotFoundException && ex is not OperationCanceledException)
            {
                throw new InvalidDataException($"Failed to load Excel file: {ex.Message}", ex);
            }
        }

    }
}
