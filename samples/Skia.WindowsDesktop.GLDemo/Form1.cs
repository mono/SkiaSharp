using System;
using System.IO;
using System.Windows.Forms;

namespace Skia.WindowsDesktop.GLDemo
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
            var dir = Path.Combine(Path.GetTempPath(), "SkiaSharp.Demos", Path.GetRandomFileName());
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            SkiaSharp.Demos.CustomFontPath = Path.Combine(Path.GetDirectoryName(typeof(Form1).Assembly.Location), "Content", fontName);
            SkiaSharp.Demos.WorkingDirectory = dir;
            SkiaSharp.Demos.OpenFileDelegate = path =>
            {
                System.Diagnostics.Process.Start(Path.Combine(dir, path));
            };
            
            comboBox.Items.AddRange(SkiaSharp.Demos.SamplesForPlatform(SkiaSharp.Demos.Platform.WindowsDesktop | SkiaSharp.Demos.Platform.OpenGL));
            comboBox.SelectedIndex = 0;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            skiaView.Sample = SkiaSharp.Demos.GetSample((string)comboBox.SelectedItem);
        }
    }
}
