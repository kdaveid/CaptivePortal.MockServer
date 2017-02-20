using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dkbe.CaptivePortal.MockServer.Models
{
    public class AppSettings
    {
        public string CaptivePortalUrl { get; set; }

        public GuestAuthSettings GuestAuthSettings { get; set; }

    }

    public static class AppSettingsExtensions
    {
        static Uri getBaseUri(AppSettings appSettings)
        {
            return new Uri(appSettings.CaptivePortalUrl, UriKind.Absolute);
        }

        public static Uri GetLoginPage(this AppSettings appSettings, string zonePath)
        {
            return new Uri(getBaseUri(appSettings), appSettings.GuestAuthSettings.LoginPage.Replace("{zone}", zonePath));
        }
        public static Uri GetSessionExpirationPage(this AppSettings appSettings, string zonePath)
        {
            return new Uri(getBaseUri(appSettings), appSettings.GuestAuthSettings.SessionExpirationPage.Replace("{zone}", zonePath));
        }

        public static Uri GetSessionIdleTimeOutPage(this AppSettings appSettings, string zonePath)
        {
            return new Uri(getBaseUri(appSettings), appSettings.GuestAuthSettings.IdleTimeOutPage.Replace("{zone}", zonePath));
        }

        public static Uri GetSessionMaxSessionsPage(this AppSettings appSettings, string zonePath)
        {
            return new Uri(getBaseUri(appSettings), appSettings.GuestAuthSettings.MaxSessionsPage.Replace("{zone}", zonePath));
        }

        public static Uri GetSessionTrafficExceeededPage(this AppSettings appSettings, string zonePath)
        {
            return new Uri(getBaseUri(appSettings), appSettings.GuestAuthSettings.TrafficExceededPage.Replace("{zone}", zonePath));
        }


    }


}
