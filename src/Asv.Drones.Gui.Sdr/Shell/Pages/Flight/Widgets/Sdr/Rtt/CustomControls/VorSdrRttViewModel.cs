using System.Reactive.Linq;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Asv.Mavlink.V2.AsvSdr;
using Avalonia.Controls;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Sdr;

public class VorSdrRttViewModel : ViewModelBase, ISdrRttWidget
{
    private readonly ILogService _logService;
    private readonly ILocalizationService _loc;
    private readonly IConfiguration _configuration;
    private readonly IMeasureUnitItem<double, FrequencyUnits> _freqInHzMeasureUnit;
    private readonly IMeasureUnitItem<double,FrequencyUnits>? _freqInKHzMeasureUnit;
    private readonly IMeasureUnitItem<double, FrequencyUnits> _freqInMHzMeasureUnit;

    public VorSdrRttViewModel() : base(new Uri($"fordesigntime://{Guid.NewGuid()}"))
    {
        if (Design.IsDesignMode)
        {
            ChannelTitle = RS.VorSdrRttViewModelChannelTitle;
            ChannelStringValue = $"{18}X";
            
            MainFrequencyTitle = RS.VorSdrRttViewModelMainFrequencyTitle;
            MainFrequencyStringValue = $"{108.1:F4}";
            MainFrequencyUnits = "MHz";
            
            MeasureTimeTitle = RS.VorSdrRttViewModelMeasureTimeTitle;
            MeasureTimeStringValue = $"{500}";
            MeasureTimeUnits = "ms";
            
            FieldStrengthTitle = RS.VorSdrRttViewModelFieldStrengthTitle;
            FieldStrengthStringValue = $"{100000}";
            FieldStrengthUnits = "μV/m";
            
            PowerTitle = RS.VorSdrRttViewModelPowerTitle;
            PowerValue = -30.53;
            PowerStringValue = "-30.53";
            PowerUnits = "dBm";

            FrequencyOffsetTitle = "";
            FrequencyOffsetStringValue = "0.036";
            FrequencyOffsetUnits = "kHz";

            BearingTitle = RS.VorSdrRttViewModelBearingTitle;
            BearingStringValue = "50.02";
            BearingUnits = "°";

            Am30HzTitle = RS.VorSdrRttViewModelAM30HzTitle;
            Am30HzStringValue = "30.31";
            Am30HzUnits = "%";
        
            Am9960HzTitle = RS.VorSdrRttViewModelAM9960HzTitle;
            Am9960HzStringValue = "30.01";
            Am9960HzUnits = "%";

            FmDeviationTitle = RS.VorSdrRttViewModelFMDeviationTitle;
            FmDeviationStringValue = "479.98";
            FmDeviationUnits = "Hz";
        
            IdCodeTitle = RS.SdrRttViewModel_ID_Code_Title;
            IdCodeStringValue = "CEK";

            VoiceAmTitle = RS.VorSdrRttViewModelVoiceAmTitle;
            VoiceAmStringValue = "0.00";
            VoiceAmUnits = "%";

            Freq30HzTitle = RS.VorSdrRttViewModelFreq30HzTitle;
            Freq30HzStringValue = "30.00";
            Freq30HzUnits = "Hz";
        
            Freq9960HzTitle = RS.VorSdrRttViewModelFreq9960HzTitle;
            Freq9960HzStringValue = "9960.09";
            Freq9960HzUnits = "Hz";

            FreqFm30Title = RS.VorSdrRttViewModelFreqFM30Title;
            FreqFm30StringValue = "30.00";
            FreqFm30Units = "Hz";

            FmIndexTitle = RS.VorSdrRttViewModelFMIndexTitle;
            FmIndexStringValue = "16.00";
        }
    }
    
