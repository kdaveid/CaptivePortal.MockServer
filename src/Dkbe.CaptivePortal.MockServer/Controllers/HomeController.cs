// Copyright (c) David E. Keller. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Dkbe.CaptivePortal.MockServer.Models;
using Dkbe.CaptivePortal.MockServer.Services;
using Dkbe.CaptivePortal.Models.SonicOS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Xml;
using System.Xml.Serialization;

namespace Dkbe.CaptivePortal.MockServer.Controllers
{
    public class HomeController : Controller
    {
        private readonly IStateProvider _stateProvider;
        private readonly ILogger<HomeController> _logger;
        private readonly CaptivePortalSettings _settings;
        private readonly List<StaticZone> _zones;

        public HomeController(IStateProvider stateProvider, IOptions<CaptivePortalSettings> captivePortalSettings, ILogger<HomeController> logger, IOptions<StaticZoneSettings> zoneSettings)
        {
            _stateProvider = stateProvider;
            _settings = captivePortalSettings.Value;
            _logger = logger;
            _zones = zoneSettings.Value.Zones;
        }

        public IActionResult Index()
        {
            var viewModel = new IndexViewModel
            {
                StaticZones = _stateProvider.StaticZones,
                Sessions = _stateProvider.GetGeneratedSessions.OrderByDescending(s => s.SessionRemaining),
                CurrentLoginReplyCode = _stateProvider.LoginReplyCode,
                CurrentUpdateSessionReplyCode = _stateProvider.UpdateSessionReplyCode,
                CurrentLogoffReplyCode = _stateProvider.LogoffReplyCode
            };
            return View(viewModel);
        }

        [HttpGet("signin/{zone}")]
        public IActionResult CreateSession(string zone)
        {
            var zoneModel = _zones.Single(s => s.LocalPath.Equals(zone, StringComparison.CurrentCultureIgnoreCase));

            if (zoneModel == null) return RedirectToAction(nameof(Error), new { message = $"Could not find Zone for \"zone\"" });

            var model = new SNWLExternalAuthenticationRedirectModel
            {
                SessionId = Guid.NewGuid().ToString(),
                MAC = FakeDataGenerator.GenerateMACAddress(),
                IP = FakeDataGenerator.GenerateRandomIp(),
                Ufi = "0006010203",
                SSID = "",
                req = "http://www.gibhub.com",
                ClientRedirectUrl = "http://localhost:5001",
                MgmtBaseUrl = "http://localhost:1234"
            };

            // keep track of generated models for session sync
            _stateProvider.AddGeneratedSession(model.Map(zoneModel));

            var fullUrl = string.Concat(_settings.GetLoginPage(zoneModel.LocalPath), buildQueryString(model));

            _logger.LogInformation($"Redirect user to: {fullUrl}");

            return Redirect(fullUrl);
        }

        private string buildQueryString(SNWLExternalAuthenticationRedirectModel model)
        {
            return $"/?" +
                    $"SessionId={model.SessionId}&" +
                    $"MAC={model.MAC}&" +
                    $"IP={model.IP}&" +
                    $"Ufi={model.Ufi}&" +
                    $"SSID={model.SSID}&" +
                    $"req={Uri.EscapeUriString(model.req)}&" +
                    $"ClientRedirectUrl={Uri.EscapeUriString(model.ClientRedirectUrl)}&" +
                    $"MgmtBaseUrl={Uri.EscapeUriString(model.MgmtBaseUrl)}";
        }

