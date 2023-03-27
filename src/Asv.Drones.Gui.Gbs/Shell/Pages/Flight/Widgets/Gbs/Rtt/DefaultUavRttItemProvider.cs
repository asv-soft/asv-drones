﻿using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Asv.Drones.Gui.Uav;
using Asv.Mavlink;

namespace Asv.Drones.Gui.Gbs;

[Export(typeof(IGbsRttItemProvider))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class DefaultGbsRttItemProvider : IGbsRttItemProvider
{
    private readonly ILocalizationService _localizationService;
    
    [ImportingConstructor]
    public DefaultGbsRttItemProvider(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }
    
    public IEnumerable<IGbsRttItem> Create(IGbsDevice device)
    {
        yield break;
    }
}