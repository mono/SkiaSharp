using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace SkiaSharpSample.Controls;

/// <summary>Grid-backed, single-instance anchored popover for gallery filters and sort.</summary>
public static class GalleryPopup
{
    public static readonly BindableProperty ContentFactoryProperty = BindableProperty.CreateAttached(
        "ContentFactory", typeof(Func<View>), typeof(GalleryPopup), null, propertyChanged: OnFactoryChanged);
    public static readonly BindableProperty CloseOnActionProperty = BindableProperty.CreateAttached(
        "CloseOnAction", typeof(bool), typeof(GalleryPopup), false);
    private static readonly BindableProperty ActiveProperty = BindableProperty.CreateAttached(
        "Active", typeof(MenuSession), typeof(GalleryPopup), null);

    public static Func<View>? GetContentFactory(BindableObject anchor) =>
        (Func<View>?)anchor.GetValue(ContentFactoryProperty);

    public static void SetContentFactory(BindableObject anchor, Func<View>? factory) =>
        anchor.SetValue(ContentFactoryProperty, factory);

    public static bool GetCloseOnAction(BindableObject anchor) => (bool)anchor.GetValue(CloseOnActionProperty);
    public static void SetCloseOnAction(BindableObject anchor, bool value) => anchor.SetValue(CloseOnActionProperty, value);

    public static bool Dismiss(Grid root)
    {
        if (root.GetValue(ActiveProperty) is not MenuSession session) return false;
        session.Close();
        return true;
    }

