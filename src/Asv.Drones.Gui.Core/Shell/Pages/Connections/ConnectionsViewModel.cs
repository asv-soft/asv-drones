using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Uav;
using Avalonia.Controls;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;

namespace Asv.Drones.Gui.Core
{
    [ExportShellPage(BaseUriString)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ConnectionsViewModel:ViewModelBase,IShellPage
    {
        public const string BaseUriString = "asv:shell.connections";
        public static readonly Uri BaseUri = new(BaseUriString);
        private readonly ReadOnlyObservableCollection<IConnectionPart> _items;
        private readonly ObservableAsPropertyHelper<bool> _isReloadRequired;


        public ConnectionsViewModel() : base(BaseUri)
        {
            if (Design.IsDesignMode)
            {
                _isReloadRequired = Observable.Return(true).ToProperty(this, _ => _.IsReloadRequired);
                _items = new ReadOnlyObservableCollection<IConnectionPart>(new ObservableCollection<IConnectionPart>(
                    new IConnectionPart[]
                    {
                        new ConnectionsIdentificationViewModel(),
                        new ConnectionsPortsViewModel()
                    }));
            }
        }

        [ImportingConstructor]
        public ConnectionsViewModel([ImportMany]IEnumerable<IViewModelProvider<IConnectionPart>> partProviders, IMavlinkDevicesService svc) : this()
        {
            partProviders.Select(_ => _.Items)
                .Merge()
                .SortBy(_ => _.Order)
                .Bind(out _items)
                .DisposeMany()
                .Subscribe()
                .DisposeItWith(Disposable);
            
            // this is for ReloadRequired showing
            _items.ToObservableChangeSet()
                .AutoRefresh(_ => _.IsRebootRequired) // update collection when any part require reboot
                .ToCollection()
                .Select(parts => parts.Any(part => part.IsRebootRequired)) // check if any part require reboot
                .ToProperty(this, _ => _.IsReloadRequired, out _isReloadRequired)
                .DisposeItWith(Disposable);
        }

        public ReadOnlyObservableCollection<IConnectionPart> Items => _items;

        public bool IsReloadRequired => _isReloadRequired.Value;

        public void SetArgs(Uri link)
        {
            
        }
    }
}