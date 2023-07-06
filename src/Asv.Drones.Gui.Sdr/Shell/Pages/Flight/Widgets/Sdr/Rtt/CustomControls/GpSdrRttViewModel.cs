using System.Reactive.Linq;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Asv.Mavlink.V2.AsvSdr;
using Avalonia.Controls;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Sdr;

public class GpSdrRttViewModel : SdrRttViewModelBase
{
    private readonly ILogService _logService;
    private readonly ILocalizationService _loc;
    private readonly IConfiguration _configuration;
    private readonly IMeasureUnitItem<double,FrequencyUnits> _freqInHzMeasureUnit;
    private readonly IMeasureUnitItem<double,FrequencyUnits> _freqInKHzMeasureUnit;
    
    public GpSdrRttViewModel() : base(new Uri($"fordesigntime://{Guid.NewGuid()}"))
    {
        if (Design.IsDesignMode)
        {
            TotalPowerTitle = RS.SdrRttViewModel_SumPower_Title;
            TotalPowerUnits = "dBm";
            TotalPowerValue = -37.45;
            TotalPowerStringValue = "-37.45";

            TotalDdmTitle = "DDM 150-90";
            TotalDdmUnits = "1";
            TotalDdmStringValue = "0.0001";

            TotalSdmTitle = "SDM 90,150";
            TotalSdmUnits = "%";
            TotalSdmStringValue = "40.00";

            TotalAm90Title = RS.IlsSdrRttViewModel_AM_90_Hz_Title;
            TotalAm90Units = "%";
            TotalAm90StringValue = "20.00";

            TotalAm150Title = RS.IlsSdrRttViewModel_AM_150_Hz_Title;
            TotalAm150Units = "%";
            TotalAm150StringValue = "20.00";

            Phi90Title = "PHI 90";
            Phi90Units = "°";
            Phi90StringValue = "0.01";
        
            Phi150Title = "PHI 150";
            Phi150Units = "°";
            Phi150StringValue = "0.02";

            TotalFreq90Title = RS.IlsSdrRttViewModel_Freq_90_Hz_Title;
            TotalFreq90Units = "Hz";
            TotalFreq90StringValue = "90.00";
        
            TotalFreq150Title = RS.IlsSdrRttViewModel_Freq_150_Hz_Title;
            TotalFreq150Units = "Hz";
            TotalFreq150StringValue = "150.00";
            
            // CRS
            CrsPowerTitle = RS.IlsSdrRttViewModel_CRSPower_Title;
            CrsPowerUnits = "dBm";
            CrsPowerValue = -40.54;
            CrsPowerStringValue = "-40.54";

            CrsDdmTitle = "DDM 150-90";
            CrsDdmUnits = "1";
            CrsDdmStringValue = "0.0005";

            CrsSdmTitle = "SDM 90,150";
            CrsSdmUnits = "%";
            CrsSdmStringValue = "40.00";

            CrsAm90Title = RS.IlsSdrRttViewModel_AM_90_Hz_Title;
            CrsAm90Units = "%";
            CrsAm90StringValue = "20.00";

            CrsAm150Title = RS.IlsSdrRttViewModel_AM_150_Hz_Title;
            CrsAm150Units = "%";
            CrsAm150StringValue = "20.00";

            CrsFrequencyTitle = "";
            CrsFrequencyUnits = "kHz";
            CrsFrequencyStringValue = "8.000";
            
            // CLR
            ClrPowerTitle = RS.IlsSdrRttViewModel_CLRPower_Title;
            ClrPowerUnits = "dBm";
            ClrPowerValue = -40.35;
            ClrPowerStringValue = "-40.35";

            ClrDdmTitle = "DDM 150-90";
            ClrDdmUnits = "1";
            ClrDdmStringValue = "0.0006";

            ClrSdmTitle = "SDM 90,150";
            ClrSdmUnits = "%";
            ClrSdmStringValue = "40.00";

            ClrAm90Title = RS.IlsSdrRttViewModel_AM_90_Hz_Title;
            ClrAm90Units = "%";
            ClrAm90StringValue = "20.00";

            ClrAm150Title = RS.IlsSdrRttViewModel_AM_150_Hz_Title;
            ClrAm150Units = "%";
            ClrAm150StringValue = "20.00";

            ClrFrequencyTitle = "";
            ClrFrequencyUnits = "kHz";
            ClrFrequencyStringValue = "-8.000";
        }
    }
    