    public VorSdrRttViewModel(ISdrClientDevice payload, ILogService log, ILocalizationService loc, IConfiguration configuration)
        :base(FlightSdrWidgetBase.GenerateUri(payload, "sdr/vor"))
    {
        _logService = log ?? throw new ArgumentNullException(nameof(log));
        _loc = loc ?? throw new ArgumentNullException(nameof(loc));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        
        _freqInHzMeasureUnit = _loc.Frequency.AvailableUnits.FirstOrDefault(_ => _.Id == FrequencyUnits.Hz);
        _freqInKHzMeasureUnit = _loc.Frequency.AvailableUnits.FirstOrDefault(_ => _.Id == FrequencyUnits.KHz);
        _freqInMHzMeasureUnit = new DoubleMeasureUnitItem<FrequencyUnits>(FrequencyUnits.MHz, "", Core.RS.Frequency_Megahertz_Unit, false, "F4", 1);
        
        ChannelTitle = RS.VorSdrRttViewModelChannelTitle;
        
        MainFrequencyTitle = RS.VorSdrRttViewModelMainFrequencyTitle;
        MainFrequencyUnits = "MHz";
        
        MeasureTimeTitle = RS.VorSdrRttViewModelMeasureTimeTitle;
        MeasureTimeUnits = "ms";
        
        FieldStrengthTitle = RS.VorSdrRttViewModelFieldStrengthTitle;
        FieldStrengthUnits = "μV/m";
        
        PowerTitle = RS.VorSdrRttViewModelPowerTitle;
        PowerUnits = _loc.Power.CurrentUnit.Value.Unit;

        FrequencyOffsetTitle = "";
        FrequencyOffsetUnits = _freqInKHzMeasureUnit?.Unit;

        BearingTitle = RS.VorSdrRttViewModelBearingTitle;
        BearingUnits = _loc.Bearing.CurrentUnit.Value.Unit;

        Am30HzTitle = RS.VorSdrRttViewModelAM30HzTitle;
        Am30HzUnits = _loc.AmplitudeModulation.CurrentUnit.Value.Unit;
        
        Am9960HzTitle = RS.VorSdrRttViewModelAM9960HzTitle;
        Am9960HzUnits = _loc.AmplitudeModulation.CurrentUnit.Value.Unit;

        FmDeviationTitle = RS.VorSdrRttViewModelFMDeviationTitle;
        FmDeviationUnits = _freqInHzMeasureUnit?.Unit;
        
        IdCodeTitle = RS.SdrRttViewModel_ID_Code_Title;

        VoiceAmTitle = RS.VorSdrRttViewModelVoiceAmTitle;
        VoiceAmUnits = _loc.AmplitudeModulation.CurrentUnit.Value.Unit;

        Freq30HzTitle = RS.VorSdrRttViewModelFreq30HzTitle;
        Freq30HzUnits = _freqInHzMeasureUnit?.Unit;
        
        Freq9960HzTitle = RS.VorSdrRttViewModelFreq9960HzTitle;
        Freq9960HzUnits = _freqInHzMeasureUnit?.Unit;

        FreqFm30Title = RS.VorSdrRttViewModelFreqFM30Title;
        FreqFm30Units = _freqInHzMeasureUnit?.Unit;

        FmIndexTitle = RS.VorSdrRttViewModelFMIndexTitle;

        _loc.Bearing.CurrentUnit
            .Subscribe(_ => BearingUnits = _.Unit)
            .DisposeItWith(Disposable);

        _loc.AmplitudeModulation.CurrentUnit.Subscribe(_ =>
        {
            Am30HzUnits = _.Unit;
            Am9960HzUnits = _.Unit;
            VoiceAmUnits = _.Unit;
        }).DisposeItWith(Disposable);

        payload.Sdr.Base.OnRecordData.Where(_ => _.MessageId == AsvSdrRecordDataVorPacket.PacketMessageId)
            .Cast<AsvSdrRecordDataVorPacket>()
            .Subscribe(_ =>
            {
                ChannelStringValue = $"{18}X";
                MainFrequencyStringValue = _freqInMHzMeasureUnit?.FromSiToString(_.Payload.TotalFreq);
                MeasureTimeStringValue = $"{_.Payload.MeasureTime}";
                FieldStrengthStringValue = $"{_.Payload.FieldStrength}";
                
                PowerValue = _.Payload.Power;
                PowerStringValue = _loc.Power.FromSiToString(_.Payload.Power);
                FrequencyOffsetStringValue = _freqInKHzMeasureUnit?.FromSiToString(_.Payload.CarrierOffset);
                BearingStringValue = _loc.Bearing.FromSiToString(_.Payload.Azimuth);
                Am30HzStringValue = _loc.AmplitudeModulation.FromSiToString(_.Payload.Am30 * 100.0);
                Am9960HzStringValue = _loc.AmplitudeModulation.FromSiToString(_.Payload.Am9960 * 100.0);
                FmDeviationStringValue = _loc.AmplitudeModulation.FromSiToString(_.Payload.Deviation * 100.0);
                IdCodeStringValue = new string(_.Payload.CodeId.Select(__ => __ == '\0' ? ' ' : __).ToArray());
                VoiceAmStringValue = _loc.AmplitudeModulation.FromSiToString(_.Payload.CodeIdAm1020 * 100.0);
                Freq30HzStringValue = _freqInHzMeasureUnit?.FromSiToString(_.Payload.Freq30);
                Freq9960HzStringValue = _freqInHzMeasureUnit?.FromSiToString(_.Payload.Freq9960);
                FreqFm30StringValue = "";
                FmIndexStringValue = "";

            }).DisposeItWith(Disposable);
    }

