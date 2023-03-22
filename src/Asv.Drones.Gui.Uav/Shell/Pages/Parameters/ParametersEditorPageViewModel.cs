using System.ComponentModel.Composition;
using System.Reactive;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Asv.Mavlink.Client;
using DynamicData.Alias;
using DynamicData.PLinq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav;

[ExportShellPage(UriString)]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class ParametersEditorPageViewModel : ViewModelBase, IShellPage
{
    private readonly IMavlinkDevicesService _svc;
    
    public const string UriString = ShellMenuItem.UriString + ".parameters";
    public static readonly Uri Uri = new Uri(UriString);

    public ParametersEditorPageViewModel() : base(Uri)
    {
        Clear = ReactiveCommand.Create(ClearImpl).DisposeItWith(Disposable);
    }

    [ImportingConstructor]
    public ParametersEditorPageViewModel(IMavlinkDevicesService svc) : this()
    {
        _svc = svc;
    }

    public ReactiveCommand<Unit,Unit> Clear { get; set; }

    [Reactive]
    public string Search { get; set; }
    
    private void ClearImpl() => Search = "";

    public void SetArgs(Uri link)
    {
        var fullid = link.GetComponents(UriComponents.Query, UriFormat.UriEscaped).Replace("Id=","");

       // _svc.Vehicles.Filter(_ => _.FullId.ToString() == fullid).Transform(_ => _.Params);
    }
}