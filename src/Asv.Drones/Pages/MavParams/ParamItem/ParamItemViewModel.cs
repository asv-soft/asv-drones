using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    public bool IsStarred { get; set; }
    public string Name { get; set; }
    public bool IsPinned { get; set; }
}

public class ParamItemViewModel : RoutableViewModel
{
    private readonly ILogger _log;
    private readonly ParamItem _paramItem;
    private readonly ReactiveProperty<string?> _value;
    private readonly ReactiveProperty<bool> _isPinned;
    private readonly ReactiveProperty<bool> _isStarred;
    private readonly ReactiveProperty<bool> _isWriting;
    private readonly ReactiveProperty<bool> _isUpdate;
    private bool _internalUpdate;

    public ParamItemViewModel()
        : base("param_item") // use base class instead of ParamItem, because there is no way to create an empty Param item
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
        ILoggerFactory log,
        ParamsConfig config
    )
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(paramItem);
        ArgumentNullException.ThrowIfNull(log);
        ArgumentNullException.ThrowIfNull(config);

        _log = log.CreateLogger<ParamItemViewModel>();
        _paramItem = paramItem;
        Name = paramItem.Name;
        DisplayName = paramItem.Info.DisplayName ?? string.Empty;
        Units = paramItem.Info.Units ?? string.Empty;
        Description = paramItem.Info.Description ?? string.Empty;
        ValueDescription = paramItem.Info.UnitsDisplayName ?? string.Empty;
        IsRebootRequired = paramItem.Info.IsRebootRequired;
        var fromCfg = config.Params.FirstOrDefault(p => p.Name == Name);

        _isPinned = new ReactiveProperty<bool>();
        _isStarred = new ReactiveProperty<bool>(fromCfg?.IsStarred ?? false);
        _value = new ReactiveProperty<string?>();

        _isUpdate = new BindableReactiveProperty<bool>();
        _isWriting = new BindableReactiveProperty<bool>();
        IsPinned = new HistoricalBoolProperty($"{id}.{nameof(IsPinned)}", _isPinned)
        {
            Parent = this,
        };
        IsStarred = new HistoricalBoolProperty($"{id}.{nameof(IsStarred)}", _isStarred)
        {
            Parent = this,
        };
        Value = new HistoricalStringProperty($"{id}.{nameof(Value)}", _value) { Parent = this };
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

            Value.ModelValue.Value = param.Type switch
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
                _ => Value.ViewValue.Value,
            };
        });

        _sub2 = Value.ViewValue.Subscribe(val =>
        {
            _internalUpdate = true;
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

        Write = new BindableAsyncCommand(WriteParamCommand.Id, this);

        Update = new BindableAsyncCommand(UpdateParamCommand.Id, this);

        _sub3 = IsStarred.ViewValue.Subscribe(isStarted =>
            StarKind.Value = isStarted ? MaterialIconKind.Star : MaterialIconKind.StarBorder
        );

        _sub4 = IsStarred.ViewValue.Subscribe(_ =>
        {
            var existItem = config.Params.FirstOrDefault(__ => __.Name == Name);

            if (existItem != null)
            {
                config.Params.Remove(existItem);
            }

            config.Params.Add(
                new ParamItemViewModelConfig
                {
                    IsStarred = IsStarred.ViewValue.CurrentValue,
                    IsPinned = IsPinned.ViewValue.CurrentValue,
                    Name = Name,
                }
            );
        });
    }

    public string Name { get; }

    public string DisplayName { get; init; }
    public string Units { get; }
    public ICommand Update { get; }
    public ICommand Write { get; }
    public ReactiveCommand PinItem { get; }
    public string ValueDescription { get; }
    public string Description { get; }
    public bool IsRebootRequired { get; }
    public BindableReactiveProperty<bool> IsSynced { get; }
    public BindableReactiveProperty<MaterialIconKind> StarKind { get; }
    public HistoricalBoolProperty IsPinned { get; }
    public HistoricalStringProperty Value { get; }
    public HistoricalBoolProperty IsStarred { get; }

    public bool Filter(string searchText, bool starredOnly)
    {
        if (starredOnly)
        {
            if (!IsStarred.ViewValue.Value)
            {
                return false;
            }
        }

        if (searchText.IsNullOrWhiteSpace())
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

    internal async ValueTask UpdateImpl(CancellationToken cancel)
    {
        try
        {
            _isUpdate.Value = true;
            await _paramItem.Read(cancel);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Read {Name} error", Name);
        }
        finally
        {
            _isUpdate.Value = false;
        }
    }

    internal async ValueTask WriteImpl(CancellationToken cancel)
    {
        try
        {
            _isWriting.Value = true;
            await _paramItem.Write(cancel);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Write {Name} error", Name);
        }
        finally
        {
            _isWriting.Value = false;
        }
    }

    internal async Task WriteParamData(CancellationToken cancel)
    {
        await _paramItem.Write(cancel);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        yield return IsPinned;
        yield return IsStarred;
        yield return Value;
    }

    #region Dispose

    private readonly IDisposable _sub;
    private readonly IDisposable _sub1;
    private readonly IDisposable _sub2;
    private readonly IDisposable _sub3;
    private readonly IDisposable _sub4;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sub.Dispose();
            _sub1.Dispose();
            _sub2.Dispose();
            _sub3.Dispose();
            _sub4.Dispose();
            _isPinned.Dispose();
            _isStarred.Dispose();
            _isWriting.Dispose();
            _isUpdate.Dispose();
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
