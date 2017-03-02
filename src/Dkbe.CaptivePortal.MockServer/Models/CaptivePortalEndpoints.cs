// Copyright (c) David E. Keller. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Dkbe.CaptivePortal.MockServer.Models
{
    /// <summary>
    /// External Authentication Pages: These are the SNWL Guest Auth page settings on the zones.
    /// Together with the Capitive Portal URL these pages will build the uri to be invoked by the Mock Server
    /// </summary>
    public class CaptivePortalEndpoints
    {
        /// <summary>
        /// Login Page. Sonicwall redirects the client with SessionId etc. to this page.
        /// This setting should include a {zone} string identifier.
        /// Example: zone1/signin
        /// </summary>
        public string LoginPage { get; set; }

        /// <summary>
        /// Session Expiration Page. In case of a session expiration
        /// SNWL will redirect the user to this URL
        /// </summary>
        public string SessionExpirationPage { get; set; }

        /// <summary>
        /// As <see cref="SessionExpirationPage"/>
        /// </summary>
        public string IdleTimeOutPage { get; set; }

        /// <summary>
        /// As <see cref="SessionExpirationPage"/>
        /// </summary>
        public string MaxSessionsPage { get; set; }

        /// <summary>
        /// As <see cref="SessionExpirationPage"/>
        /// </summary>
        public string TrafficExceededPage { get; set; }

        /// <summary>
        /// Url of auto logout endpoint
        /// </summary>
        public string AutoLogout { get; set; }

        /// <summary>
        /// Url of session sync url
        /// </summary>
        public string SessionSync { get; set; }

        /// <summary>
        /// Url of server status check url
        /// </summary>
        public string ServerStatusCheck { get; set; }

    }
}
