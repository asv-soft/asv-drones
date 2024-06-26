using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using Material.Icons;

namespace Asv.Drones.Gui;

public class UavActionSelectMode : UavActionBase
{
    private readonly ILogService _log;
    private string _initialModeName;

    public UavActionSelectMode(IVehicleClient vehicle, IMap map, ILogService log) : base(vehicle, map, log)
    {
        _log = log;
        vehicle.CurrentMode.Subscribe(_ => _initialModeName = _.Name).DisposeItWith(Disposable);
        Title = RS.SelectModeAnchorActionViewModel_Title;
        Icon = MaterialIconKind.ModeEdit;
        Vehicle.Position.IsArmed.Select(_ => _).Subscribe(CanExecute).DisposeWith(Disposable);
    }

    protected override async Task ExecuteImpl(CancellationToken cancel)
    {
        var selectedItem = Map.SelectedItem;

        Map.SelectedItem = null;


        var dialog = await Dispatcher.UIThread.InvokeAsync(() => new ContentDialog()
        {
            Title = RS.SelectModeAnchorActionViewModel_Title,
            PrimaryButtonText = RS.SelectModeAnchorActionViewModel_DialogPrimaryButton,
            IsSecondaryButtonEnabled = true,
            SecondaryButtonText = RS.SelectModeAnchorActionViewModel_DialogSecondaryButton
        });

        using var viewModel = new SelectModeViewModel(Vehicle);
        dialog.Content = viewModel;

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            _log.Info(LogName,
                string.Format(RS.SelectModeAnchorActionViewModel_LogMessage, _initialModeName,
                    viewModel.SelectedMode.Mode.Name));
            await Vehicle.SetVehicleMode(viewModel.SelectedMode.Mode, cancel);
        }

        Map.SelectedItem = selectedItem;
    }
}