using Microsoft.Maui.Layouts;

namespace DocsSamplesApp.Bitmaps
{
    class AspectRatioLayout : Layout
    {
        public static readonly BindableProperty AspectRatioProperty =
            BindableProperty.Create(
                nameof(AspectRatio),
                typeof(double),
                typeof(AspectRatioLayout),
                1.0,
                propertyChanged: (bindable, oldValue, newValue) =>
                {
                    ((AspectRatioLayout)bindable).InvalidateMeasure();
                });

        public double AspectRatio
        {
            set => SetValue(AspectRatioProperty, value);
            get => (double)GetValue(AspectRatioProperty);
        }

        protected override ILayoutManager CreateLayoutManager()
        {
            return new AspectRatioLayoutManager(this);
        }

        class AspectRatioLayoutManager : ILayoutManager
        {
            private readonly AspectRatioLayout _layout;

            public AspectRatioLayoutManager(AspectRatioLayout layout)
            {
                _layout = layout;
            }

            public Size Measure(double widthConstraint, double heightConstraint)
            {
                if (double.IsPositiveInfinity(widthConstraint) &&
                    double.IsPositiveInfinity(heightConstraint))
                {
                    throw new InvalidOperationException(
                         "AspectRatioLayout cannot be used with both dimensions unconstrained.");
                }

                double minWidth = Math.Min(widthConstraint, _layout.AspectRatio * heightConstraint);
                double minHeight = minWidth / _layout.AspectRatio;

                return new Size(minWidth, minHeight);
            }

            public Size ArrangeChildren(Rect bounds)
            {
                foreach (var child in _layout.Children.OfType<IView>())
                {
                    child.Arrange(bounds);
                }
                return bounds.Size;
            }
        }
    }
}
