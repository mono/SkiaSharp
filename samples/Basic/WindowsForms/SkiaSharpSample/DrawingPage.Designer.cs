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
			this.skiaView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMouseMove);
			this.skiaView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnMouseUp);
			this.skiaView.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.OnMouseWheel);
			this.skiaView.MouseEnter += new System.EventHandler(this.OnMouseEnter);
			this.skiaView.MouseLeave += new System.EventHandler(this.OnMouseLeave);
			// 
			// DrawingPage
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.skiaView);
			this.Name = "DrawingPage";
			this.Size = new System.Drawing.Size(800, 600);
			this.ResumeLayout(false);
		}

		#endregion

		private SkiaSharp.Views.Desktop.SKControl skiaView;
	}
}
