using System.ComponentModel.Composition;
using Asv.Cfg;
using Avalonia.Controls;

namespace Asv.Drones.Gui.Core
{
    public class PlaningPageViewConfig
    {
        public string Columns { get; set; }
    }
    
    [ExportView(typeof(PlaningPageViewModel))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class PlaningPageView:MapPageView
    {
        private IConfiguration _configuration;
        public PlaningPageView()
        {
        }

        [ImportingConstructor]
        public PlaningPageView(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        protected override void OnLoaded()
        {
            base.OnLoaded();

            if (_configuration.Exist<PlaningPageViewConfig>(nameof(PlaningPageViewConfig)))
            {
                var columns = _configuration.Get<PlaningPageViewConfig>(nameof(PlaningPageViewConfig));
                
                SetColumnDefinitions(((Grid)this.Content).ColumnDefinitions, columns.Columns);
            }
        }

        protected override void OnUnloaded()
        {
            base.OnUnloaded();

            var flightPageViewConfig = new PlaningPageViewConfig
            {
                Columns = ((Grid)this.Content).ColumnDefinitions.ToString()
            };

            _configuration.Set(nameof(PlaningPageViewConfig), flightPageViewConfig);
        }
    }
}