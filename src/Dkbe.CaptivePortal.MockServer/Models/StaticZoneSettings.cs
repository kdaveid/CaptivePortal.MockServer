using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dkbe.CaptivePortal.MockServer.Models
{
    /// <summary>
    /// For loading static zone settings from appsettings.json file.
    /// </summary>
    public class StaticZoneSettings
    {
        public List<StaticZone> Zones { get; set; }
    }

    /// <summary>
    /// Zone imitates <see cref="Dkbe.CaptivePortal.Models.Zone"/> with just two necessary properties.
    /// </summary>
    public class StaticZone
    {
        public string Name { get; set; }

        public string LocalPath { get; set; }

        public string SNWLRedirectEndpointURL { get; set; }

    }
}
