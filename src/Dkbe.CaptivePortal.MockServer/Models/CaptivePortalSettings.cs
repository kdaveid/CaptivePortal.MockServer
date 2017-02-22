namespace Dkbe.CaptivePortal.MockServer.Models
{
    public partial class CaptivePortalSettings
    {
        public string BaseUrl { get; set; }

        public int SessionSyncInterval { get; set; }

        public int AutoLogoutInterval { get; set; }

        public int ServerStatusCheckInterval { get; set; }

        public CaptivePortalEndpoints Endpoints { get; set; }
    }


    public partial class CaptivePortalSettings
    {
        public bool SessionSyncEnabled { get { return SessionSyncInterval > 0; } }

        public bool AutoLogoutEnabled { get { return AutoLogoutInterval > 0; } }

        public bool ServerStatusCheckEnabled { get { return ServerStatusCheckInterval > 0; } }
    }

}
