using System;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;

namespace Asv.Drones.Gui
{
    public class FlightAnchorBase : MapAnchorBase
    {
        public FlightAnchorBase(IVehicleClient vehicle, string name) : base(
            $"{WellKnownUri.ShellPageMapFlightAnchor}/{vehicle.FullId}/{name}")
        {
            Vehicle = vehicle;
        }

        public IVehicleClient Vehicle { get; }
    }
}