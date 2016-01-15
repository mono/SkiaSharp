using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Skia.WindowsDesktop.Demo
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			string fontName = "content-font.ttf";
			SkiaSharp.Demos.CustomFontPath = Path.Combine(typeof(Form1).Assembly.Location, "Content", fontName);

			comboBox.Items.AddRange(SkiaSharp.Demos.SamplesForPlatform(SkiaSharp.Demos.Platform.WindowsDesktop));
			comboBox.SelectedIndex = 0;
		}

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			skiaView.OnDrawCallback = SkiaSharp.Demos.MethodForSample((string)comboBox.SelectedItem);
		}
	}
}
