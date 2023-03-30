using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Asv.Mavlink.Client;
using Avalonia;
using DynamicData;
using DynamicData.Binding;
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

    private ReadOnlyObservableCollection<ParameterItem> _parameters;
    private ObservableCollection<VehicleParamDescription> _descriptions;

    public const string UriString = ShellMenuItem.UriString + ".parameters";
    public static readonly Uri Uri = new Uri(UriString);

    public ParametersEditorPageViewModel() : base(Uri)
    {
        
    }

    [ImportingConstructor]
    public ParametersEditorPageViewModel(IMavlinkDevicesService svc, ILogService log) : this()
    {
        _svc = svc;
        _log = log;

        Clear = ReactiveCommand.Create(ClearImpl).DisposeItWith(Disposable);

        PinnedParameters = new ObservableCollection<ParametersEditorParameterViewModel>();

        this.WhenValueChanged(_ => _.SelectedItem)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(AddSelectedItem)
            .DisposeItWith(Disposable);

        this.WhenAnyValue(_ => _.Search)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Throttle(TimeSpan.FromMilliseconds(500))
            .Skip(1)
            .Subscribe(_ => UpdateParams.Execute(null))
            .DisposeItWith(Disposable);

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
    public ParameterItem SelectedItem { get; set; }
    
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
    }
    
    private void OnUpdateParamsError(Exception exception)
    {   
        //TODO: Localize
        _log.Error("ParamsEditor", $"ReadAll params error {_vehicle.Name.Value}", exception);
    }
    #endregion
    
    [Reactive]
    public ReactiveCommand<Unit,Unit> Clear { get; set; }

    [Reactive]
    public string Search { get; set; }
    
    [Reactive]
    public ushort FullId { get; set; }

    public ReadOnlyObservableCollection<ParameterItem> Parameters => _parameters;
    
    [Reactive]
    public ObservableCollection<ParametersEditorParameterViewModel> PinnedParameters { get; set; }

    private void AddSelectedItem(ParameterItem item)
    {
        if (item == null) return;
        
        if(PinnedParameters.Count > 0)
            PinnedParameters = new ObservableCollection<ParametersEditorParameterViewModel>(PinnedParameters.Where(_ => _.Parameter.Pinned));
        
        if(item.Parameter != null && 
           item.Description != null && 
           PinnedParameters.FirstOrDefault(_ => _.Parameter.Parameter.Name == item.Parameter.Name) == null)
            PinnedParameters.Add(new ParametersEditorParameterViewModel(item, _vehicle));
    }
    
    private void ClearImpl() => Search = "";

    private bool FilterParams(MavParam param)
    {
        if (Search.IsNullOrWhiteSpace()) return true;
        
        return param.Name.ToLower().Contains(Search.ToLower());
    }
    
    private void UpdateVehicle()
    {
        SourceCache<MavParam, string> _list = new (_=>_.Name);
        
        _list.Connect()
            .Filter(FilterParams)
            .Transform(_ => new ParameterItem(_, _descriptions.FirstOrDefault(__ => __.Name == _.Name)))
            .SortBy(_ => _.Parameter.Name)
            .Bind(out _parameters)
            .Subscribe();

        _vehicle.Params.OnParamUpdated
            .Subscribe(_ => _list.AddOrUpdate(_)).DisposeItWith(Disposable);
        
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
            FullId = fullId;
            
            _vehicle = _svc.GetVehicleByFullId(fullId);
            
            _descriptions = new ObservableCollection<VehicleParamDescription>(_vehicle.GetParamDescription());
            
            this.WhenValueChanged(_ => _.Parameters).Subscribe(_ => UpdateVehicle());
        }
    }
}

public class ParameterItem : AvaloniaObject
{
    public ParameterItem(MavParam parameter, VehicleParamDescription description)
    {
        Parameter = parameter;
        Description = description;
    }

    #region Parameter
    private MavParam _parameter;

    public static readonly DirectProperty<ParameterItem, MavParam> ParameterProperty = AvaloniaProperty.RegisterDirect<ParameterItem, MavParam>(
        nameof(Parameter), o => o.Parameter, (o, v) => o.Parameter = v);

    public MavParam Parameter
    {
        get => _parameter;
        set => SetAndRaise(ParameterProperty, ref _parameter, value);
    }
    #endregion
    
    #region Description
    private VehicleParamDescription _description;

    public static readonly DirectProperty<ParameterItem, VehicleParamDescription> DescriptionProperty = AvaloniaProperty.RegisterDirect<ParameterItem, VehicleParamDescription>(
        nameof(Description), o => o.Description, (o, v) => o.Description = v);

    public VehicleParamDescription Description
    {
        get => _description;
        set => SetAndRaise(DescriptionProperty, ref _description, value);
    }
    #endregion
    
    #region Pinned
    private bool _pinned;

    public static readonly DirectProperty<ParameterItem, bool> PinnedProperty = AvaloniaProperty.RegisterDirect<ParameterItem, bool>(
        nameof(Pinned), o => o.Pinned, (o, v) => o.Pinned = v);

    public bool Pinned
    {
        get => _pinned;
        set => SetAndRaise(PinnedProperty, ref _pinned, value);
    }
    #endregion

    #region Starred
    private bool _starred;

    public static readonly DirectProperty<ParameterItem, bool> StaredProperty = AvaloniaProperty.RegisterDirect<ParameterItem, bool>(
        nameof(Starred), o => o.Starred, (o, v) => o.Starred = v);

    public bool Starred
    {
        get => _starred;
        set => SetAndRaise(StaredProperty, ref _starred, value);
    }
    #endregion
}