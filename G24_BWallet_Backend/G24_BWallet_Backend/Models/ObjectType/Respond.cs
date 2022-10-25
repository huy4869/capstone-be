using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Models.ObjectType
{
    public class Respond<T>
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; }
        public string Error { get; set; }
        public T Data { get; set; }

    }
}
