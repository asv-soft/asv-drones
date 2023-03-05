namespace Asv.Drones.Gui.Core
{
    public abstract class ShellStatusItem : ViewModelBase, IShellStatusItem
    {
        public static readonly Uri Uri = new(ShellViewModel.Uri, "status");
        protected ShellStatusItem(Uri id) : base(id)
        {
            
        }

        public abstract int Order { get; }
    }
}