    public GpSdrRttViewModel(ISdrClientDevice payload, ILogService log, ILocalizationService loc, IConfiguration configuration)
        :base(FlightSdrWidgetBase.GenerateUri(payload, "sdr/gp"))
    {
        _logService = log ?? throw new ArgumentNullException(nameof(log));
        _loc = loc ?? throw new ArgumentNullException(nameof(loc));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        _freqInHzMeasureUnit = _loc.Frequency.AvailableUnits.FirstOrDefault(__ => __.Id == FrequencyUnits.Hz);
        _freqInKHzMeasureUnit = _loc.Frequency.AvailableUnits.FirstOrDefault(__ => __.Id == FrequencyUnits.KHz);
        
        TotalPowerTitle = RS.SdrRttViewModel_SumPower_Title;
        TotalPowerUnits = _loc.Power.CurrentUnit.Value.Unit;
        
        TotalDdmTitle = RS.IlsSdrRttViewModel_DDM_Title;
        TotalDdmUnits = _loc.DdmGp.CurrentUnit.Value.Unit;
        
        TotalSdmTitle = RS.IlsSdrRttViewModel_SDM_Title;
        TotalSdmUnits = _loc.Sdm.CurrentUnit.Value.Unit;
        
        TotalAm90Title = RS.IlsSdrRttViewModel_AM_90_Hz_Title;
        TotalAm90Units = _loc.AmplitudeModulation.CurrentUnit.Value.Unit;

        TotalAm150Title = RS.IlsSdrRttViewModel_AM_150_Hz_Title;
        TotalAm150Units = _loc.AmplitudeModulation.CurrentUnit.Value.Unit;

        Phi90Title = "PHI 90";
        Phi90Units = _loc.Phase.CurrentUnit.Value.Unit;
        
        Phi150Title = "PHI 150";
        Phi150Units = _loc.Phase.CurrentUnit.Value.Unit;
        
        
        TotalFreq90Title = RS.IlsSdrRttViewModel_Freq_90_Hz_Title;
        TotalFreq90Units = _freqInHzMeasureUnit?.Unit;
        
        TotalFreq150Title = RS.IlsSdrRttViewModel_Freq_150_Hz_Title;
        TotalFreq150Units = _freqInHzMeasureUnit?.Unit;
        
        // CRS
        CrsPowerTitle = RS.IlsSdrRttViewModel_CRSPower_Title;
        CrsPowerUnits = _loc.Power.CurrentUnit.Value.Unit;
        
        CrsDdmTitle = RS.IlsSdrRttViewModel_DDM_Title;
        CrsDdmUnits = _loc.DdmGp.CurrentUnit.Value.Unit;

        CrsSdmTitle = RS.IlsSdrRttViewModel_SDM_Title;
        CrsSdmUnits = _loc.Sdm.CurrentUnit.Value.Unit;
        
        CrsAm90Title = RS.IlsSdrRttViewModel_AM_90_Hz_Title;
        CrsAm90Units = _loc.AmplitudeModulation.CurrentUnit.Value.Unit;

        CrsAm150Title = RS.IlsSdrRttViewModel_AM_150_Hz_Title;
        CrsAm150Units = _loc.AmplitudeModulation.CurrentUnit.Value.Unit;

        CrsFrequencyTitle = "";
        CrsFrequencyUnits = _freqInKHzMeasureUnit?.Unit;
        
        // CLR
        ClrPowerTitle = RS.IlsSdrRttViewModel_CLRPower_Title;
        ClrPowerUnits = _loc.Power.CurrentUnit.Value.Unit;

        ClrDdmTitle = RS.IlsSdrRttViewModel_DDM_Title;
        ClrDdmUnits = _loc.DdmGp.CurrentUnit.Value.Unit;


        ClrSdmTitle = RS.IlsSdrRttViewModel_SDM_Title;
        ClrSdmUnits = _loc.Sdm.CurrentUnit.Value.Unit;
        
        ClrAm90Title = RS.IlsSdrRttViewModel_AM_90_Hz_Title;
        ClrAm90Units = _loc.AmplitudeModulation.CurrentUnit.Value.Unit;

        ClrAm150Title = RS.IlsSdrRttViewModel_AM_150_Hz_Title;
        ClrAm150Units = _loc.AmplitudeModulation.CurrentUnit.Value.Unit;

        ClrFrequencyTitle = "";
        ClrFrequencyUnits = _freqInKHzMeasureUnit?.Unit;
        
        _loc.DdmGp.CurrentUnit.Subscribe(_ =>
        {
            TotalDdmUnits = _.Unit;
            CrsDdmUnits = _.Unit;
            ClrDdmUnits = _.Unit;
        }).DisposeItWith(Disposable);

        _loc.Sdm.CurrentUnit.Subscribe(_ =>
        {
            TotalSdmUnits = _.Unit;
            CrsSdmUnits = _.Unit;
            ClrSdmUnits = _.Unit;
        }).DisposeItWith(Disposable);
        
        _loc.Power.CurrentUnit.Subscribe(_ =>
        {
            TotalPowerUnits = _.Unit;
            CrsPowerUnits = _.Unit;
            ClrPowerUnits = _.Unit;
        }).DisposeItWith(Disposable);
        
        _loc.AmplitudeModulation.CurrentUnit.Subscribe(_ =>
        {
            TotalAm90Units = _.Unit;
            TotalAm150Units = _.Unit;
            CrsAm90Units = _.Unit;
            CrsAm150Units = _.Unit;
            ClrAm90Units = _.Unit;
            ClrAm150Units = _.Unit;
        }).DisposeItWith(Disposable);
        
        _loc.Phase.CurrentUnit.Subscribe(_ =>
        {
            Phi90Units = _.Unit;
            Phi150Units = _.Unit;
        }).DisposeItWith(Disposable);
        
        payload.Sdr.Base.OnRecordData.Where(_ => _.MessageId == AsvSdrRecordDataGpPacket.PacketMessageId)
            .Cast<AsvSdrRecordDataGpPacket>()
            .Subscribe(_=>
            {
                TotalPowerValue = _.Payload.TotalPower;
                TotalPowerStringValue = loc.Power.FromSiToString(_.Payload.TotalPower);
                TotalDdmStringValue = _loc.DdmGp.FromSiToString(_.Payload.TotalAm150 - _.Payload.TotalAm90);
                TotalSdmStringValue = _loc.Sdm.FromSiToString((_.Payload.TotalAm90 + _.Payload.TotalAm150) * 100.0);
                TotalAm90StringValue = _loc.AmplitudeModulation.FromSiToString(_.Payload.TotalAm90 * 100.0);
                TotalAm150StringValue = _loc.AmplitudeModulation.FromSiToString(_.Payload.TotalAm150 * 100.0);
                Phi90StringValue = _loc.Phase.FromSiToString(_.Payload.Phi90CrsVsClr);
                Phi150StringValue = _loc.Phase.FromSiToString(_.Payload.Phi150CrsVsClr);
                TotalFreq90StringValue = _freqInHzMeasureUnit?.FromSiToString(_.Payload.TotalFreq90);
                TotalFreq150StringValue = _freqInHzMeasureUnit?.FromSiToString(_.Payload.TotalFreq150);

                CrsPowerValue = _.Payload.CrsPower;
                CrsPowerStringValue = loc.Power.FromSiToString(_.Payload.CrsPower);
                CrsDdmStringValue = _loc.DdmGp.FromSiToString(_.Payload.CrsAm150 - _.Payload.CrsAm90);
                CrsSdmStringValue = _loc.Sdm.FromSiToString((_.Payload.CrsAm90 + _.Payload.CrsAm150) * 100.0);
                CrsAm90StringValue = _loc.AmplitudeModulation.FromSiToString(_.Payload.CrsAm90 * 100.0);
                CrsAm150StringValue = _loc.AmplitudeModulation.FromSiToString(_.Payload.CrsAm150 * 100.0);
                CrsFrequencyStringValue = _freqInKHzMeasureUnit?.FromSiToString(_.Payload.CrsCarrierOffset);

                ClrPowerValue = _.Payload.ClrPower;
                ClrPowerStringValue = loc.Power.FromSiToString(_.Payload.ClrPower);
                ClrDdmStringValue = _loc.DdmGp.FromSiToString(_.Payload.ClrAm150 - _.Payload.ClrAm90);
                ClrSdmStringValue = _loc.Sdm.FromSiToString((_.Payload.ClrAm90 + _.Payload.ClrAm150) * 100.0);
                ClrAm90StringValue = _loc.AmplitudeModulation.FromSiToString(_.Payload.ClrAm90 * 100.0);
                ClrAm150StringValue = _loc.AmplitudeModulation.FromSiToString(_.Payload.ClrAm150 * 100.0);
                ClrFrequencyStringValue = _freqInKHzMeasureUnit?.FromSiToString(_.Payload.ClrCarrierOffset);
            })
            .DisposeItWith(Disposable);
    }
    