    #region Main Frequency
    
    [Reactive]
    public string MainFrequencyTitle { get; set; }
    
    [Reactive]
    public string MainFrequencyStringValue { get; set; }

    [Reactive]
    public string MainFrequencyUnits { get; set; }
    
    #endregion
    
    #region Channel

    [Reactive]
    public string ChannelTitle { get; set; }
    
    [Reactive]
    public string ChannelStringValue { get; set; }

    #endregion
    
    #region Measure Time

    [Reactive]
    public string MeasureTimeTitle { get; set; }
    
    [Reactive]
    public string MeasureTimeStringValue { get; set; }

    [Reactive]
    public string MeasureTimeUnits { get; set; }

    #endregion
    
    #region Field Strength

    [Reactive]
    public string FieldStrengthTitle { get; set; }
    
    [Reactive]
    public string FieldStrengthStringValue { get; set; }

    [Reactive]
    public string FieldStrengthUnits { get; set; }

    #endregion
    
    #region Power

    [Reactive]
    public string PowerTitle { get; set; }
    
    [Reactive]
    public double PowerValue { get; set; }
    
    [Reactive]
    public string PowerStringValue { get; set; }
    
    [Reactive]
    public string PowerUnits { get; set; }
    
    #endregion

    #region Frequency Offset

    [Reactive]
    public string FrequencyOffsetTitle { get; set; }
    
    [Reactive]
    public string FrequencyOffsetStringValue { get; set; }
    
    [Reactive]
    public string FrequencyOffsetUnits { get; set; }

    #endregion
    
    #region Bearing

    [Reactive]
    public string BearingTitle { get; set; }
    
    [Reactive]
    public string BearingStringValue { get; set; }
    
    [Reactive]
    public string BearingUnits { get; set; }

    #endregion

    #region AM 30 Hz

    [Reactive]
    public string Am30HzTitle { get; set; }
    
    [Reactive]
    public string Am30HzStringValue { get; set; }
    
    [Reactive]
    public string Am30HzUnits { get; set; }
    
    
    #endregion

    #region AM 9960 Hz

    [Reactive]
    public string Am9960HzTitle { get; set; }
    
    [Reactive]
    public string Am9960HzStringValue { get; set; }
    
    [Reactive]
    public string Am9960HzUnits { get; set; }
    
    #endregion

    #region FM Deviation

    [Reactive]
    public string FmDeviationTitle { get; set; }
    
    [Reactive]
    public string FmDeviationStringValue { get; set; }
    
    [Reactive]
    public string FmDeviationUnits { get; set; }
    
    #endregion

    #region IdCode

    [Reactive]
    public string IdCodeTitle { get; set; }
    
    [Reactive]
    public string IdCodeStringValue { get; set; }
    
    #endregion

    #region Voice AM

    [Reactive]
    public string VoiceAmTitle { get; set; }
    
    [Reactive]
    public string VoiceAmStringValue { get; set; }
    
    [Reactive]
    public string VoiceAmUnits { get; set; }
    
    #endregion

    
    #region Freq 30 Hz

    [Reactive]
    public string Freq30HzTitle { get; set; }
    
    [Reactive]
    public string Freq30HzStringValue { get; set; }
    
    [Reactive]
    public string Freq30HzUnits { get; set; }

    #endregion

    #region Freq 9960 Hz

    [Reactive]
    public string Freq9960HzTitle { get; set; }
    
    [Reactive]
    public string Freq9960HzStringValue { get; set; }
    
    [Reactive]
    public string Freq9960HzUnits { get; set; }
    
    #endregion

    #region Freq FM 30

    [Reactive]
    public string FreqFm30Title { get; set; }
    
    [Reactive]
    public string FreqFm30StringValue { get; set; }
    
    [Reactive]
    public string FreqFm30Units { get; set; }
    
    #endregion

    #region FM Index

    [Reactive]
    public string FmIndexTitle { get; set; }
    
    [Reactive]
    public string FmIndexStringValue { get; set; }
    
    #endregion

    public int Order  => 0;
    public bool IsVisible { get; set; } = true;
}