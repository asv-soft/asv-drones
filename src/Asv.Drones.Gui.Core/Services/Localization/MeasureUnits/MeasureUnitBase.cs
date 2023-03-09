using System.Globalization;
using System.Reactive.Linq;
using Asv.Cfg;
using Asv.Common;

namespace Asv.Drones.Gui.Core;

public abstract class MeasureUnitBase<TValue, TEnum> : DisposableOnceWithCancel, IMeasureUnit<TValue, TEnum>
{
    protected MeasureUnitBase(IConfiguration cfgSvc,string cfgKey,IEnumerable<IMeasureUnitItem<TValue, TEnum>> items)
    {
        if (cfgSvc == null) throw new ArgumentNullException(nameof(cfgSvc));
        if (cfgKey == null) throw new ArgumentNullException(nameof(cfgKey));
        AvailableUnits = items ?? throw new ArgumentNullException(nameof(items));
        var id = cfgSvc.Get(cfgKey, default(TEnum));
        var item = AvailableUnits.FirstOrDefault(_ => _.Id.Equals(id)) ?? AvailableUnits.First(_=>_.IsSiUnit);
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
}

public class DoubleMeasureUnitItem<TEnum> : IMeasureUnitItem<double, TEnum>
{
    private readonly string _formatString;
    private readonly double _multiplier;

    public DoubleMeasureUnitItem(TEnum id, string title, string unit, bool isSiUnit,string formatString, double multiplier)
    {
        _formatString = formatString;
        _multiplier = multiplier;
        Id = id;
        Title = title;
        Unit = unit;
        IsSiUnit = isSiUnit;
    }
    public TEnum Id { get; }
    public string Title { get; }
    public string Unit { get; }
    public bool IsSiUnit { get; }
    public virtual double ConvertFromSI(double siValue)
    {
        return siValue / _multiplier;
    }

    public virtual double ConvertToSI(double value)
    {
        return value * _multiplier;
    }
    
    public virtual double ConvertToSI(string value)
    {
        value = value.Replace(',', '.');
        return ConvertToSI(double.Parse(value,CultureInfo.InvariantCulture));
    }

    public string FromSIToString(double value)
    {
        return ConvertFromSI(value).ToString(_formatString);
    }

    public string FromSIToStringWithUnits(double value)
    {
        return $"{FromSIToString(value)} {Unit}";
    }

    public virtual bool IsValid(string value)
    {
        value = value.Replace(',', '.');
        return double.TryParse(value, NumberStyles.Any,CultureInfo.InvariantCulture, out var _);
    }

    public virtual string GetErrorMessage(string value)
    {
        return double.TryParse(value,NumberStyles.Any,CultureInfo.InvariantCulture, out _) == false ? "Value must be a number" : null;
    }

    
}