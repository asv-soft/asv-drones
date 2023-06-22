using System.Collections.ObjectModel;
using Asv.Cfg;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Avalonia.Controls;
using Material.Icons;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Sdr;

public class LlzFlightSdrViewModel : FlightSdrWidgetBase
{
    private readonly ILogService _logService;
    private readonly ILocalizationService _loc;
    private readonly IConfiguration _configuration;
    
    public static Uri GenerateUri(ISdrClientDevice sdr) => FlightSdrWidgetBase.GenerateUri(sdr,"sdr");

    public LlzFlightSdrViewModel()
    {
        if (Design.IsDesignMode)
        {
            Icon = MaterialIconKind.Memory;

            TotalPower = new SdrRttItemStringParamViewModelDesignMode();
            TotalDdm = new SdrRttItemStringParamViewModelDesignMode();
            TotalSdm = new SdrRttItemStringParamViewModelDesignMode();
            TotalAm90 = new SdrRttItemStringParamViewModelDesignMode();
            TotalAm150 = new SdrRttItemStringParamViewModelDesignMode();
            Phi90 = new SdrRttItemStringParamViewModelDesignMode();
            Phi150 = new SdrRttItemStringParamViewModelDesignMode();
            IdCode = new SdrRttItemStringParamViewModelDesignMode();
            
            CrsPower = new SdrRttItemStringParamViewModelDesignMode();
            CrsDdm = new SdrRttItemStringParamViewModelDesignMode();
            CrsSdm = new SdrRttItemStringParamViewModelDesignMode();
            CrsAm90 = new SdrRttItemStringParamViewModelDesignMode();
            CrsAm150 = new SdrRttItemStringParamViewModelDesignMode();
            CrsFrequency = new SdrRttItemStringParamViewModelDesignMode();
            
            ClrPower = new SdrRttItemStringParamViewModelDesignMode();
            ClrDdm = new SdrRttItemStringParamViewModelDesignMode();
            ClrSdm = new SdrRttItemStringParamViewModelDesignMode();
            ClrAm90 = new SdrRttItemStringParamViewModelDesignMode();
            ClrAm150 = new SdrRttItemStringParamViewModelDesignMode();
            ClrFrequency = new SdrRttItemStringParamViewModelDesignMode();
        }
    }

    public LlzFlightSdrViewModel(ISdrClientDevice payload, ILogService log, ILocalizationService loc, IConfiguration configuration)
        :base(payload, GenerateUri(payload))
    {
        _logService = log ?? throw new ArgumentNullException(nameof(log));
        _loc = loc ?? throw new ArgumentNullException(nameof(loc));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        
        Icon = MaterialIconKind.Memory;
        Title = "Payload";
        Location = WidgetLocation.Right;
        
    }

    [Reactive]
    public SdrRttItemStringParamViewModel TotalPower { get; set; }
    
    [Reactive]
    public SdrRttItemStringParamViewModel TotalDdm { get; set; }
    
    [Reactive]
    public SdrRttItemStringParamViewModel TotalSdm { get; set; }
    
    [Reactive]
    public SdrRttItemStringParamViewModel TotalAm90 { get; set; }
    
    [Reactive]
    public SdrRttItemStringParamViewModel TotalAm150 { get; set; }
    
    [Reactive]
    public SdrRttItemStringParamViewModel Phi90 { get; set; }
    
    [Reactive]
    public SdrRttItemStringParamViewModel Phi150 { get; set; }
    
    [Reactive]
    public SdrRttItemStringParamViewModel IdCode { get; set; }
    
    // CRS
    [Reactive]
    public SdrRttItemStringParamViewModel CrsPower { get; set; }
    
    [Reactive]
    public SdrRttItemStringParamViewModel CrsDdm { get; set; }
    
    [Reactive]
    public SdrRttItemStringParamViewModel CrsSdm { get; set; }
    
    [Reactive]
    public SdrRttItemStringParamViewModel CrsAm90 { get; set; }
    
    [Reactive]
    public SdrRttItemStringParamViewModel CrsAm150 { get; set; }
    
    [Reactive]
    public SdrRttItemStringParamViewModel CrsFrequency { get; set; }
    
    
    // CLR
    [Reactive]
    public SdrRttItemStringParamViewModel ClrPower { get; set; }
    
    [Reactive]
    public SdrRttItemStringParamViewModel ClrDdm { get; set; }
    
    [Reactive]
    public SdrRttItemStringParamViewModel ClrSdm { get; set; }
    
    [Reactive]
    public SdrRttItemStringParamViewModel ClrAm90 { get; set; }
    
    [Reactive]
    public SdrRttItemStringParamViewModel ClrAm150 { get; set; }
    
    [Reactive]
    public SdrRttItemStringParamViewModel ClrFrequency { get; set; }
}