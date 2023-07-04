using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Drones.Uav;
using Asv.Mavlink;
using Asv.Mavlink.V2.Common;
using FluentAvalonia.UI.Controls;
using Material.Icons;

namespace Asv.Drones.Gui.Uav;

public class RebootAutopilotAnchorActionViewModel : UavActionActionBase
{
    private readonly ILogService _log;
    
    public RebootAutopilotAnchorActionViewModel(IVehicleClient vehicle, IMap map, ILogService log) : base(vehicle, map, log)
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

        var viewModel = new RebootAutopilotViewModel();
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