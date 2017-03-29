using System;
using System.Reflection;

namespace SkiaSharp
{
	// use reflection to access the real type in another assembly
	// this will be moved at a later stage

	internal class SKSvgDomInternal : IDisposable
	{
		private static readonly Type svgDomType;
		private static readonly MethodInfo svgDomCreateMember;
		private static readonly PropertyInfo svgDomContainerSizeProperty;
		private static readonly MethodInfo svgDomRenderMember;

		static SKSvgDomInternal()
		{
			var assembly = typeof(SKObject).GetTypeInfo().Assembly;

			svgDomType = assembly.GetType("SkiaSharp.SKSvgDom");
			svgDomCreateMember = svgDomType.GetRuntimeMethod("Create", new[] { typeof(SKStream) });
			svgDomContainerSizeProperty = svgDomType.GetRuntimeProperty("ContainerSize");
			svgDomRenderMember = svgDomType.GetRuntimeMethod("Render", new[] { typeof(SKCanvas) });
		}

		private object dom;

		private SKSvgDomInternal()
		{
		}

		public static SKSvgDomInternal Create(SKStream stream)
		{
			return new SKSvgDomInternal { dom = svgDomCreateMember.Invoke(null, new[] { stream }) };
		}

		public void Dispose()
		{
			((IDisposable)dom).Dispose();
		}

		public SKSize ContainerSize
		{
			get { return (SKSize)svgDomContainerSizeProperty.GetValue(dom); }
			set { svgDomContainerSizeProperty.SetValue(dom, value); }
		}

		public void Render(SKCanvas canvas)
		{
			svgDomRenderMember.Invoke(dom, new[] { canvas });
		}
	}
}
