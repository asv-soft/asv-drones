namespace Asv.Drones.Gui.Core
{
    public abstract class SettingsPartBase : ViewModelBase, ISettingsPart
    {
        public const string UriString = SettingsViewModel.UriString + ".part";
        public static readonly Uri Uri = new(UriString);
        protected SettingsPartBase(Uri id) : base(id)
        {
            
        }
        public abstract int Order { get; }
    }
}