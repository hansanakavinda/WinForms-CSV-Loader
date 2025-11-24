using System.Data;

namespace WinFormsApp1.Interfaces
{
    /// <summary>
    /// Interface for loading data from various file formats into a DataTable
    /// </summary>
    public interface IDataLoader
    {
        /// <summary>
        /// Asynchronously loads data from the specified file path
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <param name="progress">Progress reporter for loading percentage (0-100)</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>Task containing DataTable with the loaded data</returns>
        Task<DataTable> LoadAsync(string path, IProgress<int>? progress = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if the file at the specified path can be loaded
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <returns>True if the file can be loaded, false otherwise</returns>
        bool CanLoad(string path);

        /// <summary>
        /// Gets the supported file extensions (e.g., ".csv", ".xlsx")
        /// </summary>
        string[] SupportedExtensions { get; }

        /// <summary>
        /// Gets the display name for the file type
        /// </summary>
        string FileTypeDescription { get; }
    }
}
