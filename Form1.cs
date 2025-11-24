using System.Data;
using WinFormsApp1.Factories;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        private readonly DataLoaderFactory _loaderFactory;
        private DataTable? _currentDataTable;
        private CancellationTokenSource? _cancellationTokenSource;

        public Form1()
        {
            InitializeComponent();
            ConfigureDataGridView();
            _loaderFactory = new DataLoaderFactory();
        }

        private void ConfigureDataGridView()
        {
            dataGridView1.VirtualMode = false;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ShowLoading(false);
        }

        private async void selectFileButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var dialog = new OpenFileDialog())
                {
                    dialog.Filter = _loaderFactory.GetFileFilter();
                    dialog.Title = "Select a data file";
                    dialog.CheckFileExists = true;
                    dialog.CheckPathExists = true;

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        string filePath = dialog.FileName;

                        var (isValid, errorMessage) = ValidateFile(filePath);
                        if (!isValid)
                        {
                            MessageBox.Show(errorMessage, "Validation Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        await LoadDataFileAsync(filePath);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open file dialog: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadDataFileAsync(string path)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                ShowLoading(true);

                string extension = Path.GetExtension(path);
                var loader = _loaderFactory.GetLoaderOrThrow(extension);

                // Create progress reporter
                var progress = new Progress<int>(percentage =>
                {
                    UpdateProgress(percentage);
                });

                // Use async loader method with cancellation and progress
                var dataTable = await loader.LoadAsync(path, progress, _cancellationTokenSource.Token);

                // Ensure progress bar reaches 100% and is visible
                UpdateProgress(100);
                await Task.Delay(300); // Brief delay to show completion

                // Dispose previous DataTable
                SetDataSource(dataTable);

                // Configure columns with constraints
                foreach (DataGridViewColumn column in dataGridView1.Columns)
                {
                    column.MinimumWidth = 80;
                    column.Width = 200;
                }

                ShowLoading(false);

                MessageBox.Show(
                    $"Loaded {dataTable.Rows.Count:N0} rows and {dataTable.Columns.Count} columns successfully",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (OperationCanceledException)
            {
                ShowLoading(false);
                MessageBox.Show("File loading was cancelled.", "Cancelled",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (FileNotFoundException ex)
            {
                ShowLoading(false);
                MessageBox.Show($"File not found: {ex.Message}", "File Not Found",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (UnauthorizedAccessException)
            {
                ShowLoading(false);
                MessageBox.Show("Access denied. Please check file permissions.", "Access Denied",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (InvalidDataException ex)
            {
                ShowLoading(false);
                MessageBox.Show($"Invalid data format: {ex.Message}", "Data Format Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (NotSupportedException ex)
            {
                ShowLoading(false);
                MessageBox.Show(ex.Message, "Unsupported File Type",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                ShowLoading(false);
                MessageBox.Show($"Unexpected error: {ex.Message}\n\nDetails: {ex.GetType().Name}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowLoading(bool show)
        {
            loadingPanel.Visible = show;
            loadingPanel.BringToFront();
            selectFileButton.Enabled = !show;
            dataGridView1.Enabled = !show;

            if (show)
            {
                loadingBar.Style = ProgressBarStyle.Continuous;
                loadingBar.Value = 0;
                label1.Text = "Loading CSV file... 0%";
            }
        }

        private void UpdateProgress(int percentage)
        {
            if (loadingBar.InvokeRequired)
            {
                loadingBar.Invoke(() => UpdateProgress(percentage));
                return;
            }

            int value = Math.Min(percentage, 100);
            
            // Windows ProgressBar visual bug workaround
            if (value == 100)
            {
                // Set to max+1 then back to 100 to force full render
                loadingBar.Maximum = 101;
                loadingBar.Value = 101;
                loadingBar.Maximum = 100;
                loadingBar.Value = 100;
            }
            else
            {
                loadingBar.Value = value;
            }
            
            label1.Text = $"Loading CSV file... {percentage}%";
        }

        private (bool isValid, string? errorMessage) ValidateFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return (false, "Invalid file path selected.");

            if (!File.Exists(path))
                return (false, $"File not found: {path}");

            var fileInfo = new FileInfo(path);
            if (fileInfo.Length == 0)
                return (false, "The selected file is empty.");

            string extension = Path.GetExtension(path);
            if (!_loaderFactory.IsExtensionSupported(extension))
                return (false, $"File type '{extension}' is not supported.");

            return (true, null);
        }

        private void SetDataSource(DataTable newDataTable)
        {
            dataGridView1.SuspendLayout();
            try
            {
                _currentDataTable?.Dispose();
                _currentDataTable = newDataTable;
                dataGridView1.DataSource = null;
                dataGridView1.DataSource = newDataTable;
            }
            finally
            {
                dataGridView1.ResumeLayout();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _currentDataTable?.Dispose();
            _cancellationTokenSource?.Dispose();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            _cancellationTokenSource?.Cancel();
        }
    }
}
