using System.ComponentModel.Composition.Hosting;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using ReactiveUI;

namespace Asv.Drones.Gui.Core;

public class ViewLocator : IDataTemplate
{
    public static readonly Type BaseViewType = typeof(Control);
    private readonly CompositionContainer _container;
    

    public ViewLocator(CompositionContainer container)
    {
        _container = container ?? throw new ArgumentNullException(nameof(container));
    }
    
    public Control? Build(object? data)
    {
        if (data is null)
            return null;
        var viewModelType = data.GetType();
        // try to find view by attribute
        var defaultView = _container.GetExports<Control, IViewMetadata>().FirstOrDefault(_ => _.Metadata.ViewModelType == viewModelType);
        if (defaultView != null) return defaultView.Value;
        // try to find view for base class
        defaultView = _container.GetExports<Control, IViewMetadata>().FirstOrDefault(view => viewModelType.IsSubclassOf(view.Metadata.ViewModelType));
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