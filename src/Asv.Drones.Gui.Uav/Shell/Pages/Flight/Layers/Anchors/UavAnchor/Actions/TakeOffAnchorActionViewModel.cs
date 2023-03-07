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
        private readonly ITakeOffService _takeOffService;

        public TakeOffAnchorActionViewModel(IVehicle vehicle, IMap map, ILogService log, ITakeOffService takeOffService) : base(vehicle, map, log)
        {
            _log = log;
            _takeOffService = takeOffService;
            
            Title = "TakeOff";
            Icon = MaterialIconKind.ArrowUpBoldHexagonOutline;
            
            Command = ReactiveCommand.CreateFromTask(ExecuteImpl, CanExecute);
            Vehicle.IsArmed.ObserveOn(RxApp.MainThreadScheduler).Select(_ => !_).Subscribe(CanExecute).DisposeWith(Disposable);
        }

        protected override async Task ExecuteImpl(CancellationToken cancel)
        {
            Map.IsInDialogMode = true;

            var dialog = new ContentDialog()
            {
                Title = RS.TakeOffAnchorActionViewModel_Title,
                PrimaryButtonText = RS.TakeOffAnchorActionViewModel_DialogPrimaryButton,
                IsSecondaryButtonEnabled = true,
                SecondaryButtonText = RS.TakeOffAnchorActionViewModel_DialogSecondaryButton
            };
            
            var viewModel = new TakeOffViewModel(_takeOffService);
            viewModel.ApplyDialog(dialog);
            dialog.Content = viewModel;
            
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                _log.Info(LogName, string.Format(RS.TakeOffAnchorActionViewModel_LogMessage, Vehicle.Name.Value));
                await Vehicle.TakeOff(viewModel.Altitude.Value, cancel);
            }

            Map.IsInDialogMode = false;
        }
    }
}