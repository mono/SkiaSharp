namespace SkiaSharpSample
{
	partial class DrawingPage
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
			if (disposing)
			{
				foreach (var (path, _, _) in strokes)
					path.Dispose();
				currentPath?.Dispose();
				components?.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.skiaView = new SkiaSharp.Views.Desktop.SKControl();
			this.toolboxPanel = new System.Windows.Forms.FlowLayoutPanel();
			this.btnBlack = new System.Windows.Forms.Button();
			this.btnRed = new System.Windows.Forms.Button();
			this.btnBlue = new System.Windows.Forms.Button();
			this.btnGreen = new System.Windows.Forms.Button();
			this.btnOrange = new System.Windows.Forms.Button();
			this.btnPurple = new System.Windows.Forms.Button();
			this.brushSlider = new System.Windows.Forms.TrackBar();
			this.sizeLabel = new System.Windows.Forms.Label();
			this.btnClear = new System.Windows.Forms.Button();
			this.toolboxPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.brushSlider)).BeginInit();
			this.SuspendLayout();
			// 
			// skiaView
			// 
			this.skiaView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.skiaView.Location = new System.Drawing.Point(0, 0);
			this.skiaView.Name = "skiaView";
			this.skiaView.Size = new System.Drawing.Size(800, 600);
			this.skiaView.TabIndex = 0;
			this.skiaView.PaintSurface += new System.EventHandler<SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs>(this.OnPaintSurface);
			this.skiaView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnMouseDown);
			this.skiaView.MouseEnter += new System.EventHandler(this.OnMouseEnter);
			this.skiaView.MouseLeave += new System.EventHandler(this.OnMouseLeave);
			this.skiaView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMouseMove);
			this.skiaView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnMouseUp);
			this.skiaView.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.OnMouseWheel);
			// 
			// toolboxPanel
			// 
			this.toolboxPanel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.toolboxPanel.AutoSize = true;
			this.toolboxPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.toolboxPanel.BackColor = System.Drawing.Color.FromArgb(200, 30, 30, 30);
			this.toolboxPanel.Controls.Add(this.btnBlack);
			this.toolboxPanel.Controls.Add(this.btnRed);
			this.toolboxPanel.Controls.Add(this.btnBlue);
			this.toolboxPanel.Controls.Add(this.btnGreen);
			this.toolboxPanel.Controls.Add(this.btnOrange);
			this.toolboxPanel.Controls.Add(this.btnPurple);
			this.toolboxPanel.Controls.Add(this.brushSlider);
			this.toolboxPanel.Controls.Add(this.sizeLabel);
			this.toolboxPanel.Location = new System.Drawing.Point(175, 540);
			this.toolboxPanel.Name = "toolboxPanel";
			this.toolboxPanel.Padding = new System.Windows.Forms.Padding(10, 10, 10, 10);
			this.toolboxPanel.Size = new System.Drawing.Size(450, 52);
			this.toolboxPanel.TabIndex = 1;
			this.toolboxPanel.WrapContents = false;
			// 
			// btnBlack
			// 
			this.btnBlack.Cursor = System.Windows.Forms.Cursors.Hand;
			this.btnBlack.FlatAppearance.BorderSize = 0;
			this.btnBlack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnBlack.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.btnBlack.Name = "btnBlack";
			this.btnBlack.Size = new System.Drawing.Size(32, 32);
			this.btnBlack.TabIndex = 0;
			this.btnBlack.Click += new System.EventHandler(this.OnColorClick);
			// 
			// btnRed
			// 
			this.btnRed.Cursor = System.Windows.Forms.Cursors.Hand;
			this.btnRed.FlatAppearance.BorderSize = 0;
			this.btnRed.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnRed.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.btnRed.Name = "btnRed";
			this.btnRed.Size = new System.Drawing.Size(32, 32);
			this.btnRed.TabIndex = 1;
			this.btnRed.Click += new System.EventHandler(this.OnColorClick);
			// 
			// btnBlue
			// 
			this.btnBlue.Cursor = System.Windows.Forms.Cursors.Hand;
			this.btnBlue.FlatAppearance.BorderSize = 0;
			this.btnBlue.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnBlue.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.btnBlue.Name = "btnBlue";
			this.btnBlue.Size = new System.Drawing.Size(32, 32);
			this.btnBlue.TabIndex = 2;
			this.btnBlue.Click += new System.EventHandler(this.OnColorClick);
			// 
			// btnGreen
			// 
			this.btnGreen.Cursor = System.Windows.Forms.Cursors.Hand;
			this.btnGreen.FlatAppearance.BorderSize = 0;
			this.btnGreen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnGreen.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.btnGreen.Name = "btnGreen";
			this.btnGreen.Size = new System.Drawing.Size(32, 32);
			this.btnGreen.TabIndex = 3;
			this.btnGreen.Click += new System.EventHandler(this.OnColorClick);
			// 
			// btnOrange
			// 
			this.btnOrange.Cursor = System.Windows.Forms.Cursors.Hand;
			this.btnOrange.FlatAppearance.BorderSize = 0;
			this.btnOrange.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnOrange.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.btnOrange.Name = "btnOrange";
			this.btnOrange.Size = new System.Drawing.Size(32, 32);
			this.btnOrange.TabIndex = 4;
			this.btnOrange.Click += new System.EventHandler(this.OnColorClick);
			// 
			// btnPurple
			// 
			this.btnPurple.Cursor = System.Windows.Forms.Cursors.Hand;
			this.btnPurple.FlatAppearance.BorderSize = 0;
			this.btnPurple.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnPurple.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.btnPurple.Name = "btnPurple";
			this.btnPurple.Size = new System.Drawing.Size(32, 32);
			this.btnPurple.TabIndex = 5;
			this.btnPurple.Click += new System.EventHandler(this.OnColorClick);
			// 
			// brushSlider
			// 
			this.brushSlider.BackColor = System.Drawing.Color.FromArgb(50, 50, 50);
			this.brushSlider.Margin = new System.Windows.Forms.Padding(12, 2, 4, 0);
			this.brushSlider.Maximum = 50;
			this.brushSlider.Minimum = 1;
			this.brushSlider.Name = "brushSlider";
			this.brushSlider.Size = new System.Drawing.Size(140, 28);
			this.brushSlider.TabIndex = 6;
			this.brushSlider.TickStyle = System.Windows.Forms.TickStyle.None;
			this.brushSlider.Value = 4;
			this.brushSlider.ValueChanged += new System.EventHandler(this.OnBrushSliderChanged);
			// 
			// sizeLabel
			// 
			this.sizeLabel.AutoSize = true;
			this.sizeLabel.BackColor = System.Drawing.Color.Transparent;
			this.sizeLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
			this.sizeLabel.ForeColor = System.Drawing.Color.White;
			this.sizeLabel.Margin = new System.Windows.Forms.Padding(4, 8, 4, 0);
			this.sizeLabel.Name = "sizeLabel";
			this.sizeLabel.Size = new System.Drawing.Size(14, 15);
			this.sizeLabel.TabIndex = 7;
			this.sizeLabel.Text = "4";
			// 
			// btnClear
			// 
			this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnClear.BackColor = System.Drawing.Color.FromArgb(200, 50, 50, 50);
			this.btnClear.Cursor = System.Windows.Forms.Cursors.Hand;
			this.btnClear.FlatAppearance.BorderSize = 0;
			this.btnClear.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(100, 100, 100);
			this.btnClear.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(80, 80, 80);
			this.btnClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnClear.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
			this.btnClear.ForeColor = System.Drawing.Color.White;
			this.btnClear.Location = new System.Drawing.Point(712, 16);
			this.btnClear.Name = "btnClear";
			this.btnClear.Size = new System.Drawing.Size(72, 36);
			this.btnClear.TabIndex = 2;
			this.btnClear.Text = "Clear";
			this.btnClear.Click += new System.EventHandler(this.OnClearClick);
			// 
			// DrawingPage
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.btnClear);
			this.Controls.Add(this.toolboxPanel);
			this.Controls.Add(this.skiaView);
			this.Name = "DrawingPage";
			this.Size = new System.Drawing.Size(800, 600);
			this.toolboxPanel.ResumeLayout(false);
			this.toolboxPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.brushSlider)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private SkiaSharp.Views.Desktop.SKControl skiaView;
		private System.Windows.Forms.FlowLayoutPanel toolboxPanel;
		private System.Windows.Forms.Button btnBlack;
		private System.Windows.Forms.Button btnRed;
		private System.Windows.Forms.Button btnBlue;
		private System.Windows.Forms.Button btnGreen;
		private System.Windows.Forms.Button btnOrange;
		private System.Windows.Forms.Button btnPurple;
		private System.Windows.Forms.TrackBar brushSlider;
		private System.Windows.Forms.Label sizeLabel;
		private System.Windows.Forms.Button btnClear;
	}
}
