// Copyright (c) David E. Keller. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Mvc;
using Dkbe.CaptivePortal.MockServer;

namespace MockServer.Controllers
{
    [Route("api/[controller]")]
    public class SettingsController : Controller
    {
        private readonly IStateProvider _stateProvider;

        public SettingsController(IStateProvider stateProvider)
        {
            _stateProvider = stateProvider;

        }
        [HttpPost("login")]
        public IActionResult Login(int code)
        {
            if (code == 0) return BadRequest();

            _stateProvider.LoginReplyCode = code;

            return Json(new { message = "Changed to " + code });
        }
        [HttpPost("logoff")]
        public IActionResult Logoff(int code)
        {
            if (code == 0) return BadRequest();

            _stateProvider.LogoffReplyCode = code;

            return Json(new { message = "Changed to " + code });
        }

        [HttpPost("updatesession")]
        public IActionResult UpdateSession(int code)
        {
            if (code == 0) return BadRequest();

            _stateProvider.UpdateSessionReplyCode = code;

            return Json(new { message = "Changed to " + code });
        }
    }
}
