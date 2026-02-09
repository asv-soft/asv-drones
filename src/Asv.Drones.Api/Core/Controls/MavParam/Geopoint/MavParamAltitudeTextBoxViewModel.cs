using Asv.Avalonia;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones.Api;

public class MavParamAltitudeTextBoxViewModel : MavParamTextBoxViewModel
{
    private readonly IUnitItem _unit;

    public MavParamAltitudeTextBoxViewModel(
        MavParamInfo param,
        Observable<MavParamValue> update,
        InitialReadParamDelegate initReadCallback,
        ILoggerFactory loggerFactory,
        IUnitService measureService
    )
        : base(param, update, initReadCallback, loggerFactory)
    {
        _unit =
            measureService[AltitudeUnit.Id]?.CurrentUnitItem.CurrentValue
            ?? throw new ArgumentNullException(LatitudeUnit.Id);
    }

    protected override string ValueToText(ValueType remoteValue)
    {
        var meter = MavlinkTypesHelper.AltFromMmToDoubleMeter((int)remoteValue);
        return _unit.PrintFromSi(meter);
    }

    public override string? Units => _unit.Symbol;

    protected override Exception? TextToValue(string valueAsString, out ValueType value)
    {
        var result = _unit.ValidateValue(valueAsString);

        if (result.IsSuccess == false)
        {
            value = Info.DefaultValue;
            return result.ValidationException;
        }

        var degE6 = _unit.ParseToSi(valueAsString);
        value = MavlinkTypesHelper.AltFromDoubleMeterToInt32Mm(degE6);
        return null;
    }
}
