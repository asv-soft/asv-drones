using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Asv.Mavlink.Client;
using DynamicData;
using DynamicData.Alias;
using DynamicData.Binding;
using DynamicData.PLinq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav;

[ExportShellPage(UriString)]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class ParametersEditorPageViewModel : ViewModelBase, IShellPage
{
    private readonly IMavlinkDevicesService _svc;
    private readonly ILogService _log;
    private IVehicle _vehicle;

    private ReadOnlyObservableCollection<MavParam> _params;

    public const string UriString = ShellMenuItem.UriString + ".parameters";
    public static readonly Uri Uri = new Uri(UriString);

    public ParametersEditorPageViewModel() : base(Uri)
    {
        Clear = ReactiveCommand.Create(ClearImpl).DisposeItWith(Disposable);
    }

    [ImportingConstructor]
    public ParametersEditorPageViewModel(IMavlinkDevicesService svc, ILogService log) : this()
    {
        _svc = svc;
        _log = log;
        
        _updateParams = ReactiveCommand.CreateFromObservable(
                () => Observable.FromAsync(UpdateParamsImpl).SubscribeOn(RxApp.TaskpoolScheduler).TakeUntil(_cancelUpdateParams), 
                this.WhenAnyValue(_ => _.IsInProgress).Select(_ => !_))
            .DisposeItWith(Disposable);
        _updateParams.IsExecuting.ToProperty(this, _ => _.IsUpdatingParams, out _isUpdatingParams)
            .DisposeItWith(Disposable);
        _updateParams.ThrownExceptions.Subscribe(OnUpdateParamsError)
            .DisposeItWith(Disposable);
        _cancelUpdateParams = ReactiveCommand.Create(() => { }, _updateParams.IsExecuting)
            .DisposeItWith(Disposable);
    }

    [Reactive]
    public bool IsInProgress { get; set; }
    [Reactive]
    public IProgress<double> Progress { get; set; }
    
    #region Update Params
    private readonly ObservableAsPropertyHelper<bool> _isUpdatingParams;
    public bool IsUpdatingParams => _isUpdatingParams.Value;
    public readonly ReactiveCommand<Unit,Unit> _updateParams;
    public ICommand UpdateParams => _updateParams;
    private readonly ReactiveCommand<Unit,Unit> _cancelUpdateParams;
    public ICommand CancelUpdateParams => _cancelUpdateParams;

    private async Task UpdateParamsImpl(CancellationToken cancel)
    {
        await _vehicle.Params.ReadAllParams(cancel, Progress);
        
        UpdateVehicle();
    }
    
    private void OnUpdateParamsError(Exception exception)
    {   
        //TODO: Localize
        _log.Error("ParamsEditor", $"ReadAll params error {_vehicle.Name.Value}", exception);
    }
    #endregion
    
    public ReactiveCommand<Unit,Unit> Clear { get; set; }

    [Reactive]
    public string Search { get; set; }

    public ReadOnlyObservableCollection<MavParam> Params => _params;
    
    private void ClearImpl() => Search = "";

    private void UpdateVehicle()
    {
        SourceCache<MavParam,string> _list = new (_=>_.Name);
        _list.Connect()
            .Filter(_ => true)
            .SortBy(_ => _.Name)
            .Bind(out _params)
            .Subscribe();

        _vehicle.Params.OnParamUpdated
            .Subscribe(_ => _list.AddOrUpdate(_));

        foreach (var paramItem in _vehicle.Params.Params.Values)
        {
            _list.AddOrUpdate(paramItem);
        }
    }

    
    public void SetArgs(Uri link)
    {
        var fullIdString = link.GetComponents(UriComponents.Query, UriFormat.UriEscaped).Replace("Id=","");
        
        if(ushort.TryParse(fullIdString, out var fullId))
        {
            _vehicle = _svc.GetVehicleByFullId(fullId);
            UpdateVehicle();
        }
    }
}