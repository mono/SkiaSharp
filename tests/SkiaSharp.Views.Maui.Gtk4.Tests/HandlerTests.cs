using System;
using System.Reflection;
using System.Runtime.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui.Controls.Hosting;
using CanvasHandler = SkiaSharp.Views.Maui.Handlers.Gtk4.SKCanvasViewHandler;
using GLHandler = SkiaSharp.Views.Maui.Handlers.Gtk4.SKGLViewHandler;
using Xunit;

namespace SkiaSharp.Views.Maui.Gtk4.Tests;

public class HandlerTests
{
	[Theory]
	[InlineData(typeof(CanvasHandler), "SKCanvasViewMapper", "SKCanvasViewCommandMapper")]
	[InlineData(typeof(GLHandler), "SKGLViewMapper", "SKGLViewCommandMapper")]
	public void HandlersExposeBackendParity(Type handlerType, string mapperName, string commandMapperName)
	{
		Assert.False(handlerType.IsSealed);
		Assert.Equal("SkiaSharp.Views.Maui.Handlers.Gtk4", handlerType.Namespace);
		Assert.NotNull(handlerType.GetConstructor(Type.EmptyTypes));
		Assert.NotNull(handlerType.GetConstructor(new[] { typeof(PropertyMapper), typeof(CommandMapper) }));
		CheckMapper(handlerType, mapperName);
		CheckMapper(handlerType, commandMapperName);
		Assert.NotNull(handlerType.GetMethod("MapEnableTouchEvents", BindingFlags.Public | BindingFlags.Static));
		Assert.NotNull(handlerType.GetMethod("MapIgnorePixelScaling", BindingFlags.Public | BindingFlags.Static));
		Assert.NotNull(handlerType.GetMethod("OnInvalidateSurface", BindingFlags.Public | BindingFlags.Static));
		Assert.Empty(handlerType.GetCustomAttributes<SupportedOSPlatformAttribute>());
	}

	[Fact]
	public void GpuHandlerExposesRenderLoopMapping() =>
		Assert.NotNull(typeof(GLHandler).GetMethod("MapHasRenderLoop", BindingFlags.Public | BindingFlags.Static));

	[Fact]
	public void MauiGpuHandlerUsesNativeGtkView() =>
		Assert.Contains(typeof(SkiaSharp.Views.Gtk.SKGLArea),
			typeof(GLHandler).BaseType!.GenericTypeArguments);

	[Fact]
	public void ConstructorsAcceptDefaultAndCustomMappersWithoutCreatingNativeWidgets()
	{
		var canvasProperties = new PropertyMapper<ISKCanvasView, CanvasHandler>(CanvasHandler.SKCanvasViewMapper);
		var canvasCommands = new CommandMapper<ISKCanvasView, CanvasHandler>(CanvasHandler.SKCanvasViewCommandMapper);
		var gpuProperties = new PropertyMapper<ISKGLView, GLHandler>(GLHandler.SKGLViewMapper);
		var gpuCommands = new CommandMapper<ISKGLView, GLHandler>(GLHandler.SKGLViewCommandMapper);

		Assert.Null(((IElementHandler)new CanvasHandler()).PlatformView);
		Assert.Null(((IElementHandler)new CanvasHandler(null, null)).PlatformView);
		Assert.Null(((IElementHandler)new CanvasHandler(canvasProperties, canvasCommands)).PlatformView);
		Assert.Null(((IElementHandler)new GLHandler()).PlatformView);
		Assert.Null(((IElementHandler)new GLHandler(null, null)).PlatformView);
		Assert.Null(((IElementHandler)new GLHandler(gpuProperties, gpuCommands)).PlatformView);
		Assert.Null(((IElementHandler)new DerivedCanvasHandler(canvasProperties, canvasCommands)).PlatformView);
		Assert.Null(((IElementHandler)new DerivedGLHandler(gpuProperties, gpuCommands)).PlatformView);
	}

	[Fact]
	public void Gtk4RegistrationOverridesPortableHandlersWithoutCreatingNativeWidgets()
	{
		var builder = MauiApp.CreateBuilder();
		Assert.Same(builder, builder.UseSkiaSharpGtk4());
		using var app = builder.Build();
		var handlers = app.Services.GetRequiredService<IMauiHandlersFactory>();

		Assert.Equal(typeof(CanvasHandler), handlers.GetHandlerType(typeof(SKCanvasView)));
		Assert.Equal(typeof(GLHandler), handlers.GetHandlerType(typeof(SKGLView)));
		var images = app.Services.GetRequiredService<IImageSourceServiceProvider>();
		Assert.Throws<InvalidOperationException>(() => images.GetImageSourceService<ISKImageImageSource>());
		Assert.Throws<InvalidOperationException>(() => images.GetImageSourceService<ISKBitmapImageSource>());
		Assert.Throws<InvalidOperationException>(() => images.GetImageSourceService<ISKPixmapImageSource>());
		Assert.Throws<InvalidOperationException>(() => images.GetImageSourceService<ISKPictureImageSource>());
		Assert.Empty(typeof(Gtk4AppHostBuilderExtensions).GetMethod(nameof(Gtk4AppHostBuilderExtensions.UseSkiaSharpGtk4))!
			.GetCustomAttributes<SupportedOSPlatformAttribute>());
	}

	private static void CheckMapper(Type handlerType, string name)
	{
		var field = handlerType.GetField(name, BindingFlags.Public | BindingFlags.Static);
		Assert.NotNull(field);
		Assert.False(field.IsInitOnly);
		Assert.NotNull(field.GetValue(null));
	}

	private sealed class DerivedCanvasHandler : CanvasHandler
	{
		public DerivedCanvasHandler(PropertyMapper mapper, CommandMapper commands) : base(mapper, commands) { }
	}

	private sealed class DerivedGLHandler : GLHandler
	{
		public DerivedGLHandler(PropertyMapper mapper, CommandMapper commands) : base(mapper, commands) { }
	}
}
