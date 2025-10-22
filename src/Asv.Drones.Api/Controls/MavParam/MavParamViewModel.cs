using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using Asv.Avalonia;
using Asv.Common;
using Asv.Mavlink;
using Asv.Mavlink.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;
using ZLogger;

namespace Asv.Drones.Api;

public delegate ValueTask<MavParamValue> InitialReadParamDelegate(
    string paramName,
    CancellationToken cancel
);

public class MavParamViewModel
    : RoutableViewModel,
        ISupportRefresh,
        ISupportCancel,
        IComparable<MavParamViewModel>,
        IComparable
{
    protected MavParamViewModel(
        MavParamInfo metadata,
        Observable<MavParamValue> update,
        InitialReadParamDelegate initReadCallback,
        ILoggerFactory loggerFactory
    )
        : base(metadata.Id, loggerFactory)
    {
        Info = metadata;
        update
            .ObserveOnCurrentSynchronizationContext()
            .Subscribe(InternalOnRemoteChanged)
            .DisposeItWith(Disposable);

        Value = new BindableReactiveProperty<ValueType>(metadata.DefaultValue).DisposeItWith(
            Disposable
        );
        Value
            .Where(_ => IsRemoteChange == false)
            .Subscribe(_ => IsSync = false)
            .DisposeItWith(Disposable);

        // this is random delay for inital read to avoid many requests at the same time
        Observable
            .Timer(TimeSpan.FromMilliseconds(Random.Shared.Next(1, 1000)))
            .Take(1)
            .Subscribe(initReadCallback, (_, callback) => Init(callback))
            .DisposeItWith(Disposable);
    }

    public MavParamInfo Info { get; }

    private async void Init(InitialReadParamDelegate callback)
    {
        try
        {
            var value = await callback(Info.Metadata.Name, DisposeCancel);
            InternalOnRemoteChanged(value);
        }
        catch (Exception e)
        {
            Logger.ZLogError(
                e,
                $"Failed to read initial value for param {Info.Metadata.Name}:{e.Message}"
            );
            IsNetworkError = true;
            NetworkErrorMessage = e.Message;
        }
    }

    private void InternalOnRemoteChanged(MavParamValue value)
    {
        if (IsInEditMode)
        {
            return;
        }
        IsRemoteChange = true;
        Value.OnNext(Info.Convert(value));
        IsSync = true;
        IsNetworkError = false;
        IsRemoteChange = false;
    }

    public void ResetToDefault()
    {
        Value.Value = Info.DefaultValue;
        Write();
    }

    public async void Refresh()
    {
        try
        {
            IsBusy = true;
            IsNetworkError = false;
            NetworkErrorMessage = null;
            IsInEditMode = false;
            await Api.Commands.Mavlink.ReadParam(this, Info.Metadata.Name);
        }
        catch (Exception e)
        {
            IsNetworkError = true;
            NetworkErrorMessage = e.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async void Write()
    {
        if (HasValidationErrors)
        {
            return;
        }
        var lastValue = IsInEditMode;
        try
        {
            NetworkErrorMessage = null;
            IsNetworkError = false;
            IsBusy = true;
            await Commands.Mavlink.WriteParam(this, Info.Metadata.Name, Info.Convert(Value.Value));
        }
        catch (Exception e)
        {
            IsNetworkError = true;
            NetworkErrorMessage = e.Message;
        }
        finally
        {
            IsInEditMode = lastValue;
            IsBusy = false;
        }
    }

    public bool HasValidationErrors
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsFocused
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsNetworkError
    {
        get;
        set => SetField(ref field, value);
    }

    public string? NetworkErrorMessage
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsSync
    {
        get;
        set => SetField(ref field, value);
    } = true;

    public BindableReactiveProperty<ValueType> Value { get; }

    public bool IsRemoteChange
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsInEditMode
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsBusy
    {
        get;
        set => SetField(ref field, value);
    }

    public bool ShowHeader
    {
        get;
        set => SetField(ref field, value);
    } = true;

    public bool IsVisible
    {
        get;
        set => SetField(ref field, value);
    } = true;

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        yield break;
    }

    public void Cancel() { }

    public int CompareTo(MavParamViewModel? other)
    {
        if (ReferenceEquals(this, other))
        {
            return 0;
        }

        if (other is null)
        {
            return 1;
        }

        return Info.Order.CompareTo(other.Info.Order);
    }

    public int CompareTo(object? obj)
    {
        if (obj is null)
        {
            return 1;
        }

        if (ReferenceEquals(this, obj))
        {
            return 0;
        }

        return obj is MavParamViewModel other
            ? CompareTo(other)
            : throw new ArgumentException($"Object must be of type {nameof(MavParamViewModel)}");
    }

    public static bool operator <(MavParamViewModel? left, MavParamViewModel? right)
    {
        return Comparer<MavParamViewModel>.Default.Compare(left, right) < 0;
    }

    public static bool operator >(MavParamViewModel? left, MavParamViewModel? right)
    {
        return Comparer<MavParamViewModel>.Default.Compare(left, right) > 0;
    }

    public static bool operator <=(MavParamViewModel? left, MavParamViewModel? right)
    {
        return Comparer<MavParamViewModel>.Default.Compare(left, right) <= 0;
    }

    public static bool operator >=(MavParamViewModel? left, MavParamViewModel? right)
    {
        return Comparer<MavParamViewModel>.Default.Compare(left, right) >= 0;
    }
}
