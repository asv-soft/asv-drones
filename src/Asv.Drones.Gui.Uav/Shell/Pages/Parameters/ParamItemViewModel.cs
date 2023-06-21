using System.Reactive;
using System.Windows.Input;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Avalonia.Controls;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav;

public class ParamItemViewModelConfig
{
    public bool IsStared { get; set; }
    public string Name { get; set; }
    public bool IsPined { get; set; }
}

public class ParamItemViewModel:ViewModelBase
{
    private readonly ILogService _log;
    private readonly ObservableAsPropertyHelper<bool> _isWriting;
    private readonly ObservableAsPropertyHelper<bool> _isUpdate;

    public ParamItemViewModel():base(new Uri("asv:designMode"))
    {
        if (Design.IsDesignMode)
        {
            Name = "param" + Guid.NewGuid();
            DisplayName = Name;
            
        }
    }
    
    public ParamItemViewModel(IParamItem paramItem, ILogService log):base(new Uri(ParamPageViewModel.UriString+$".item{paramItem.Name}"))
    {
        _log = log;
        PinItem = ReactiveCommand.Create(() => IsPinned = !IsPinned).DisposeItWith(Disposable);
        Name = paramItem.Name;
        DisplayName = paramItem.Info.DisplayName;
        Description = paramItem.Info.Description;
        Units = paramItem.Info.Units;
        ValueDescription = paramItem.Info.UnitsDisplayName;
        IsRebootRequired = paramItem.Info.IsRebootRequired;

        paramItem.Value.Subscribe(_ =>
            {
                Value = _;
            })
            .DisposeItWith(Disposable);
        this.WhenAnyValue(_ => _.Value)
            .Subscribe(_=>paramItem.Value.OnNext(_))
            .DisposeItWith(Disposable);
        
        paramItem.IsSynced.Subscribe(_=>IsSynced =_)
            .DisposeItWith(Disposable);
        Write = ReactiveCommand.CreateFromTask(async _=>    
        {
            await paramItem.Write(_);
        }).DisposeItWith(Disposable);
        Write.IsExecuting.ToProperty(this, _ => _.IsWriting, out _isWriting).DisposeItWith(Disposable);
        Write.ThrownExceptions.Subscribe(OnWriteError).DisposeItWith(Disposable);
        
        Update = ReactiveCommand.CreateFromTask(async _=>
        {
            await paramItem.Read(_);
        }).DisposeItWith(Disposable);
        Update.IsExecuting.ToProperty(this, _ => _.IsUpdate, out _isUpdate).DisposeItWith(Disposable);
        Update.ThrownExceptions.Subscribe(OnReadError).DisposeItWith(Disposable);
        
        this.WhenAnyValue(_=>_.IsPinned)
            .Subscribe(_=>StarKind = _?MaterialIconKind.Star:MaterialIconKind.StarBorder)
            .DisposeItWith(Disposable);
        Disposable.AddAction(() =>
        {

        });
    }
    
    private void OnWriteError(Exception ex)
    {
        _log.Error("Params",$"Write {Name} error",ex);
    }
    private void OnReadError(Exception ex)
    {
        _log.Error("Params",$"Read {Name} error",ex);
    }
    public bool IsWriting => _isWriting.Value;
    public bool IsUpdate => _isUpdate.Value;

    public string Name { get; }
    
    public string DisplayName { get; set; }
    public string Units { get; set; }
    public ReactiveCommand<Unit, Unit> Update { get; set; }
    public ReactiveCommand<Unit, Unit> Write { get; }
    public ICommand PinItem { get; }
    public string ValueDescription { get; }
    public string Description { get; }
    public bool IsRebootRequired { get; }
    [Reactive]
    public bool IsPinned { get; set; }
    [Reactive]
    public bool IsSynced { get; set; }
    [Reactive]
    public MavParamValue Value { get; set; }
    [Reactive]
    public bool Starred { get; set; }
    [Reactive]
    public MaterialIconKind StarKind { get; set; }
    
    public bool Filter(string searchText, bool starredOnly)
    {
        if (starredOnly)
        {
            if (Starred == false) return false;
        }
        if (searchText.IsNullOrWhiteSpace()) return true;
        return Name.Contains(searchText);
    }

    public ParamItemViewModelConfig GetConfig()
    {
        return new ParamItemViewModelConfig
        {
            IsStared = Starred,
            IsPined = IsPinned,
            Name = Name,
        };
    }

    public void SetConfig(ParamItemViewModelConfig item)
    {
        Starred = item.IsStared;
        IsPinned = item.IsPined;
    }
}