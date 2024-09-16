using System;
using System.Collections.Generic;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui;

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

public class RebootAutopilotViewModel : DisposableReactiveObject
{
    [Reactive] public AutopilotRebootShutdownWithDescription SelectedAutopilotRebootShutdown { get; set; }

    [Reactive] public CompanionRebootShutdownWithDescription SelectedCompanionRebootShutdown { get; set; }

    [Reactive] public List<AutopilotRebootShutdownWithDescription> AutopilotValues { get; set; } = new();
    [Reactive] public List<CompanionRebootShutdownWithDescription> CompanionValues { get; set; } = new();

    public RebootAutopilotViewModel()
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