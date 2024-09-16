using System.Globalization;
using System.Reactive;
using Asv.Common;
using Asv.Mavlink;
using Asv.Mavlink.V2.Common;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Api;

public class ParamItemViewModelConfig
{
    public bool IsStarred { get; set; }
    public string Name { get; set; }
    public bool IsPinned { get; set; }
}

public class ParamItemViewModel : ViewModelBase
{
    private readonly ILogService _log;
    private readonly ObservableAsPropertyHelper<bool> _isWriting;
    private readonly ObservableAsPropertyHelper<bool> _isUpdate;
    private readonly IParamItem _paramItem;
    private bool _internalUpdate;

    public ParamItemViewModel() : base(WellKnownUri.UndefinedUri)
    {
        DesignTime.ThrowIfNotDesignMode();
        Name = "param" + Guid.NewGuid();
        DisplayName = Name;
    }

    public ParamItemViewModel(Uri id, IParamItem paramItem, ILogService log) : base(
        $"{id.AbsoluteUri}.param{paramItem.Name}")
    {
        _log = log;
        _paramItem = paramItem;
        PinItem = ReactiveCommand.Create(() => { IsPinned = !IsPinned; }).DisposeItWith(Disposable);
        Name = paramItem.Name;
        DisplayName = paramItem.Info.DisplayName;
        Description = paramItem.Info.Description;
        Units = paramItem.Info.Units;
        ValueDescription = paramItem.Info.UnitsDisplayName;
        IsRebootRequired = paramItem.Info.IsRebootRequired;

        paramItem.Value.Subscribe(_ =>
            {
                if (_internalUpdate) return;
                switch (_.Type)
                {
                    case MavParamType.MavParamTypeUint8:
                        Value = ((byte)_).ToString();
                        break;
                    case MavParamType.MavParamTypeInt8:
                        Value = ((sbyte)_).ToString();
                        break;
                    case MavParamType.MavParamTypeUint16:
                        Value = ((ushort)_).ToString();
                        break;
                    case MavParamType.MavParamTypeInt16:
                        Value = ((short)_).ToString();
                        break;
                    case MavParamType.MavParamTypeUint32:
                        Value = ((uint)_).ToString();
                        break;
                    case MavParamType.MavParamTypeInt32:
                    case MavParamType.MavParamTypeInt64:
                        Value = ((int)_).ToString();
                        break;
                    case MavParamType.MavParamTypeUint64:
                        Value = ((ulong)_).ToString();
                        break;
                    case MavParamType.MavParamTypeReal32:
                        Value = ((float)_).ToString();
                        break;
                    case MavParamType.MavParamTypeReal64:
                        Value = ((double)_).ToString();
                        break;
                }
            })
            .DisposeItWith(Disposable);

        this.WhenAnyValue(_ => _.Value).Subscribe(_ =>
            {
                _internalUpdate = true;
                switch (paramItem.Type)
                {
                    case MavParamType.MavParamTypeUint8:
                    {
                        if (byte.TryParse(_, out var result))
                        {
                            paramItem.Value.OnNext(result);
                        }

                        break;
                    }
                    case MavParamType.MavParamTypeInt8:
                    {
                        if (sbyte.TryParse(_, out var result))
                        {
                            paramItem.Value.OnNext(result);
                        }

                        break;
                    }
                    case MavParamType.MavParamTypeUint16:
                    {
                        if (ushort.TryParse(_, out var result))
                        {
                            paramItem.Value.OnNext(result);
                        }

                        break;
                    }
                    case MavParamType.MavParamTypeInt16:
                    {
                        if (short.TryParse(_, out var result))
                        {
                            paramItem.Value.OnNext(result);
                        }

                        break;
                    }
                    case MavParamType.MavParamTypeUint32:
                    {
                        if (uint.TryParse(_, out var result))
                        {
                            paramItem.Value.OnNext(result);
                        }

                        break;
                    }
                    case MavParamType.MavParamTypeInt32:
                    {
                        if (int.TryParse(_, out var result))
                        {
                            paramItem.Value.OnNext(result);
                        }

                        break;
                    }
                    case MavParamType.MavParamTypeUint64:
                    {
                        if (ulong.TryParse(_, out var result))
                        {
                            paramItem.Value.OnNext(result);
                        }

                        break;
                    }
                    case MavParamType.MavParamTypeInt64:
                    {
                        if (long.TryParse(_, out var result))
                        {
                            paramItem.Value.OnNext(result);
                        }

                        break;
                    }
                    case MavParamType.MavParamTypeReal32:
                    case MavParamType.MavParamTypeReal64:
                    {
                        if (float.TryParse(_.Replace(",", "."),
                                CultureInfo.InvariantCulture,
                                out var result))
                        {
                            paramItem.Value.OnNext(result);
                        }

                        break;
                    }
                }

                _internalUpdate = false;
            })
            .DisposeItWith(Disposable);

        paramItem.IsSynced.Subscribe(_ => IsSynced = _).DisposeItWith(Disposable);
        Write = ReactiveCommand.CreateFromTask(async cancel => { await paramItem.Write(cancel); })
            .DisposeItWith(Disposable);
        Write.IsExecuting.ToProperty(this, _ => _.IsWriting, out _isWriting).DisposeItWith(Disposable);
        Write.ThrownExceptions.Subscribe(OnWriteError).DisposeItWith(Disposable);

        Update = ReactiveCommand.CreateFromTask(async cancel => { await paramItem.Read(cancel); })
            .DisposeItWith(Disposable);
        Update.IsExecuting.ToProperty(this, _ => _.IsUpdate, out _isUpdate).DisposeItWith(Disposable);
        Update.ThrownExceptions.Subscribe(OnReadError).DisposeItWith(Disposable);

        this.WhenAnyValue(_ => _.IsStarred)
            .Subscribe(_ => StarKind = _ ? MaterialIconKind.Star : MaterialIconKind.StarBorder)
            .DisposeItWith(Disposable);
    }

    private void OnWriteError(Exception? ex)
    {
        _log.Error("Params", $"Write {Name} error", ex);
    }

    private void OnReadError(Exception? ex)
    {
        _log.Error("Params", $"Read {Name} error", ex);
    }

    public bool IsWriting => _isWriting.Value;
    public bool IsUpdate => _isUpdate.Value;

    public string Name { get; }

    public string DisplayName { get; set; }
    public string Units { get; set; }
    public ReactiveCommand<Unit, Unit> Update { get; set; }
    public ReactiveCommand<Unit, Unit> Write { get; }
    public ReactiveCommand<Unit, Unit> PinItem { get; }
    public string ValueDescription { get; }
    public string Description { get; }
    public bool IsRebootRequired { get; }
    [Reactive] public bool IsPinned { get; set; }
    [Reactive] public bool IsSynced { get; set; }
    [Reactive] public string Value { get; set; }
    [Reactive] public bool IsStarred { get; set; }
    [Reactive] public MaterialIconKind StarKind { get; set; }

    public bool Filter(string searchText, bool starredOnly)
    {
        if (starredOnly)
        {
            if (IsStarred == false) return false;
        }

        if (searchText.IsNullOrWhiteSpace()) return true;
        return Name.Contains(searchText);
    }

    public ParamItemViewModelConfig GetConfig()
    {
        return new ParamItemViewModelConfig
        {
            IsStarred = IsStarred,
            IsPinned = IsPinned,
            Name = Name
        };
    }

    public void SetConfig(ParamItemViewModelConfig item)
    {
        IsStarred = item.IsStarred;
        IsPinned = item.IsPinned;
    }

    public async void WriteParamData()
    {
        await _paramItem.Write();
    }
}