using System;
using System.Collections.Generic;
using Asv.Avalonia.IO;

namespace Asv.Drones;

public abstract class PacketFilterComparerBase<TFilter>
    : IEqualityComparer<PacketFilterViewModelBase<TFilter>>
    where TFilter : PacketFilterViewModelBase<TFilter>
{
    public virtual bool Equals(
        PacketFilterViewModelBase<TFilter>? x,
        PacketFilterViewModelBase<TFilter>? y
    )
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (x is null)
        {
            return false;
        }

        if (y is null)
        {
            return false;
        }

        if (x.GetType() != y.GetType())
        {
            return false;
        }

        return x.Id.Equals(y.Id)
            && x.FilterValue.Value.Equals(y.FilterValue.Value)
            && x.MessageRateText.Value.Equals(y.MessageRateText.Value);
    }

    public virtual int GetHashCode(PacketFilterViewModelBase<TFilter> obj)
    {
        return HashCode.Combine(obj.Id, obj.FilterValue.Value, obj.MessageRateText.Value);
    }
}
