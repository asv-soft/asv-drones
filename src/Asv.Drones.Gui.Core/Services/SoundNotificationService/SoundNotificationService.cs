using System.ComponentModel.Composition;
using Asv.Cfg;

namespace Asv.Drones.Gui.Core;

public class SoundNotificationServiceConfig
{
    
}

[Export(typeof(ISoundNotificationService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class SoundNotificationService : ServiceWithConfigBase<SoundNotificationServiceConfig>, ISoundNotificationService
{
    [ImportingConstructor]
    public SoundNotificationService(IConfiguration cfgService) : base(cfgService)
    {
    }

    public void Notify()
    {
        Console.Beep();
    }
}