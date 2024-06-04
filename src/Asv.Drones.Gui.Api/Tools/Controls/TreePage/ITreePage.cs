using System.Collections.ObjectModel;

namespace Asv.Drones.Gui.Api;

public interface ITreePage : IViewModel
{
    ReadOnlyObservableCollection<IMenuItem>? Actions { get; }
    Task<bool> TryClose();
    
}

public class TreePageViewModel : ViewModelBase, ITreePage
{
    protected TreePageViewModel(Uri id) : base(id)
    {
    }

    protected TreePageViewModel(string id) : base(id)
    {
    }

    public ReadOnlyObservableCollection<IMenuItem>? Actions { get; set; }
    public virtual Task<bool> TryClose()
    {
        return Task.FromResult(true);
    }
}

public class TreePageWithValidationViewModel : ViewModelBaseWithValidation, ITreePage
{
    protected TreePageWithValidationViewModel(Uri id) : base(id)
    {
    }

    protected TreePageWithValidationViewModel(string id) : base(id)
    {
    }

    public ReadOnlyObservableCollection<IMenuItem>? Actions { get; set; }
    
    public virtual Task<bool> TryClose()
    {
        return Task.FromResult(true);
    }
}