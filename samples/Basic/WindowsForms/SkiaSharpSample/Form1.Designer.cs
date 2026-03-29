namespace SkiaSharpSample
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
			if (disposing)
			{
				components?.Dispose();
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
			var resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			tabControl = new System.Windows.Forms.TabControl();
			tabCpu = new System.Windows.Forms.TabPage();
			tabGpu = new System.Windows.Forms.TabPage();
			tabDrawing = new System.Windows.Forms.TabPage();
			tabControl.SuspendLayout();
			SuspendLayout();
			// 
			// tabControl
			// 
			tabControl.Controls.Add(tabCpu);
			tabControl.Controls.Add(tabGpu);
			tabControl.Controls.Add(tabDrawing);
			tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			tabControl.Location = new System.Drawing.Point(0, 0);
			tabControl.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			tabControl.Name = "tabControl";
			tabControl.SelectedIndex = 0;
			tabControl.Size = new System.Drawing.Size(884, 561);
			tabControl.TabIndex = 0;
			tabControl.SelectedIndexChanged += OnTabSelectionChanged;
			// 
			// tabCpu
			// 
			tabCpu.Location = new System.Drawing.Point(4, 24);
			tabCpu.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			tabCpu.Name = "tabCpu";
			tabCpu.Size = new System.Drawing.Size(876, 533);
			tabCpu.TabIndex = 0;
			tabCpu.Tag = "cpu";
			tabCpu.Text = "CPU";
			tabCpu.UseVisualStyleBackColor = true;
			// 
			// tabGpu
			// 
			tabGpu.Location = new System.Drawing.Point(4, 24);
			tabGpu.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			tabGpu.Name = "tabGpu";
			tabGpu.Size = new System.Drawing.Size(479, 286);
			tabGpu.TabIndex = 1;
			tabGpu.Tag = "gpu";
			tabGpu.Text = "GPU";
			tabGpu.UseVisualStyleBackColor = true;
			// 
			// tabDrawing
			// 
			tabDrawing.Location = new System.Drawing.Point(4, 24);
			tabDrawing.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			tabDrawing.Name = "tabDrawing";
			tabDrawing.Size = new System.Drawing.Size(479, 286);
			tabDrawing.TabIndex = 2;
			tabDrawing.Tag = "drawing";
			tabDrawing.Text = "Drawing";
			tabDrawing.UseVisualStyleBackColor = true;
			// 
			// Form1
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			ClientSize = new System.Drawing.Size(884, 561);
			Controls.Add(tabControl);
			Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			Name = "Form1";
			Text = "SkiaSharp Sample";
			tabControl.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion

		private System.Windows.Forms.TabControl tabControl;
		private System.Windows.Forms.TabPage tabCpu;
		private System.Windows.Forms.TabPage tabGpu;
		private System.Windows.Forms.TabPage tabDrawing;
	}
}

