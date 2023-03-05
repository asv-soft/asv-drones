namespace Asv.Drones.Gui.Core
{
    /// <summary>
    /// All pages in shell must implement this interface
    /// </summary>
    public interface IShellPage : IViewModel
    {
        /// <summary>
        /// Addition arguments for page
        /// </summary>
        /// <param name="link"></param>
        void SetArgs(Uri link);
    }

    public class ShellPage : ViewModelBase,IShellPage
    {
        public const string UriString = ShellViewModel.UriString + ".page";
        public static readonly Uri Uri = new(UriString);
        protected ShellPage(Uri absoluteUri) : base(absoluteUri)
        {
            
        }

        public virtual void SetArgs(Uri link)
        {
            
        }
    }
}