     #region Total Power

    [Reactive]
    public string TotalPowerTitle { get; set; }
    
    [Reactive]
    public double TotalPowerValue { get; set; }
    
    [Reactive]
    public string TotalPowerStringValue { get; set; }
    
    [Reactive]
    public string TotalPowerUnits { get; set; }
    
    #endregion

    #region Total Ddm

    [Reactive]
    public string TotalDdmTitle { get; set; }
    
    [Reactive]
    public string TotalDdmStringValue { get; set; }
    
    [Reactive]
    public string TotalDdmUnits { get; set; }

    #endregion

    #region Total Sdm

    [Reactive]
    public string TotalSdmTitle { get; set; }
    
    [Reactive]
    public string TotalSdmStringValue { get; set; }
    
    [Reactive]
    public string TotalSdmUnits { get; set; }
    
    
    #endregion

    #region Total Am90

    [Reactive]
    public string TotalAm90Title { get; set; }
    
    [Reactive]
    public string TotalAm90StringValue { get; set; }
    
    [Reactive]
    public string TotalAm90Units { get; set; }
    
    #endregion

    #region Total Am150

    [Reactive]
    public string TotalAm150Title { get; set; }
    
    [Reactive]
    public string TotalAm150StringValue { get; set; }
    
    [Reactive]
    public string TotalAm150Units { get; set; }
    
