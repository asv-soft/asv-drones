namespace Asv.Drones.Gui.Core
{
    public abstract class PlaningWidgetBase : MapWidgetBase
    {
        public const string UriString = PlaningPageViewModel.UriString + ".widget";
        public static readonly Uri Uri = new(UriString);
        
        protected PlaningWidgetBase(Uri id) : base(id)
        {
            
        }
    }
}