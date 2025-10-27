using System.Collections.Generic;
using Asv.Mavlink;

namespace Asv.Drones;

public class FakeMotorFrame : IMotorFrame
{
    public required string Id { get; init; }
    public IReadOnlyDictionary<string, string>? Meta { get; init; } =
        new Dictionary<string, string> { ["meta1"] = "val1", ["meta2"] = "val2" };
}
