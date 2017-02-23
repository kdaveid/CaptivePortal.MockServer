using Dkbe.CaptivePortal.Models.SonicOS;

namespace Dkbe.CaptivePortal.MockServer.Models
{
    public class FakeSNWLSession : SNWLSession
    {
        public FakeSNWLSession(StaticZone zone) { Zone = zone; }

        public SessionStatus Status { get; set; } = 0;

        public StaticZone Zone { get; private set; }
    }

    public enum SessionStatus
    {
        REQUESTED = 0,
        LOGIN_SUCCEEDED = 50,
        SESSION_LIMIT_EXCEEDED = 51,
        LOGOFF_SUCCEEDED = 150,
        LOGIN_FAILED = 100,
        INVALID_HMAC = 251,
        INVALID_SESSION_ID = 253,
        INVALID_OR_MISSING_PARAM = 254,
        INTERNAL_ERROR = 255
    }
}
