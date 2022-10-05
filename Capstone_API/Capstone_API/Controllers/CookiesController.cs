using Capstone_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Capstone_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // Save account and password to cookie after a user login
    public class CookiesController : ControllerBase
    {
        private readonly string Key_Account = "Acc";
        private readonly string Key_Password = "Pass";

        [HttpPost]
        public IActionResult AddCookie([FromForm] string acc, [FromForm] string pass)
        {
            try
            {
                CookieOptions cookie = new CookieOptions();
                cookie.Expires = DateTime.Now.AddDays(1);
                Response.Cookies.Append(Key_Account, acc, cookie);
                Response.Cookies.Append(Key_Password, pass, cookie);
                return Ok("Add cookie success");
            }
            catch (Exception ex)
            {
                return BadRequest("Add cookie fail: " + ex);
            }
        }

        [HttpGet]
        public IActionResult GetCookie()
        {
            try
            {
                string account = Request.Cookies[Key_Account];
                string password = Request.Cookies[Key_Password];
                return Ok(string.Format("Get success: Account: {0} - Password: {1}", account
                    , password));
            }
            catch (Exception ex)
            {
                return BadRequest("Get cookie fail: " + ex);
            }
        }

        [HttpDelete]
        public IActionResult DeleteCookie()
        {
            try
            {
                CookieOptions cookie = new CookieOptions();
                cookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Append(Key_Account, null, cookie);
                Response.Cookies.Append(Key_Password, null, cookie);
                return Ok("Delete cookie success");
            }
            catch (Exception ex)
            {
                return BadRequest("Delete cookie fail: " + ex);
            }
        }
    }
}
