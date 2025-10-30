using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Input;
using Asv.Avalonia;
using Asv.Common;
using Asv.Mavlink;
using Asv.Mavlink.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public class ParamItemViewModelConfig
{
    public bool IsStarred { get; init; }
    public string Name { get; init; }
    public bool IsPinned { get; init; }
}

public class ParamItemViewModel : RoutableViewModel
{
    private readonly ParamItem _paramItem;
    private readonly ReactiveProperty<bool> _isPinned;
    private readonly ReactiveProperty<bool> _isStarred;
    private bool _internalUpdate;

    public ParamItemViewModel()
        : base(DesignTime.Id, DesignTime.LoggerFactory) // use base class instead of ParamItem, because there is no way to create an empty Param item
    {
        DesignTime.ThrowIfNotDesignMode();

        Name = "param" + Guid.NewGuid();
        DisplayName = Name;
        Description = "Design description";
        ValueDescription = "Value design description";
        IsRebootRequired = true;
    }

    public ParamItemViewModel(
        NavigationId id,
        ParamItem paramItem,
        ILoggerFactory loggerFactory,
        ParamItemViewModelConfig? config
    )
        : base(id, loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(paramItem);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _paramItem = paramItem;
        Name = paramItem.Name;
        DisplayName = paramItem.Info.DisplayName ?? string.Empty;
        Units = paramItem.Info.Units ?? string.Empty;
        Description = paramItem.Info.Description ?? string.Empty;
        ValueDescription = paramItem.Info.UnitsDisplayName ?? string.Empty;
        IsRebootRequired = paramItem.Info.IsRebootRequired;

        _isPinned = new ReactiveProperty<bool>(config?.IsPinned ?? false);
        _isStarred = new ReactiveProperty<bool>(config?.IsStarred ?? false);

        IsPinned = new HistoricalBoolProperty(nameof(IsPinned), _isPinned, loggerFactory) 
        {
            Parent = this,
        };
        IsPinned.ForceValidate();
        IsStarred = new HistoricalBoolProperty(nameof(IsStarred), _isStarred, loggerFactory)
        {
            Parent = this,
        };
        IsStarred.ForceValidate();

        Value = new BindableReactiveProperty<string?>();
        IsSynced = new BindableReactiveProperty<bool>();
        StarKind = new BindableReactiveProperty<MaterialIconKind>();

        PinItem = new ReactiveCommand(_ => IsPinned.ViewValue.Value = !IsPinned.ViewValue.Value);

        _sub = paramItem.IsSynced.AsObservable().Subscribe(_ => IsSynced.Value = _);

        _sub1 = paramItem.Value.Subscribe(param =>
        {
            if (_internalUpdate)
            {
                return;
            }

            Value.Value = param.Type switch
            {
                MavParamType.MavParamTypeUint8 => ((byte)param).ToString(),
                MavParamType.MavParamTypeInt8 => ((sbyte)param).ToString(),
                MavParamType.MavParamTypeUint16 => ((ushort)param).ToString(),
                MavParamType.MavParamTypeInt16 => ((short)param).ToString(),
                MavParamType.MavParamTypeUint32 => ((uint)param).ToString(),
                MavParamType.MavParamTypeInt32 or MavParamType.MavParamTypeInt64 => (
                    (int)param
                ).ToString(),
                MavParamType.MavParamTypeUint64 => ((ulong)param).ToString(),
                MavParamType.MavParamTypeReal32 => ((float)param).ToString(
                    CultureInfo.InvariantCulture
                ),
                MavParamType.MavParamTypeReal64 => ((double)param).ToString(
                    CultureInfo.InvariantCulture
                ),
                _ => Value.Value,
            };
        });

        _sub2 = Value.Subscribe(val =>
        {
            _internalUpdate = true;
            if (string.IsNullOrWhiteSpace(val))
            {
                paramItem.Value.OnNext(0);
            }

            switch (paramItem.Type)
            {
                case MavParamType.MavParamTypeUint8:
                {
                    if (byte.TryParse(val, out var result))
                    {
                        paramItem.Value.OnNext(result);
                    }

                    break;
                }

                case MavParamType.MavParamTypeInt8:
                {
                    if (sbyte.TryParse(val, out var result))
                    {
                        paramItem.Value.OnNext(result);
                    }

                    break;
                }

                case MavParamType.MavParamTypeUint16:
                {
                    if (ushort.TryParse(val, out var result))
                    {
                        paramItem.Value.OnNext(result);
                    }

                    break;
                }

                case MavParamType.MavParamTypeInt16:
                {
                    if (short.TryParse(val, out var result))
                    {
                        paramItem.Value.OnNext(result);
                    }

                    break;
                }

                case MavParamType.MavParamTypeUint32:
                {
                    if (uint.TryParse(val, out var result))
                    {
                        paramItem.Value.OnNext(result);
                    }

                    break;
                }

                case MavParamType.MavParamTypeInt32:
                {
                    if (int.TryParse(val, out var result))
                    {
                        paramItem.Value.OnNext(result);
                    }

                    break;
                }

                case MavParamType.MavParamTypeUint64:
                {
                    if (ulong.TryParse(val, out var result))
                    {
                        paramItem.Value.OnNext(result);
                    }

                    break;
                }

                case MavParamType.MavParamTypeInt64:
                {
                    if (long.TryParse(val, out var result))
                    {
                        paramItem.Value.OnNext(result);
                    }

                    break;
                }

                case MavParamType.MavParamTypeReal32:
                case MavParamType.MavParamTypeReal64:
                {
                    if (
                        float.TryParse(
                            val?.Replace(",", "."),
                            CultureInfo.InvariantCulture,
                            out var result
                        )
                    )
                    {
                        paramItem.Value.OnNext(result);
                    }

                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }

            _internalUpdate = false;
        });

        Write = new ReactiveCommand(
            async (_, ct) =>
            {
                try
                {
                    await Api.Commands.Mavlink.WriteParam(
                        this,
                        paramItem.Name,
                        paramItem.Value.Value,
                        ct
                    );
                    IsSynced.Value = true;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Write {Name} error", Name);
                }
            }
        ).DisposeItWith(Disposable);

        Read = new ReactiveCommand(
            async (_, ct) =>
            {
                try
                {
                    await Api.Commands.Mavlink.ReadParam(this, paramItem.Name, ct);
                    IsSynced.Value = true;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Read {Name} error", Name);
                }
            }
        ).DisposeItWith(Disposable);

        _sub3 = IsStarred.ViewValue.Subscribe(isStarted =>
            StarKind.Value = isStarted ? MaterialIconKind.Star : MaterialIconKind.StarBorder
        );

        IsPinned
            .ViewValue.SubscribeAwait(
                async (_, _) => await Rise(new ParamItemChangedEvent(this, IsPinned))
            )
            .DisposeItWith(Disposable);
        IsStarred
            .ViewValue.SubscribeAwait(
                async (_, _) => await Rise(new ParamItemChangedEvent(this, IsStarred))
            )
            .DisposeItWith(Disposable);
    }

    public string Name { get; }

    private readonly string _displayName;
    public string DisplayName
    {
        get => _displayName;
        init => SetField(ref _displayName, value);
    }

    public string Units { get; }
    public ICommand Read { get; }
    public ReactiveCommand Write { get; }
    public ReactiveCommand PinItem { get; }
    public string ValueDescription { get; }
    public string Description { get; }
    public bool IsRebootRequired { get; }
    public BindableReactiveProperty<bool> IsSynced { get; }
    public BindableReactiveProperty<MaterialIconKind> StarKind { get; }
    public HistoricalBoolProperty IsPinned { get; }
    public BindableReactiveProperty<string?> Value { get; }
    public HistoricalBoolProperty IsStarred { get; }

    public bool Filter(string? searchText, bool starredOnly)
    {
        if (starredOnly)
        {
            if (!IsStarred.ViewValue.Value)
            {
                return false;
            }
        }

        if (string.IsNullOrWhiteSpace(searchText))
        {
            return true;
        }

        return Name.Contains(searchText, StringComparison.InvariantCultureIgnoreCase);
    }

    public ParamItemViewModelConfig GetConfig()
    {
        return new ParamItemViewModelConfig
        {
            IsStarred = IsStarred.ViewValue.Value,
            IsPinned = IsPinned.ViewValue.Value,
            Name = Name,
        };
    }

    public void SetConfig(ParamItemViewModelConfig item)
    {
        IsStarred.ViewValue.Value = item.IsStarred;
        IsPinned.ViewValue.Value = item.IsPinned;
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        yield return IsPinned;
        yield return IsStarred;
    }

    #region Dispose

    private readonly IDisposable _sub;
    private readonly IDisposable _sub1;
    private readonly IDisposable _sub2;
    private readonly IDisposable _sub3;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sub.Dispose();
            _sub1.Dispose();
            _sub2.Dispose();
            _sub3.Dispose();
            _isPinned.Dispose();
            _isStarred.Dispose();
            IsSynced.Dispose();
            IsStarred.Dispose();
            IsPinned.Dispose();
            Value.Dispose();
            PinItem.Dispose();
            StarKind.Dispose();
        }

        base.Dispose(disposing);
    }

    #endregion
}
