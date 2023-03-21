using System.ComponentModel.Composition;
using System.Reactive;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Uav;
using Asv.Mavlink;
using Asv.Mavlink.Client;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core;

[ExportShellPage(UriString)]
[PartCreationPolicy(CreationPolicy.Shared)]
public class ParametersEditorPageViewModel : ViewModelBase, IShellPage
{
    private readonly IReadOnlyDictionary<string, MavParam> _params;
    
    public const string UriString = ShellPage.UriString + ".parameters";
    public static readonly Uri Uri = new Uri(UriString);

    public ParametersEditorPageViewModel() : base(Uri)
    {
        Clear = ReactiveCommand.Create(ClearImpl).DisposeItWith(Disposable);
        
    }

    //[ImportingConstructor]
    //public ParametersEditorPageViewModel(IVehicle svc) : this()
    //{
    //    _vehicle = svc;
    //}
    
    public ReactiveCommand<Unit,Unit> Clear { get; set; }

    [Reactive]
    public string Search { get; set; }
    
    private void ClearImpl() => Search = "";

    public void SetArgs(Uri link)
    {
        
    }
}