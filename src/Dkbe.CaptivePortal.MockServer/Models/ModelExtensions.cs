// Copyright (c) David E. Keller. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Dkbe.CaptivePortal.Models.SonicOS;

namespace Dkbe.CaptivePortal.MockServer.Models
{
    public static class ModelExtensions
    {
        public static FakeSNWLSession Map(this SNWLExternalAuthenticationRedirectModel requestModel, StaticZone zone)
        {
            return new FakeSNWLSession(zone)
            {
                ID = requestModel.SessionId,
                IP = requestModel.IP,
                MAC = requestModel.MAC,
                BaseMgmtUrl = requestModel.MgmtBaseUrl,
                Ssid = requestModel.SSID
            };
        }
    }
}
