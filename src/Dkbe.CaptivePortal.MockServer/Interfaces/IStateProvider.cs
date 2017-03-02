// Copyright (c) David E. Keller. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Dkbe.CaptivePortal.MockServer.Models;
using Dkbe.CaptivePortal.Models.SonicOS;
using System.Collections.Generic;

namespace Dkbe.CaptivePortal.MockServer
{
    /// <summary>
    /// Fake SNWL core. Handels session bookkeeping.
    /// </summary>
    public interface IStateProvider
    {
        /// <summary>
        /// Gets statically, from appsettings.json loaded Zones
        /// </summary>
        IEnumerable<StaticZone> StaticZones { get; }

        /// <summary>
        /// Gets bookkeeping of faked sessions
        /// </summary>
        IEnumerable<FakeSNWLSession> GetGeneratedSessions { get; }

        /// <summary>
        /// Gets or sets current login reply setting
        /// </summary>
        int LoginReplyCode { get; set; }

        /// <summary>
        /// Gets or sets update session reply setting
        /// </summary>
        int UpdateSessionReplyCode { get; set; }

        /// <summary>
        /// Gets or sets logoff reply setting
        /// </summary>
        int LogoffReplyCode { get; set; }

        /// <summary>
        /// Adds newly created fake session to bookkeeping list with <see cref="SessionStatus"/>.REQUESTED
        /// </summary>
        /// <param name="session">Fake (internal) SNWL session.</param>
        void AddGeneratedSession(FakeSNWLSession session);

        /// <summary>
        /// Processes official (to LHM standard) requested login
        /// </summary>
        /// <param name="loginRequestModel">SNWL login model which comes from CaptivePortal</param>
        void ProcessLoginRequest(SNWLExternalAuthenticationRequest loginRequestModel);

        /// <summary>
        /// Processes official (to LHM standard) requested update
        /// </summary>
        /// <param name="updateRequestModel">SNWL update model which comes from CaptivePortal</param>
        void ProcessUpdateRequest(SNWLUpdateSessionRequest updateRequestModel);

        /// <summary>
        /// Processes official (to LHM standard) requested logoff
        /// </summary>
        /// <param name="logoffRequestModel">SNWL logoff model which comes from CaptivePortal</param>
        void ProcessLogoutRequest(SNWLLogoffRequest logoffRequestModel);

        /// <summary>
        /// Delete session (cleanup). Nonofficial function.
        /// </summary>
        /// <param name="sessionId"></param>
        void Delete(string sessionId);
    }
}
