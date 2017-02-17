using Dkbe.CaptivePortal.Models.SonicOS;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Dkbe.CaptivePortal.MockServer.Services;
using Microsoft.Extensions.Options;

namespace Dkbe.CaptivePortal.MockServer
{
    public interface IStateProvider
    {
        string[] Zones { get; }

        int LoginReplyCode { get; set; }

        int UpdateSessionReplyCode { get; set; }

        int LogoffReplyCode { get; set; }

        IEnumerable<SNWLSession> GetGeneratedSessions { get; }

        void AddGeneratedSession(SNWLSession session);

        void UpdateSessionWithLogin(SNWLExternalAuthenticationRequest login);

        void UpdateSessionWithUpdate(SNWLUpdateSessionRequest login);

        void RemoveGeneratedSession(SNWLLogoffRequest logoff);
    }

    public class StateProvider : IStateProvider
    {
        List<SNWLSession> _generatedSessions;
        private readonly ILogger<StateProvider> _logger;
        public string[] Zones { get; }

        public StateProvider(ILogger<StateProvider> logger, IOptions<StateProviderOptions> options)
        {
            _generatedSessions = new List<SNWLSession>();
            _logger = logger;
            Zones = options.Value.ZoneNames;
        }

        public int LoginReplyCode { get; set; } = ResponseHelper.Login.LOGIN_SUCCEEDED;

        public int UpdateSessionReplyCode { get; set; } = ResponseHelper.UpdateSession.SESSION_UPDATE_SUCCEEDED;

        public int LogoffReplyCode { get; set; } = ResponseHelper.Logoff.LOGOFF_SUCCEEDED;

        public void AddGeneratedSession(SNWLSession session)
        {
            if (!_generatedSessions.Contains(session))
                _generatedSessions.Add(session);
        }

        public void UpdateSessionWithLogin(SNWLExternalAuthenticationRequest loginModel)
        {
            var session = _generatedSessions.Where(s => s.ID == loginModel.sessId).SingleOrDefault();
            if (session == null)
            {
                _logger.LogCritical($"Session with SessionID \"{loginModel.sessId}\" not found in local collection. Could not update.");
                return;
            }

            session.UserName = loginModel.userName;
            session.SessionRemaining = loginModel.sessionLifetime;
        }

        public void UpdateSessionWithUpdate(SNWLUpdateSessionRequest updateModel)
        {
            var session = _generatedSessions.Where(s => s.ID == updateModel.sessID).SingleOrDefault();
            if (session == null)
                _logger.LogCritical($"Session with SessionID \"{updateModel.sessID}\" not found in local collection. Could not update.");

            session.UserName = updateModel.userName;
            session.SessionRemaining = updateModel.sessionLifetime;
        }

        public void RemoveGeneratedSession(SNWLLogoffRequest logoff)
        {
            var session = _generatedSessions.Where(s => s.ID == logoff.sessId).SingleOrDefault();
            if (session == null)
                _logger.LogCritical($"Session with SessionID \"{logoff.sessId}\" not found in local collection. Could not remove it.");

            _generatedSessions.Remove(session);
        }

        public IEnumerable<SNWLSession> GetGeneratedSessions { get { return _generatedSessions; } }
    }
}
