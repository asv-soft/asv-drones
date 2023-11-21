using System.ComponentModel.Composition;
using System.Windows.Input;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav;

[Export(typeof(IQuickParamsPart))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class ControllerReloadQuickParamViewModel : QuickParamsPartBase
{
    private static readonly Uri Uri = new(QuickParamsPartBase.Uri, "controllerreset");
    private readonly IVehicleClient _vehicle;
    private readonly ILogService _log;
    public override int Order => 2;

    public override bool IsRebootRequired => false;

    public override bool IsVisibleInAdvancedMode => false;

    [ImportingConstructor]
    public ControllerReloadQuickParamViewModel(IVehicleClient vehicle, ILogService log) : base(Uri)
    {
        _vehicle = vehicle;
        _log = log;
        foreach (var value in Enum.GetValues<AutopilotRebootShutdown>())
        {
            var item = new AutopilotRebootShutdownWithDescription() { Value = value };
            
            if (value == AutopilotRebootShutdown.RebootAutopilot)
            {
                SelectedAutopilotRebootShutdown = item;
            }
            
            AutopilotValues.Add(item);
        }
        
        foreach (var value in Enum.GetValues<CompanionRebootShutdown>())
        {
            var item = new CompanionRebootShutdownWithDescription() { Value = value };
            
            if (value == CompanionRebootShutdown.DoNothingForOnboardComputer)
            {
                SelectedCompanionRebootShutdown = item;
            }
            
            CompanionValues.Add(item);
        }

        Reload = ReactiveCommand.CreateFromTask(ReloadImpl);
    }

    private async Task ReloadImpl(CancellationToken token)
    {
        try
        {
            await _vehicle.Commands.PreflightRebootShutdown(SelectedAutopilotRebootShutdown.Value, SelectedCompanionRebootShutdown.Value);
        }
        catch (Exception e)
        {
            _log.Error("ControllerReloadQuickdParam", e.Message, e);
        }
    }
    
    public ICommand Reload { get; set; }
    
    [Reactive]
    public AutopilotRebootShutdownWithDescription SelectedAutopilotRebootShutdown { get; set; }

    [Reactive]
    public CompanionRebootShutdownWithDescription SelectedCompanionRebootShutdown { get; set; }

    [Reactive] 
    public List<AutopilotRebootShutdownWithDescription> AutopilotValues { get; set; } = new();
    [Reactive] 
    public List<CompanionRebootShutdownWithDescription> CompanionValues { get; set; } = new();
    
    public override async Task Write()
    {
        await Task.Delay(0);
    }

    public override async Task Read()
    {
        await Task.Delay(0);
    }
}