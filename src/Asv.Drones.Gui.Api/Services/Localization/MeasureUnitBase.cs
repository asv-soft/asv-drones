using System.Diagnostics;
using System.Globalization;
using System.Reactive.Linq;
using Asv.Cfg;
using Asv.Common;

namespace Asv.Drones.Gui.Api;

public abstract class MeasureUnitBase<TValue, TEnum> : DisposableOnceWithCancel, IMeasureUnit<TValue, TEnum>
{
    protected MeasureUnitBase(IConfiguration cfgSvc, string cfgKey, IMeasureUnitItem<TValue, TEnum>[] items)
    {
        if (cfgSvc == null) throw new ArgumentNullException(nameof(cfgSvc));
        if (cfgKey == null) throw new ArgumentNullException(nameof(cfgKey));
        AvailableUnits = items ?? throw new ArgumentNullException(nameof(items));
        var id = cfgSvc.Get(cfgKey, default(TEnum));
        SiUnit = AvailableUnits.First(_ => _.IsSiUnit);
        var item = AvailableUnits.FirstOrDefault(x =>
        {
            Debug.Assert(x.Id != null, "x.Id != null");
            return x.Id.Equals(id);
        }) ?? SiUnit;
        CurrentUnit = new RxValue<IMeasureUnitItem<TValue, TEnum>>(item).DisposeItWith(Disposable);
        CurrentUnit.DistinctUntilChanged(_ => _.Id)
            .Subscribe(_ => { cfgSvc.Set(cfgKey, _.Id); })
            .DisposeItWith(Disposable);
    }

    public abstract string Title { get; }
    public abstract string Description { get; }
    public IEnumerable<IMeasureUnitItem<TValue, TEnum>> AvailableUnits { get; }
    public IRxEditableValue<IMeasureUnitItem<TValue, TEnum>> CurrentUnit { get; }
    public IMeasureUnitItem<TValue, TEnum> SiUnit { get; }
}

public class DoubleMeasureUnitItem<TEnum> : IMeasureUnitItem<double, TEnum>
{
    private readonly string _formatString;
    private readonly double _multiplierCoef;

    public DoubleMeasureUnitItem(TEnum id, string title, string unit, bool isSiUnit, string formatString,
        double multiplierCoef)
    {
        _formatString = formatString;
        _multiplierCoef = multiplierCoef;
        Id = id;
        Title = title;
        Unit = unit;
        IsSiUnit = isSiUnit;
    }

    public TEnum Id { get; }
    public string Title { get; }
    public string Unit { get; }
    public bool IsSiUnit { get; }

    public virtual double ConvertFromSi(double siValue)
    {
        return siValue / _multiplierCoef;
    }

    public virtual double ConvertToSi(double value)
    {
        return value * _multiplierCoef;
    }

    public double Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return double.NaN;
        value = value.Replace(',', '.');
        return double.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture);
    }

    public string Print(double value)
    {
        return value.ToString(_formatString, CultureInfo.InvariantCulture);
    }

    public string PrintWithUnits(double value)
    {
        return $"{value.ToString(_formatString, CultureInfo.InvariantCulture)} {Unit}";
    }

    public virtual bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        value = value.Replace(',', '.');
        return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var _);
    }

    public virtual string? GetErrorMessage(string? value)
    {
        if (value.IsNullOrWhiteSpace()) return RS.MeasureUnitBase_ErrorMessage_NullOrWhiteSpace;
        value = value.Replace(',', '.');
        return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out _) == false
            ? RS.MeasureUnitBase_ErrorMessage_NotANumber
            : null;
    }
}