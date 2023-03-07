using System.ComponentModel.Composition;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Asv.Drones.Gui.Core;
using Asv.Drones.Uav;
using Asv.Mavlink;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using Material.Icons;
using ReactiveUI;

namespace Asv.Drones.Gui.Uav
{
    public class TakeOffAnchorActionViewModel : UavActionActionBase
    {
        private readonly ILogService _log;
        
        public TakeOffAnchorActionViewModel(IVehicle vehicle, IMap map, ILogService log) : base(vehicle, map, log)
        {
            _log = log;
            // TODO: Localize
            Title = "Take off";
            Icon = MaterialIconKind.ArrowUpBoldHexagonOutline;
            
            Command = ReactiveCommand.CreateFromTask(ExecuteImpl, CanExecute);
            Vehicle.IsArmed.ObserveOn(RxApp.MainThreadScheduler).Select(_ => !_).Subscribe(CanExecute).DisposeWith(Disposable);
        }

        protected override async Task ExecuteImpl(CancellationToken cancel)
        {
            Map.IsInDialogMode = true;

            var dialog = new ContentDialog()
            {
                Title = Title,
                PrimaryButtonText = "Set",
                IsSecondaryButtonEnabled = true,
                SecondaryButtonText = "Cancel"
            };
            
            var viewModel = new TakeOffViewModel();
            viewModel.ApplyDialog(dialog);
            dialog.Content = viewModel;
            


            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                _log.Info(LogName, $"User send TakeOff for {Vehicle.Name.Value}");
                await Vehicle.TakeOff(viewModel.Altitude.Value, cancel);
            }

            Map.IsInDialogMode = false;
        }
    }
}