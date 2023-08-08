namespace Asv.Drones.Gui.Core
{
    public abstract class ShellStatusItem : ViewModelBase, IShellStatusItem
    {
        protected ShellStatusItem(Uri id) : base(id)
        {
            
        }
        protected ShellStatusItem(string id) : base(id)
        {
            
        }

        public abstract int Order { get; }
    }
}