using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui.Controls.Hosting;
using SkiaSharp.Views.Maui.Handlers;
using SkiaSharp.Views.Maui.Platform;
using Xunit;
using SkiaAppHostBuilderExtensions = SkiaSharp.Views.Maui.Controls.Hosting.AppHostBuilderExtensions;

namespace SkiaSharp.Views.Maui.Controls.WPF.Tests;

[Collection("WPF handlers")]
public class WPFHandlerTests
{
	[Fact]
	public void WPFRegistrationSharesStableHostingNamespace()
	{
		Assert.Equal(typeof(SkiaAppHostBuilderExtensions).Namespace, typeof(WPFAppHostBuilderExtensions).Namespace);
	}

	[Fact]
	public void WPFHandlersShareStableMauiNamespaces()
	{
		Assert.Equal(typeof(SKCanvasViewHandler).Namespace, typeof(WPFSKCanvasViewHandler).Namespace);
		Assert.Equal(typeof(SKGLViewHandler).Namespace, typeof(WPFSKGLViewHandler).Namespace);
		Assert.Equal("SkiaSharp.Views.Maui.Platform", typeof(WPFTouchHandler).Namespace);
	}

	[Theory]
	[InlineData(1)]
	[InlineData(2)]
	public void RegistrationConfiguresOnlyWPFCanvasHandlers(int registrationCount)
	{
		var builder = MauiApp.CreateBuilder(useDefaults: false);
		for (var i = 0; i < registrationCount; i++)
			Assert.Same(builder, builder.UseSkiaSharpWPF());

		using var app = builder.Build();
		var handlers = app.Services.GetRequiredService<IMauiHandlersFactory>();
		Assert.Equal(typeof(WPFSKCanvasViewHandler), handlers.GetHandlerType(typeof(SKCanvasView)));
		Assert.Equal(typeof(WPFSKGLViewHandler), handlers.GetHandlerType(typeof(SKGLView)));
		Assert.Null(handlers.GetHandlerType(typeof(Image)));
		Assert.Null(handlers.GetHandlerType(typeof(ImageButton)));
	}

	[Fact]
	public void CanvasMapperIncludesAllCanvasPropertiesAndCommands()
	{
		Assert.NotNull(WPFSKCanvasViewHandler.SKCanvasViewMapper[nameof(ISKCanvasView.EnableTouchEvents)]);
		Assert.NotNull(WPFSKCanvasViewHandler.SKCanvasViewMapper[nameof(ISKCanvasView.IgnorePixelScaling)]);
		Assert.NotNull(WPFSKCanvasViewHandler.SKCanvasViewCommandMapper[nameof(ISKCanvasView.InvalidateSurface)]);
	}

	[Fact]
	public void GLMapperIncludesAllGPUPropertiesAndCommands()
	{
		Assert.NotNull(WPFSKGLViewHandler.SKGLViewMapper[nameof(ISKGLView.EnableTouchEvents)]);
		Assert.NotNull(WPFSKGLViewHandler.SKGLViewMapper[nameof(ISKGLView.IgnorePixelScaling)]);
		Assert.NotNull(WPFSKGLViewHandler.SKGLViewMapper[nameof(ISKGLView.HasRenderLoop)]);
		Assert.NotNull(WPFSKGLViewHandler.SKGLViewCommandMapper[nameof(ISKGLView.InvalidateSurface)]);
	}

	[Fact]
	public void CanvasConstructorUsesCustomPropertyAndCommandMappers() => WPFTestThread.Run(() =>
	{
		var mapped = 0;
		var invoked = 0;
		var mapper = new PropertyMapper<ISKCanvasView, WPFSKCanvasViewHandler>
		{
			[nameof(ISKCanvasView.IgnorePixelScaling)] = (_, view) => mapped += view.IgnorePixelScaling ? 1 : 0,
		};
		var commands = new CommandMapper<ISKCanvasView, WPFSKCanvasViewHandler>
		{
			[nameof(ISKCanvasView.InvalidateSurface)] = (_, _, _) => invoked++,
		};
		using var app = WPFTestThread.CreateApp();
		var handler = new WPFSKCanvasViewHandler(mapper, commands);
		handler.SetMauiContext(new MauiContext(app.Services));
		var canvas = new SKCanvasView { IgnorePixelScaling = true };
		handler.SetVirtualView(canvas);
		handler.Invoke(nameof(ISKCanvasView.InvalidateSurface), null);
		Assert.Equal(1, mapped);
		Assert.Equal(1, invoked);
		((IElementHandler)handler).DisconnectHandler();
	});

	[Fact]
	public void GLConstructorUsesCustomPropertyAndCommandMappers() => WPFTestThread.Run(() =>
	{
		var mapped = 0;
		var invoked = 0;
		var mapper = new PropertyMapper<ISKGLView, WPFSKGLViewHandler>
		{
			[nameof(ISKGLView.IgnorePixelScaling)] = (_, view) => mapped += view.IgnorePixelScaling ? 1 : 0,
		};
		var commands = new CommandMapper<ISKGLView, WPFSKGLViewHandler>
		{
			[nameof(ISKGLView.InvalidateSurface)] = (_, _, _) => invoked++,
		};
		using var app = WPFTestThread.CreateApp();
		var handler = new WPFSKGLViewHandler(mapper, commands);
		handler.SetMauiContext(new MauiContext(app.Services));
		var canvas = new SKGLView { IgnorePixelScaling = true };
		handler.SetVirtualView(canvas);
		handler.Invoke(nameof(ISKGLView.InvalidateSurface), null);
		Assert.Equal(1, mapped);
		Assert.Equal(1, invoked);
		((IElementHandler)handler).DisconnectHandler();
	});

	[Fact]
	public void ConstructorsAcceptDefaultAndSingleCustomMapperOverloads() => WPFTestThread.Run(() =>
	{
		Assert.NotNull(new WPFSKCanvasViewHandler());
		Assert.NotNull(new WPFSKCanvasViewHandler(null));
		Assert.NotNull(new WPFSKCanvasViewHandler(null, null));
		Assert.NotNull(new WPFSKGLViewHandler());
		Assert.NotNull(new WPFSKGLViewHandler(null));
		Assert.NotNull(new WPFSKGLViewHandler(null, null));
	});

	[Theory]
	[InlineData(false, 50, 75)]
	[InlineData(true, 25, 30)]
	public void TouchCoordinatesFollowCanvasScaling(bool ignorePixelScaling, float expectedX, float expectedY)
	{
		var point = WPFTouchHandler.GetTouchLocation(25, 30, 2, 2.5, ignorePixelScaling);
		Assert.Equal(expectedX, point.X);
		Assert.Equal(expectedY, point.Y);
	}

}
