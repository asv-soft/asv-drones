using Asv.Avalonia;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones.Api;

public class MavParamLatLonTextBoxViewModel : MavParamTextBoxViewModel
{
    private readonly IUnitItem _unit;

    public MavParamLatLonTextBoxViewModel(
        MavParamInfo param,
        Observable<MavParamValue> update,
        InitialReadParamDelegate initReadCallback,
        ILoggerFactory loggerFactory,
        IUnitService measureService,
        bool isLatitude
    )
        : base(param, update, initReadCallback, loggerFactory)
    {
        _unit =
            measureService[isLatitude ? LatitudeBase.Id : LongitudeBase.Id]
                ?.CurrentUnitItem
                .CurrentValue ?? throw new ArgumentNullException(LatitudeBase.Id);
    }

    public override string? Units => null;

    protected override string ValueToText(ValueType remoteValue)
    {
        var degree = MavlinkTypesHelper.LatLonFromInt32E7ToDegDouble((int)remoteValue);
        return _unit.PrintFromSi(degree);
    }

    protected override Exception? TextToValue(string valueAsString, out ValueType value)
    {
        var result = _unit.ValidateValue(valueAsString);

        if (result.IsFailed)
        {
            value = Info.DefaultValue;
            return result.ValidationException;
        }

        var degE6 = _unit.ParseToSi(valueAsString);
        value = MavlinkTypesHelper.LatLonDegDoubleToFromInt32E7To(degE6);
        return null;
    }
}
