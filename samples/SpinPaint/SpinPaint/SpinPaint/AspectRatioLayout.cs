using System;
using Xamarin.Forms;

namespace SpinPaint
{
    class AspectRatioLayout : Layout<View>
    {
        public static readonly BindableProperty AspectRatioProperty = 
            BindableProperty.Create("AspectRatio", 
                                    typeof(double), 
                                    typeof(AspectRatioLayout), 
                                    1.00,
                                    propertyChanged: (bindable, oldValue, newValue) =>
                                    { 
                                        ((AspectRatioLayout)bindable).InvalidateLayout(); 
                                    }); 
 
        public double AspectRatio
        {
            set { SetValue(AspectRatioProperty, value); }
            get { return (double)GetValue(AspectRatioProperty); }
        }

        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            if (Double.IsPositiveInfinity(widthConstraint) &&
                Double.IsPositiveInfinity(heightConstraint))
            {
                throw new InvalidOperationException(
                     "AspectRatioLayout cannot be used with both dimensions unconstrained.");
            }

            double minWidth = Math.Min(widthConstraint, AspectRatio * heightConstraint);
            double minHeight = minWidth / AspectRatio;

            return new SizeRequest(new Size(minWidth, minHeight));
        }

        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            foreach (View child in Children)
            {
                child.Layout(new Rectangle(x, y, width, height));
            }
        }
    }
}
