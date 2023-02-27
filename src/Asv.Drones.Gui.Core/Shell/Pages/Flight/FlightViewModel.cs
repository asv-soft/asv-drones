using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Asv.Avalonia.Map;
using Asv.Common;
using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core
{
    [ExportShellPage(WellKnownUri.ShellPageFlight)]
    [PartCreationPolicy(CreationPolicy.Shared)] //Important shared mode
    public class FlightViewModel: ViewModelBase,IShellPage
    {
        public FlightViewModel() : base(new(WellKnownUri.ShellPageFlight))
        {
            if (Design.IsDesignMode)
            {

            }
        }

        [ImportingConstructor]
        public FlightViewModel(IMapService map):this()
        {
            map.CurrentMapProvider.Subscribe(_ => MapProvider = _).DisposeItWith(Disposable);
            this.WhenAnyValue(_ => _.MapProvider).Subscribe(map.CurrentMapProvider).DisposeItWith(Disposable);
          
        }

        [Reactive] 
        public GMapProvider MapProvider { get; set; } = EmptyProvider.Instance;

        public void SetArgs(Uri link)
        {
            
        }
    }
}