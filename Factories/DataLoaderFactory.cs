using WinFormsApp1.Interfaces;
using WinFormsApp1.DataLoaders;
using System.Collections.Concurrent;

namespace WinFormsApp1.Factories
{
    /// <summary>
    /// Factory for creating appropriate data loaders based on file extension
    /// </summary>
    public class DataLoaderFactory
    {
        private readonly ConcurrentDictionary<string, IDataLoader> _loaders;

        public DataLoaderFactory()
        {
            _loaders = new ConcurrentDictionary<string, IDataLoader>(StringComparer.OrdinalIgnoreCase);
            RegisterDefaultLoaders();
        }

        private void RegisterDefaultLoaders()
        {
            RegisterLoader(new ExcelDataLoader(), allowOverride: false);
            RegisterLoader(new CsvDataLoader(), allowOverride: false);
        }

        /// <summary>
        /// Registers a data loader for its supported extensions
        /// </summary>
        /// <param name="loader">The loader to register</param>
        /// <param name="allowOverride">Whether to allow overriding existing loaders</param>
        public void RegisterLoader(IDataLoader loader, bool allowOverride = true)
        {
            if (loader == null)
                throw new ArgumentNullException(nameof(loader));

            foreach (var extension in loader.SupportedExtensions)
            {
                if (!allowOverride && _loaders.ContainsKey(extension))
                {
                    throw new InvalidOperationException(
                        $"Loader already registered for extension '{extension}'. " +
                        $"Set allowOverride to true to replace it.");
                }
                _loaders[extension] = loader;
            }
        }

        /// <summary>
        /// Gets the appropriate loader for a file extension
        /// </summary>
        public IDataLoader? GetLoader(string extension)
        {
            _loaders.TryGetValue(extension, out var loader);
            return loader;
        }

        /// <summary>
        /// Gets the appropriate loader for a file extension, throwing if not found
        /// </summary>
        public IDataLoader GetLoaderOrThrow(string extension)
        {
            return GetLoader(extension) 
                ?? throw new NotSupportedException($"No loader registered for extension '{extension}'");
        }

        /// <summary>
        /// Gets the file filter string for OpenFileDialog
        /// </summary>
        public string GetFileFilter()
        {
            var filters = _loaders.Values
                .Distinct()
                .Select(loader => $"{loader.FileTypeDescription} ({string.Join(", ", loader.SupportedExtensions.Select(e => "*" + e))})|{string.Join(";", loader.SupportedExtensions.Select(e => "*" + e))}")
                .ToList();

            filters.Add("All files (*.*)|*.*");

            return string.Join("|", filters);
        }

        /// <summary>
        /// Checks if a file extension is supported
        /// </summary>
        public bool IsExtensionSupported(string extension)
        {
            return _loaders.ContainsKey(extension);
        }
    }
}
