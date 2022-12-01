using G24_BWallet_Backend.DBContexts;
using G24_BWallet_Backend.Models;
using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FAQController : ControllerBase
    {
        private readonly MyDBContext context;

        public FAQController(MyDBContext context1)
        {
            this.context = context1;
        }

        [HttpGet]
        public async Task<Respond<List<FAQ>>> Check([FromForm] string phone)
        {
            List<FAQ> listFAQ = context.FAQ.ToList();
            return new Respond<List<FAQ>>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "",
                Data = listFAQ
            }; ;
        }
    }
}
