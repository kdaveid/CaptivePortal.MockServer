// Copyright (c) David E. Keller. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;

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
