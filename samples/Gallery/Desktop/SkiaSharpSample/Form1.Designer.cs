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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
			this.glview = new SkiaSharp.Views.Desktop.SKGLControl();
			this.canvas = new SkiaSharp.Views.Desktop.SKControl();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.samplesMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.toggleSamplesSlideshowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.configureBackendToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.memoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openGLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.vulkanToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripContainer1.ContentPanel.SuspendLayout();
			this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
			this.toolStripContainer1.SuspendLayout();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStripContainer1
			// 
			// 
			// toolStripContainer1.ContentPanel
			// 
			this.toolStripContainer1.ContentPanel.Controls.Add(this.glview);
			this.toolStripContainer1.ContentPanel.Controls.Add(this.canvas);
			this.toolStripContainer1.ContentPanel.Margin = new System.Windows.Forms.Padding(4);
			this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(774, 489);
			this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
			this.toolStripContainer1.Margin = new System.Windows.Forms.Padding(4);
			this.toolStripContainer1.Name = "toolStripContainer1";
			this.toolStripContainer1.Size = new System.Drawing.Size(774, 529);
			this.toolStripContainer1.TabIndex = 0;
			this.toolStripContainer1.Text = "toolStripContainer1";
			// 
			// toolStripContainer1.TopToolStripPanel
			// 
			this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.menuStrip1);
			// 
			// glview
			// 
			this.glview.BackColor = System.Drawing.Color.Black;
			this.glview.Dock = System.Windows.Forms.DockStyle.Fill;
			this.glview.Location = new System.Drawing.Point(0, 0);
			this.glview.Margin = new System.Windows.Forms.Padding(8);
			this.glview.Name = "glview";
			this.glview.Size = new System.Drawing.Size(774, 489);
			this.glview.TabIndex = 1;
			this.glview.Visible = false;
			this.glview.VSync = false;
			this.glview.PaintSurface += new System.EventHandler<SkiaSharp.Views.Desktop.SKPaintGLSurfaceEventArgs>(this.OnPaintGL);
			this.glview.Click += new System.EventHandler(this.OnSampleClicked);
			// 
			// canvas
			// 
			this.canvas.Dock = System.Windows.Forms.DockStyle.Fill;
			this.canvas.Location = new System.Drawing.Point(0, 0);
			this.canvas.Margin = new System.Windows.Forms.Padding(4);
			this.canvas.Name = "canvas";
			this.canvas.Size = new System.Drawing.Size(774, 489);
			this.canvas.TabIndex = 0;
			this.canvas.PaintSurface += new System.EventHandler<SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs>(this.OnPaintCanvas);
			this.canvas.Click += new System.EventHandler(this.OnSampleClicked);
			// 
			// menuStrip1
			// 
			this.menuStrip1.Dock = System.Windows.Forms.DockStyle.None;
			this.menuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.samplesMenu,
            this.toolsToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(774, 40);
			this.menuStrip1.TabIndex = 0;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(64, 36);
			this.fileToolStripMenuItem.Text = "&File";
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(237, 38);
			this.exitToolStripMenuItem.Text = "E&xit";
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.OnExitClicked);
			// 
			// samplesMenu
			// 
			this.samplesMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toggleSamplesSlideshowToolStripMenuItem,
            this.toolStripSeparator1});
			this.samplesMenu.Name = "samplesMenu";
			this.samplesMenu.Size = new System.Drawing.Size(116, 36);
			this.samplesMenu.Text = "&Samples";
			// 
			// toggleSamplesSlideshowToolStripMenuItem
			// 
			this.toggleSamplesSlideshowToolStripMenuItem.Name = "toggleSamplesSlideshowToolStripMenuItem";
			this.toggleSamplesSlideshowToolStripMenuItem.Size = new System.Drawing.Size(397, 38);
			this.toggleSamplesSlideshowToolStripMenuItem.Text = "Toggle Samples Slideshow";
			this.toggleSamplesSlideshowToolStripMenuItem.Click += new System.EventHandler(this.OnToggleSlideshow);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(394, 6);
			// 
			// toolsToolStripMenuItem
			// 
			this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.configureBackendToolStripMenuItem});
			this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
			this.toolsToolStripMenuItem.Size = new System.Drawing.Size(82, 36);
			this.toolsToolStripMenuItem.Text = "&Tools";
			// 
			// configureBackendToolStripMenuItem
			// 
			this.configureBackendToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.memoryToolStripMenuItem,
            this.openGLToolStripMenuItem,
            this.vulkanToolStripMenuItem});
			this.configureBackendToolStripMenuItem.Name = "configureBackendToolStripMenuItem";
			this.configureBackendToolStripMenuItem.Size = new System.Drawing.Size(318, 38);
			this.configureBackendToolStripMenuItem.Text = "Configure &Backend";
			// 
			// memoryToolStripMenuItem
			// 
			this.memoryToolStripMenuItem.Name = "memoryToolStripMenuItem";
			this.memoryToolStripMenuItem.Size = new System.Drawing.Size(205, 38);
			this.memoryToolStripMenuItem.Tag = "Memory";
			this.memoryToolStripMenuItem.Text = "&Memory";
			this.memoryToolStripMenuItem.Click += new System.EventHandler(this.OnBackendChanged);
			// 
			// openGLToolStripMenuItem
			// 
			this.openGLToolStripMenuItem.Name = "openGLToolStripMenuItem";
			this.openGLToolStripMenuItem.Size = new System.Drawing.Size(205, 38);
			this.openGLToolStripMenuItem.Tag = "OpenGL";
			this.openGLToolStripMenuItem.Text = "&OpenGL";
			this.openGLToolStripMenuItem.Click += new System.EventHandler(this.OnBackendChanged);
			// 
			// vulkanToolStripMenuItem
			// 
			this.vulkanToolStripMenuItem.Name = "vulkanToolStripMenuItem";
			this.vulkanToolStripMenuItem.Size = new System.Drawing.Size(205, 38);
			this.vulkanToolStripMenuItem.Tag = "Vulkan";
			this.vulkanToolStripMenuItem.Text = "&Vulkan";
			this.vulkanToolStripMenuItem.Click += new System.EventHandler(this.OnBackendChanged);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(192F, 192F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.ClientSize = new System.Drawing.Size(774, 529);
			this.Controls.Add(this.toolStripContainer1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MainMenuStrip = this.menuStrip1;
			this.Margin = new System.Windows.Forms.Padding(4);
			this.Name = "Form1";
			this.Text = "SkiaSharp for Windows";
			this.toolStripContainer1.ContentPanel.ResumeLayout(false);
			this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
			this.toolStripContainer1.TopToolStripPanel.PerformLayout();
			this.toolStripContainer1.ResumeLayout(false);
			this.toolStripContainer1.PerformLayout();
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ToolStripContainer toolStripContainer1;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem samplesMenu;
		private System.Windows.Forms.ToolStripMenuItem toggleSamplesSlideshowToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem configureBackendToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem memoryToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openGLToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem vulkanToolStripMenuItem;
		private SkiaSharp.Views.Desktop.SKControl canvas;
		private SkiaSharp.Views.Desktop.SKGLControl glview;
	}
}

