// Copyright (c) David E. Keller. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;

namespace Dkbe.CaptivePortal.MockServer.Models
{
    public static class CaptivePortalSettingsExtensions
    {
        public static Uri GetBaseUri(this CaptivePortalSettings settings)
        {
            return new Uri(settings.BaseUrl, UriKind.Absolute);
        }

        public static Uri GetLoginPage(this CaptivePortalSettings settings, string zonePath)
        {
            return new Uri(settings.GetBaseUri(), settings.Endpoints.LoginPage.Replace("{zone}", zonePath));
        }
        public static Uri GetSessionExpirationPage(this CaptivePortalSettings settings, string zonePath)
        {
            return new Uri(settings.GetBaseUri(), settings.Endpoints.SessionExpirationPage.Replace("{zone}", zonePath));
        }

        public static Uri GetSessionIdleTimeOutPage(this CaptivePortalSettings settings, string zonePath)
        {
            return new Uri(settings.GetBaseUri(), settings.Endpoints.IdleTimeOutPage.Replace("{zone}", zonePath));
        }

        public static Uri GetSessionMaxSessionsPage(this CaptivePortalSettings settings, string zonePath)
        {
            return new Uri(settings.GetBaseUri(), settings.Endpoints.MaxSessionsPage.Replace("{zone}", zonePath));
        }

        public static Uri GetSessionTrafficExceeededPage(this CaptivePortalSettings settings, string zonePath)
        {
            return new Uri(settings.GetBaseUri(), settings.Endpoints.TrafficExceededPage.Replace("{zone}", zonePath));
        }

        public static Uri GetAutoLogoutEndpoint(this CaptivePortalSettings settings)
        {
            return new Uri(settings.GetBaseUri(), settings.Endpoints.AutoLogout);
        }
        public static Uri GetServerStatusCheckEndpoint(this CaptivePortalSettings settings)
        {
            return new Uri(settings.GetBaseUri(), settings.Endpoints.ServerStatusCheck);
        }

        public static Uri GetSessionSyncEndpoint(this CaptivePortalSettings settings, string zonePath)
        {
            return new Uri(settings.GetBaseUri(), settings.Endpoints.SessionSync.Replace("{zone}", zonePath));
        }
    }
}
