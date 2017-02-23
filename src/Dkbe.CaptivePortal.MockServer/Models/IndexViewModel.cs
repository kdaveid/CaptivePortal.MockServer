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

        public IEnumerable<FakeSNWLSession> Sessions { get; set; }

        public bool HasSessions { get { return Sessions.Any(); } }

        public int CurrentLoginReplyCode { get; set; }

        public int CurrentUpdateSessionReplyCode { get; set; }

        public int CurrentLogoffReplyCode { get; set; }
    }
}
