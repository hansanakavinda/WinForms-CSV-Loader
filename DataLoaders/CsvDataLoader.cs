using CsvHelper;
using CsvHelper.Configuration;
using System.Data;
using System.Globalization;
using WinFormsApp1.Interfaces;

namespace WinFormsApp1.DataLoaders
{
    /// <summary>
    /// CSV file data loader implementation using CsvHelper
    /// </summary>
    public class CsvDataLoader : BaseDataLoader
    {
        public CsvDataLoader(bool removeEmptyRows = true) : base(removeEmptyRows)
        {
        }

        public override string[] SupportedExtensions => new[] { ".csv" };

        public override string FileTypeDescription => "CSV Files";

        public override async Task<DataTable> LoadAsync(string path, IProgress<int>? progress = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("File path cannot be null or empty", nameof(path));

            if (!File.Exists(path))
                throw new FileNotFoundException($"File not found: {path}");

            try
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    IgnoreBlankLines = true,
                    BadDataFound = null,
                    TrimOptions = TrimOptions.Trim,
                    MissingFieldFound = null,
                    HeaderValidated = null
                };

                using (var reader = new StreamReader(path))
                using (var csv = new CsvReader(reader, config))
                {
                    var dataTable = new DataTable(Path.GetFileNameWithoutExtension(path));
                    
                    // Get file size for progress calculation
                    var fileInfo = new FileInfo(path);
                    long fileSize = fileInfo.Length;
                    long lastReportedPosition = 0;
                    int reportInterval = Math.Max(1, (int)(fileSize / 100)); // Report every 1%
                    
                    // Read header
                    await csv.ReadAsync();
                    csv.ReadHeader();
                    
                    foreach (var header in csv.HeaderRecord ?? Enumerable.Empty<string>())
                    {
                        dataTable.Columns.Add(header);
                    }

                    progress?.Report(5); // Header read

                    // Read data rows
                    int rowCount = 0;
                    while (await csv.ReadAsync())
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        
                        var row = dataTable.NewRow();
                        for (int i = 0; i < csv.Parser.Count; i++)
                        {
                            row[i] = csv.GetField(i) ?? string.Empty;
                        }
                        dataTable.Rows.Add(row);
                        
                        rowCount++;
                        
                        // Report progress based on stream position
                        if (progress != null && reader.BaseStream.Position - lastReportedPosition > reportInterval)
                        {
                            lastReportedPosition = reader.BaseStream.Position;
                            int percentage = (int)((lastReportedPosition * 95) / fileSize) + 5; // 5-100%
                            progress.Report(Math.Min(percentage, 100));
                        }
                    }

                    // Report final reading progress before post-processing
                    progress?.Report(95);

                    if (_removeEmptyRows)
                        RemoveEmptyRows(dataTable);

                    progress?.Report(100); // Complete after all processing

                    return dataTable;
                }
            }
            catch (CsvHelperException ex)
            {
                throw new InvalidDataException(
                    $"CSV parsing failed at row {ex.Context?.Parser?.Row ?? 0}: {ex.Message}", 
                    ex);
            }
            catch (Exception ex) when (ex is not ArgumentException && ex is not FileNotFoundException && ex is not OperationCanceledException)
            {
                throw new InvalidDataException($"Failed to load CSV file: {ex.Message}", ex);
            }
        }

    }
}
