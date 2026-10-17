using AppKit;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platforms.MacOS.Platform;
using MacOSLayoutHandler = Microsoft.Maui.Platforms.MacOS.Handlers.LayoutHandler;

namespace SkiaSharpSample;

internal sealed class GalleryLayoutHandler : MacOSLayoutHandler
{
    protected override void ConnectHandler(MacOSContainerView platformView)
    {
        base.ConnectHandler(platformView);
        if (VirtualView is Grid grid)
        {
            grid.ColumnDefinitions.ItemSizeChanged += DefinitionsChanged;
            grid.RowDefinitions.ItemSizeChanged += DefinitionsChanged;
        }
    }

    protected override void DisconnectHandler(MacOSContainerView platformView)
    {
        if (VirtualView is Grid grid)
        {
            grid.ColumnDefinitions.ItemSizeChanged -= DefinitionsChanged;
            grid.RowDefinitions.ItemSizeChanged -= DefinitionsChanged;
        }
        base.DisconnectHandler(platformView);
    }

    private void DefinitionsChanged(object? sender, EventArgs e)
    {
        if (VirtualView is Layout layout)
            InvalidateLayout(layout);
    }

    public override void Invoke(string command, object? args)
    {
        // The pinned AppKit preview has no layout child-command mapper.
        if (command == nameof(ILayoutHandler.Clear))
        {
            Clear();
        }
        else if (args is LayoutHandlerUpdate update)
        {
            switch (command)
            {
                case nameof(ILayoutHandler.Add):
                    Add(update.View);
                    break;
                case nameof(ILayoutHandler.Insert):
                    Insert(update.Index, update.View);
                    break;
                case nameof(ILayoutHandler.Update):
                    Update(update.Index, update.View);
                    break;
                case nameof(ILayoutHandler.Remove):
                    Remove(update.View);
                    break;
                default:
                    base.Invoke(command, args);
                    return;
            }
        }
        else
        {
            base.Invoke(command, args);
            return;
        }

        if (VirtualView is Layout view)
            InvalidateLayout(view);
    }

    private void InvalidateLayout(Layout view)
    {
        view.Dispatcher.Dispatch(() =>
        {
            if (view.Handler != this)
                return;

            for (Element? element = view; element != null; element = element.Parent)
            {
                if (element is VisualElement visual)
                    visual.InvalidateMeasure();
            }

            for (NSView? native = PlatformView; native != null; native = native.Superview)
            {
                native.InvalidateIntrinsicContentSize();
                native.NeedsLayout = true;
            }
        });
    }
}
