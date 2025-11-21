using CsvHelper;
using CsvHelper.Configuration;
using System.Data;
using System.Globalization;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            ConfigureDataGridView();
        }

        private void ConfigureDataGridView()
        {

            // Enable virtual mode for better performance with large datasets
            dataGridView1.VirtualMode = false; // Keep simple binding for now
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.RowHeadersVisible = false; // Improves performance
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
                    dialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                    dialog.Title = "Select a CSV file";
                    dialog.CheckFileExists = true;
                    dialog.CheckPathExists = true;

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        string filePath = dialog.FileName;

                        // Validate file before attempting to load
                        if (string.IsNullOrWhiteSpace(filePath))
                        {
                            MessageBox.Show("Invalid file path selected.", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        if (!File.Exists(filePath))
                        {
                            MessageBox.Show($"File not found: {filePath}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        var fileInfo = new FileInfo(filePath);
                        if (fileInfo.Length == 0)
                        {
                            MessageBox.Show("The selected file is empty.", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        await LoadCsvFileAsync(filePath);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open file dialog: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadCsvFileAsync(string path)
        {
            try
            {
                // Show loading indicator
                ShowLoading(true);

                // Load CSV on background thread
                var dataTable = await Task.Run(() => LoadCsvData(path));

                // Update UI on main thread
                dataGridView1.DataSource = null;
                dataGridView1.DataSource = dataTable;

                foreach (DataGridViewColumn column in dataGridView1.Columns)
                {
                    column.MinimumWidth = 120;
                    column.Width = 180;
                }

                dataGridView1.Refresh();

                // Hide loading indicator
                ShowLoading(false);

                MessageBox.Show($"Loaded successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowLoading(false);
                MessageBox.Show($"Unexpected error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DataTable LoadCsvData(string path)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                IgnoreBlankLines = true,
                BadDataFound = null,
                TrimOptions = TrimOptions.Trim
            };

            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, config))
            {
                using var dataReader = new CsvDataReader(csv);
                var dataTable = new DataTable();
                dataTable.Load(dataReader);
                return dataTable;
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
                loadingBar.MarqueeAnimationSpeed = 10;
            }
        }
    }
}
