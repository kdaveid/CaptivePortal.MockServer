using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Dkbe.CaptivePortal.Models.SonicOS;
using Dkbe.CaptivePortal.MockServer.Services;
using System.Net.Http;
using Microsoft.Extensions.Options;
using Dkbe.CaptivePortal.MockServer.Models;
using Microsoft.Extensions.Logging;

namespace Dkbe.CaptivePortal.MockServer.Controllers
{
    public class HomeController : Controller
    {
        private readonly IStateProvider _stateProvider;
        private readonly AppSettings _appSettings;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IStateProvider stateProvider, IOptions<AppSettings> appSettings, ILogger<HomeController> logger)
        {
            _stateProvider = stateProvider;
            _appSettings = appSettings.Value;
            _logger = logger;
        }

        public IActionResult Index()
        {
            ViewBag.Zones = _stateProvider.Zones;
            return View();
        }

        [HttpGet("login/{zone}")]
        public IActionResult Login(string zone)
        {
            var sessionId = Guid.NewGuid();
            var model = new SNWLExternalAuthenticationRedirectModel
            {
                SessionId = sessionId.ToString(),
                MAC = FakeDataGenerator.GenerateMACAddress(),
                IP = FakeDataGenerator.GenerateRandomIp(),
                Ufi = "0006010203",
                SSID = "",
                req = "http://www.google.ch",
                ClientRedirectUrl = "http://localhost:5001",
                MgmtBaseUrl = "http://localhost:1234"
            };

            // keep track of generated models for session sync
            _stateProvider.AddGeneratedSession(model.Map());

            string queryString = $"/?" +
                        $"SessionId={model.SessionId}&" +
                        $"MAC={model.MAC}&" +
                        $"IP={model.IP}&" +
                        $"Ufi={model.Ufi}&" +
                        $"SSID={model.SSID}&" +
                        $"req={Uri.EscapeUriString(model.req)}&" +
                        $"ClientRedirectUrl={Uri.EscapeUriString(model.ClientRedirectUrl)}&" +
                        $"MgmtBaseUrl={Uri.EscapeUriString(model.MgmtBaseUrl)}";

            var fullUrl = $"{_appSettings.GetLoginPage(zone)}{queryString}";
            _logger.LogInformation($"Redirect user to: {fullUrl}");
            return Redirect(fullUrl);
        }

        [HttpPost("externalGuestLogin.cgi")]
        [Produces("application/xml")]
        public IActionResult ExternalGuestLogin([FromForm] SNWLExternalAuthenticationRequest model)
        {
            SNWLExternalAuthenticationXMLResponse responseModel;

            if (!ModelState.IsValid)
            {
                return new ObjectResult(ResponseHelper.Login.InvalidOrMissingCGIParamResponse());
            }

            _stateProvider.UpdateSessionWithLogin(model);

            switch (_stateProvider.LoginReplyCode)
            {
                case ResponseHelper.Login.LOGIN_SUCCEEDED:
                    responseModel = ResponseHelper.Login.LoginSucceededResponse();
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
            return new ObjectResult(responseModel);
        }

        [HttpPost("externalGuestUpdateSession.cgi")]
        [Produces("application/xml")]
        public IActionResult UpdateSession([FromForm] SNWLUpdateSessionRequest model)
        {
            SNWLUpdateSessionReplyXMLResponse responseModel;

            if (!ModelState.IsValid)
            {
                return new ObjectResult(ResponseHelper.UpdateSession.InvalidOrMissingCGIParamResponse());
            }

            _stateProvider.UpdateSessionWithUpdate(model);

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

            _stateProvider.RemoveGeneratedSession(model);

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
            return new ObjectResult(responseModel);
        }

        [HttpGet("invoke-sessionsync")]
        public IActionResult InvokeSessionSync(SNWLLogoffRequest model)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(_appSettings.CaptivePortalUrl, UriKind.Absolute);

            var response = client.GetAsync("api/manage/sync").Result;
            response.EnsureSuccessStatusCode();

            return Ok();
        }

        [HttpPost("set-session-remaining")]
        public IActionResult UpdateSession(string sessionid, int remaining)
        {
            var session = _stateProvider.GetGeneratedSessions.Where(s => s.ID == sessionid).SingleOrDefault();
            session.SessionRemaining = remaining;
            return Ok();
        }

        [HttpGet("redirect-to-session-expiration/{zone}")]
        public IActionResult RedirectToSessionExpiration(string zone)
        {
            return Redirect(_appSettings.GetSessionExpirationPage(zone).ToString());
        }

        [HttpGet("redirect-to-max-session/{zone}")]
        public IActionResult RedirectToMaxSessions(string zone)
        {
            return Redirect(_appSettings.GetSessionMaxSessionsPage(zone).ToString());
        }

        [HttpGet("redirect-to-idle-timeout/{zone}")]
        public IActionResult RedirectToIdleTimeout(string zone)
        {
            return Redirect(_appSettings.GetSessionIdleTimeOutPage(zone).ToString());
        }

        [HttpGet("redirect-to-traffic-excceeded/{zone}")]
        public IActionResult RedirectToTrafficExceeded(string zone)
        {
            return Redirect(_appSettings.GetSessionTrafficExceeededPage(zone).ToString());
        }

        public IActionResult Error()
        {
            return View();
        }
    }



}
