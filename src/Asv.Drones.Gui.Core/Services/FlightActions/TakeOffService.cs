using System.ComponentModel.Composition;
using System.Globalization;
using Asv.Cfg;
using Asv.Common;
using Avalonia.Controls.Mixins;

namespace Asv.Drones.Gui.Core
{
    public class TakeOffServiceConfig
    {
        public double? CurrentAltitude { get; set; }

    }
    
    [Export(typeof(ITakeOffService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TakeOffService : ServiceWithConfigBase<TakeOffServiceConfig>, ITakeOffService
    {
        private readonly double _currentAltitude;

        [ImportingConstructor]
        public TakeOffService(IConfiguration cfgSvc) : base(cfgSvc)
        {
            double? suggestedAltitude = 30;
            var altitudeFromConfig = InternalGetConfig(_ => _.CurrentAltitude);

            if (altitudeFromConfig != null && altitudeFromConfig != 0)
            {
                suggestedAltitude = altitudeFromConfig;
            }

            CurrentAltitude = new RxValue<double?>(suggestedAltitude).DisposeItWith(Disposable);
            CurrentAltitude.Subscribe(SetAltitude).DisposeWith(Disposable);
        }

        public IRxEditableValue<double?> CurrentAltitude { get; }
        
        private void SetAltitude(double? altitude)
        {
            if (altitude == null) return;
            InternalSaveConfig(_ => _.CurrentAltitude = altitude);
        }

    }
}