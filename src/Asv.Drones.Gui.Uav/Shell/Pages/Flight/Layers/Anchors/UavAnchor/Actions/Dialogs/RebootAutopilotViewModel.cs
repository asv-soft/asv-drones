using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav;

public class AutopilotRebootShutdownWithDescription
{
    public AutopilotRebootShutdown Value { get; init; }
    public string Description => Value.GetAutopilotRebootShutdownDescription();
}

public class CompanionRebootShutdownWithDescription
{
    public CompanionRebootShutdown Value { get; init; }
    public string Description => Value.GetCompanionRebootShutdownDescription();
}

[ExportShellPage(UriString)]
[PartCreationPolicy(CreationPolicy.Shared)]
public class RebootAutopilotViewModel : ViewModelBase
{
    private const string UriString = UavAnchor.BaseUriString + ".actions.reboot-ap";

    [Reactive]
    public AutopilotRebootShutdownWithDescription SelectedAutopilotRebootShutdown { get; set; }

    [Reactive]
    public CompanionRebootShutdownWithDescription SelectedCompanionRebootShutdown { get; set; }

    [Reactive] 
    public List<AutopilotRebootShutdownWithDescription> AutopilotValues { get; set; } = new();
    [Reactive] 
    public List<CompanionRebootShutdownWithDescription> CompanionValues { get; set; } = new();
    
    public RebootAutopilotViewModel() : base(new Uri(UriString))
    {
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
    }
}