using System.Globalization;
using System.Reactive.Linq;
using Asv.Cfg;
using Asv.Common;

namespace Asv.Drones.Gui.Core;

public abstract class MeasureUnitBase<TValue, TEnum> : DisposableOnceWithCancel, IMeasureUnit<TValue, TEnum>
{
    protected MeasureUnitBase(IConfiguration cfgSvc,string cfgKey,IMeasureUnitItem<TValue, TEnum>[] items)
    {
        if (cfgSvc == null) throw new ArgumentNullException(nameof(cfgSvc));
        if (cfgKey == null) throw new ArgumentNullException(nameof(cfgKey));
        AvailableUnits = items ?? throw new ArgumentNullException(nameof(items));
        var id = cfgSvc.Get(cfgKey, default(TEnum));
        SiUnit = AvailableUnits.First(_ => _.IsSiUnit);
        var item = AvailableUnits.FirstOrDefault(_ => _.Id.Equals(id)) ?? SiUnit;
        CurrentUnit = new RxValue<IMeasureUnitItem<TValue, TEnum>>(item).DisposeItWith(Disposable);
        CurrentUnit.DistinctUntilChanged(_ => _.Id)
            .Subscribe(_ =>
            {
                cfgSvc.Set(cfgKey, _.Id);
            })
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

    public DoubleMeasureUnitItem(TEnum id, string title, string unit, bool isSiUnit,string formatString, double multiplierCoef)
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
    
    public virtual double ConvertToSi(string value)
    {
        value = value.Replace(',', '.');
        return ConvertToSi(double.Parse(value,CultureInfo.InvariantCulture));
    }

    public virtual string FromSiToString(double value)
    {
        return ConvertFromSi(value).ToString(_formatString);
    }

    public virtual string FromSiToStringWithUnits(double value)
    {
        return $"{FromSiToString(value)} {Unit}";
    }

    public virtual bool IsValid(string value)
    {
        if (value.IsNullOrWhiteSpace()) return false;
        value = value.Replace(',', '.');
        return double.TryParse(value, NumberStyles.Any,CultureInfo.InvariantCulture, out var _);
    }

    public virtual string? GetErrorMessage(string value)
    {
        if (value.IsNullOrWhiteSpace()) return "Value can't be null or white space"; // TODO: Localize
        value = value.Replace(',', '.');
        return double.TryParse(value,NumberStyles.Any,CultureInfo.InvariantCulture, out _) == false ? "Value must be a number" : null;
    }

    
    
}