    #endregion

    #region Phi 90

    [Reactive]
    public string Phi90Title { get; set; }
    
    [Reactive]
    public string Phi90StringValue { get; set; }
    
    [Reactive]
    public string Phi90Units { get; set; }
    
    #endregion

    #region Phi 150

    [Reactive]
    public string Phi150Title { get; set; }
    
    [Reactive]
    public string Phi150StringValue { get; set; }
    
    [Reactive]
    public string Phi150Units { get; set; }
    
    #endregion
    

    #region Total Freq 90

    [Reactive]
    public string TotalFreq90Title { get; set; }
    
    [Reactive]
    public string TotalFreq90StringValue { get; set; }
    
    [Reactive]
    public string TotalFreq90Units { get; set; }
    
    #endregion

    #region Total Freq 150

    [Reactive]
    public string TotalFreq150Title { get; set; }
    
    [Reactive]
    public string TotalFreq150StringValue { get; set; }
    
    [Reactive]
    public string TotalFreq150Units { get; set; }
    
    #endregion
    
    
    // CRS
    
    #region Crs Power

    [Reactive]
    public string CrsPowerTitle { get; set; }
    
    [Reactive]
    public double CrsPowerValue { get; set; }
    
    [Reactive]
    public string CrsPowerStringValue { get; set; }
    
    [Reactive]
    public string CrsPowerUnits { get; set; }

    #endregion

    #region Crs Ddm

    [Reactive]
    public string CrsDdmTitle { get; set; }
    
    [Reactive]
    public string CrsDdmStringValue { get; set; }
    
    [Reactive]
    public string CrsDdmUnits { get; set; }
    
    #endregion

    #region Crs Sdm

    [Reactive]
    public string CrsSdmTitle { get; set; }
    
    [Reactive]
    public string CrsSdmStringValue { get; set; }
    
    [Reactive]
    public string CrsSdmUnits { get; set; }
    
    #endregion

    #region Crs Am90

    [Reactive]
    public string CrsAm90Title { get; set; }
    
    [Reactive]
    public string CrsAm90StringValue { get; set; }
    
    [Reactive]
    public string CrsAm90Units { get; set; }
    
    #endregion

    #region Crs Am150

    [Reactive]
    public string CrsAm150Title { get; set; }
    
    [Reactive]
    public string CrsAm150StringValue { get; set; }
    
    [Reactive]
    public string CrsAm150Units { get; set; }
    
    #endregion
    
    #region Crs Frequency

    [Reactive]
    public string CrsFrequencyTitle { get; set; }
    
    [Reactive]
    public string CrsFrequencyStringValue { get; set; }
    
    [Reactive]
    public string CrsFrequencyUnits { get; set; }
    
    #endregion
    
    
    // CLR
    
    #region Clr Power

    [Reactive]
    public string ClrPowerTitle { get; set; }
    
    [Reactive]
    public double ClrPowerValue { get; set; }
    
    [Reactive]
    public string ClrPowerStringValue { get; set; }
    
    [Reactive]
    public string ClrPowerUnits { get; set; }

    #endregion

    #region Clr Ddm

    [Reactive]
    public string ClrDdmTitle { get; set; }
    
    [Reactive]
    public string ClrDdmStringValue { get; set; }
    
    [Reactive]
    public string ClrDdmUnits { get; set; }

    #endregion

    #region Clr Sdm

    [Reactive]
    public string ClrSdmTitle { get; set; }
    
    [Reactive]
    public string ClrSdmStringValue { get; set; }
    
    [Reactive]
    public string ClrSdmUnits { get; set; }
    
    #endregion

    #region Clr Am90

    [Reactive]
    public string ClrAm90Title { get; set; }
    
    [Reactive]
    public string ClrAm90StringValue { get; set; }
    
    [Reactive]
    public string ClrAm90Units { get; set; }
    
    #endregion

    #region Clr Am150

    [Reactive]
    public string ClrAm150Title { get; set; }
    
    [Reactive]
    public string ClrAm150StringValue { get; set; }
    
    [Reactive]
    public string ClrAm150Units { get; set; }
    
    #endregion
    
    #region Clr Frequency

    [Reactive]
    public string ClrFrequencyTitle { get; set; }
    
    [Reactive]
    public string ClrFrequencyStringValue { get; set; }
    
    [Reactive]
    public string ClrFrequencyUnits { get; set; }
    
    #endregion
}