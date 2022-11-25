using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly IImageRepository imageRepo;
        //private readonly IUserR

        public ImageController(IImageRepository InitImageRepo)
        {
            imageRepo = InitImageRepo;
        }

        [HttpPost("{folder}")]
        public async Task<Respond<string>> saveIMG(string folder, [FromForm] IFormFile imgFile)
        {
            string fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + imgFile.FileName;
            var imglinks = imageRepo.SaveIMMGFile(folder, imgFile, fileName);

            return new Respond<string>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "lưu ảnh thành công",
                Data = await imglinks
            };
        }

        [HttpPut("{folder}")]
        public async Task<Respond<string>> UpdateIMG(string folder, [FromForm] IFormFile imgFile)
        {
            string fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + imgFile.FileName;
            var imglinks = imageRepo.SaveIMMGFile(folder, imgFile, fileName);

            return new Respond<string>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "lưu ảnh thành công",
                Data = await imglinks
            };
        }
    }
}
