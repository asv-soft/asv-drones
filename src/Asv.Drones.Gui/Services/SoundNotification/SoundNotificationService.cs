using System;
using System.Composition;
using Asv.Cfg;
using Asv.Drones.Gui.Api;

namespace Asv.Drones.Gui;

public class SoundNotificationServiceConfig
{
}

[Export(typeof(ISoundNotificationService))]
[Shared]
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