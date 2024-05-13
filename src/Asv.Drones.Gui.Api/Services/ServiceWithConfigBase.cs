using Asv.Cfg;

namespace Asv.Drones.Gui.Api
{
    public class ServiceWithConfigBase<TConfig> : DisposableReactiveObject
        where TConfig : new()
    {
        private readonly IConfiguration _cfgService;
        private readonly object _sync = new();
        private readonly TConfig _config;

        protected ServiceWithConfigBase(IConfiguration cfg)
        {
            _cfgService = cfg ?? throw new ArgumentNullException(nameof(cfg));
            _config = cfg.Get<TConfig>();
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