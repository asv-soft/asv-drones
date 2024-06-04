namespace Asv.Drones.Gui.Api;

public interface ITreePageExplorer
{
    Task<bool> GoTo(Uri pageId);
    ITreePageMenuItem? SelectedMenu { get; }
    ITreePage? CurrentPage { get; }
    bool IsCompactMode { get; set; }
}