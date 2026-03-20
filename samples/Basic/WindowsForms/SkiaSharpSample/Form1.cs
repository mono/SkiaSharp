using System;
using System.Windows.Forms;

namespace SkiaSharpSample;

public partial class Form1 : Form
{
	public static SamplePage DefaultPage { get; set; } = SamplePage.Cpu;

	public Form1()
	{
		InitializeComponent();
		Shown += (_, _) =>
		{
			tabControl.SelectedIndex = (int)DefaultPage;
			// Force content creation even if index was already 0
			OnTabSelectionChanged(tabControl, EventArgs.Empty);
		};
	}

	void OnTabSelectionChanged(object sender, EventArgs e)
	{
		if (tabControl.SelectedTab is not TabPage tab)
			return;

		if (tab.Controls.Count > 0)
			return;

		UserControl page = tab.Tag?.ToString() switch
		{
			"cpu" => new CpuPage(),
			"gpu" => new GpuPage(),
			"drawing" => new DrawingPage(),
			_ => new CpuPage(),
		};

		page.Dock = DockStyle.Fill;
		tab.Controls.Add(page);
		page.Invalidate();
	}
}
