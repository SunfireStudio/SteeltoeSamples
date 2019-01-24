﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;


namespace CloudFoundrySingleSignon.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Policy = "testgroup")]
        public IActionResult About()
        {
            ViewData["Message"] = "Your About page.";
            return View();
        }


        [Authorize(Policy = "testgroup1")]
        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public async Task<IActionResult> InvokeJwtSample()
        {
            var token = await HttpContext.GetTokenAsync("access_token");

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            string values = null;
            string jwtSamplesUrl = GetJwtSamplesUrl(this.HttpContext);

            try
            {
                values = await client.GetStringAsync(jwtSamplesUrl);
            }
            catch (Exception e)
            {
                values = "Request failed: " + e.Message + " , expect JWT Sample app to be listening at: " + jwtSamplesUrl;
            }

            ViewData["Message"] = values;
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LogOff()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }


        [HttpGet]
        [Authorize]
        public IActionResult Login()
        {
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        public IActionResult Manage()
        {
            ViewData["Message"] = "Manage accounts using UAA or CF command line.";
            return View();
        }

        public IActionResult AccessDenied()
        {
            ViewData["Message"] = "Insufficient permissions.";
            return View();
        }

        const string JWTAPPS_HOSTNAME = "jwtauth";
        const string SSO_HOSTNAME = "single-signon";
        private string GetJwtSamplesUrl(HttpContext httpContext)
        {
            string hostName = httpContext.Request.Host.Host;
            string jwtappsHostname = hostName;
            int indx = hostName.IndexOf(SSO_HOSTNAME);
            if (indx >= 0)
            {
                var prefix = hostName.Substring(indx + 13, 0);
                var suffix = hostName.Substring(indx + 13, hostName.Length - indx - 13);
                jwtappsHostname = prefix + JWTAPPS_HOSTNAME + suffix;
            }
            else
            {
                indx = hostName.IndexOf('.');
                if (indx < 0)
                {
                    jwtappsHostname = hostName + ":63807";
                }
                else
                {
                    jwtappsHostname = JWTAPPS_HOSTNAME + hostName.Substring(indx);
                }
            }
            return "http://" + jwtappsHostname + "/api/values";
            //return "http://localhost:63807/api/values";
        }
    }
}
