using Dkbe.CaptivePortal.Models.SonicOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dkbe.CaptivePortal.MockServer.Models
{
    public class IndexViewModel
    {
        public IEnumerable<StaticZone> StaticZones { get; set; }

        public IEnumerable<SNWLSession> Sessions { get; set; }

        public bool HasSessions { get { return Sessions.Any(); } }
    }
}
