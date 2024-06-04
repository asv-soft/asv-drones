using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using FluentAvalonia.UI.Controls;
using Material.Icons;

namespace Asv.Drones.Gui;

public class UavActionRebootAutopilot : UavActionBase
{
    private readonly ILogService _log;

    public UavActionRebootAutopilot(IVehicleClient vehicle, IMap map, ILogService log) : base(vehicle, map, log)
    {
        Title = RS.RebootAutopilotAnchorActionViewModel_Title;
        Icon = MaterialIconKind.Automatic;
        Vehicle.Position.IsArmed.Select(_ => !_).Subscribe(CanExecute).DisposeItWith(Disposable);
    }

    protected override async Task ExecuteImpl(CancellationToken cancel)
    {
        var selectedItem = Map.SelectedItem;

        Map.SelectedItem = null;

        var dialog = new ContentDialog()
        {
            Title = RS.RebootAutopilotAnchorActionViewModel_Title,
            PrimaryButtonText = RS.RebootAutopilotAnchorActionViewModel_DialogPrimaryButton,
            IsSecondaryButtonEnabled = true,
            SecondaryButtonText = RS.RebootAutopilotAnchorActionViewModel_DialogSecondaryButton
        };

        using var viewModel = new RebootAutopilotViewModel();
        dialog.Content = viewModel;

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            _log.Info(LogName, string.Format(RS.RebootAutopilotAnchorActionViewModel_LogMessage,
                viewModel.SelectedAutopilotRebootShutdown, viewModel.SelectedCompanionRebootShutdown));

            await Vehicle.Commands.PreflightRebootShutdown(viewModel.SelectedAutopilotRebootShutdown.Value,
                viewModel.SelectedCompanionRebootShutdown.Value, cancel);
        }

        Map.SelectedItem = selectedItem;
    }
}