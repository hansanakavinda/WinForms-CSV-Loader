using System.Data;
using WinFormsApp1.Interfaces;

namespace WinFormsApp1.DataLoaders
{
    /// <summary>
    /// Base class for data loaders with common functionality
    /// </summary>
    public abstract class BaseDataLoader : IDataLoader
    {
        protected readonly bool _removeEmptyRows;

        protected BaseDataLoader(bool removeEmptyRows = true)
        {
            _removeEmptyRows = removeEmptyRows;
        }

        public abstract string[] SupportedExtensions { get; }
        public abstract string FileTypeDescription { get; }
        public abstract Task<DataTable> LoadAsync(string path, IProgress<int>? progress = null, CancellationToken cancellationToken = default);

        public virtual bool CanLoad(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            if (!File.Exists(path))
                return false;

            var extension = Path.GetExtension(path);
            return SupportedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
        }

        protected void RemoveEmptyRows(DataTable dataTable)
        {
            var emptyRows = dataTable.AsEnumerable()
                .Where(row => row.ItemArray.All(field =>
                    field == null ||
                    field is DBNull ||
                    string.IsNullOrWhiteSpace(field.ToString())))
                .ToList();

            foreach (var row in emptyRows)
            {
                dataTable.Rows.Remove(row);
            }

            dataTable.AcceptChanges();
        }
    }
}
