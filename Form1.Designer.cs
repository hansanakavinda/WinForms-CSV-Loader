namespace WinFormsApp1
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            selectFileButton = new Button();
            dataGridView1 = new DataGridView();
            loadingPanel = new Panel();
            label1 = new Label();
            loadingBar = new ProgressBar();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            loadingPanel.SuspendLayout();
            SuspendLayout();
            // 
            // selectFileButton
            // 
            selectFileButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            selectFileButton.BackColor = SystemColors.GradientActiveCaption;
            selectFileButton.Location = new Point(52, 48);
            selectFileButton.Name = "selectFileButton";
            selectFileButton.Size = new Size(146, 49);
            selectFileButton.TabIndex = 0;
            selectFileButton.Text = "Select File";
            selectFileButton.UseVisualStyleBackColor = false;
            selectFileButton.Click += selectFileButton_Click;
            // 
            // dataGridView1
            // 
            dataGridView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(52, 140);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersWidth = 51;
            dataGridView1.Size = new Size(724, 590);
            dataGridView1.TabIndex = 2;
            // 
            // loadingPanel
            // 
            loadingPanel.Anchor = AnchorStyles.None;
            loadingPanel.Controls.Add(label1);
            loadingPanel.Controls.Add(loadingBar);
            loadingPanel.Location = new Point(81, 397);
            loadingPanel.MaximumSize = new Size(664, 127);
            loadingPanel.MinimumSize = new Size(664, 127);
            loadingPanel.Name = "loadingPanel";
            loadingPanel.Size = new Size(664, 127);
            loadingPanel.TabIndex = 3;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(252, 32);
            label1.Name = "label1";
            label1.Size = new Size(122, 20);
            label1.TabIndex = 0;
            label1.Text = "Loading CSV file!";
            // 
            // loadingBar
            // 
            loadingBar.Location = new Point(44, 73);
            loadingBar.Name = "loadingBar";
            loadingBar.Size = new Size(569, 29);
            loadingBar.Style = ProgressBarStyle.Marquee;
            loadingBar.TabIndex = 1;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(834, 788);
            Controls.Add(loadingPanel);
            Controls.Add(dataGridView1);
            Controls.Add(selectFileButton);
            MinimumSize = new Size(852, 835);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "CSV Loader";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            loadingPanel.ResumeLayout(false);
            loadingPanel.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Button selectFileButton;
        private DataGridView dataGridView1;
        private Panel loadingPanel;
        private ProgressBar loadingBar;
        private Label label1;
    }
}
