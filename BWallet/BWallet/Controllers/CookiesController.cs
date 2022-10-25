using BWallet.Models;
using BWallet.Models.ObjectType;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Principal;

namespace BWallet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // Save account and password to cookie after a user login
    public class CookiesController : ControllerBase
    {
        private readonly string Key_Account = "Acc";
        private readonly string Key_Password = "Pass";

        [HttpPost]
        public Respond<bool> AddCookie([FromForm] string acc, [FromForm] string pass)
        {
            try
            {
                CookieOptions cookie = new CookieOptions();
                cookie.Expires = DateTime.Now.AddDays(1);
                Response.Cookies.Append(Key_Account, acc, cookie);
                Response.Cookies.Append(Key_Password, pass, cookie);
                return new Respond<bool>()
                {
                    StatusCode = HttpStatusCode.Accepted,
                    Error = "",
                    Message = "Add cookie success!",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new Respond<bool>()
                {
                    StatusCode = HttpStatusCode.Forbidden,
                    Error = ex.ToString(),
                    Message = "Add cookie fail!",
                    Data = false
                };
            }
        }

        [HttpGet]
        public Respond<List<string>> GetCookie()
        {
            try
            {
                string account = Request.Cookies[Key_Account];
                string password = Request.Cookies[Key_Password];
                return new Respond<List<string>>()
                {
                    StatusCode = HttpStatusCode.Accepted,
                    Error = "",
                    Message = "Get cookie success",
                    Data = new List<string>() { account, password }
                };
            }
            catch (Exception ex)
            {
                return new Respond<List<string>>()
                {
                    StatusCode = HttpStatusCode.NotAcceptable,
                    Error = ex.ToString(),
                    Message = "Get cookie fail",
                    Data = null
                };
            }
        }

        [HttpDelete]
        public Respond<bool> DeleteCookie()
        {
            try
            {
                CookieOptions cookie = new CookieOptions();
                cookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Append(Key_Account, null, cookie);
                Response.Cookies.Append(Key_Password, null, cookie);
                return new Respond<bool>()
                {
                    StatusCode = HttpStatusCode.Accepted,
                    Error = "",
                    Message = "Delete cookie success",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new Respond<bool>()
                {
                    StatusCode = HttpStatusCode.Accepted,
                    Error = ex.ToString(),
                    Message = "Delete cookie fail",
                    Data = false
                };
            }
        }
    }
}
