namespace Skia.WindowsDesktop.Demo
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.comboBox = new System.Windows.Forms.ComboBox();
			this.skiaView = new Skia.WindowsDesktop.Demo.SkiaView();
			this.SuspendLayout();
			// 
			// comboBox
			// 
			this.comboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.comboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBox.FormattingEnabled = true;
			this.comboBox.Location = new System.Drawing.Point(466, 504);
			this.comboBox.Name = "comboBox";
			this.comboBox.Size = new System.Drawing.Size(300, 28);
			this.comboBox.TabIndex = 0;
			this.comboBox.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
			// 
			// skiaView
			// 
			this.skiaView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.skiaView.Location = new System.Drawing.Point(0, 0);
			this.skiaView.Name = "skiaView";
			this.skiaView.OnDrawCallback = null;
			this.skiaView.Size = new System.Drawing.Size(778, 544);
			this.skiaView.TabIndex = 1;
			this.skiaView.Text = "skiaView";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.ClientSize = new System.Drawing.Size(778, 544);
			this.Controls.Add(this.comboBox);
			this.Controls.Add(this.skiaView);
			this.Name = "Form1";
			this.Text = "Form1";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBox;
        private SkiaView skiaView;
    }
}