        [HttpPost("externalGuestLogin.cgi")]
        [Produces("application/xml")]
        public IActionResult ExternalGuestLogin(SNWLExternalAuthenticationRequest model)
        {
            SNWLExternalAuthenticationXMLResponse responseModel;

            if (!ModelState.IsValid)
            {
                return new ObjectResult(ResponseHelper.Login.InvalidOrMissingCGIParamResponse());
            }

            switch (_stateProvider.LoginReplyCode)
            {
                case ResponseHelper.Login.LOGIN_SUCCEEDED:
                    responseModel = ResponseHelper.Login.LoginSucceededResponse();
                    break;
                case ResponseHelper.Login.SESSION_LIMIT_EXCEEDED:
                    responseModel = ResponseHelper.Login.SessionLimitExceededResponse();
                    break;
                case ResponseHelper.Login.LOGIN_FAILED:
                    responseModel = ResponseHelper.Login.LoginFailedResponse();
                    break;
                case ResponseHelper.Login.INVALID_OR_MISSING_PARAM:
                    responseModel = ResponseHelper.Login.InvalidOrMissingCGIParamResponse();
                    break;
                case ResponseHelper.Login.INVALID_HMAC:
                    responseModel = ResponseHelper.Login.InvalidHMACResponse();
                    break;
                default:
                case ResponseHelper.Login.INTERNAL_ERROR:
                    responseModel = ResponseHelper.Login.InternalErrorResponse();
                    break;
            }

            // Local bookkeeping
            _stateProvider.ProcessLoginRequest(model);

            return new ObjectResult(responseModel);
        }

        [HttpPost("externalGuestUpdateSession.cgi")]
        [Produces("application/xml")]
        public IActionResult UpdateSession(SNWLUpdateSessionRequest model)
        {
            SNWLUpdateSessionReplyXMLResponse responseModel;

            if (!ModelState.IsValid)
            {
                return new ObjectResult(ResponseHelper.UpdateSession.InvalidOrMissingCGIParamResponse());
            }

            switch (_stateProvider.UpdateSessionReplyCode)
            {
                case ResponseHelper.UpdateSession.SESSION_UPDATE_SUCCEEDED:
                    responseModel = ResponseHelper.UpdateSession.UpdateSessionSucceededResponse();
                    break;
                case ResponseHelper.UpdateSession.SESSION_UPDATE_FAILED:
                    responseModel = ResponseHelper.UpdateSession.UpdateSessionFailedResponse();
                    break;
                case ResponseHelper.UpdateSession.INVALID_OR_MISSING_PARAM:
                    responseModel = ResponseHelper.UpdateSession.InvalidOrMissingCGIParamResponse();
                    break;
                case ResponseHelper.UpdateSession.INVALID_HMAC:
                    responseModel = ResponseHelper.UpdateSession.InvalidHMACResponse();
                    break;
                default:
                case ResponseHelper.UpdateSession.INTERNAL_ERROR:
                    responseModel = ResponseHelper.UpdateSession.InternalErrorResponse();
                    break;
            }

            // local bookkeeping
            _stateProvider.ProcessUpdateRequest(model);

            return new ObjectResult(responseModel);
        }

        [HttpPost("externalGuestLogoff.cgi")]
        [Produces("application/xml")]
        public IActionResult Logoff(SNWLLogoffRequest model)
        {
            SNWLLogoffReplyXMLResponse responseModel;

            if (!ModelState.IsValid)
            {
                return new ObjectResult(ResponseHelper.Logoff.InvalidOrMissingCGIParamResponse());
            }

            switch (_stateProvider.LogoffReplyCode)
            {
                case ResponseHelper.Logoff.LOGOFF_SUCCEEDED:
                    responseModel = ResponseHelper.Logoff.LogoffSucceededResponse();
                    break;
                case ResponseHelper.Logoff.INVALID_SESSION_ID:
                    responseModel = ResponseHelper.Logoff.InvalidSessionIDResponse();
                    break;
                case ResponseHelper.Logoff.INVALID_OR_MISSING_PARAM:
                    responseModel = ResponseHelper.Logoff.InvalidOrMissingCGIParamResponse();
                    break;
                case ResponseHelper.Logoff.INVALID_HMAC:
                    responseModel = ResponseHelper.Logoff.InvalidHMACResponse();
                    break;
                default:
                case ResponseHelper.Logoff.INTERNAL_ERROR:
                    responseModel = ResponseHelper.Logoff.InternalErrorResponse();
                    break;
            }

            // local bookkeeping
            _stateProvider.ProcessLogoutRequest(model);

            return new ObjectResult(responseModel);
        }