    private static void OnFactoryChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not Button and not ImageButton)
            throw new InvalidOperationException("GalleryPopup needs a Button or ImageButton anchor.");
        if (oldValue is not null) Attach(bindable, false);
        if (newValue is not null) Attach(bindable, true);
    }

    private static void Attach(BindableObject anchor, bool add)
    {
        if (anchor is Button button)
        {
            if (add) button.Clicked += AnchorClicked;
            else button.Clicked -= AnchorClicked;
        }
        else if (anchor is ImageButton image)
        {
            if (add) image.Clicked += AnchorClicked;
            else image.Clicked -= AnchorClicked;
        }
    }

    private static void AnchorClicked(object? sender, EventArgs e)
    {
        if (sender is not View anchor || GetContentFactory(anchor) is not { } factory) return;
        var root = FindRoot(anchor);
        if (root.GetValue(ActiveProperty) is MenuSession previous)
        {
            var sameAnchor = ReferenceEquals(previous.Anchor, anchor);
            previous.Close();
            if (sameAnchor) return;
        }
        var session = new MenuSession(root, anchor, factory(), GetCloseOnAction(anchor));
        root.SetValue(ActiveProperty, session);
        session.Open();
    }

    private static Grid FindRoot(View anchor)
    {
        for (Element? parent = anchor.Parent; parent is not null; parent = parent.Parent)
            if (parent is ContentPage { Content: Grid root }) return root;
        throw new InvalidOperationException("GalleryPopup requires a ContentPage with a Grid root.");
    }

    private sealed class MenuSession
    {
        private readonly Grid root;
        private readonly View content;
        private readonly GalleryPopupView overlay;
        private readonly double requestedHeight;
        private readonly List<Button> buttons = [];
        private readonly List<ImageButton> imageButtons = [];
        private bool closed;

        public MenuSession(Grid root, View anchor, View content, bool closeOnAction)
        {
            this.root = root;
            Anchor = anchor;
            this.content = content;
            requestedHeight = content.HeightRequest;
            overlay = new GalleryPopupView { MenuContent = content, AutomationId = "gallery-popup" };
            overlay.DismissRequested += OnDismissRequested;
            if (closeOnAction) SubscribeActions(content);
        }

        public View Anchor { get; }

        public void Open()
        {
            root.SetRowSpan(overlay, Math.Max(1, root.RowDefinitions.Count));
            root.SetColumnSpan(overlay, Math.Max(1, root.ColumnDefinitions.Count));
            overlay.ZIndex = 100;
            // GTK4/WPF can ignore ZIndex; insertion order also keeps the scrim above the page.
            root.Children.Add(overlay);
            overlay.SizeChanged += OnGeometryChanged;
            content.SizeChanged += OnGeometryChanged;
            Anchor.SizeChanged += OnGeometryChanged;
            Anchor.Unloaded += OnAnchorUnloaded;
            Anchor.PropertyChanged += OnAnchorChanged;
            Position();
        }

        public void Close()
        {
            if (closed) return;
            closed = true;
            overlay.SizeChanged -= OnGeometryChanged;
            content.SizeChanged -= OnGeometryChanged;
            Anchor.SizeChanged -= OnGeometryChanged;
            Anchor.Unloaded -= OnAnchorUnloaded;
            Anchor.PropertyChanged -= OnAnchorChanged;
            overlay.DismissRequested -= OnDismissRequested;
            foreach (var button in buttons) button.Clicked -= OnAction;
            foreach (var image in imageButtons) image.Clicked -= OnAction;
            if (ReferenceEquals(root.GetValue(ActiveProperty), this))
                root.ClearValue(ActiveProperty);
            root.Children.Remove(overlay);
            overlay.MenuContent = null;
            content.HeightRequest = requestedHeight;
            if (content is IDisposable disposable) disposable.Dispose();
            if (Anchor.Handler is not null && Anchor.IsVisible) Anchor.Focus();
        }

        private void SubscribeActions(View view)
        {
            switch (view)
            {
                case Button button:
                    button.Clicked += OnAction;
                    buttons.Add(button);
                    break;
                case ImageButton image:
                    image.Clicked += OnAction;
                    imageButtons.Add(image);
                    break;
                case ScrollView { Content: View child }:
                    SubscribeActions(child);
                    break;
                case Border { Content: View child }:
                    SubscribeActions(child);
                    break;
                case ContentView { Content: View child }:
                    SubscribeActions(child);
                    break;
                case Layout layout:
                    foreach (var child in layout.Children.OfType<View>()) SubscribeActions(child);
                    break;
            }
        }

        private void OnAction(object? sender, EventArgs e) => Close();
        private void OnDismissRequested(object? sender, EventArgs e) => Close();
        private void OnAnchorUnloaded(object? sender, EventArgs e) => Close();
        private void OnGeometryChanged(object? sender, EventArgs e) => Position();
        private void OnAnchorChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(View.IsEnabled) && !Anchor.IsEnabled) Close();
        }

        private void Position()
        {
            if (closed || overlay.Width <= 0 || overlay.Height <= 0) return;
            const double inset = 12;
            const double gap = 7;
            var widthLimit = Math.Max(0, overlay.Width - inset * 2);
            var heightLimit = Math.Max(0, overlay.Height - inset * 2);
            if (requestedHeight > 0)
            {
                var fitted = Math.Min(requestedHeight, Math.Max(100, overlay.Height * 0.78));
                if (content.HeightRequest != fitted) content.HeightRequest = fitted;
            }
            var measured = overlay.MeasureMenu(widthLimit, heightLimit);
            var preferredWidth = content.WidthRequest > 0 ? content.WidthRequest + 26 : measured.Width;
            var width = Math.Min(widthLimit, Math.Max(180, preferredWidth));
            var height = Math.Min(heightLimit, Math.Max(44, measured.Height));
            double anchorX = 0, anchorY = 0;
            for (Element? element = Anchor; element is VisualElement view && element != root; element = element.Parent)
            {
                anchorX += view.X;
                anchorY += view.Y;
                if (view.Parent is ScrollView scroll)
                {
                    anchorX -= scroll.ScrollX;
                    anchorY -= scroll.ScrollY;
                }
            }
            anchorX -= overlay.X;
            anchorY -= overlay.Y;
            var x = anchorX + Anchor.Width / 2 > overlay.Width / 2
                ? anchorX + Anchor.Width - width : anchorX;
            x = Math.Clamp(x, inset, Math.Max(inset, overlay.Width - width - inset));
            var below = anchorY + Anchor.Height + gap;
            var y = below + height + inset <= overlay.Height
                ? below : anchorY - height - gap;
            y = Math.Clamp(y, inset, Math.Max(inset, overlay.Height - height - inset));
            overlay.SetMenuPosition(new Rect(x, y, width, height));
        }
    }
}
