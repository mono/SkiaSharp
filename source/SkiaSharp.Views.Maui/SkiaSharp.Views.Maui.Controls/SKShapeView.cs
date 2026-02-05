#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace SkiaSharp.Views.Maui.Controls
{
	/// <summary>
	/// A canvas-like control that renders MAUI shapes using SkiaSharp.
	/// Children can be any MAUI <see cref="Shape"/> and will be rendered to pixels.
	/// Supports Canvas.Left/Canvas.Top attached properties for positioning.
	/// </summary>
	[ContentProperty(nameof(Children))]
	public class SKShapeView : View, ISKCanvasView
	{
		/// <summary>
		/// Attached property for the left position of a child element.
		/// </summary>
		public static readonly BindableProperty LeftProperty =
			BindableProperty.CreateAttached("Left", typeof(double), typeof(SKShapeView), 0.0);

		/// <summary>
		/// Attached property for the top position of a child element.
		/// </summary>
		public static readonly BindableProperty TopProperty =
			BindableProperty.CreateAttached("Top", typeof(double), typeof(SKShapeView), 0.0);

		/// <summary>
		/// Bindable property for <see cref="IgnorePixelScaling"/>.
		/// </summary>
		public static readonly BindableProperty IgnorePixelScalingProperty =
			BindableProperty.Create(nameof(IgnorePixelScaling), typeof(bool), typeof(SKShapeView), false);

		/// <summary>
		/// Bindable property for <see cref="EnableTouchEvents"/>.
		/// </summary>
		public static readonly BindableProperty EnableTouchEventsProperty =
			BindableProperty.Create(nameof(EnableTouchEvents), typeof(bool), typeof(SKShapeView), false);

		/// <summary>
		/// Bindable property for <see cref="Background"/>.
		/// </summary>
		public static new readonly BindableProperty BackgroundProperty =
			BindableProperty.Create(nameof(Background), typeof(Brush), typeof(SKShapeView), null,
				propertyChanged: (bindable, _, _) => ((SKShapeView)bindable).InvalidateSurface());

		private SKSizeI lastCanvasSize;
		private readonly ObservableCollection<Shape> children;

		/// <summary>
		/// Initializes a new instance of the <see cref="SKShapeView"/> class.
		/// </summary>
		public SKShapeView()
		{
			children = new ObservableCollection<Shape>();
			children.CollectionChanged += OnChildrenCollectionChanged;
		}

		/// <summary>
		/// Gets the collection of child shapes to render.
		/// </summary>
		public IList<Shape> Children => children;

		/// <summary>
		/// Occurs when the surface needs to be painted.
		/// Called after all child shapes have been rendered.
		/// </summary>
		public event EventHandler<SKPaintSurfaceEventArgs>? PaintSurface;

		/// <summary>
		/// Occurs when touch events are received.
		/// </summary>
		public event EventHandler<SKTouchEventArgs>? Touch;

		/// <inheritdoc/>
		public SKSize CanvasSize => lastCanvasSize;

		/// <summary>
		/// Gets or sets a value indicating whether to ignore pixel scaling.
		/// </summary>
		public bool IgnorePixelScaling
		{
			get => (bool)GetValue(IgnorePixelScalingProperty);
			set => SetValue(IgnorePixelScalingProperty, value);
		}

		/// <summary>
		/// Gets or sets a value indicating whether touch events are enabled.
		/// </summary>
		public bool EnableTouchEvents
		{
			get => (bool)GetValue(EnableTouchEventsProperty);
			set => SetValue(EnableTouchEventsProperty, value);
		}

		/// <summary>
		/// Gets or sets the background brush for the canvas.
		/// </summary>
		public new Brush? Background
		{
			get => (Brush?)GetValue(BackgroundProperty);
			set => SetValue(BackgroundProperty, value);
		}

		/// <summary>
		/// Gets the left position of an element.
		/// </summary>
		public static double GetLeft(BindableObject bindable) =>
			(double)bindable.GetValue(LeftProperty);

		/// <summary>
		/// Sets the left position of an element.
		/// </summary>
		public static void SetLeft(BindableObject bindable, double value) =>
			bindable.SetValue(LeftProperty, value);

		/// <summary>
		/// Gets the top position of an element.
		/// </summary>
		public static double GetTop(BindableObject bindable) =>
			(double)bindable.GetValue(TopProperty);

		/// <summary>
		/// Sets the top position of an element.
		/// </summary>
		public static void SetTop(BindableObject bindable, double value) =>
			bindable.SetValue(TopProperty, value);

		/// <summary>
		/// Invalidates the surface, causing a repaint.
		/// </summary>
		public void InvalidateSurface()
		{
			Handler?.Invoke(nameof(ISKCanvasView.InvalidateSurface));
		}

		/// <summary>
		/// Called when the surface needs to be painted.
		/// Override this to add custom drawing after the shapes are rendered.
		/// </summary>
		protected virtual void OnPaintSurface(SKPaintSurfaceEventArgs e)
		{
			var canvas = e.Surface.Canvas;
			var info = e.Info;

			// Clear the canvas
			canvas.Clear(SKColors.Transparent);

			// Draw background if set
			var bounds = new SKRect(0, 0, info.Width, info.Height);
			using (var backgroundPaint = Background?.ToSKPaint(bounds))
			{
				if (backgroundPaint != null)
				{
					canvas.DrawRect(bounds, backgroundPaint);
				}
			}

			// Draw all child shapes
			foreach (var shape in children)
			{
				DrawShape(canvas, shape);
			}

			// Invoke the PaintSurface event for additional custom drawing
			PaintSurface?.Invoke(this, e);
		}

		/// <summary>
		/// Called when a touch event is received.
		/// </summary>
		protected virtual void OnTouch(SKTouchEventArgs e)
		{
			Touch?.Invoke(this, e);
		}

		private void DrawShape(SKCanvas canvas, Shape shape)
		{
			if (shape == null || !shape.IsVisible)
				return;

			var path = shape.ToSKPath();
			if (path == null)
				return;

			// Get position from attached properties
			var left = (float)GetLeft(shape);
			var top = (float)GetTop(shape);

			// Save canvas state
			canvas.Save();

			// Apply transform for positioning
			canvas.Translate(left, top);

			// Apply shape's render transform if any
			ApplyRenderTransform(canvas, shape);

			// Calculate bounds for gradient brushes
			var pathBounds = path.Bounds;

			// Apply opacity if less than 1
			var opacity = (float)shape.Opacity;
			if (opacity < 1)
			{
				canvas.SaveLayerAlpha((byte)(opacity * 255));
			}

			// Draw fill
			using (var fillPaint = shape.Fill?.ToSKPaint(pathBounds))
			{
				if (fillPaint != null)
				{
					canvas.DrawPath(path, fillPaint);
				}
			}

			// Draw stroke
			if (shape.Stroke != null && shape.StrokeThickness > 0)
			{
				using var strokePaint = shape.Stroke.ToSKStrokePaint(
					pathBounds,
					(float)shape.StrokeThickness,
					shape.StrokeLineCap,
					shape.StrokeLineJoin,
					shape.StrokeDashArray,
					(float)shape.StrokeDashOffset,
					(float)shape.StrokeMiterLimit);

				if (strokePaint != null)
				{
					canvas.DrawPath(path, strokePaint);
				}
			}

			// Restore opacity layer if applied
			if (opacity < 1)
			{
				canvas.Restore();
			}

			// Restore canvas state
			canvas.Restore();

			// Dispose the path
			path.Dispose();
		}

		private void ApplyRenderTransform(SKCanvas canvas, Shape shape)
		{
			var transform = shape.RenderTransform;
			if (transform == null)
				return;

			var matrix = TransformToSKMatrix(transform);
			canvas.Concat(ref matrix);
		}

		private SKMatrix TransformToSKMatrix(Transform transform)
		{
			return transform switch
			{
				TranslateTransform translate => SKMatrix.CreateTranslation(
					(float)translate.X, 
					(float)translate.Y),

				ScaleTransform scale => CreateScaleMatrix(scale),

				RotateTransform rotate => CreateRotateMatrix(rotate),

				SkewTransform skew => SKMatrix.CreateSkew(
					(float)Math.Tan(skew.AngleX * Math.PI / 180),
					(float)Math.Tan(skew.AngleY * Math.PI / 180)),

				CompositeTransform composite => CreateCompositeMatrix(composite),

				TransformGroup group => CreateTransformGroupMatrix(group),

				MatrixTransform matrixTransform => CreateMatrixFromValues(matrixTransform.Matrix),

				_ => SKMatrix.Identity
			};
		}

		private static SKMatrix CreateScaleMatrix(ScaleTransform scale)
		{
			var matrix = SKMatrix.Identity;
			matrix = matrix.PostConcat(SKMatrix.CreateTranslation(-(float)scale.CenterX, -(float)scale.CenterY));
			matrix = matrix.PostConcat(SKMatrix.CreateScale((float)scale.ScaleX, (float)scale.ScaleY));
			matrix = matrix.PostConcat(SKMatrix.CreateTranslation((float)scale.CenterX, (float)scale.CenterY));
			return matrix;
		}

		private static SKMatrix CreateRotateMatrix(RotateTransform rotate)
		{
			var matrix = SKMatrix.Identity;
			matrix = matrix.PostConcat(SKMatrix.CreateTranslation(-(float)rotate.CenterX, -(float)rotate.CenterY));
			matrix = matrix.PostConcat(SKMatrix.CreateRotationDegrees((float)rotate.Angle));
			matrix = matrix.PostConcat(SKMatrix.CreateTranslation((float)rotate.CenterX, (float)rotate.CenterY));
			return matrix;
		}

		private static SKMatrix CreateCompositeMatrix(CompositeTransform composite)
		{
			var matrix = SKMatrix.Identity;

			// Apply transforms in order: Scale, Skew, Rotate, Translate
			// First, move to center
			matrix = matrix.PostConcat(SKMatrix.CreateTranslation(-(float)composite.CenterX, -(float)composite.CenterY));

			// Scale
			if (composite.ScaleX != 1 || composite.ScaleY != 1)
			{
				matrix = matrix.PostConcat(SKMatrix.CreateScale((float)composite.ScaleX, (float)composite.ScaleY));
			}

			// Skew
			if (composite.SkewX != 0 || composite.SkewY != 0)
			{
				matrix = matrix.PostConcat(SKMatrix.CreateSkew(
					(float)Math.Tan(composite.SkewX * Math.PI / 180),
					(float)Math.Tan(composite.SkewY * Math.PI / 180)));
			}

			// Rotate
			if (composite.Rotation != 0)
			{
				matrix = matrix.PostConcat(SKMatrix.CreateRotationDegrees((float)composite.Rotation));
			}

			// Move back from center
			matrix = matrix.PostConcat(SKMatrix.CreateTranslation((float)composite.CenterX, (float)composite.CenterY));

			// Translate
			matrix = matrix.PostConcat(SKMatrix.CreateTranslation((float)composite.TranslateX, (float)composite.TranslateY));

			return matrix;
		}

		private SKMatrix CreateTransformGroupMatrix(TransformGroup group)
		{
			var matrix = SKMatrix.Identity;

			foreach (var child in group.Children)
			{
				var childMatrix = TransformToSKMatrix(child);
				matrix = matrix.PostConcat(childMatrix);
			}

			return matrix;
		}

		private static SKMatrix CreateMatrixFromValues(Microsoft.Maui.Controls.Shapes.Matrix mauiMatrix)
		{
			return new SKMatrix(
				(float)mauiMatrix.M11, (float)mauiMatrix.M21, (float)mauiMatrix.OffsetX,
				(float)mauiMatrix.M12, (float)mauiMatrix.M22, (float)mauiMatrix.OffsetY,
				0, 0, 1);
		}

		private void OnChildrenCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			// Subscribe to property changes on new items
			if (e.NewItems != null)
			{
				foreach (Shape shape in e.NewItems)
				{
					shape.PropertyChanged += OnShapePropertyChanged;
				}
			}

			// Unsubscribe from removed items
			if (e.OldItems != null)
			{
				foreach (Shape shape in e.OldItems)
				{
					shape.PropertyChanged -= OnShapePropertyChanged;
				}
			}

			InvalidateSurface();
		}

		private void OnShapePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			InvalidateSurface();
		}

		void ISKCanvasView.OnCanvasSizeChanged(SKSizeI size) =>
			lastCanvasSize = size;

		void ISKCanvasView.OnPaintSurface(SKPaintSurfaceEventArgs e) =>
			OnPaintSurface(e);

		void ISKCanvasView.OnTouch(SKTouchEventArgs e) =>
			OnTouch(e);
	}
}
