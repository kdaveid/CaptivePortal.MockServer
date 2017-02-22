using Dkbe.CaptivePortal.Models.SonicOS;

namespace Dkbe.CaptivePortal.MockServer.Models
{
    public static class ModelExtensions
    {
        public static FakeSNWLSession Map(this SNWLExternalAuthenticationRedirectModel requestModel)
        {
            return new FakeSNWLSession
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
