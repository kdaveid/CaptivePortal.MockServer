using Dkbe.CaptivePortal.MockServer.Models;
using Dkbe.CaptivePortal.Models.SonicOS;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;

namespace Dkbe.CaptivePortal.MockServer
{
    public class StateProvider : IStateProvider
    {
        #region Properties

        public IEnumerable<FakeSNWLSession> GetGeneratedSessions { get { return _generatedSessions; } }

        public IEnumerable<StaticZone> StaticZones { get; }

        public int LoginReplyCode { get; set; } = ResponseHelper.Login.LOGIN_SUCCEEDED;

        public int UpdateSessionReplyCode { get; set; } = ResponseHelper.UpdateSession.SESSION_UPDATE_SUCCEEDED;

        public int LogoffReplyCode { get; set; } = ResponseHelper.Logoff.LOGOFF_SUCCEEDED;

        #endregion

        #region CTOR and private fields

        private List<FakeSNWLSession> _generatedSessions = new List<FakeSNWLSession>();
        private readonly ILogger<StateProvider> _logger;

        public StateProvider(ILogger<StateProvider> logger, IOptions<StaticZoneSettings> options)
        {
            _logger = logger;
            StaticZones = options.Value.Zones;
        }

        #endregion

        public void AddGeneratedSession(FakeSNWLSession session)
        {
            if (!_generatedSessions.Contains(session))
                _generatedSessions.Add(session);
        }

        public void ProcessLoginRequest(SNWLExternalAuthenticationRequest loginModel)
        {
            var session = _generatedSessions.Where(s => s.ID == loginModel.sessId).SingleOrDefault();
            if (session == null)
            {
                _logger.LogCritical($"Session with SessionID \"{loginModel.sessId}\" not found in local collection. Could not update.");
                return;
            }

            session.Status = (SessionStatus)LoginReplyCode;
            session.UserName = loginModel.userName;
            session.SessionRemaining = loginModel.sessionLifetime;
        }

      
        public void ProcessUpdateRequest(SNWLUpdateSessionRequest updateModel)
        {
            var session = _generatedSessions.Where(s => s.ID == updateModel.sessID).SingleOrDefault();
            if (session == null)
                _logger.LogCritical($"Session with SessionID \"{updateModel.sessID}\" not found in local collection. Could not update.");

            session.Status = (SessionStatus)UpdateSessionReplyCode;
            session.UserName = updateModel.userName;
            session.SessionRemaining = updateModel.sessionLifetime;
        }

        public void ProcessLogoutRequest(SNWLLogoffRequest logoff)
        {
            var session = _generatedSessions.Where(s => s.ID == logoff.sessId).SingleOrDefault();
            if (session == null)
                _logger.LogCritical($"Session with SessionID \"{logoff.sessId}\" not found in local collection. Could not mark it signedout.");

            session.Status = (SessionStatus)LogoffReplyCode;
            _generatedSessions.Remove(session);
        }

        public void Delete(string sessionId)
        {
            var session = _generatedSessions.Where(s => s.ID == sessionId).SingleOrDefault();
            if (session == null)
                _logger.LogCritical($"Session with SessionID \"{sessionId}\" not found in local collection. Could not delete it.");

            _generatedSessions.Remove(session);
        }
    }
}
