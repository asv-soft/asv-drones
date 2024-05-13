using System;
using System.Composition.Hosting;
using System.Linq;
using Asv.Drones.Gui.Api;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using ReactiveUI;

namespace Asv.Drones.Gui;

public class ViewMetadata : IViewMetadata
{
    public Type ViewModelType { get; set; }
}

public class ViewLocator : IDataTemplate
{
    private readonly CompositionHost _container;


    public ViewLocator(CompositionHost container)
    {
        _container = container ?? throw new ArgumentNullException(nameof(container));
    }

    public Control? Build(object? data)
    {
        if (data is null)
            return null;
        var viewModelType = data.GetType();
        // try to find view by attribute
        var defaultView = _container.GetExports<Lazy<Control, ViewMetadata>>()
            .FirstOrDefault(view => view.Metadata.ViewModelType == viewModelType);
        if (defaultView != null) return defaultView.Value;
        // try to find view for base class
        defaultView = _container.GetExports<Lazy<Control, ViewMetadata>>()
            .FirstOrDefault(view => viewModelType.IsSubclassOf(view.Metadata.ViewModelType));
        if (defaultView != null) return defaultView.Value;

        var name = data.GetType().FullName!.Replace("ViewModel", "View");
        var type = Type.GetType(name);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }

        return new TextBlock { Text = name };
    }

    public bool Match(object? data)
    {
        return data is ReactiveObject;
    }
}