using Asv.Drones.Gui.Api;
using NuGet.Protocol.Core.Types;

namespace Asv.Drones.Gui;

public class SourceInfo : IPluginServerInfo
{
    private readonly SourceRepository _sourceRepository;

    public SourceInfo(SourceRepository sourceRepository)
    {
        _sourceRepository = sourceRepository;
    }

    public string SourceUri => _sourceRepository.PackageSource.Source;
    public string Name => _sourceRepository.PackageSource.Name;
    public string? Username => _sourceRepository.PackageSource.Credentials?.Username;
}