using System.Collections.Generic;
using Asv.Mavlink;

namespace Asv.Drones;

public class NullDroneFrame : IDroneFrame
{
    public static IDroneFrame Instance { get; } = new NullDroneFrame { Id = "frame-id-1" };

    public required string Id { get; init; }
    public IReadOnlyDictionary<string, string>? Meta { get; init; } =
        new Dictionary<string, string> { ["meta1"] = "val1", ["meta2"] = "val2" };
}
