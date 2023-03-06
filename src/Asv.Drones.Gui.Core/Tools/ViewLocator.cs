using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Asv.Drones.Gui.Core;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using ReactiveUI;

namespace Asv.Drones.Gui
{
    public class ViewLocator : IDataTemplate,IDisposable
    {
        public static readonly Type BaseViewType = typeof(IControl);

        private readonly CompositionContainer _container;
        private bool _isDisposed;

        public ViewLocator(CompositionContainer container)
        {
            _container = container;
        }

        public IControl? Build(object? data)
        {
            if (_isDisposed || data == null) return null;
            var viewModelType = data.GetType();
            var defaultView = _container.GetExports<IControl, IViewMetadata>().FirstOrDefault(_ => _.Metadata.ViewModelType == viewModelType);
            if (defaultView != null) return defaultView.Value;
            defaultView = _container.GetExports<IControl, IViewMetadata>().FirstOrDefault(_ => viewModelType.IsSubclassOf(_.Metadata.ViewModelType));
            if (defaultView != null) return defaultView.Value;
            // if have no attribute, just search view by name
            var name = viewModelType.FullName!.Replace("ViewModel", "View");
            var type = Type.GetType(name);
            if (type == null) return new TextBlock { Text = "Not Found: " + name };
            var contract = AttributedModelServices.GetContractName(type);
            try
            {
                return (IControl)_container.GetExportedValue<object>(contract)!;
            }
            catch (Exception e)
            {
                return null;
            }
            
        }

        public bool Match(object? data)
        {
            return data is ReactiveObject;
        }

        public void Dispose()
        {
            _isDisposed = true;
        }
    }
}
