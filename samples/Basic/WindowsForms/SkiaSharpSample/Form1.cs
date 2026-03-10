using System;
using System.Windows.Forms;

namespace SkiaSharpSample;

public partial class Form1 : Form
{
	private UserControl currentPage;

	public static SamplePage DefaultPage { get; set; } = SamplePage.Cpu;

	public Form1()
	{
		InitializeComponent();

		sidebarList.SelectedIndexChanged += OnSidebarSelectionChanged;
		sidebarList.SelectedIndex = (int)DefaultPage;
	}

	private void OnSidebarSelectionChanged(object sender, EventArgs e)
	{
		if (sidebarList.SelectedIndex < 0)
			return;

		currentPage?.Dispose();
		contentPanel.Controls.Clear();

		currentPage = sidebarList.SelectedIndex switch
		{
			0 => new CpuPage(),
			1 => new GpuPage(),
			2 => new DrawingPage(),
			_ => null,
		};

		if (currentPage != null)
		{
			currentPage.Dock = DockStyle.Fill;
			contentPanel.Controls.Add(currentPage);
		}
	}
}
