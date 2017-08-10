// Copyright (c) David E. Keller. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Dkbe.CaptivePortal.MockServer.Models
{
    public class AppSettings
    {
        public string Name { get; set; }

        /// <summary>
        /// This is for the creation of "mgmtBaseUrl" part of <see cref="Dkbe.CaptivePortal.Models.SonicOS.SNWLExternalAuthenticationRedirectModel"/> for signin. See Tech Note for more info.
        /// </summary>
        public string ManagementBaseUrl { get; set; }
    }
}
