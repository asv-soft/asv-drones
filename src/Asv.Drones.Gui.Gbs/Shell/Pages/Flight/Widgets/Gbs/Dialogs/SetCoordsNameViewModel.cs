using Asv.Drones.Gui.Core;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;

namespace Asv.Drones.Gui.Gbs;

public class SetCoordsNameViewModel : ViewModelBaseWithValidation
{
    [Reactive] public string Name { get; set; }

    public SetCoordsNameViewModel() : base(new Uri(FlightGbsViewModel.Uri, "name"))
    {
        this.ValidationRule(x => x.Name, _ => !string.IsNullOrWhiteSpace(_), RS.SetCoordsNameViewModel_Name_ValidValue);
    }
}