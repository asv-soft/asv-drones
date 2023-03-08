namespace Asv.Drones.Gui.Core
{
    public interface ISettingsPart : IViewModel
    {
        int Order { get; }
        
        bool IsRebootRequired { get; }
    }
}