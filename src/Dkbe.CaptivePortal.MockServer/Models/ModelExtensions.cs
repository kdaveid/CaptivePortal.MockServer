using Dkbe.CaptivePortal.Models.SonicOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dkbe.CaptivePortal.MockServer.Models
{
    public static class ModelExtensions
    {
        public static SNWLSession Map(this SNWLExternalAuthenticationRedirectModel requestModel)
        {
            return new SNWLSession
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
