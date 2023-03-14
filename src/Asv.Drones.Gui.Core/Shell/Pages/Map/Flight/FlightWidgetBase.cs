namespace Asv.Drones.Gui.Core
{
    public abstract class FlightWidgetBase:MapWidgetBase
    {
        public const string UriString = PlaningPageViewModel.UriString + ".flight";
        public static readonly Uri Uri = new(UriString);
        
        protected FlightWidgetBase(Uri id) : base(id)
        {
            
        }
    }
}