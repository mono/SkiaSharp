namespace SkiaSharpSample
{
	partial class GpuPage
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
				animationTimer.Stop();
				animationTimer.Dispose();
				if (shaderBuilder?.IsValueCreated == true)
					shaderBuilder.Value.Dispose();
				shaderBuilder = null;
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
			this.components = new System.ComponentModel.Container();
			this.glControl = new SkiaSharp.Views.Desktop.SKGLControl();
			this.fpsLabel = new System.Windows.Forms.Label();
			this.animationTimer = new System.Windows.Forms.Timer(this.components);
			this.SuspendLayout();
			// 
			// glControl
			// 
			this.glControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.glControl.Location = new System.Drawing.Point(0, 0);
			this.glControl.Name = "glControl";
			this.glControl.Size = new System.Drawing.Size(800, 600);
			this.glControl.TabIndex = 0;
			this.glControl.PaintSurface += new System.EventHandler<SkiaSharp.Views.Desktop.SKPaintGLSurfaceEventArgs>(this.OnPaintSurface);
			this.glControl.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnMouseDown);
			this.glControl.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMouseMove);
			this.glControl.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnMouseUp);
			// 
			// fpsLabel
			// 
			this.fpsLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.fpsLabel.AutoSize = true;
			this.fpsLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(170)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.fpsLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.fpsLabel.ForeColor = System.Drawing.Color.White;
			this.fpsLabel.Location = new System.Drawing.Point(370, 8);
			this.fpsLabel.Name = "fpsLabel";
			this.fpsLabel.Padding = new System.Windows.Forms.Padding(12, 6, 12, 6);
			this.fpsLabel.Size = new System.Drawing.Size(80, 35);
			this.fpsLabel.TabIndex = 1;
			this.fpsLabel.Text = "FPS: --";
			// 
			// animationTimer
			// 
			this.animationTimer.Interval = 16;
			this.animationTimer.Tick += new System.EventHandler(this.OnAnimationTick);
			// 
			// GpuPage
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.fpsLabel);
			this.Controls.Add(this.glControl);
			this.Name = "GpuPage";
			this.Size = new System.Drawing.Size(800, 600);
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private SkiaSharp.Views.Desktop.SKGLControl glControl;
		private System.Windows.Forms.Label fpsLabel;
		private System.Windows.Forms.Timer animationTimer;
	}
}