        [HttpGet("invoke-sessionsync/{zone}")]
        public IActionResult InvokeSessionSync(string zone)
        {
            var loggedInZoneSessions = _stateProvider.GetGeneratedSessions
                                    .Where(s =>
                                        s.Status == SessionStatus.LOGIN_SUCCEEDED &&
                                        s.Zone.Name.Equals(zone, StringComparison.CurrentCultureIgnoreCase
                                        ));

            var requestModel = new SNWLSessionSyncRequest
            {
                SessionSync = new SNWLSessionSync
                {
                    SessionCount = loggedInZoneSessions.Count(),
                    SessionList = loggedInZoneSessions.Cast<SNWLSession>().ToArray()

                }
            };

            using (var client = new HttpClient())
            {
                var serializedRequestModel = Serializers.Serialize(requestModel);

                var formContent = new FormUrlEncodedContent(
                    new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("sessionList", serializedRequestModel)
                    });

                var response = client.PostAsync(_settings.GetSessionSyncEndpoint(zone), formContent).Result;
                response.EnsureSuccessStatusCode();

                var serializer = new XmlSerializer(typeof(SNWLSessionStateSyncReply));
                var responseString = response.Content.ReadAsStringAsync().Result;

                SNWLSessionStateSyncReply reply;

                try
                {
                    using (var reader = new StringReader(responseString))
                    {
                        reply = (SNWLSessionStateSyncReply)serializer.Deserialize(reader);
                    }
                }
                catch (Exception)
                {
                    var msg = $"Can not deserialize sessionList. Payload: {responseString}";
                    _logger.LogCritical(msg);
                    return Json(new { success = false, message = msg });
                }

                if (reply.SessionSync.ResponseCode != ResponseHelper.SessionSync.SESSION_SYNC_SUCCESS)
                    return Json(new { success = false, message = "CaptivePortal returned failed code.", responsecode = reply.SessionSync.ResponseCode });

                return Json(new { success = true, message = "OK", responsecode = reply.SessionSync.ResponseCode }); ;
            }
        }

        [HttpGet("invoke-autologout")]
        public IActionResult InvokeAutoLogout()
        {
            throw new NotImplementedException();

            //var client = new HttpClient();
            //var httpContent = new FormUrlEncodedContent();

            //var response = client.PostAsync(_settings.GetAutoLogoutEndpoint(), content).Result;
            //response.EnsureSuccessStatusCode();

            //return Ok();
        }

        [HttpGet("invoke-serverstatuscheck")]
        public IActionResult InvokeServerStatusCheck()
        {
            using (var client = new HttpClient())
            {
                var response = client.GetAsync(_settings.GetServerStatusCheckEndpoint()).Result;
                response.EnsureSuccessStatusCode();

                // TODO: parse result

                // TODO: check return code

                // TODO: return JSON

                return Ok();
            }
        }

        [HttpPost("set-session-remaining")]
        public IActionResult SetSessionRemaining(string sessionid, int remaining)
        {
            var session = _stateProvider.GetGeneratedSessions.SingleOrDefault(s => s.ID == sessionid);
            session.SessionRemaining = remaining;
            return Ok();
        }

        #region Simple Redirects

        [HttpGet("redirect-to-session-expiration/{zone}")]
        public IActionResult RedirectToSessionExpiration(string zone)
        {
            return Redirect(_settings.GetSessionExpirationPage(zone).ToString());
        }

        [HttpGet("redirect-to-max-session/{zone}")]
        public IActionResult RedirectToMaxSessions(string zone)
        {
            return Redirect(_settings.GetSessionMaxSessionsPage(zone).ToString());
        }

        [HttpGet("redirect-to-idle-timeout/{zone}")]
        public IActionResult RedirectToIdleTimeout(string zone)
        {
            return Redirect(_settings.GetSessionIdleTimeOutPage(zone).ToString());
        }

        [HttpGet("redirect-to-traffic-excceeded/{zone}")]
        public IActionResult RedirectToTrafficExceeded(string zone)
        {
            return Redirect(_settings.GetSessionTrafficExceeededPage(zone).ToString());
        }

        #endregion Simple Redirects

        public IActionResult Error(string message)
        {
            ViewBag.Message = message;
            return View();
        }

    }

    public static class Serializers
    {
        public static string Serialize<T>(this T value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            try
            {
                var xmlserializer = new XmlSerializer(typeof(T));
                var stringWriter = new StringWriter();
                using (var writer = XmlWriter.Create(stringWriter))
                {
                    xmlserializer.Serialize(writer, value);
                    return stringWriter.ToString();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred", ex);
            }
        }

    }
}
