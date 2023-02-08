using Asv.Cfg;

namespace Asv.Drones.Gui.Core
{
    public class ServiceWithConfigBase<TConfig>:DisposableViewModelBase 
        where TConfig : new()
    {
        private readonly IConfiguration _cfgService;
        private readonly object _sync = new();
        private readonly TConfig _config;

        public ServiceWithConfigBase(IConfiguration cfgService)
        {
            _cfgService = cfgService ?? throw new ArgumentNullException(nameof(cfgService));
            _config = cfgService.Get<TConfig>();
        }

        protected TConfigValue InternalGetConfig<TConfigValue>(Func<TConfig, TConfigValue> getProperty)
        {
            lock (_sync)
            {
                return getProperty(_config);
            }
        }

        protected void InternalSaveConfig(Action<TConfig> changeCallback)
        {
            lock (_sync)
            {
                changeCallback(_config);
                _cfgService.Set(_config);
            }
        }
    }
}