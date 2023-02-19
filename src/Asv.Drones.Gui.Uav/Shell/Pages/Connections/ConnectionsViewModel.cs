using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Windows.Input;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Avalonia.Controls;
using DynamicData;
using DynamicData.Binding;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav
{
    [ExportShellPage(BaseUriString)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ConnectionsViewModel:ViewModelBase,IShellPage
    {
        public const string BaseUriString = "asv:shell.connections";
        public static readonly Uri BaseUri = new(BaseUriString);
        private readonly ReadOnlyObservableCollection<IConnectionPart> _items;


        public ConnectionsViewModel() : base(BaseUri)
        {
            if (Design.IsDesignMode)
            {
                IsReloadRequired = true;
                _items = new ReadOnlyObservableCollection<IConnectionPart>(new ObservableCollection<IConnectionPart>(
                    new IConnectionPart[]
                    {
                        new ConnectionsIdentificationViewModel(),
                        new ConnectionsPortsViewModel()
                    }));
            }
        }

        [ImportingConstructor]
        public ConnectionsViewModel([ImportMany]IEnumerable<IViewModelProvider<IConnectionPart>> parts, IMavlinkDevicesService svc) : this()
        {
            svc.NeedReloadToApplyConfig.Subscribe(_ => IsReloadRequired = _).DisposeItWith(Disposable);

            parts.Select(_ => _.Items)
                .Merge()
                .ObserveOn(RxApp.MainThreadScheduler)
                .SortBy(_ => _.Order)
                .Bind(out _items)
                .DisposeMany()
                .Subscribe()
                .DisposeItWith(Disposable);
        }

        public ReadOnlyObservableCollection<IConnectionPart> Items => _items;

        [Reactive]
        public bool IsReloadRequired { get; set; }

        public void SetArgs(Uri link)
        {
            
        }
    }
}